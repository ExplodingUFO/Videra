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
if ([string]$summary.expectedVersion -ne $ExpectedVersion)
{
    throw "Release dry-run summary expectedVersion '$($summary.expectedVersion)' does not match '$ExpectedVersion'."
}

$outputRootFull = Resolve-RepositoryPath $OutputRoot
New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null

$index = [ordered]@{
    schemaVersion = 1
    expectedVersion = $ExpectedVersion
    evidenceContractPath = $EvidenceContractPath
    releaseDryRunSummaryPath = $ReleaseDryRunSummaryPath
    packageContractPath = [string]$summary.packageContractPath
    packageValidationScript = [string]$summary.packageValidationScript
    validationArtifacts = $summary.validationArtifacts
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
