#!/usr/bin/env pwsh
param(
    [string]$OutputRoot = "artifacts/doctor"
)

$ErrorActionPreference = "Stop"

# Videra Doctor is repo-only and non-mutating. It reports repository support
# state and writes local artifacts; it is not a public package or installed tool.
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$defaultOutputRoot = "artifacts/doctor"

if ([System.IO.Path]::IsPathFullyQualified($OutputRoot))
{
    $resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
}
else
{
    $resolvedOutputRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $OutputRoot))
}

New-Item -ItemType Directory -Force -Path $resolvedOutputRoot | Out-Null

$checks = [System.Collections.Generic.List[object]]::new()

function Add-Check([string]$Id, [string]$Status, [string]$Message, [string]$Path = "")
{
    $checks.Add([ordered]@{
        id = $Id
        status = $Status
        message = $Message
        path = $Path
    }) | Out-Null
}

function Test-RelativeFile([string]$Id, [string]$RelativePath)
{
    $path = Join-Path $repositoryRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Leaf)
    {
        Add-Check -Id $Id -Status "pass" -Message "Found $RelativePath." -Path $RelativePath
        return $true
    }

    Add-Check -Id $Id -Status "unavailable" -Message "Missing $RelativePath." -Path $RelativePath
    return $false
}

function Test-RelativeDirectory([string]$Id, [string]$RelativePath)
{
    $path = Join-Path $repositoryRoot $RelativePath
    if (Test-Path -LiteralPath $path -PathType Container)
    {
        Add-Check -Id $Id -Status "pass" -Message "Found $RelativePath." -Path $RelativePath
        return $true
    }

    Add-Check -Id $Id -Status "unavailable" -Message "Missing $RelativePath." -Path $RelativePath
    return $false
}

$dotnetVersion = $null
$dotnetInfo = $null
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand)
{
    Add-Check -Id "dotnet-sdk" -Status "unavailable" -Message "dotnet was not found on PATH."
}
else
{
    $dotnetVersion = (& dotnet --version).Trim()
    $dotnetInfo = (& dotnet --info) -join [Environment]::NewLine
    Add-Check -Id "dotnet-sdk" -Status "pass" -Message "dotnet SDK $dotnetVersion is available."
}

$gitStatusLines = @()
$gitBranch = $null
$gitCommand = Get-Command git -ErrorAction SilentlyContinue
if ($null -eq $gitCommand)
{
    Add-Check -Id "git-repository" -Status "unavailable" -Message "git was not found on PATH."
}
else
{
    $gitStatusLines = @(& git -C $repositoryRoot status --short --branch 2>$null)
    if ($LASTEXITCODE -eq 0)
    {
        $gitBranch = if ($gitStatusLines.Count -gt 0) { $gitStatusLines[0] } else { "" }
        $status = if ($gitStatusLines.Count -eq 1) { "pass" } else { "warn" }
        $message = if ($status -eq "pass") { "Repository worktree is clean." } else { "Repository worktree has local changes." }
        Add-Check -Id "git-repository" -Status $status -Message $message
    }
    else
    {
        Add-Check -Id "git-repository" -Status "unavailable" -Message "git status could not read repository state."
    }
}

$contractFiles = @(
    @{ Id = "public-api-contract"; Path = "eng/public-api-contract.json" },
    @{ Id = "package-size-budgets"; Path = "eng/package-size-budgets.json" },
    @{ Id = "benchmark-contract"; Path = "benchmarks/benchmark-contract.json" },
    @{ Id = "benchmark-thresholds"; Path = "benchmarks/benchmark-thresholds.json" }
)

foreach ($contractFile in $contractFiles)
{
    Test-RelativeFile -Id $contractFile.Id -RelativePath $contractFile.Path | Out-Null
}

$validationScripts = @(
    "Validate-Packages.ps1",
    "Run-Benchmarks.ps1",
    "Test-BenchmarkThresholds.ps1",
    "Invoke-ConsumerSmoke.ps1",
    "run-native-validation.ps1",
    "Invoke-ReleaseDryRun.ps1"
)

foreach ($script in $validationScripts)
{
    Test-RelativeFile -Id "script:$script" -RelativePath "scripts/$script" | Out-Null
}

$platformProjects = @(
    "src/Videra.Platform.Windows/Videra.Platform.Windows.csproj",
    "src/Videra.Platform.Linux/Videra.Platform.Linux.csproj",
    "src/Videra.Platform.macOS/Videra.Platform.macOS.csproj"
)

foreach ($platformProject in $platformProjects)
{
    $platformName = [System.IO.Path]::GetFileNameWithoutExtension($platformProject)
    Test-RelativeFile -Id "platform:$platformName" -RelativePath $platformProject | Out-Null
}

$supportArtifactPaths = @(
    $defaultOutputRoot,
    "artifacts/benchmarks",
    "artifacts/release-dry-run",
    "artifacts/consumer-smoke",
    "artifacts/native-validation"
)

foreach ($supportArtifactPath in $supportArtifactPaths)
{
    Test-RelativeDirectory -Id "artifact:$supportArtifactPath" -RelativePath $supportArtifactPath | Out-Null
}

$summaryPath = Join-Path $resolvedOutputRoot "doctor-summary.txt"
$reportPath = Join-Path $resolvedOutputRoot "doctor-report.json"

$report = [ordered]@{
    schemaVersion = 1
    name = "Videra Doctor"
    repoOnly = $true
    mutatesConfiguration = $false
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    repositoryRoot = $repositoryRoot
    outputRoot = $resolvedOutputRoot
    environment = [ordered]@{
        osDescription = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        processArchitecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
        frameworkDescription = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        powerShellVersion = $PSVersionTable.PSVersion.ToString()
        dotnetVersion = $dotnetVersion
        dotnetInfo = $dotnetInfo
    }
    git = [ordered]@{
        branch = $gitBranch
        statusLines = $gitStatusLines
    }
    contractFiles = $contractFiles
    validationScripts = $validationScripts
    platformProjects = $platformProjects
    supportArtifactPaths = $supportArtifactPaths
    checks = @($checks)
}

$summaryLines = [System.Collections.Generic.List[string]]::new()
$summaryLines.Add("Videra Doctor") | Out-Null
$summaryLines.Add("Mode: repo-only, non-mutating") | Out-Null
$summaryLines.Add("Repository: $repositoryRoot") | Out-Null
$summaryLines.Add("Output: $resolvedOutputRoot") | Out-Null
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Checks:") | Out-Null
foreach ($check in $checks)
{
    $line = "- {0}: {1} - {2}" -f $check.id, $check.status, $check.message
    $summaryLines.Add($line) | Out-Null
}
$summaryLines.Add("") | Out-Null
$summaryLines.Add("Reports:") | Out-Null
$summaryLines.Add("- $summaryPath") | Out-Null
$summaryLines.Add("- $reportPath") | Out-Null

Set-Content -LiteralPath $summaryPath -Value $summaryLines -Encoding utf8
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding utf8

Write-Host "Videra Doctor wrote $summaryPath"
Write-Host "Videra Doctor wrote $reportPath"
