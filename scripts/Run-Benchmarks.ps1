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
$contractPath = Join-Path $root "benchmarks/benchmark-contract.json"
$outputPath = Join-Path $root $OutputRoot
$requestedExporters = @("json", "csv", "markdown")

if (-not (Test-Path -LiteralPath $contractPath))
{
    throw "Benchmark contract file not found at '$contractPath'."
}

$contract = Get-Content -LiteralPath $contractPath -Raw | ConvertFrom-Json -AsHashtable
if ($contract.schemaVersion -ne 1)
{
    throw "Unsupported benchmark contract schema version '$($contract.schemaVersion)'."
}

$suiteContract = $contract.suites | Where-Object { $_.name -eq $Suite } | Select-Object -First 1
if ($null -eq $suiteContract)
{
    throw "Benchmark contract does not define suite '$Suite'."
}

$suiteName = $suiteContract.artifactDirectory
$suiteOutput = Join-Path $outputPath $suiteName

if (Test-Path -LiteralPath $suiteOutput)
{
    Remove-Item -LiteralPath $suiteOutput -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $suiteOutput | Out-Null

$projectPath = Join-Path $root $suiteContract.project

Write-Host "=== Benchmark Build: $Suite ===" -ForegroundColor Cyan
dotnet build $projectPath --configuration $Configuration
if ($LASTEXITCODE -ne 0)
{
    throw "$Suite benchmark build failed."
}

Write-Host "=== Benchmark Run: $Suite ===" -ForegroundColor Cyan
dotnet run --project $projectPath --configuration $Configuration -- --filter "*" --artifacts "$suiteOutput" --exporters $requestedExporters
if ($LASTEXITCODE -ne 0)
{
    throw "$Suite benchmark run failed."
}

$manifestPath = Join-Path $suiteOutput "benchmark-manifest.json"
$rawArtifactList = Get-ChildItem -Path $suiteOutput -Recurse -File | Sort-Object FullName | ForEach-Object { $_.FullName.Substring($suiteOutput.Length).TrimStart('\', '/') }
$suiteFamilies = @(
    $suiteContract.families | ForEach-Object {
        [ordered]@{
            name = $_.name
            benchmarks = @($_.benchmarks)
        }
    }
)

$manifest = [ordered]@{
    schemaVersion = 1
    suite = $Suite
    configuration = $Configuration
    generatedAtUtc = [DateTime]::UtcNow.ToString("O")
    contractPath = "benchmarks/benchmark-contract.json"
    benchmarkProject = $suiteContract.project
    artifactDirectory = $suiteContract.artifactDirectory
    requestedExporters = $requestedExporters
    families = $suiteFamilies
    rawArtifacts = $rawArtifactList
}

$manifest | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestPath

$summaryPath = Join-Path $suiteOutput "SUMMARY.txt"
$artifactList = Get-ChildItem -Path $suiteOutput -Recurse | Sort-Object FullName | ForEach-Object { $_.FullName.Substring($suiteOutput.Length).TrimStart('\', '/') }

@(
    "Suite: $Suite"
    "Configuration: $Configuration"
    "Artifact root: $suiteOutput"
    "Contract: benchmarks/benchmark-contract.json"
    "Manifest: benchmark-manifest.json"
    ""
    "Artifacts:"
    $artifactList
) | Set-Content -Path $summaryPath

Write-Host "$Suite benchmark artifacts written to $suiteOutput" -ForegroundColor Green
