#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Videra project verification script — runs build and tests.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
.PARAMETER IncludeNativeLinux
    Run the Linux native validation package (requires a Linux host with X11/Vulkan).
.PARAMETER IncludeNativeMacOS
    Run the macOS native validation package (requires a macOS host with NSView/Metal).
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$IncludeNativeLinux,
    [switch]$IncludeNativeMacOS
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$allPass = $true
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Write-Step([string]$title) {
    Write-Host ""
    Write-Host "=== $title ===" -ForegroundColor Cyan
}

function Invoke-Check([string]$title, [scriptblock]$command, [string]$successMessage, [string]$failureMessage) {
    Write-Step $title
    & $command
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [PASS] $successMessage" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $failureMessage" -ForegroundColor Red
        $script:allPass = $false
    }
}

# Step 1: Build
Invoke-Check "Build ($Configuration)" {
    dotnet build "$root/Videra.slnx" --configuration $Configuration -v q 2>$null
} "Build succeeded" "Build failed"

# Step 2: Tests
Invoke-Check "Tests" {
    dotnet test "$root/Videra.slnx" --configuration $Configuration -v q 2>$null
} "All tests passed" "Some tests failed"

# Step 3: Demo build
Invoke-Check "Demo Build" {
    dotnet build "$root/samples/Videra.Demo/Videra.Demo.csproj" --configuration $Configuration -v q 2>$null
} "Demo builds" "Demo build failed"

# Optional native validation packages
if ($IncludeNativeLinux) {
    Invoke-Check "Linux Native Validation" {
        dotnet test "$root/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
    } "Linux native validation passed" "Linux native validation failed"
}

if ($IncludeNativeMacOS) {
    Invoke-Check "macOS Native Validation" {
        dotnet test "$root/tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
    } "macOS native validation passed" "macOS native validation failed"
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
