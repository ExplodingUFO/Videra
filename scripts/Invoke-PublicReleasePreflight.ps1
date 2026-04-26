#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [string]$ExpectedCommit = "",

    [string]$EvidenceRoot = "artifacts",

    [string]$ReleaseDryRunRoot = "",

    [string]$BenchmarkRoot = "",

    [string]$ConsumerSmokeRoot = "",

    [string]$NativeValidationRoot = "",

    [string]$OutputRoot = "artifacts/public-release-preflight",

    [int]$MaxEvidenceAgeHours = 72,

    [switch]$SkipRepositoryStateCheck
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$checks = [System.Collections.Generic.List[object]]::new()

function Resolve-RepositoryPath([string]$Path)
{
    if ([System.IO.Path]::IsPathRooted($Path))
    {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $root $Path))
}

function Add-Check([string]$Id, [string]$Status, [string]$Message, [string]$Path = "")
{
    $checks.Add([ordered]@{
        id = $Id
        status = $Status
        message = $Message
        path = $Path
    }) | Out-Null
}

function Read-JsonFile([string]$Path, [string]$Description)
{
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf))
    {
        Add-Check -Id $Description -Status "fail" -Message "$Description not found." -Path $Path
        return $null
    }

    try
    {
        return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch
    {
        Add-Check -Id $Description -Status "fail" -Message "$Description is not valid JSON: $($_.Exception.Message)" -Path $Path
        return $null
    }
}

function Assert-File([string]$Id, [string]$Path)
{
    if (Test-Path -LiteralPath $Path -PathType Leaf)
    {
        Add-Check -Id $Id -Status "pass" -Message "Found required file." -Path $Path
        return $true
    }

    Add-Check -Id $Id -Status "fail" -Message "Required file is missing." -Path $Path
    return $false
}

function Assert-DirectoryHasFiles([string]$Id, [string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path -PathType Container))
    {
        Add-Check -Id $Id -Status "fail" -Message "Required evidence directory is missing." -Path $Path
        return $false
    }

    $files = @(Get-ChildItem -LiteralPath $Path -Recurse -File -ErrorAction SilentlyContinue)
    if ($files.Count -eq 0)
    {
        Add-Check -Id $Id -Status "fail" -Message "Required evidence directory contains no files." -Path $Path
        return $false
    }

    Add-Check -Id $Id -Status "pass" -Message "Found required evidence directory." -Path $Path
    return $true
}

function Assert-RecursiveFile([string]$Id, [string]$RootPath, [string]$FileName)
{
    if (-not (Test-Path -LiteralPath $RootPath -PathType Container))
    {
        Add-Check -Id $Id -Status "fail" -Message "Evidence root is missing." -Path $RootPath
        return $false
    }

    $match = Get-ChildItem -LiteralPath $RootPath -Recurse -File -Filter $FileName -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $match)
    {
        Add-Check -Id $Id -Status "fail" -Message "Required evidence file '$FileName' is missing." -Path $RootPath
        return $false
    }

    Add-Check -Id $Id -Status "pass" -Message "Found required evidence file '$FileName'." -Path $match.FullName
    return $true
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

if ($MaxEvidenceAgeHours -le 0)
{
    throw "MaxEvidenceAgeHours must be positive."
}

$evidenceRootFull = Resolve-RepositoryPath $EvidenceRoot
if ([string]::IsNullOrWhiteSpace($ReleaseDryRunRoot)) { $ReleaseDryRunRoot = Join-Path $evidenceRootFull "release-dry-run" }
if ([string]::IsNullOrWhiteSpace($BenchmarkRoot)) { $BenchmarkRoot = Join-Path $evidenceRootFull "benchmarks" }
if ([string]::IsNullOrWhiteSpace($ConsumerSmokeRoot)) { $ConsumerSmokeRoot = Join-Path $evidenceRootFull "consumer-smoke" }
if ([string]::IsNullOrWhiteSpace($NativeValidationRoot)) { $NativeValidationRoot = Join-Path $evidenceRootFull "native-validation" }

