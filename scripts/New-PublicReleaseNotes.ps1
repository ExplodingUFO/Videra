#!/usr/bin/env pwsh
param(
    [Parameter(Mandatory = $true)]
    [string]$EvidenceSummaryPath,

    [string]$OutputPath = "artifacts/public-release-notes/public-release-notes.md"
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

$evidencePathFull = Resolve-RepositoryPath $EvidenceSummaryPath
$outputPathFull = Resolve-RepositoryPath $OutputPath

if (-not (Test-Path -LiteralPath $evidencePathFull -PathType Leaf))
{
    throw "Evidence summary '$evidencePathFull' was not found."
}

$evidence = Get-Content -LiteralPath $evidencePathFull -Raw | ConvertFrom-Json
if ([string]$evidence.stage -ne "after-publish")
{
    throw "Public release notes require public-publish-after-summary.json evidence."
}

$version = [string]$evidence.expectedVersion
if ([string]::IsNullOrWhiteSpace($version))
{
    throw "Evidence summary is missing expectedVersion."
}

$releaseTag = [string]$evidence.releaseTag
$sourceCommit = [string]$evidence.sourceCommit
$publishTarget = [string]$evidence.publishTarget
$packages = @($evidence.packages | Sort-Object id)
if ($packages.Count -eq 0)
{
    throw "Evidence summary does not list released packages."
}

function Test-Package([string]$PackageId)
{
    return $null -ne ($packages | Where-Object { [string]$_.id -eq $PackageId } | Select-Object -First 1)
}

$installCommands = @()
foreach ($package in $packages)
{
    $installCommands += "dotnet add package $($package.id) --version $version"
}

$lines = @(
    "# Videra $version"
    ""
    "Release tag: $releaseTag"
    "Source commit: $sourceCommit"
    "Publish target: $publishTarget"
    ""
    "## Install"
    ""
    "Public packages are installed from nuget.org. GitHub Packages is preview/internal and is not the default public install path."
    ""
    '```pwsh'
)

$lines += $installCommands
$lines += @(
    '```'
    ""
    "## Package matrix"
    ""
    "See docs/package-matrix.md for supported package combinations. Released package assets:"
)

foreach ($package in $packages)
{
    $lines += "- $($package.id) $version"
}

$lines += @(
    ""
    "## Known alpha limitations"
    ""
    "- The viewer remains a static-scene alpha surface; animation, skeletons, morph targets, broad lighting/shadow systems, extra UI adapters, and WebGL/OpenGL are outside this release boundary."
    "- Videra.Avalonia requires exactly one matching Videra.Platform.* package for the target host."
    "- Videra.Import.Gltf and Videra.Import.Obj remain explicit importer packages."
    "- Videra.Demo remains repository-only; sample and demo applications are support/reference paths, not public package products."
    "- SurfaceCharts remains a separate package family and is not a VideraView mode."
    ""
    "## Release evidence"
    ""
    "- public-publish-after-summary.json: $evidencePathFull"
    "- public-publish-before-summary.json: produced by the same approved publish workflow before feed mutation."
    "- public-release-preflight-summary.json: produced before public publish dispatch."
)

if (Test-Package "Videra.SurfaceCharts.Avalonia")
{
    $lines += "- SurfaceCharts package assets are part of this release evidence."
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputPathFull) | Out-Null
$lines | Set-Content -LiteralPath $outputPathFull

Write-Host "Public release notes written to '$outputPathFull'." -ForegroundColor Green
