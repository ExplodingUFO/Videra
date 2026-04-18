#!/usr/bin/env pwsh
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Project = "smoke/Videra.ConsumerSmoke/Videra.ConsumerSmoke.csproj",

    [string]$OutputRoot = "artifacts/consumer-smoke"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root $Project
$outputPath = Join-Path $root $OutputRoot
$jsonPath = Join-Path $outputPath "consumer-smoke-result.json"
$snapshotPath = Join-Path $outputPath "diagnostics-snapshot.txt"
$packageOutputPath = Join-Path $outputPath "packages"
$nugetConfigPath = Join-Path $outputPath "NuGet.Config"
$publicPackageProjects = @(
    "src/Videra.Core/Videra.Core.csproj",
    "src/Videra.Avalonia/Videra.Avalonia.csproj",
    "src/Videra.Platform.Windows/Videra.Platform.Windows.csproj",
    "src/Videra.Platform.Linux/Videra.Platform.Linux.csproj",
    "src/Videra.Platform.macOS/Videra.Platform.macOS.csproj"
)

if (-not (Test-Path -LiteralPath $projectPath))
{
    throw "Consumer smoke project '$projectPath' was not found."
}

if (Test-Path -LiteralPath $outputPath)
{
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

Write-Host "=== Pack Public Consumer Packages ===" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path $packageOutputPath | Out-Null

$versionProject = Join-Path $root "src/Videra.Avalonia/Videra.Avalonia.csproj"
$resolvedVersion = (dotnet msbuild $versionProject -nologo -getProperty:Version | Select-Object -Last 1).Trim()
if ([string]::IsNullOrWhiteSpace($resolvedVersion))
{
    throw "Unable to resolve Videra consumer package version from '$versionProject'."
}

$packageVersion =
    if ($resolvedVersion.Contains('-'))
    {
        "$resolvedVersion.consumer-smoke"
    }
    else
    {
        "$resolvedVersion-consumer-smoke"
    }

foreach ($relativeProject in $publicPackageProjects)
{
    $fullProjectPath = Join-Path $root $relativeProject
    dotnet pack $fullProjectPath --configuration $Configuration --output $packageOutputPath -p:PackageVersion=$packageVersion
    if ($LASTEXITCODE -ne 0)
    {
        throw "Packing '$relativeProject' failed."
    }
}

$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="consumer-local" value="$packageOutputPath" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
$nugetConfig | Set-Content -Path $nugetConfigPath

Write-Host "=== Consumer Smoke Restore ===" -ForegroundColor Cyan
dotnet restore $projectPath --configfile $nugetConfigPath -p:VideraConsumerPackageVersion=$packageVersion
if ($LASTEXITCODE -ne 0)
{
    throw "Consumer smoke restore failed."
}

Write-Host "=== Consumer Smoke Build ===" -ForegroundColor Cyan
dotnet build $projectPath --configuration $Configuration --no-restore -p:VideraConsumerPackageVersion=$packageVersion
if ($LASTEXITCODE -ne 0)
{
    throw "Consumer smoke build failed."
}

Write-Host "=== Consumer Smoke Run ===" -ForegroundColor Cyan
$previousOutput = $env:VIDERA_CONSUMER_SMOKE_OUTPUT
$hadPreviousOutput = Test-Path Env:VIDERA_CONSUMER_SMOKE_OUTPUT

try
{
    $env:VIDERA_CONSUMER_SMOKE_OUTPUT = $jsonPath
    dotnet run --project $projectPath --configuration $Configuration --no-build -p:VideraConsumerPackageVersion=$packageVersion
    if ($LASTEXITCODE -ne 0)
    {
        throw "Consumer smoke app exited with code $LASTEXITCODE."
    }
}
finally
{
    if ($hadPreviousOutput)
    {
        $env:VIDERA_CONSUMER_SMOKE_OUTPUT = $previousOutput
    }
    else
    {
        Remove-Item Env:VIDERA_CONSUMER_SMOKE_OUTPUT -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path -LiteralPath $jsonPath))
{
    throw "Consumer smoke did not produce '$jsonPath'."
}

if (-not (Test-Path -LiteralPath $snapshotPath))
{
    throw "Consumer smoke did not produce '$snapshotPath'."
}

$report = Get-Content -Raw $jsonPath | ConvertFrom-Json
if (-not $report.Succeeded)
{
    throw "Consumer smoke reported failure: $($report.Failure)"
}

if (-not $report.IsReady)
{
    throw "Consumer smoke completed without a ready backend diagnostics snapshot."
}

if (-not $report.FrameAllReturned)
{
    throw "Consumer smoke completed without a successful FrameAll result."
}

Write-Host "Consumer smoke passed." -ForegroundColor Green
Write-Host "Resolved package version: $packageVersion"
Write-Host "ResolvedBackend: $($report.ResolvedBackend)"
Write-Host "ResolvedDisplayServer: $($report.ResolvedDisplayServer)"
Write-Host "IsUsingSoftwareFallback: $($report.IsUsingSoftwareFallback)"
Write-Host "DiagnosticsSnapshot: $snapshotPath"
