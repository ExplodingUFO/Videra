#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [string]$ExpectedCommit = "",

    [string]$EvidenceRoot = "artifacts",

    [string]$OutputRoot = "artifacts/final-release-simulation",

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

function Assert-Contains([string]$Id, [string]$Content, [string]$Expected, [string]$Path)
{
    if ($Content.Contains($Expected))
    {
        Add-Check -Id $Id -Status "pass" -Message "Found expected release-control text." -Path $Path
        return
    }

    Add-Check -Id $Id -Status "fail" -Message "Missing expected text '$Expected'." -Path $Path
}

function Assert-DoesNotContain([string]$Id, [string]$Content, [string]$Unexpected, [string]$Path)
{
    if (-not $Content.Contains($Unexpected))
    {
        Add-Check -Id $Id -Status "pass" -Message "Public release mutation text is absent." -Path $Path
        return
    }

    Add-Check -Id $Id -Status "fail" -Message "Unexpected text '$Unexpected' was found." -Path $Path
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

$evidenceRootFull = Resolve-RepositoryPath $EvidenceRoot
$outputRootFull = Resolve-RepositoryPath $OutputRoot
$preflightOutputRoot = Join-Path $outputRootFull "public-release-preflight"
New-Item -ItemType Directory -Force -Path $outputRootFull | Out-Null

$preflightScript = Join-Path $root "scripts/Invoke-PublicReleasePreflight.ps1"
$preflightArgs = @(
    "-NoProfile",
    "-ExecutionPolicy",
    "Bypass",
    "-File",
    $preflightScript,
    "-ExpectedVersion",
    $ExpectedVersion,
    "-EvidenceRoot",
    $evidenceRootFull,
    "-OutputRoot",
    $preflightOutputRoot
)

if (-not [string]::IsNullOrWhiteSpace($ExpectedCommit))
{
    $preflightArgs += "-ExpectedCommit"
    $preflightArgs += $ExpectedCommit
}

if ($SkipRepositoryStateCheck)
{
    $preflightArgs += "-SkipRepositoryStateCheck"
}

$preflightOutput = @(& pwsh @preflightArgs 2>&1)
if ($LASTEXITCODE -eq 0)
{
    Add-Check -Id "public-release-preflight" -Status "pass" -Message "Public release preflight passed." -Path $preflightOutputRoot
}
else
{
    Add-Check -Id "public-release-preflight" -Status "fail" -Message ($preflightOutput -join "`n") -Path $preflightOutputRoot
}

$publicWorkflowPath = Join-Path $root ".github/workflows/publish-public.yml"
$existingPublicWorkflowPath = Join-Path $root ".github/workflows/publish-existing-public-release.yml"
$previewWorkflowPath = Join-Path $root ".github/workflows/publish-github-packages.yml"
$releasingPath = Join-Path $root "docs/releasing.md"
$releasePolicyPath = Join-Path $root "docs/release-policy.md"
$cutoverPath = Join-Path $root "docs/release-candidate-cutover.md"

$publicWorkflow = Get-Content -LiteralPath $publicWorkflowPath -Raw
$existingPublicWorkflow = Get-Content -LiteralPath $existingPublicWorkflowPath -Raw
$previewWorkflow = Get-Content -LiteralPath $previewWorkflowPath -Raw
$releasing = Get-Content -LiteralPath $releasingPath -Raw
$releasePolicy = Get-Content -LiteralPath $releasePolicyPath -Raw
$cutover = Get-Content -LiteralPath $cutoverPath -Raw

foreach ($workflow in @(
    @{ id = "publish-public"; content = $publicWorkflow; path = $publicWorkflowPath },
    @{ id = "publish-existing-public-release"; content = $existingPublicWorkflow; path = $existingPublicWorkflowPath }))
{
    Assert-Contains -Id "$($workflow.id)-manual-dispatch" -Content $workflow.content -Expected "workflow_dispatch:" -Path $workflow.path
    Assert-Contains -Id "$($workflow.id)-expected-commit" -Content $workflow.content -Expected "expected_commit:" -Path $workflow.path
    Assert-Contains -Id "$($workflow.id)-approval-environment" -Content $workflow.content -Expected "public-release" -Path $workflow.path
}

Assert-Contains -Id "preview-manual-dispatch" -Content $previewWorkflow -Expected "workflow_dispatch:" -Path $previewWorkflowPath
Assert-Contains -Id "preview-environment" -Content $previewWorkflow -Expected "preview-packages" -Path $previewWorkflowPath
Assert-Contains -Id "preview-package-contract" -Content $previewWorkflow -Expected "Validate-Packages.ps1" -Path $previewWorkflowPath
Assert-Contains -Id "preview-target" -Content $previewWorkflow -Expected "https://nuget.pkg.github.com/ExplodingUFO/index.json" -Path $previewWorkflowPath
Assert-DoesNotContain -Id "preview-public-feed" -Content $previewWorkflow -Unexpected "https://api.nuget.org/v3/index.json" -Path $previewWorkflowPath

Assert-Contains -Id "release-policy-preview-boundary" -Content $releasePolicy -Expected "not the default public consumer path" -Path $releasePolicyPath
Assert-Contains -Id "releasing-final-simulation" -Content $releasing -Expected "final non-mutating public-release simulation" -Path $releasingPath
Assert-Contains -Id "cutover-before-state" -Content $cutover -Expected "## Before-publish state" -Path $cutoverPath
Assert-Contains -Id "cutover-after-state" -Content $cutover -Expected "## After-publish state" -Path $cutoverPath

$failedChecks = @($checks | Where-Object { $_.status -ne "pass" })
$status = if ($failedChecks.Count -eq 0) { "pass" } else { "fail" }
$summary = [ordered]@{
    schemaVersion = 1
    status = $status
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    expectedVersion = $ExpectedVersion
    expectedCommit = $ExpectedCommit
    evidenceRoot = $evidenceRootFull
    artifactPaths = [ordered]@{
        summaryJson = Join-Path $outputRootFull "final-release-simulation-summary.json"
        summaryText = Join-Path $outputRootFull "final-release-simulation-summary.txt"
    }
    checks = @($checks)
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summary.artifactPaths.summaryJson

$lines = @(
    "Final release simulation"
    "Status: $status"
    "Version: $ExpectedVersion"
    ""
    "Checks:"
)
foreach ($check in $checks)
{
    $lines += "- [$($check.status.ToUpperInvariant())] $($check.id): $($check.message)"
}
$lines | Set-Content -LiteralPath $summary.artifactPaths.summaryText

if ($failedChecks.Count -gt 0)
{
    $failedIds = ($failedChecks | ForEach-Object { $_.id }) -join ", "
    throw "final release simulation failed: $failedIds"
}

Write-Host "Final release simulation passed for version '$ExpectedVersion'." -ForegroundColor Green