$releaseDryRunRootFull = Resolve-RepositoryPath $ReleaseDryRunRoot
$benchmarkRootFull = Resolve-RepositoryPath $BenchmarkRoot
$consumerSmokeRootFull = Resolve-RepositoryPath $ConsumerSmokeRoot
$nativeValidationRootFull = Resolve-RepositoryPath $NativeValidationRoot
$outputRootFull = Resolve-RepositoryPath $OutputRoot
New-Item -ItemType Directory -Force -Path $outputRootFull | Out-Null

if (-not $SkipRepositoryStateCheck)
{
    $statusLines = @(& git status --porcelain --untracked-files=no)
    if ($LASTEXITCODE -ne 0)
    {
        Add-Check -Id "repository-status" -Status "fail" -Message "Could not read repository status."
    }
    elseif ($statusLines.Count -eq 0)
    {
        Add-Check -Id "repository-status" -Status "pass" -Message "Repository tracked working tree is clean."
    }
    else
    {
        Add-Check -Id "repository-status" -Status "fail" -Message "Repository has tracked working tree changes."
    }
}

$currentCommit = ""
try
{
    $currentCommit = (& git rev-parse HEAD).Trim()
    if (-not [string]::IsNullOrWhiteSpace($ExpectedCommit) -and -not $currentCommit.StartsWith($ExpectedCommit, [System.StringComparison]::OrdinalIgnoreCase))
    {
        Add-Check -Id "source-commit" -Status "fail" -Message "Current commit '$currentCommit' does not match expected commit '$ExpectedCommit'."
    }
    else
    {
        Add-Check -Id "source-commit" -Status "pass" -Message "Source commit is acceptable."
    }
}
catch
{
    Add-Check -Id "source-commit" -Status "fail" -Message "Could not resolve current source commit: $($_.Exception.Message)"
}

$summaryPath = Join-Path $releaseDryRunRootFull "release-dry-run-summary.json"
$evidenceIndexJsonPath = Join-Path $releaseDryRunRootFull "release-candidate-evidence-index.json"
$evidenceIndexTextPath = Join-Path $releaseDryRunRootFull "release-candidate-evidence-index.txt"
$packageSizeEvaluationPath = Join-Path (Join-Path $releaseDryRunRootFull "packages") ".validation/package-size-evaluation.json"
$packageSizeSummaryPath = Join-Path (Join-Path $releaseDryRunRootFull "packages") ".validation/package-size-summary.txt"

Assert-File -Id "release-dry-run-summary.json" -Path $summaryPath | Out-Null
Assert-File -Id "release-candidate-evidence-index.json" -Path $evidenceIndexJsonPath | Out-Null
Assert-File -Id "release-candidate-evidence-index.txt" -Path $evidenceIndexTextPath | Out-Null
Assert-File -Id "package-size-evaluation.json" -Path $packageSizeEvaluationPath | Out-Null
Assert-File -Id "package-size-summary.txt" -Path $packageSizeSummaryPath | Out-Null

$summary = Read-JsonFile -Path $summaryPath -Description "release-dry-run-summary.json"
if ($null -ne $summary)
{
    if ([string]$summary.status -ne "pass")
    {
        Add-Check -Id "release-dry-run-status" -Status "fail" -Message "Release dry-run summary status must be pass."
    }
    elseif ([string]$summary.expectedVersion -ne $ExpectedVersion)
    {
        Add-Check -Id "release-dry-run-version" -Status "fail" -Message "Release dry-run summary version '$($summary.expectedVersion)' does not match '$ExpectedVersion'."
    }
    else
    {
        Add-Check -Id "release-dry-run-summary" -Status "pass" -Message "Release dry-run summary matches expected version."
    }

    $generatedAtUtc = [DateTimeOffset]::MinValue
    if ([DateTimeOffset]::TryParse([string]$summary.generatedAtUtc, [ref]$generatedAtUtc))
    {
        $age = [DateTimeOffset]::UtcNow - $generatedAtUtc
        if ($age.TotalHours -le $MaxEvidenceAgeHours)
        {
            Add-Check -Id "release-dry-run-freshness" -Status "pass" -Message "Release dry-run evidence is within the freshness window."
        }
        else
        {
            Add-Check -Id "release-dry-run-freshness" -Status "fail" -Message "Release dry-run evidence is stale."
        }
    }
    else
    {
        Add-Check -Id "release-dry-run-freshness" -Status "fail" -Message "Release dry-run summary is missing generatedAtUtc."
    }
}

