#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [string]$CandidateTag,

    [string]$ReleaseDryRunSummaryPath,

    [string]$PublicApiContractPath = "eng/public-api-contract.json",

    [string]$RepositoryPropsPath = "Directory.Build.props"
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

function Get-RepositoryVersion([string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path))
    {
        throw "Repository version metadata file not found at '$Path'."
    }

    [xml]$props = Get-Content -LiteralPath $Path -Raw
    $version = $props.Project.PropertyGroup |
        ForEach-Object { [string]$_.Version } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($version))
    {
        throw "Repository version metadata file '$Path' does not define a Version property."
    }

    return $version
}

function Assert-PackageVersion([string]$Version, [string]$Description)
{
    if ([string]::IsNullOrWhiteSpace($Version))
    {
        throw "$Description is required."
    }

    if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z][0-9A-Za-z.-]*)?$')
    {
        throw "$Description '$Version' must be a release package version such as '1.2.3' or '1.2.3-alpha.1'."
    }
}

function Assert-Equal([string]$Actual, [string]$Expected, [string]$Description)
{
    if ($Actual -ne $Expected)
    {
        throw "$Description mismatch. Expected '$Expected', found '$Actual'."
    }
}

Assert-PackageVersion -Version $ExpectedVersion -Description "ExpectedVersion"

if ([string]::IsNullOrWhiteSpace($CandidateTag))
{
    $CandidateTag = "v$ExpectedVersion"
}

if ($CandidateTag -notmatch '^v(.+)$')
{
    throw "CandidateTag '$CandidateTag' must be a simulated repository release tag prefixed with 'v'."
}

$tagVersion = $Matches[1]
Assert-PackageVersion -Version $tagVersion -Description "CandidateTag version"
Assert-Equal -Actual $tagVersion -Expected $ExpectedVersion -Description "CandidateTag-derived version"

$propsPath = Resolve-RepositoryPath $RepositoryPropsPath
$repositoryVersion = Get-RepositoryVersion -Path $propsPath
Assert-Equal -Actual $repositoryVersion -Expected $ExpectedVersion -Description "Repository Version metadata"

$contractPath = Resolve-RepositoryPath $PublicApiContractPath
$contract = Read-JsonFile -Path $contractPath
if ($contract.version -ne 1)
{
    throw "Unsupported public API contract version '$($contract.version)'."
}

$contractPackages = @($contract.packages)
if ($contractPackages.Count -eq 0)
{
    throw "Public API contract does not list any packages."
}

foreach ($package in $contractPackages)
{
    $packageId = [string]$package.id
    $projectPath = [string]$package.project
    if ([string]::IsNullOrWhiteSpace($packageId) -or [string]::IsNullOrWhiteSpace($projectPath))
    {
        throw "Public API contract contains a package entry without id or project."
    }

    $projectFullPath = Resolve-RepositoryPath $projectPath
    if (-not (Test-Path -LiteralPath $projectFullPath))
    {
        throw "Public API contract package '$packageId' points to missing project '$projectPath'."
    }
}

if (-not [string]::IsNullOrWhiteSpace($ReleaseDryRunSummaryPath))
{
    $summaryPath = Resolve-RepositoryPath $ReleaseDryRunSummaryPath
    $summary = Read-JsonFile -Path $summaryPath

    Assert-Equal -Actual ([string]$summary.expectedVersion) -Expected $ExpectedVersion -Description "Release dry-run summary expectedVersion"
    Assert-Equal -Actual ([string]$summary.packageContractPath) -Expected $PublicApiContractPath -Description "Release dry-run summary packageContractPath"

    $summaryPackages = @($summary.packages)
    if ($summaryPackages.Count -ne $contractPackages.Count)
    {
        throw "Release dry-run summary package count '$($summaryPackages.Count)' does not match public API contract count '$($contractPackages.Count)'."
    }

    $contractPackageIds = @($contractPackages | ForEach-Object { [string]$_.id } | Sort-Object)
    $summaryPackageIds = @($summaryPackages | ForEach-Object { [string]$_.id } | Sort-Object)
    Assert-Equal -Actual ($summaryPackageIds -join '|') -Expected ($contractPackageIds -join '|') -Description "Release dry-run summary package ids"
}

Write-Host "Release candidate version simulation passed for '$CandidateTag' -> '$ExpectedVersion'." -ForegroundColor Green
