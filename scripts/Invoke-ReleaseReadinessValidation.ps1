#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OutputRoot = "artifacts/release-readiness-validation",

    [switch]$ConsumerSmokeBuildOnly
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$results = [System.Collections.Generic.List[object]]::new()

function Resolve-RepositoryPath([string]$Path)
{
    if ([System.IO.Path]::IsPathRooted($Path))
    {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $root $Path))
}

function Add-Result([string]$Id, [string]$Status, [string]$Category, [string]$Message, [string]$Command = "")
{
    $results.Add([ordered]@{
        id = $Id
        status = $Status
        category = $Category
        message = $Message
        command = $Command
    }) | Out-Null
}

function Format-Command([string]$FileName, [string[]]$Arguments)
{
    return (($FileName) + " " + (($Arguments | ForEach-Object {
        if ($_ -match "\s") { "'$_'" } else { $_ }
    }) -join " ")).Trim()
}

function Invoke-ValidationStep([string]$Id, [string]$Description, [string]$FileName, [string[]]$Arguments)
{
    $commandText = Format-Command -FileName $FileName -Arguments $Arguments
    Write-Host ""
    Write-Host "=== PASS/FAIL: $Description ===" -ForegroundColor Cyan
    Write-Host $commandText

    & $FileName @Arguments
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0)
    {
        Add-Result -Id $Id -Status "fail" -Category "pass-fail" -Message "$Description failed with exit code $exitCode." -Command $commandText
        throw "$Description failed with exit code $exitCode."
    }

    Add-Result -Id $Id -Status "pass" -Category "pass-fail" -Message "$Description passed." -Command $commandText
}

function Add-EnvironmentWarnings
{
    Write-Host ""
    Write-Host "=== LOCAL ENVIRONMENT WARNINGS ===" -ForegroundColor Yellow

    $warningCount = 0
    if ($IsLinux -and [string]::IsNullOrWhiteSpace($env:DISPLAY) -and [string]::IsNullOrWhiteSpace($env:WAYLAND_DISPLAY))
    {
        Add-Result -Id "linux-display" -Status "warning" -Category "local-environment" -Message "DISPLAY and WAYLAND_DISPLAY are unset; full SurfaceCharts smoke may require a display host."
        Write-Host "[WARNING] DISPLAY and WAYLAND_DISPLAY are unset; full SurfaceCharts smoke may require a display host." -ForegroundColor Yellow
        $warningCount++
    }

    if ($IsWindows -and [string]::IsNullOrWhiteSpace($env:USERPROFILE))
    {
        Add-Result -Id "windows-userprofile" -Status "warning" -Category "local-environment" -Message "USERPROFILE is unset; NuGet and dotnet caches may not resolve normally."
        Write-Host "[WARNING] USERPROFILE is unset; NuGet and dotnet caches may not resolve normally." -ForegroundColor Yellow
        $warningCount++
    }

    if ($warningCount -eq 0)
    {
        Add-Result -Id "local-environment" -Status "pass" -Category "local-environment" -Message "No local environment warnings detected by the validation script."
        Write-Host "[PASS] No local environment warnings detected by the validation script." -ForegroundColor Green
    }
}