$evidenceIndex = Read-JsonFile -Path $evidenceIndexJsonPath -Description "release-candidate-evidence-index.json"
if ($null -ne $evidenceIndex)
{
    if ([string]$evidenceIndex.expectedVersion -eq $ExpectedVersion -and [string]$evidenceIndex.dryRunStatus -eq "pass")
    {
        Add-Check -Id "release-candidate-evidence-index" -Status "pass" -Message "Evidence index matches expected version and dry-run status."
    }
    else
    {
        Add-Check -Id "release-candidate-evidence-index" -Status "fail" -Message "Evidence index version or dry-run status does not match."
    }
}

Assert-File -Id "benchmark-viewer-thresholds" -Path (Join-Path $benchmarkRootFull "viewer/benchmark-threshold-evaluation.json") | Out-Null
Assert-File -Id "benchmark-surfacecharts-thresholds" -Path (Join-Path $benchmarkRootFull "surfacecharts/benchmark-threshold-evaluation.json") | Out-Null
Assert-DirectoryHasFiles -Id "native-validation" -Path $nativeValidationRootFull | Out-Null
Assert-RecursiveFile -Id "consumer-smoke-result.json" -RootPath $consumerSmokeRootFull -FileName "consumer-smoke-result.json" | Out-Null
Assert-RecursiveFile -Id "surfacecharts-support-summary.txt" -RootPath $consumerSmokeRootFull -FileName "surfacecharts-support-summary.txt" | Out-Null

foreach ($requiredFile in @(
    "eng/public-api-contract.json",
    "CHANGELOG.md",
    "docs/releasing.md",
    "docs/release-policy.md",
    "docs/release-candidate-cutover.md",
    ".github/workflows/publish-public.yml",
    ".github/workflows/publish-existing-public-release.yml",
    ".github/workflows/release-dry-run.yml"))
{
    Assert-File -Id $requiredFile -Path (Join-Path $root $requiredFile) | Out-Null
}

$failedChecks = @($checks | Where-Object { $_.status -ne "pass" })
$status = if ($failedChecks.Count -eq 0) { "pass" } else { "fail" }
$summaryDocument = [ordered]@{
    schemaVersion = 1
    status = $status
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    expectedVersion = $ExpectedVersion
    expectedCommit = $ExpectedCommit
    sourceCommit = $currentCommit
    evidenceRoot = $evidenceRootFull
    releaseDryRunRoot = $releaseDryRunRootFull
    benchmarkRoot = $benchmarkRootFull
    consumerSmokeRoot = $consumerSmokeRootFull
    nativeValidationRoot = $nativeValidationRootFull
    artifactPaths = [ordered]@{
        summaryJson = Join-Path $outputRootFull "public-release-preflight-summary.json"
        summaryText = Join-Path $outputRootFull "public-release-preflight-summary.txt"
    }
    checks = @($checks)
}

$summaryJsonPath = $summaryDocument.artifactPaths.summaryJson
$summaryTextPath = $summaryDocument.artifactPaths.summaryText
$summaryDocument | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryJsonPath

$lines = @(
    "Public release preflight"
    "Status: $status"
    "Version: $ExpectedVersion"
    "Source commit: $currentCommit"
    ""
    "Checks:"
)
foreach ($check in $checks)
{
    $lines += "- [$($check.status.ToUpperInvariant())] $($check.id): $($check.message)"
}
$lines | Set-Content -LiteralPath $summaryTextPath

if ($failedChecks.Count -gt 0)
{
    $failedIds = ($failedChecks | ForEach-Object { $_.id }) -join ", "
    throw "Public release preflight failed: $failedIds"
}

Write-Host "Public release preflight passed for version '$ExpectedVersion'." -ForegroundColor Green
Write-Host "Summary written to '$summaryJsonPath'." -ForegroundColor Green
