#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [string]$Configuration = "Release",

    [string]$OutputRoot = "artifacts/release-dry-run"
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

function Assert-RepositoryChildPath([string]$Path, [string]$Description)
{
    $repositoryRoot = [System.IO.Path]::GetFullPath($root).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $repositoryRootWithSeparator = $repositoryRoot + [System.IO.Path]::DirectorySeparatorChar
    $fullPath = [System.IO.Path]::GetFullPath($Path)

    if ($fullPath.Equals($repositoryRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        -not $fullPath.StartsWith($repositoryRootWithSeparator, [System.StringComparison]::OrdinalIgnoreCase))
    {
        throw "$Description must stay inside the repository and cannot be the repository root: '$fullPath'."
    }
}

function Invoke-PackageBuild([string]$ProjectPath, [string]$PackageRoot, [string]$Version, [string]$Configuration)
{
    Write-Host "Packing $ProjectPath" -ForegroundColor Cyan
    & dotnet pack $ProjectPath -c $Configuration -o $PackageRoot /p:PackageVersion=$Version
    if ($LASTEXITCODE -ne 0)
    {
        throw "dotnet pack failed for '$ProjectPath' with exit code $LASTEXITCODE."
    }
}

function Assert-CommandSucceeded([bool]$Succeeded, $ExitCode, [string]$Description)
{
    if (-not $Succeeded)
    {
        throw "$Description failed."
    }

    if ($null -ne $ExitCode -and $ExitCode -ne 0)
    {
        throw "$Description failed with exit code $ExitCode."
    }
}

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

if ([string]::IsNullOrWhiteSpace($Configuration))
{
    throw "Configuration is required."
}

$outputRootFull = Resolve-RepositoryPath $OutputRoot
Assert-RepositoryChildPath -Path $outputRootFull -Description "OutputRoot"

$packageRoot = Join-Path $outputRootFull "packages"
Assert-RepositoryChildPath -Path $packageRoot -Description "PackageRoot"

$contractPath = Join-Path $root "eng/public-api-contract.json"
if (-not (Test-Path -LiteralPath $contractPath))
{
    throw "Public API contract not found at '$contractPath'."
}

$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
if ($contract.version -ne 1)
{
    throw "Unsupported public API contract version '$($contract.version)'."
}

$packages = @($contract.packages)
if ($packages.Count -eq 0)
{
    throw "Public API contract does not list any packages."
}

& (Join-Path $root "scripts/Test-ReleaseCandidateVersion.ps1") -ExpectedVersion $ExpectedVersion -CandidateTag "v$ExpectedVersion"
Assert-CommandSucceeded -Succeeded $? -ExitCode $LASTEXITCODE -Description "Release candidate version simulation"

Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $packageRoot | Out-Null

foreach ($package in $packages)
{
    $projectPath = Join-Path $root ([string]$package.project)
    if (-not (Test-Path -LiteralPath $projectPath))
    {
        throw "Package project '$($package.project)' was not found."
    }

    Invoke-PackageBuild -ProjectPath $projectPath -PackageRoot $packageRoot -Version $ExpectedVersion -Configuration $Configuration
}

& (Join-Path $root "scripts/Validate-Packages.ps1") -PackageRoot $packageRoot -ExpectedVersion $ExpectedVersion
Assert-CommandSucceeded -Succeeded $? -ExitCode $LASTEXITCODE -Description "Package validation"

$packageFiles = @(Get-ChildItem -Path $packageRoot -Filter *.nupkg | Sort-Object Name)
$symbolPackageFiles = @(Get-ChildItem -Path $packageRoot -Filter *.snupkg | Sort-Object Name)
$validationRoot = Join-Path $packageRoot ".validation"
$summaryPath = Join-Path $outputRootFull "release-dry-run-summary.json"
$textSummaryPath = Join-Path $outputRootFull "release-dry-run-summary.txt"
$evidenceIndexJsonPath = Join-Path $outputRootFull "release-candidate-evidence-index.json"
$evidenceIndexTextPath = Join-Path $outputRootFull "release-candidate-evidence-index.txt"
$packageSizeEvaluationPath = Join-Path $validationRoot "package-size-evaluation.json"
$packageSizeSummaryPath = Join-Path $validationRoot "package-size-summary.txt"
$doctorReportPath = "artifacts/doctor/doctor-report.json"
$performanceLabVisualEvidenceManifestPath = "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-manifest.json"
$performanceLabVisualEvidenceSummaryPath = "artifacts/performance-lab-visual-evidence/performance-lab-visual-evidence-summary.txt"

$summary = [ordered]@{
    schemaVersion = 1
    status = "pass"
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    expectedVersion = $ExpectedVersion
    configuration = $Configuration
    packageRoot = $packageRoot
    packageCount = $packageFiles.Count
    symbolPackageCount = $symbolPackageFiles.Count
    packageContractPath = "eng/public-api-contract.json"
    packageSizeBudgetPath = "eng/package-size-budgets.json"
    releaseDryRunScript = "scripts/Invoke-ReleaseDryRun.ps1"
    versionValidationScript = "scripts/Test-ReleaseCandidateVersion.ps1"
    packageValidationScript = "scripts/Validate-Packages.ps1"
    evidenceIndexScript = "scripts/New-ReleaseCandidateEvidenceIndex.ps1"
    artifactPaths = [ordered]@{
        releaseDryRunSummaryJson = $summaryPath
        releaseDryRunSummaryText = $textSummaryPath
        evidenceIndexJson = $evidenceIndexJsonPath
        evidenceIndexText = $evidenceIndexTextPath
        packageRoot = $packageRoot
        packageSizeEvaluation = $packageSizeEvaluationPath
        packageSizeSummary = $packageSizeSummaryPath
    }
    validationArtifacts = [ordered]@{
        packageSizeEvaluation = $packageSizeEvaluationPath
        packageSizeSummary = $packageSizeSummaryPath
    }
    optionalEvidence = [ordered]@{
        evidenceOnly = $true
        publishBlocker = $false
        doctorReportPath = $doctorReportPath
        performanceLabVisualEvidenceManifestPath = $performanceLabVisualEvidenceManifestPath
        performanceLabVisualEvidenceSummaryPath = $performanceLabVisualEvidenceSummaryPath
    }
    steps = @(
        [ordered]@{
            id = "version-simulation-prepack"
            status = "pass"
            script = "scripts/Test-ReleaseCandidateVersion.ps1"
            artifacts = @()
        },
        [ordered]@{
            id = "package-build"
            status = "pass"
            script = "dotnet pack"
            artifacts = @($packageRoot)
        },
        [ordered]@{
            id = "package-validation"
            status = "pass"
            script = "scripts/Validate-Packages.ps1"
            artifacts = @($packageSizeEvaluationPath, $packageSizeSummaryPath)
        },
        [ordered]@{
            id = "version-simulation-summary"
            status = "pass"
            script = "scripts/Test-ReleaseCandidateVersion.ps1"
            artifacts = @($summaryPath)
        },
        [ordered]@{
            id = "evidence-index"
            status = "pass"
            script = "scripts/New-ReleaseCandidateEvidenceIndex.ps1"
            artifacts = @($evidenceIndexJsonPath, $evidenceIndexTextPath)
        }
    )
    packages = @($packages | ForEach-Object {
        [ordered]@{
            id = [string]$_.id
            project = [string]$_.project
        }
    })
}

New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath
@(
    "Release dry run passed."
    "Status: pass"
    "Version: $ExpectedVersion"
    "Configuration: $Configuration"
    "Package root: $packageRoot"
    "NuGet packages: $($packageFiles.Count)"
    "Symbol packages: $($symbolPackageFiles.Count)"
    "Package contract: eng/public-api-contract.json"
    "Package size budgets: eng/package-size-budgets.json"
    "Validator: scripts/Validate-Packages.ps1"
    "Evidence index: $evidenceIndexJsonPath"
    "Optional visual evidence: evidence-only; publish blocker: false"
    "Doctor visual evidence status path: $doctorReportPath"
    "Performance Lab visual evidence manifest: $performanceLabVisualEvidenceManifestPath"
) | Set-Content -LiteralPath $textSummaryPath

& (Join-Path $root "scripts/Test-ReleaseCandidateVersion.ps1") -ExpectedVersion $ExpectedVersion -CandidateTag "v$ExpectedVersion" -ReleaseDryRunSummaryPath $summaryPath
Assert-CommandSucceeded -Succeeded $? -ExitCode $LASTEXITCODE -Description "Release dry-run summary version simulation"

& (Join-Path $root "scripts/New-ReleaseCandidateEvidenceIndex.ps1") -ExpectedVersion $ExpectedVersion -ReleaseDryRunSummaryPath $summaryPath -OutputRoot $outputRootFull
Assert-CommandSucceeded -Succeeded $? -ExitCode $LASTEXITCODE -Description "Release candidate evidence index generation"

Write-Host "Release dry run passed for version '$ExpectedVersion'." -ForegroundColor Green
Write-Host "Summary written to '$summaryPath'." -ForegroundColor Green
