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
if ($LASTEXITCODE -ne 0)
{
    throw "Release candidate version simulation failed with exit code $LASTEXITCODE."
}

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
if ($LASTEXITCODE -ne 0)
{
    throw "Package validation failed with exit code $LASTEXITCODE."
}

$packageFiles = @(Get-ChildItem -Path $packageRoot -Filter *.nupkg | Sort-Object Name)
$symbolPackageFiles = @(Get-ChildItem -Path $packageRoot -Filter *.snupkg | Sort-Object Name)
$validationRoot = Join-Path $packageRoot ".validation"

$summary = [ordered]@{
    schemaVersion = 1
    expectedVersion = $ExpectedVersion
    configuration = $Configuration
    packageRoot = $packageRoot
    packageCount = $packageFiles.Count
    symbolPackageCount = $symbolPackageFiles.Count
    packageContractPath = "eng/public-api-contract.json"
    packageValidationScript = "scripts/Validate-Packages.ps1"
    validationArtifacts = [ordered]@{
        packageSizeEvaluation = Join-Path $validationRoot "package-size-evaluation.json"
        packageSizeSummary = Join-Path $validationRoot "package-size-summary.txt"
    }
    packages = @($packages | ForEach-Object {
        [ordered]@{
            id = [string]$_.id
            project = [string]$_.project
        }
    })
}

New-Item -ItemType Directory -Path $outputRootFull -Force | Out-Null
$summaryPath = Join-Path $outputRootFull "release-dry-run-summary.json"
$textSummaryPath = Join-Path $outputRootFull "release-dry-run-summary.txt"

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryPath
@(
    "Release dry run passed."
    "Version: $ExpectedVersion"
    "Configuration: $Configuration"
    "Package root: $packageRoot"
    "NuGet packages: $($packageFiles.Count)"
    "Symbol packages: $($symbolPackageFiles.Count)"
    "Package contract: eng/public-api-contract.json"
    "Validator: scripts/Validate-Packages.ps1"
) | Set-Content -LiteralPath $textSummaryPath

& (Join-Path $root "scripts/Test-ReleaseCandidateVersion.ps1") -ExpectedVersion $ExpectedVersion -CandidateTag "v$ExpectedVersion" -ReleaseDryRunSummaryPath $summaryPath
if ($LASTEXITCODE -ne 0)
{
    throw "Release dry-run summary version simulation failed with exit code $LASTEXITCODE."
}

& (Join-Path $root "scripts/New-ReleaseCandidateEvidenceIndex.ps1") -ExpectedVersion $ExpectedVersion -ReleaseDryRunSummaryPath $summaryPath -OutputRoot $outputRootFull
if ($LASTEXITCODE -ne 0)
{
    throw "Release candidate evidence index generation failed with exit code $LASTEXITCODE."
}

Write-Host "Release dry run passed for version '$ExpectedVersion'." -ForegroundColor Green
Write-Host "Summary written to '$summaryPath'." -ForegroundColor Green