function Add-SkippedPublishSteps
{
    Write-Host ""
    Write-Host "=== SKIPPED PUBLIC PUBLISH/TAG STEPS ===" -ForegroundColor Yellow

    $skipped = @(
        [ordered]@{
            id = "public-nuget-publish"
            message = "Skipped by design: release-readiness validation does not run dotnet nuget push."
        },
        [ordered]@{
            id = "release-tag"
            message = "Skipped by design: release-readiness validation does not create or push git tags."
        },
        [ordered]@{
            id = "github-release"
            message = "Skipped by design: release-readiness validation does not create GitHub releases."
        }
    )

    foreach ($item in $skipped)
    {
        Add-Result -Id $item.id -Status "skipped" -Category "publish-tag-skip" -Message $item.message
        Write-Host "[SKIPPED] $($item.id): $($item.message)" -ForegroundColor Yellow
    }
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

$outputRootFull = Resolve-RepositoryPath $OutputRoot
$repositoryRootFull = [System.IO.Path]::GetFullPath($root).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
$repositoryRootWithSeparator = $repositoryRootFull + [System.IO.Path]::DirectorySeparatorChar
if (-not $outputRootFull.StartsWith($repositoryRootWithSeparator, [System.StringComparison]::OrdinalIgnoreCase))
{
    throw "OutputRoot must stay inside the repository because composed validation scripts expect repository-relative artifact paths: '$outputRootFull'."
}

$outputRootForScripts = [System.IO.Path]::GetRelativePath($repositoryRootFull, $outputRootFull)
New-Item -ItemType Directory -Force -Path $outputRootFull | Out-Null

$summaryJsonPath = Join-Path $outputRootFull "release-readiness-validation-summary.json"
$summaryTextPath = Join-Path $outputRootFull "release-readiness-validation-summary.txt"
$releaseDryRunRoot = Join-Path $outputRootForScripts "release-dry-run"
$consumerSmokeRoot = Join-Path $outputRootForScripts "surfacecharts-consumer-smoke"

try
{
    Add-EnvironmentWarnings

    Invoke-ValidationStep `
        -Id "package-build" `
        -Description "Package build and package validation" `
        -FileName "pwsh" `
        -Arguments @(
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", (Join-Path $root "scripts/Invoke-ReleaseDryRun.ps1"),
            "-ExpectedVersion", $ExpectedVersion,
            "-Configuration", $Configuration,
            "-OutputRoot", $releaseDryRunRoot)

    $consumerSmokeArguments = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", (Join-Path $root "scripts/Invoke-ConsumerSmoke.ps1"),
        "-Configuration", $Configuration,
        "-Scenario", "SurfaceCharts",
        "-OutputRoot", $consumerSmokeRoot)
    if ($ConsumerSmokeBuildOnly)
    {
        $consumerSmokeArguments += "-BuildOnly"
    }

    Invoke-ValidationStep `
        -Id "surfacecharts-consumer-smoke" `
        -Description "SurfaceCharts packaged consumer smoke" `
        -FileName "pwsh" `
        -Arguments $consumerSmokeArguments

    Invoke-ValidationStep `
        -Id "surfacecharts-focused-tests" `
        -Description "Focused SurfaceCharts and script-facing repository tests" `
        -FileName "dotnet" `
        -Arguments @(
            "test",
            (Join-Path $root "tests/Videra.Core.Tests/Videra.Core.Tests.csproj"),
            "-c", $Configuration,
            "--filter", "FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~ReleaseDryRunRepositoryTests")

    Invoke-ValidationStep `
        -Id "snapshot-scope-guardrails" `
        -Description "Snapshot export scope guardrails" `
        -FileName "pwsh" `
        -Arguments @(
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", (Join-Path $root "scripts/Test-SnapshotExportScope.ps1"))

    Add-SkippedPublishSteps
}
finally
{
    $failed = @($results | Where-Object { $_.status -eq "fail" })
    $summaryStatus = if ($failed.Count -eq 0) { "pass" } else { "fail" }
    $summary = [ordered]@{
        schemaVersion = 1
        status = $summaryStatus
        generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
        expectedVersion = $ExpectedVersion
        configuration = $Configuration
        outputRoot = $outputRootFull
        consumerSmokeBuildOnly = [bool]$ConsumerSmokeBuildOnly
        results = @($results)
        artifactPaths = [ordered]@{
            summaryJson = $summaryJsonPath
            summaryText = $summaryTextPath
            releaseDryRunRoot = $releaseDryRunRoot
            surfaceChartsConsumerSmokeRoot = $consumerSmokeRoot
        }
    }

    $summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryJsonPath

    $lines = @(
        "Release readiness validation"
        "Status: $summaryStatus"
        "Version: $ExpectedVersion"
        "Configuration: $Configuration"
        "Consumer smoke build-only: $([bool]$ConsumerSmokeBuildOnly)"
        "",
        "Pass/fail checks:"
    )
    $lines += @($results | Where-Object { $_.category -eq "pass-fail" } | ForEach-Object { "- [$($_.status.ToUpperInvariant())] $($_.id): $($_.message)" })
    $lines += ""
    $lines += "Local environment warnings:"
    $lines += @($results | Where-Object { $_.category -eq "local-environment" } | ForEach-Object { "- [$($_.status.ToUpperInvariant())] $($_.id): $($_.message)" })
    $lines += ""
    $lines += "Skipped public publish/tag steps:"
    $lines += @($results | Where-Object { $_.category -eq "publish-tag-skip" } | ForEach-Object { "- [$($_.status.ToUpperInvariant())] $($_.id): $($_.message)" })
    $lines | Set-Content -LiteralPath $summaryTextPath
}

Write-Host ""
Write-Host "=== RELEASE READINESS SUMMARY ===" -ForegroundColor Cyan
Get-Content -LiteralPath $summaryTextPath

if ((Get-Content -LiteralPath $summaryJsonPath -Raw | ConvertFrom-Json).status -ne "pass")
{
    exit 1
}
