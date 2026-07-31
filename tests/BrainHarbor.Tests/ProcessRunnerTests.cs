using BrainHarbor.Pipeline.Claude;

namespace BrainHarbor.Tests;

/// <summary>
/// The real Claude launch. The load-bearing case is Windows: the installed
/// `claude` is an npm .cmd shim, and .NET can't launch a .cmd with redirected
/// I/O directly — so it must go through cmd.exe (the .NET equivalent of the
/// Python shell=True trick). Found the hard way running the pipeline locally.
/// </summary>
public class ProcessRunnerTests
{
    private static ClaudeOptions Opts(string exe = "claude", string model = "claude-opus-5") =>
        new() { Executable = exe, Model = model };

    [Fact]
    public void OnWindowsClaudeRunsThroughCmdSoTheShimResolves()
    {
        var info = ClaudeProcessRunner.BuildStartInfo(Opts(), isWindows: true);

        Assert.Equal("cmd.exe", info.FileName);
        Assert.Equal(
            new[] { "/c", "claude", "-p", "--output-format", "json", "--model", "claude-opus-5" },
            info.ArgumentList.ToArray());
        Assert.False(info.UseShellExecute);   // still redirecting stdio, not a shell
        Assert.True(info.RedirectStandardInput);
    }

    [Fact]
    public void OnNonWindowsClaudeIsLaunchedDirectly()
    {
        var info = ClaudeProcessRunner.BuildStartInfo(Opts(), isWindows: false);

        Assert.Equal("claude", info.FileName);
        Assert.Equal(
            new[] { "-p", "--output-format", "json", "--model", "claude-opus-5" },
            info.ArgumentList.ToArray());
    }

    [Fact]
    public void AnEmptyModelOmitsTheModelFlag()
    {
        var info = ClaudeProcessRunner.BuildStartInfo(Opts(model: ""), isWindows: false);

        Assert.DoesNotContain("--model", info.ArgumentList);
    }

    [Fact]
    public void AConfiguredExecutablePathIsHonoredOnBothPlatforms()
    {
        Assert.Equal(@"C:\tools\claude.cmd",
            ClaudeProcessRunner.BuildStartInfo(Opts(exe: @"C:\tools\claude.cmd"), isWindows: true)
                .ArgumentList[1]); // after "/c"
        Assert.Equal("/usr/local/bin/claude",
            ClaudeProcessRunner.BuildStartInfo(Opts(exe: "/usr/local/bin/claude"), isWindows: false)
                .FileName);
    }
}
