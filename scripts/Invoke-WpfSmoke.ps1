#!/usr/bin/env pwsh
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Project = "smoke/Videra.WpfSmoke/Videra.WpfSmoke.csproj",

    [string]$OutputRoot = "artifacts/test-results/verify/wpf-smoke",

    [int]$LightingProofHoldSeconds = 0
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $root $Project
$outputPath = Join-Path $root $OutputRoot
$diagnosticsPath = Join-Path $outputPath "wpf-smoke-diagnostics.txt"
$stdoutPath = Join-Path $outputPath "wpf-smoke-stdout.log"
$stderrPath = Join-Path $outputPath "wpf-smoke-stderr.log"

if (-not [OperatingSystem]::IsWindows())
{
    throw "WPF smoke can only run on Windows."
}

if (-not (Test-Path -LiteralPath $projectPath))
{
    throw "WPF smoke project '$projectPath' was not found."
}

if (Test-Path -LiteralPath $outputPath)
{
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

dotnet build $projectPath --configuration $Configuration
if ($LASTEXITCODE -ne 0)
{
    throw "WPF smoke build failed."
}

$previousOutput = $env:VIDERA_WPF_SMOKE_OUTPUT
$hadPreviousOutput = Test-Path Env:VIDERA_WPF_SMOKE_OUTPUT
$previousLightingProofHoldSeconds = $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS
$hadPreviousLightingProofHoldSeconds = Test-Path Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS

try
{
    $env:VIDERA_WPF_SMOKE_OUTPUT = $diagnosticsPath
    if ($LightingProofHoldSeconds -gt 0)
    {
        $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS = $LightingProofHoldSeconds
    }
    else
    {
        Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS -ErrorAction SilentlyContinue
    }
    $process = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList @(
            "run",
            "--project",
            $projectPath,
            "--configuration",
            $Configuration,
            "--no-build") `
        -Wait `
        -PassThru `
        -NoNewWindow `
        -RedirectStandardOutput $stdoutPath `
        -RedirectStandardError $stderrPath

    if ($process.ExitCode -ne 0)
    {
        throw "WPF smoke app exited with code $($process.ExitCode)."
    }
}
finally
{
    if ($hadPreviousOutput)
    {
        $env:VIDERA_WPF_SMOKE_OUTPUT = $previousOutput
    }
    else
    {
        Remove-Item Env:VIDERA_WPF_SMOKE_OUTPUT -ErrorAction SilentlyContinue
    }

    if ($hadPreviousLightingProofHoldSeconds)
    {
        $env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS = $previousLightingProofHoldSeconds
    }
    else
    {
        Remove-Item Env:VIDERA_LIGHTING_PROOF_HOLD_SECONDS -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path -LiteralPath $diagnosticsPath))
{
    throw "WPF smoke did not produce '$diagnosticsPath'."
}

$diagnostics = Get-Content -Raw -LiteralPath $diagnosticsPath
foreach ($expected in @(
    "IsReady: True",
    "RenderPipelineProfile:",
    "LastFrameStageNames:",
    "LastFrameObjectCount:",
    "SupportedRenderFeatureNames:",
    "NativeHostBound: True"))
{
    if (-not $diagnostics.Contains($expected, [System.StringComparison]::Ordinal))
    {
        throw "WPF smoke diagnostics were missing expected line fragment '$expected'."
    }
}

Write-Host "WPF smoke passed." -ForegroundColor Green
Write-Host "Diagnostics: $diagnosticsPath"
