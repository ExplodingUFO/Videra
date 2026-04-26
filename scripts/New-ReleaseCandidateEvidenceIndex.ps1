#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseDryRunSummaryPath,

    [string]$OutputRoot = "artifacts/release-dry-run",

    [string]$EvidenceContractPath = "eng/release-candidate-evidence.json"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

function Resolve-RepositoryPath([string]$Path)
{
    if ([System.IO.Path]::IsPathRooted($Path))
    {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $root $Path))
}

function Read-JsonFile([string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path))
    {
        throw "Required JSON file not found at '$Path'."
    }

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
}

function Assert-StringProperty($Object, [string]$PropertyName, [string]$Description)
{
    $value = [string]$Object.$PropertyName
    if ([string]::IsNullOrWhiteSpace($value))
    {
        throw "$Description is required in release dry-run summary."
    }

    return $value
}

function Assert-ExistingFile([string]$Path, [string]$Description)
{
    $fullPath = Resolve-RepositoryPath $Path
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf))
    {
        throw "$Description not found at '$fullPath'."
    }

    return $fullPath
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

$contractPath = Resolve-RepositoryPath $EvidenceContractPath
$contract = Read-JsonFile -Path $contractPath
if ($contract.schemaVersion -ne 1)
{
    throw "Unsupported release-candidate evidence contract schema version '$($contract.schemaVersion)'."
}

$summaryPath = Resolve-RepositoryPath $ReleaseDryRunSummaryPath
$summary = Read-JsonFile -Path $summaryPath
if ($summary.schemaVersion -ne 1)
{
    throw "Unsupported release dry-run summary schema version '$($summary.schemaVersion)'."
}

if ([string]$summary.expectedVersion -ne $ExpectedVersion)
{
    throw "Release dry-run summary expectedVersion '$($summary.expectedVersion)' does not match '$ExpectedVersion'."
}

if ([string]$summary.status -ne "pass")
{
    throw "Release dry-run summary status must be 'pass'. Found '$($summary.status)'."
}

$packageContractPath = Assert-StringProperty -Object $summary -PropertyName "packageContractPath" -Description "packageContractPath"
$packageValidationScript = Assert-StringProperty -Object $summary -PropertyName "packageValidationScript" -Description "packageValidationScript"
Assert-ExistingFile -Path $packageContractPath -Description "Package contract" | Out-Null
Assert-ExistingFile -Path $packageValidationScript -Description "Package validation script" | Out-Null

if ($null -eq $summary.validationArtifacts)
{
    throw "validationArtifacts is required in release dry-run summary."
}

$packageSizeEvaluationPath = Assert-StringProperty -Object $summary.validationArtifacts -PropertyName "packageSizeEvaluation" -Description "validationArtifacts.packageSizeEvaluation"
$packageSizeSummaryPath = Assert-StringProperty -Object $summary.validationArtifacts -PropertyName "packageSizeSummary" -Description "validationArtifacts.packageSizeSummary"
Assert-ExistingFile -Path $packageSizeEvaluationPath -Description "Package size evaluation artifact" | Out-Null
Assert-ExistingFile -Path $packageSizeSummaryPath -Description "Package size summary artifact" | Out-Null

$requiredStepIds = @(
    "version-simulation-prepack",
    "package-build",
    "package-validation",
    "version-simulation-summary",
    "evidence-index"
)
$summarySteps = @($summary.steps)
foreach ($requiredStepId in $requiredStepIds)
{
    $step = $summarySteps | Where-Object { [string]$_.id -eq $requiredStepId } | Select-Object -First 1
    if ($null -eq $step)
    {
        throw "Release dry-run summary is missing required step '$requiredStepId'."
    }

    if ([string]$step.status -ne "pass")
    {
        throw "Release dry-run summary step '$requiredStepId' must be 'pass'. Found '$($step.status)'."
    }
}

$outputRootFull = Resolve-RepositoryPath $OutputRoot
New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null

$index = [ordered]@{
    schemaVersion = 1
    expectedVersion = $ExpectedVersion
    dryRunStatus = [string]$summary.status
    evidenceContractPath = $EvidenceContractPath
    releaseDryRunSummaryPath = $ReleaseDryRunSummaryPath
    packageContractPath = $packageContractPath
    packageValidationScript = $packageValidationScript
    validationArtifacts = $summary.validationArtifacts
    dryRunArtifacts = $summary.artifactPaths
    validationSteps = @($summary.steps)
    requiredChecks = @($contract.requiredChecks)
    requiredArtifacts = @($contract.requiredArtifacts)
    supportDocs = @($contract.supportDocs)
    reviewerChecklist = @(
        "Confirm the Release Dry Run workflow completed for the expected version.",
        "Confirm package validation artifacts are present in release-dry-run-evidence.",
        "Confirm Benchmark Gates completed for viewer and SurfaceCharts suites.",
        "Confirm native validation completed for Windows, Linux X11, Linux XWayland, and macOS.",
        "Confirm consumer smoke completed for viewer and SurfaceCharts package paths.",
        "Confirm support docs and changelog describe the same release-candidate boundary."
    )
}

$indexPath = Join-Path $outputRootFull "release-candidate-evidence-index.json"
$textPath = Join-Path $outputRootFull "release-candidate-evidence-index.txt"

$index | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $indexPath

$lines = @(
    "Release candidate evidence index"
    "Version: $ExpectedVersion"
    "Contract: $EvidenceContractPath"
    "Dry-run summary: $ReleaseDryRunSummaryPath"
    ""
    "Required checks:"
)

foreach ($check in @($contract.requiredChecks))
{
    $lines += "- $($check.id): $($check.workflow)"
}

$lines += ""
$lines += "Required artifacts:"
foreach ($artifact in @($contract.requiredArtifacts))
{
    $lines += "- $artifact"
}

$lines += ""
$lines += "Support docs:"
foreach ($doc in @($contract.supportDocs))
{
    $lines += "- $doc"
}

$lines | Set-Content -LiteralPath $textPath

Write-Host "Release candidate evidence index written to '$indexPath'." -ForegroundColor Green
