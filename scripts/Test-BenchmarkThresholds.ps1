#!/usr/bin/env pwsh
param(
    [ValidateSet("Viewer", "SurfaceCharts")]
    [string]$Suite,

    [string]$OutputRoot = "artifacts/benchmarks"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$benchmarkContractPath = Join-Path $root "benchmarks/benchmark-contract.json"
$thresholdContractPath = Join-Path $root "benchmarks/benchmark-thresholds.json"

function Read-JsonHashtable([string]$Path)
{
    if (-not (Test-Path -LiteralPath $Path))
    {
        throw "Required JSON file not found at '$Path'."
    }

    return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable
}

function Get-SuiteDefinition([hashtable]$Contract, [string]$Name, [string]$ContractLabel)
{
    $suiteDefinition = $Contract.suites | Where-Object { $_.name -eq $Name } | Select-Object -First 1
    if ($null -eq $suiteDefinition)
    {
        throw "$ContractLabel does not define suite '$Name'."
    }

    return $suiteDefinition
}

function Format-Nanoseconds([double]$Nanoseconds)
{
    if ($Nanoseconds -ge 1000000d)
    {
        return "{0:N3} ms" -f ($Nanoseconds / 1000000d)
    }

    if ($Nanoseconds -ge 1000d)
    {
        return "{0:N3} us" -f ($Nanoseconds / 1000d)
    }

    return "{0:N3} ns" -f $Nanoseconds
}

$benchmarkContract = Read-JsonHashtable $benchmarkContractPath
if ($benchmarkContract.schemaVersion -ne 1)
{
    throw "Unsupported benchmark contract schema version '$($benchmarkContract.schemaVersion)'."
}

$thresholdContract = Read-JsonHashtable $thresholdContractPath
if ($thresholdContract.schemaVersion -ne 1)
{
    throw "Unsupported benchmark threshold schema version '$($thresholdContract.schemaVersion)'."
}

$benchmarkSuite = Get-SuiteDefinition $benchmarkContract $Suite "Benchmark contract"
$thresholdSuite = Get-SuiteDefinition $thresholdContract $Suite "Benchmark threshold contract"
$suiteOutput = Join-Path (Join-Path $root $OutputRoot) $benchmarkSuite.artifactDirectory
$manifestPath = Join-Path $suiteOutput "benchmark-manifest.json"
$evaluationPath = Join-Path $suiteOutput "benchmark-threshold-evaluation.json"
$summaryPath = Join-Path $suiteOutput "benchmark-threshold-summary.txt"

if (-not (Test-Path -LiteralPath $suiteOutput))
{
    throw "Benchmark artifact directory not found at '$suiteOutput'. Run Run-Benchmarks.ps1 first."
}

$manifest = Read-JsonHashtable $manifestPath
if ($manifest.suite -ne $Suite)
{
    throw "Benchmark manifest suite '$($manifest.suite)' does not match requested suite '$Suite'."
}

$reportBenchmarks = @{}
Get-ChildItem -Path $suiteOutput -Recurse -Filter "*-report-full-compressed.json" -File | ForEach-Object {
    $report = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json -AsHashtable
    foreach ($benchmark in $report.Benchmarks)
    {
        if ($null -eq $benchmark.Statistics)
        {
            continue
        }

        $reportBenchmarks[$benchmark.FullName] = [ordered]@{
            meanNs = [double]$benchmark.Statistics.Mean
            allocatedBytes = if ($null -ne $benchmark.Memory.BytesAllocatedPerOperation) { [double]$benchmark.Memory.BytesAllocatedPerOperation } else { $null }
        }
    }
}

if ($reportBenchmarks.Count -eq 0)
{
    throw "No benchmark report JSON files with statistics were found under '$suiteOutput'."
}

$evaluations = @()
$failures = @()

foreach ($threshold in $thresholdSuite.thresholds)
{
    $benchmarkName = [string]$threshold.benchmark
    $baselineMeanNs = [double]$threshold.baselineMeanNs
    $maxRegressionPercent = [double]$threshold.maxRegressionPercent
    $allowedMeanNs = $baselineMeanNs * (1d + ($maxRegressionPercent / 100d))

    if (-not $reportBenchmarks.ContainsKey($benchmarkName))
    {
        $evaluation = [ordered]@{
            benchmark = $benchmarkName
            baselineMeanNs = $baselineMeanNs
            maxRegressionPercent = $maxRegressionPercent
            maxAllowedMeanNs = $allowedMeanNs
            actualMeanNs = $null
            regressionPercent = $null
            allocatedBytes = $null
            passed = $false
            failure = "Benchmark result not found in suite artifacts."
        }

        $evaluations += $evaluation
        $failures += "[FAIL] $benchmarkName missing from benchmark artifacts."
        continue
    }

    $result = $reportBenchmarks[$benchmarkName]
    $actualMeanNs = [double]$result.meanNs
    $regressionPercent = (($actualMeanNs / $baselineMeanNs) - 1d) * 100d
    $passed = $actualMeanNs -le $allowedMeanNs

    $evaluation = [ordered]@{
        benchmark = $benchmarkName
        baselineMeanNs = $baselineMeanNs
        maxRegressionPercent = $maxRegressionPercent
        maxAllowedMeanNs = $allowedMeanNs
        actualMeanNs = $actualMeanNs
        regressionPercent = $regressionPercent
        allocatedBytes = $result.allocatedBytes
        passed = $passed
        failure = if ($passed) { $null } else { "Mean runtime exceeded committed threshold." }
    }

    $evaluations += $evaluation

    if (-not $passed)
    {
        $failures += "[FAIL] $benchmarkName actual $(Format-Nanoseconds $actualMeanNs) exceeded allowed $(Format-Nanoseconds $allowedMeanNs) ({0:N2}% vs baseline)." -f $regressionPercent
    }
}

$summaryLines = @(
    "Suite: $Suite"
    "Threshold contract: benchmarks/benchmark-thresholds.json"
    "Manifest: benchmark-manifest.json"
    ""
)

foreach ($evaluation in $evaluations)
{
    if ($evaluation.passed)
    {
        $summaryLines += "[PASS] {0} actual {1} within allowed {2} ({3:N2}% vs baseline)." -f $evaluation.benchmark, (Format-Nanoseconds $evaluation.actualMeanNs), (Format-Nanoseconds $evaluation.maxAllowedMeanNs), $evaluation.regressionPercent
    }
    else
    {
        $summaryLines += "[FAIL] {0} {1}" -f $evaluation.benchmark, $evaluation.failure
    }
}

$evaluationDocument = [ordered]@{
    schemaVersion = 1
    suite = $Suite
    evaluatedAtUtc = [DateTime]::UtcNow.ToString("O")
    thresholdContractPath = "benchmarks/benchmark-thresholds.json"
    manifestPath = "benchmark-manifest.json"
    evaluations = $evaluations
}

$evaluationDocument | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $evaluationPath
$summaryLines | Set-Content -LiteralPath $summaryPath

if ($failures.Count -gt 0)
{
    throw "Benchmark thresholds failed for ${Suite}:`n$($failures -join "`n")"
}

Write-Host "$Suite benchmark thresholds passed. Evaluation written to $evaluationPath" -ForegroundColor Green
