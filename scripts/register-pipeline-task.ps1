<#
.SYNOPSIS
    Registers the BrainHarbor pipeline as a daily Windows scheduled task
    (WI-210, architecture.md §6/§8).

.DESCRIPTION
    The pipeline is stateless and runs on Dan's PC — Task Scheduler IS the
    loop, so the app never daemonizes. Key settings:

      * StartWhenAvailable  — if the PC was asleep at the scheduled time, the
                              run happens when it wakes rather than being
                              skipped. The fetchers' cursors mean a late run
                              simply asks for a wider window and catches up.
      * No RunOnlyIfIdle    — this must run whether or not the PC is in use.
      * ExecutionTimeLimit  — a hung fetch is killed rather than blocking the
                              next day's run.

    Exit codes surface in the task history: 0 all sources ok, 1 some sources
    failed, 2 cancelled, 3 bad config, 4 the run itself blew up.

    Task Scheduler captures no console output, so the pipeline writes its own
    per-run log file (WI-417) — nothing to wire up here, it is on by default.
    See the path printed at the end of this script.

.EXAMPLE
    ./scripts/register-pipeline-task.ps1 -At 06:30

.EXAMPLE
    ./scripts/register-pipeline-task.ps1 -Unregister
#>
[CmdletBinding()]
param(
    [string]$TaskName = 'BrainHarbor Pipeline',
    [string]$At = '06:00',
    [switch]$Unregister
)

$ErrorActionPreference = 'Stop'

if ($Unregister) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Host "Removed scheduled task '$TaskName'."
    return
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot 'src/BrainHarbor.Pipeline'
$publishDir = Join-Path $repoRoot 'artifacts/pipeline'

Write-Host "Publishing the pipeline to $publishDir ..."
dotnet publish $projectPath -c Release -o $publishDir | Out-Null

$exePath = Join-Path $publishDir 'BrainHarbor.Pipeline.exe'
if (-not (Test-Path $exePath)) {
    throw "Publish did not produce $exePath"
}

# Working directory matters: user-secrets are resolved per user, and the
# pipeline reads its config from there.
$action = New-ScheduledTaskAction -Execute $exePath -Argument '--once' -WorkingDirectory $publishDir
$trigger = New-ScheduledTaskTrigger -Daily -At $At

$settings = New-ScheduledTaskSettingsSet `
    -StartWhenAvailable `
    -DontStopIfGoingOnBatteries `
    -AllowStartIfOnBatteries `
    -ExecutionTimeLimit (New-TimeSpan -Hours 2) `
    -MultipleInstances IgnoreNew

# Runs as the signed-in user so dotnet user-secrets resolve. The account never
# needs elevation — the pipeline only makes outbound HTTPS calls.
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Principal $principal `
    -Description 'Fetches brain tumor research daily and uploads it for review at brainharbor.org.' `
    -Force | Out-Null

Write-Host "Registered '$TaskName' to run daily at $At."
Write-Host ""
Write-Host "Before the first run, confirm the pipeline's secrets are set:"
Write-Host "  dotnet user-secrets set `"Pipeline:SyncApiBaseUrl`" `"https://brainharbor.org`" --project src/BrainHarbor.Pipeline"
Write-Host "  dotnet user-secrets set `"Pipeline:SyncApiKey`" `"...`" --project src/BrainHarbor.Pipeline"
Write-Host "  dotnet user-secrets set `"Pipeline:NcbiApiKey`" `"...`" --project src/BrainHarbor.Pipeline"
Write-Host ""
Write-Host "Run it once now:  Start-ScheduledTask -TaskName '$TaskName'"
Write-Host "Check history:    Get-ScheduledTaskInfo -TaskName '$TaskName'"
Write-Host ""

# Task Scheduler records only the exit code, so the run log is the only place
# the detail survives: what was excluded, what was flagged and why. The path is
# this user's LOCALAPPDATA, which is right because the principal above is this
# same user; change the principal and the logs move with it.
$logDir = Join-Path $env:LOCALAPPDATA 'BrainHarbor\logs'
Write-Host "Run logs:         $logDir"
Write-Host "                  one file per run, kept 30 days, then pruned."
Write-Host "Read the newest:  Get-ChildItem '$logDir' | Sort-Object LastWriteTime | Select-Object -Last 1 | Get-Content -Tail 40"
