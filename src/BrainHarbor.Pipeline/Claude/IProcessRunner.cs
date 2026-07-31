using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace BrainHarbor.Pipeline.Claude;

public sealed record ProcessResult(int ExitCode, string Stdout, string Stderr, bool TimedOut);

/// <summary>
/// Runs the `claude` CLI once with a prompt on stdin. Abstracted so the
/// wrapper's parsing/retry/timeout logic is unit-testable against a fake CLI
/// without spawning a process (WI-302).
/// </summary>
public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string prompt, CancellationToken cancellationToken);
}

/// <summary>
/// The real runner: `claude -p --output-format json`, prompt written to stdin
/// (never a CLI arg — prompts carry a full abstract and can be long), killed
/// if it exceeds the timeout. Never throws for an operational failure (the CLI
/// isn't installed, spawn fails, a pipe hangs): those come back as a
/// non-success <see cref="ProcessResult"/> so the wrapper can fail the item
/// safely instead of crashing the run. Only outer-token cancellation propagates.
/// </summary>
public sealed class ClaudeProcessRunner(IOptions<ClaudeOptions> options) : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(string prompt, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        // Bound the WHOLE invocation — including the stdin write, which can
        // block if a large prompt fills the pipe and the child stops draining.
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(opts.TimeoutSeconds));

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = opts.Executable,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.StartInfo.ArgumentList.Add("-p");
        process.StartInfo.ArgumentList.Add("--output-format");
        process.StartInfo.ArgumentList.Add("json");
        if (!string.IsNullOrWhiteSpace(opts.Model))
        {
            process.StartInfo.ArgumentList.Add("--model");
            process.StartInfo.ArgumentList.Add(opts.Model);
        }

        // Drain both streams to completion; only the null Data event marks EOF,
        // and reading the buffers before that races a dropped final chunk.
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var stdoutDone = new TaskCompletionSource();
        var stderrDone = new TaskCompletionSource();
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) stdoutDone.TrySetResult(); else stdout.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) stderrDone.TrySetResult(); else stderr.AppendLine(e.Data);
        };

        try
        {
            process.Start();
        }
        catch (Exception exception)
        {
            // Most commonly: `claude` not found / not executable. Fail safe.
            return new ProcessResult(-1, "", $"could not start '{opts.Executable}': {exception.Message}", false);
        }

        try
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.StandardInput.WriteAsync(prompt.AsMemory(), timeout.Token);
            process.StandardInput.Close();

            await process.WaitForExitAsync(timeout.Token);
            await Task.WhenAll(stdoutDone.Task, stderrDone.Task);

            return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString(), TimedOut: false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Our timeout fired (not the caller's). Kill and report a timeout.
            TryKill(process);
            return new ProcessResult(-1, stdout.ToString(), stderr.ToString(), TimedOut: true);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Pipe/IO failure mid-call — fail safe rather than crash the run.
            TryKill(process);
            return new ProcessResult(-1, stdout.ToString(), $"claude IO error: {exception.Message}", false);
        }
        finally
        {
            // Outer cancellation (graceful shutdown) also lands here — never
            // leave an orphaned claude process behind.
            TryKill(process);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best effort — the run continues regardless.
        }
    }
}
