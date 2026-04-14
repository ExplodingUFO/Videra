#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Videra project verification script — runs build and tests.
.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release
.PARAMETER IncludeNativeLinux
    Run the Linux native validation package (requires a Linux host with X11/Vulkan).
.PARAMETER IncludeNativeLinuxXWayland
    Run the Linux native validation package inside a Wayland session that exposes XWayland.
.PARAMETER IncludeNativeMacOS
    Run the macOS native validation package (requires a macOS host with NSView/Metal).
.PARAMETER IncludeNativeWindows
    Run the Windows native validation package (requires a Windows host with a real HWND/D3D11 path).
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$IncludeNativeLinux,
    [switch]$IncludeNativeLinuxXWayland,
    [switch]$IncludeNativeMacOS,
    [switch]$IncludeNativeWindows
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$allPass = $true
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$testResultsDirectory = Join-Path $root "artifacts/test-results/verify"

if (Test-Path -LiteralPath $testResultsDirectory)
{
    Remove-Item -LiteralPath $testResultsDirectory -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $testResultsDirectory | Out-Null

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

function Invoke-TestCheck([string]$title, [scriptblock]$command, [string]$successMessage, [string]$failureMessage) {
    Write-Step $title
    & $command
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [PASS] $successMessage" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $failureMessage" -ForegroundColor Red
        Write-Host "  Test results directory: $testResultsDirectory" -ForegroundColor Yellow
        $script:allPass = $false
    }
}

# Step 1: Build
Invoke-Check "Build ($Configuration)" {
    dotnet build "$root/Videra.slnx" --configuration $Configuration -v q 2>$null
} "Build succeeded" "Build failed"

# Step 2: Tests
Invoke-TestCheck "Tests" {
    dotnet test "$root/Videra.slnx" --configuration $Configuration -v m --logger "console;verbosity=detailed" --logger "trx;LogFileName=verify.trx" --results-directory "$testResultsDirectory"
} "All tests passed" "Some tests failed"

# Step 3: Demo build
Invoke-Check "Demo Build" {
    dotnet build "$root/samples/Videra.Demo/Videra.Demo.csproj" --configuration $Configuration -v q 2>$null
} "Demo builds" "Demo build failed"

# Step 4: Surface Charts Demo build
Invoke-Check "Surface Charts Demo Build" {
    dotnet build "$root/samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj" --configuration $Configuration -v q 2>$null
} "Surface charts demo builds" "Surface charts demo build failed"

# Optional native validation packages
if ($IncludeNativeLinux) {
    Invoke-Check "Linux X11 Native Validation" {
        $previous = $env:VIDERA_RUN_LINUX_NATIVE_TESTS
        $hadPrevious = Test-Path Env:VIDERA_RUN_LINUX_NATIVE_TESTS
        $previousExpected = $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER
        $hadExpected = Test-Path Env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER
        try {
            $env:VIDERA_RUN_LINUX_NATIVE_TESTS = "true"
            $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER = "X11"
            dotnet test "$root/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
        }
        finally {
            if ($hadPrevious) {
                $env:VIDERA_RUN_LINUX_NATIVE_TESTS = $previous
            }
            else {
                Remove-Item Env:VIDERA_RUN_LINUX_NATIVE_TESTS -ErrorAction SilentlyContinue
            }

            if ($hadExpected) {
                $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER = $previousExpected
            }
            else {
                Remove-Item Env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER -ErrorAction SilentlyContinue
            }
        }
    } "Linux X11 native validation passed" "Linux X11 native validation failed"
}

if ($IncludeNativeLinuxXWayland) {
    Invoke-Check "Linux XWayland Native Validation" {
        $previous = $env:VIDERA_RUN_LINUX_NATIVE_TESTS
        $hadPrevious = Test-Path Env:VIDERA_RUN_LINUX_NATIVE_TESTS
        $previousExpected = $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER
        $hadExpected = Test-Path Env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER
        try {
            $env:VIDERA_RUN_LINUX_NATIVE_TESTS = "true"
            $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER = "XWayland"
            dotnet test "$root/tests/Videra.Platform.Linux.Tests/Videra.Platform.Linux.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
        }
        finally {
            if ($hadPrevious) {
                $env:VIDERA_RUN_LINUX_NATIVE_TESTS = $previous
            }
            else {
                Remove-Item Env:VIDERA_RUN_LINUX_NATIVE_TESTS -ErrorAction SilentlyContinue
            }

            if ($hadExpected) {
                $env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER = $previousExpected
            }
            else {
                Remove-Item Env:VIDERA_EXPECT_LINUX_DISPLAY_SERVER -ErrorAction SilentlyContinue
            }
        }
    } "Linux XWayland native validation passed" "Linux XWayland native validation failed"
}

if ($IncludeNativeMacOS) {
    Invoke-Check "macOS Native Validation" {
        dotnet test "$root/tests/Videra.Platform.macOS.Tests/Videra.Platform.macOS.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
    } "macOS native validation passed" "macOS native validation failed"
}

if ($IncludeNativeWindows) {
    Invoke-Check "Windows Native Validation" {
        dotnet test "$root/tests/Videra.Platform.Windows.Tests/Videra.Platform.Windows.Tests.csproj" --configuration $Configuration -v m --logger "console;verbosity=detailed" 2>$null
    } "Windows native validation passed" "Windows native validation failed"
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
