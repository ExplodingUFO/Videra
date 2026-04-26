#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$PackageRoot,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedVersion,

    [Parameter(Mandatory = $true)]
    [string]$ReleaseTag,

    [Parameter(Mandatory = $true)]
    [string]$SourceCommit,

    [Parameter(Mandatory = $true)]
    [string]$PublishTarget,

    [Parameter(Mandatory = $true)]
    [ValidateSet("before-publish", "after-publish")]
    [string]$Stage,

    [string]$OutputRoot = "artifacts/public-publish",

    [string[]]$ValidationArtifacts = @()
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

if ([string]::IsNullOrWhiteSpace($ExpectedVersion))
{
    throw "ExpectedVersion is required."
}

if ([string]::IsNullOrWhiteSpace($ReleaseTag) -or -not $ReleaseTag.StartsWith("v"))
{
    throw "ReleaseTag must be a version tag that starts with 'v'."
}

if ($ReleaseTag.Substring(1) -ne $ExpectedVersion)
{
    throw "ReleaseTag '$ReleaseTag' does not match ExpectedVersion '$ExpectedVersion'."
}

if ([string]::IsNullOrWhiteSpace($SourceCommit))
{
    throw "SourceCommit is required."
}

$packageRootFull = Resolve-RepositoryPath $PackageRoot
$outputRootFull = Resolve-RepositoryPath $OutputRoot
$contractPath = Join-Path $root "eng/public-api-contract.json"

if (-not (Test-Path -LiteralPath $packageRootFull -PathType Container))
{
    throw "PackageRoot '$packageRootFull' does not exist."
}

if (-not (Test-Path -LiteralPath $contractPath -PathType Leaf))
{
    throw "Public API contract was not found at '$contractPath'."
}

$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json
$packageRecords = @()

foreach ($package in $contract.packages)
{
    $packageId = [string]$package.id
    if ([string]::IsNullOrWhiteSpace($packageId))
    {
        throw "eng/public-api-contract.json contains a package entry without an id."
    }

    $packageFile = Get-ChildItem -LiteralPath $packageRootFull -Recurse -File -Filter "$packageId.$ExpectedVersion.nupkg" |
        Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
        Select-Object -First 1
    if ($null -eq $packageFile)
    {
        throw "Missing package '$packageId.$ExpectedVersion.nupkg' under '$packageRootFull'."
    }

    $symbolPackageFile = Get-ChildItem -LiteralPath $packageRootFull -Recurse -File -Filter "$packageId.$ExpectedVersion.snupkg" |
        Select-Object -First 1
    if ($null -eq $symbolPackageFile)
    {
        throw "Missing symbol package '$packageId.$ExpectedVersion.snupkg' under '$packageRootFull'."
    }

    $packageRecords += [ordered]@{
        id = $packageId
        version = $ExpectedVersion
        packagePath = $packageFile.FullName
        symbolPackagePath = $symbolPackageFile.FullName
    }
}

New-Item -ItemType Directory -Force -Path $outputRootFull | Out-Null
if ($Stage -eq "before-publish")
{
    $summaryJsonPath = Join-Path $outputRootFull "public-publish-before-summary.json"
    $summaryTextPath = Join-Path $outputRootFull "public-publish-before-summary.txt"
}
else
{
    $summaryJsonPath = Join-Path $outputRootFull "public-publish-after-summary.json"
    $summaryTextPath = Join-Path $outputRootFull "public-publish-after-summary.txt"
}

$summary = [ordered]@{
    schemaVersion = 1
    stage = $Stage
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    releaseTag = $ReleaseTag
    expectedVersion = $ExpectedVersion
    sourceCommit = $SourceCommit
    publishTarget = $PublishTarget
    packageRoot = $packageRootFull
    publicApiContract = "eng/public-api-contract.json"
    validationArtifacts = @($ValidationArtifacts)
    packages = @($packageRecords)
    artifactPaths = [ordered]@{
        summaryJson = $summaryJsonPath
        summaryText = $summaryTextPath
    }
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $summaryJsonPath

$lines = @(
    "Public publish evidence"
    "Stage: $Stage"
    "Release tag: $ReleaseTag"
    "Version: $ExpectedVersion"
    "Source commit: $SourceCommit"
    "Publish target: $PublishTarget"
    ""
    "Packages:"
)

foreach ($package in $packageRecords)
{
    $lines += "- $($package.id) $($package.version)"
}

if ($ValidationArtifacts.Count -gt 0)
{
    $lines += ""
    $lines += "Validation artifacts:"
    foreach ($artifact in $ValidationArtifacts)
    {
        $lines += "- $artifact"
    }
}

$lines | Set-Content -LiteralPath $summaryTextPath

Write-Host "Public publish evidence written to '$summaryJsonPath'." -ForegroundColor Green
