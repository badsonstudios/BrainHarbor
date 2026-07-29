using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace BrainHarbor.Pipeline;

/// <summary>
/// WI-210: tells Dan the run finished and how many items are waiting
/// (architecture.md §6). A scheduled task that finishes silently is a
/// scheduled task nobody notices has stopped working.
///
/// Uses PowerShell's toast API — no extra package, and it degrades to a log
/// line anywhere that isn't Windows or where the toast fails.
/// </summary>
public sealed class DesktopNotifier(ILogger<DesktopNotifier> logger)
{
    public void Notify(RunResult result)
    {
        var failures = result.Failures.Count;
        var title = failures == 0
            ? "BrainHarbor: run finished"
            : $"BrainHarbor: {failures} source(s) failed";

        var message = result.TotalUploaded > 0
            ? $"{result.TotalUploaded} item(s) awaiting review."
            : "Nothing new to review.";

        if (failures > 0)
        {
            message += " " + string.Join(", ", result.Failures.Select(f => f.Source)) + " need attention.";
        }

        logger.LogInformation("{Title} — {Message}", title, message);

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        TryToast(title, message);
    }

    private void TryToast(string title, string message)
    {
        try
        {
            // Single-quoted PowerShell strings: escape by doubling.
            var safeTitle = title.Replace("'", "''");
            var safeMessage = message.Replace("'", "''");

            var script =
                "[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, " +
                "ContentType=WindowsRuntime] > $null; " +
                "$t=[Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent(" +
                "[Windows.UI.Notifications.ToastTemplateType]::ToastText02); " +
                "$n=$t.GetElementsByTagName('text'); " +
                $"$n.Item(0).AppendChild($t.CreateTextNode('{safeTitle}')) > $null; " +
                $"$n.Item(1).AppendChild($t.CreateTextNode('{safeMessage}')) > $null; " +
                "[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier(" +
                "'BrainHarbor').Show([Windows.UI.Notifications.ToastNotification]::new($t))";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                ArgumentList = { "-NoProfile", "-NonInteractive", "-Command", script },
                CreateNoWindow = true,
                UseShellExecute = false,
            });

            process?.WaitForExit(10_000);
        }
        catch (Exception exception)
        {
            // A missing toast is not worth failing a successful run over.
            logger.LogDebug(exception, "Desktop notification failed; the log line above stands.");
        }
    }
}
