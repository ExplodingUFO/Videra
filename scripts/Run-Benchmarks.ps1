#!/usr/bin/env pwsh
param(
    [ValidateSet("Viewer", "SurfaceCharts")]
    [string]$Suite,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OutputRoot = "artifacts/benchmarks"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outputPath = Join-Path $root $OutputRoot
$suiteName = if ($Suite -eq "Viewer") { "viewer" } else { "surfacecharts" }
$suiteOutput = Join-Path $outputPath $suiteName

if (Test-Path -LiteralPath $suiteOutput)
{
    Remove-Item -LiteralPath $suiteOutput -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $suiteOutput | Out-Null

switch ($Suite)
{
    "Viewer"
    {
        $projectPath = Join-Path $root "benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj"
    }
    "SurfaceCharts"
    {
        $projectPath = Join-Path $root "benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj"
    }
    default
    {
        throw "Unsupported suite '$Suite'."
    }
}

Write-Host "=== Benchmark Build: $Suite ===" -ForegroundColor Cyan
dotnet build $projectPath --configuration $Configuration
if ($LASTEXITCODE -ne 0)
{
    throw "$Suite benchmark build failed."
}

Write-Host "=== Benchmark Run: $Suite ===" -ForegroundColor Cyan
dotnet run --project $projectPath --configuration $Configuration -- --filter "*" --artifacts "$suiteOutput" --exporters json csv markdown
if ($LASTEXITCODE -ne 0)
{
    throw "$Suite benchmark run failed."
}

$summaryPath = Join-Path $suiteOutput "SUMMARY.txt"
$artifactList = Get-ChildItem -Path $suiteOutput -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName.Substring($suiteOutput.Length).TrimStart('\', '/') }

@(
    "Suite: $Suite"
    "Configuration: $Configuration"
    "Artifact root: $suiteOutput"
    ""
    "Artifacts:"
    $artifactList
) | Set-Content -Path $summaryPath

Write-Host "$Suite benchmark artifacts written to $suiteOutput" -ForegroundColor Green
