using System.Diagnostics;
using System.Runtime.InteropServices;
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
            StartInfo = BuildStartInfo(opts, RuntimeInformation.IsOSPlatform(OSPlatform.Windows)),
        };

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

    /// <summary>
    /// Builds the launch for `claude -p --output-format json [--model X]`.
    /// On Windows the installed <c>claude</c> is an npm <c>.cmd</c> shim, and
    /// .NET can't launch a <c>.cmd</c> with redirected I/O directly (CreateProcess
    /// needs a real executable image) — so it's run through <c>cmd.exe /c</c>,
    /// which resolves the shim via PATHEXT. This is the .NET equivalent of the
    /// Python <c>shell=True</c> trick the Trading app uses. The prompt still goes
    /// on stdin, which cmd passes straight through to claude. Non-Windows launches
    /// the executable directly. Static + <paramref name="isWindows"/> so the
    /// platform branch is unit-testable without spawning anything.
    /// </summary>
    internal static ProcessStartInfo BuildStartInfo(ClaudeOptions opts, bool isWindows)
    {
        var info = new ProcessStartInfo
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (isWindows)
        {
            info.FileName = "cmd.exe";
            info.ArgumentList.Add("/c");
            info.ArgumentList.Add(opts.Executable);
        }
        else
        {
            info.FileName = opts.Executable;
        }

        info.ArgumentList.Add("-p");
        info.ArgumentList.Add("--output-format");
        info.ArgumentList.Add("json");
        if (!string.IsNullOrWhiteSpace(opts.Model))
        {
            info.ArgumentList.Add("--model");
            info.ArgumentList.Add(opts.Model);
        }

        return info;
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
