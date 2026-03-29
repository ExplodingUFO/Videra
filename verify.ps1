#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Videra project verification script — runs build and tests.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$allPass = $true
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Write-Step([string]$title) {
    Write-Host ""
    Write-Host "=== $title ===" -ForegroundColor Cyan
}

# Step 1: Build
Write-Step "Build ($Configuration)"
dotnet build "$root/Videra.slnx" --configuration $Configuration -v q 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [PASS] Build succeeded" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Build failed" -ForegroundColor Red
    $allPass = $false
}

# Step 2: Tests
Write-Step "Tests"
dotnet test "$root/Videra.slnx" --configuration $Configuration -v q 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [PASS] All tests passed" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Some tests failed" -ForegroundColor Red
    $allPass = $false
}

# Step 3: Demo build
Write-Step "Demo Build"
dotnet build "$root/samples/Videra.Demo/Videra.Demo.csproj" --configuration $Configuration -v q 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [PASS] Demo builds" -ForegroundColor Green
} else {
    Write-Host "  [FAIL] Demo build failed" -ForegroundColor Red
    $allPass = $false
}

# Summary
$sw.Stop()
Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "  Duration: $($sw.Elapsed.ToString('mm\:ss'))"
if ($allPass) {
    Write-Host "  All checks passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "  Some checks failed." -ForegroundColor Red
    exit 1
}
