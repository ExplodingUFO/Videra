#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies no snapshot export scope violations exist in the codebase.
.DESCRIPTION
    Searches src/ for reintroduction of old chart view types, direct Source API,
    PDF/vector export code, viewer-level export service coupling from chart code,
    and hidden fallback patterns. Read-only verification only.
#>

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$srcRoot = Join-Path $root "src"
$allPass = $true

function Write-Check([string]$title) {
    Write-Host ""
    Write-Host "--- $title ---" -ForegroundColor Cyan
}

function Write-Pass([string]$message) {
    Write-Host "  [PASS] $message" -ForegroundColor Green
}

function Write-Fail([string]$message, [string[]]$files) {
    Write-Host "  [FAIL] $message" -ForegroundColor Red
    foreach ($file in $files) {
        Write-Host "    -> $file" -ForegroundColor Yellow
    }
    $script:allPass = $false
}

# Check 1: No old chart view types as public class declarations
Write-Check "Old chart view types (SurfaceChartView, WaterfallChartView, ScatterChartView)"
$oldViewPatterns = @("SurfaceChartView", "WaterfallChartView", "ScatterChartView")
$oldViewViolations = @()
foreach ($pattern in $oldViewPatterns) {
    # Search for public class declarations (not test references)
    $matches = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
        Where-Object { $_.FullName -notlike "*\tests\*" -and $_.FullName -notlike "*\test\*" } |
        Select-String -Pattern "public\s+(partial\s+)?class\s+$pattern\b"
    if ($matches) {
        $oldViewViolations += $matches | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
    }
}
if ($oldViewViolations.Count -eq 0) {
    Write-Pass "No old chart view types found as public class declarations"
} else {
    Write-Fail "Old chart view types detected as public class declarations" $oldViewViolations
}

# Check 2: No direct Source property as public API on public chart controls
Write-Check "Direct Source property API"
$sourceViolations = @()
$chartFiles = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
    Where-Object { $_.FullName -notlike "*\tests\*" -and $_.FullName -notlike "*\test\*" -and ($_.FullName -like "*Chart*" -or $_.FullName -like "*Plot*") }
foreach ($file in $chartFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    # Check if file contains a public class (not internal)
    if ($content -match "public\s+(partial\s+)?class\s+") {
        $matches = Select-String -Path $file.FullName -Pattern "public\s+.*\bSource\s*\{.*get.*set"
        if ($matches) {
            $sourceViolations += $matches | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
        }
    }
}
if ($sourceViolations.Count -eq 0) {
    Write-Pass "No direct Source property API found on public chart controls"
} else {
    Write-Fail "Direct Source property API detected on public chart controls" $sourceViolations
}

# Check 3: No PDF or vector export code patterns
Write-Check "PDF/vector export code"
$pdfVectorPatterns = @("PdfExport", "VectorExport", "SvgExport")
$pdfVectorViolations = @()
foreach ($pattern in $pdfVectorPatterns) {
    $matches = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
        Where-Object { $_.FullName -notlike "*\tests\*" -and $_.FullName -notlike "*\test\*" } |
        Select-String -Pattern "public\s+.*$pattern\b.*=\s*(true|enabled|implemented)" -CaseSensitive:$false
    if ($matches) {
        $pdfVectorViolations += $matches | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
    }
    # Also check for class declarations implementing these features
    $classMatches = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
        Where-Object { $_.FullName -notlike "*\tests\*" -and $_.FullName -notlike "*\test\*" } |
        Select-String -Pattern "public\s+(partial\s+)?class\s+.*$pattern"
    if ($classMatches) {
        $pdfVectorViolations += $classMatches | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
    }
}
if ($pdfVectorViolations.Count -eq 0) {
    Write-Pass "No PDF/vector export code patterns found"
} else {
    Write-Fail "PDF/vector export code patterns detected" $pdfVectorViolations
}

# Check 4: No VideraSnapshotExportService references from chart-local code
Write-Check "Viewer-level export service coupling"
$exportServiceViolations = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
    Where-Object { $_.FullName -like "*SurfaceCharts*" -or $_.FullName -like "*Chart*" -or $_.FullName -like "*Plot*" } |
    Select-String -Pattern "VideraSnapshotExportService"
if ($exportServiceViolations) {
    Write-Fail "Chart-local code references viewer-level VideraSnapshotExportService" ($exportServiceViolations | ForEach-Object { "$($_.Path):$($_.LineNumber)" })
} else {
    Write-Pass "No viewer-level export service coupling from chart-local code"
}

# Check 5: No hidden fallback patterns in snapshot paths
Write-Check "Hidden fallback in snapshot paths"
$fallbackViolations = Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
    Select-String -Pattern "FallbackReason\s*=\s*""[^""]+""" |
    Where-Object { $_.Path -like "*Snapshot*" -or $_.Path -like "*Plot*" }
if ($fallbackViolations) {
    Write-Fail "Hidden fallback patterns detected in snapshot paths" ($fallbackViolations | ForEach-Object { "$($_.Path):$($_.LineNumber)" })
} else {
    Write-Pass "No hidden fallback patterns in snapshot paths"
}

# Summary
Write-Host ""
Write-Host "=== Scope Guardrail Summary ===" -ForegroundColor Cyan
if ($allPass) {
    Write-Host "  All scope checks passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "  Some scope violations detected." -ForegroundColor Red
    exit 1
}
