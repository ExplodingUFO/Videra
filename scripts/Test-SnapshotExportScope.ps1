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

function Get-SourceCodeFiles {
    Get-ChildItem -Path $srcRoot -Recurse -Include "*.cs" |
        Where-Object { $_.FullName -notlike "*\tests\*" -and $_.FullName -notlike "*\test\*" }
}

function Get-LineNumber([string]$content, [int]$index) {
    return ([regex]::Matches($content.Substring(0, $index), "`n").Count + 1)
}

function Select-CodeMatches([System.IO.FileInfo]$file, [string]$pattern, [System.Text.RegularExpressions.RegexOptions]$options) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($content, $pattern, $options)) {
        "$($file.FullName):$(Get-LineNumber $content $match.Index)"
    }
}

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
$regexOptions = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor
    [System.Text.RegularExpressions.RegexOptions]::Multiline -bor
    [System.Text.RegularExpressions.RegexOptions]::Singleline
$oldViewPattern = "\bpublic\s+(?:(?:new|partial|sealed|abstract|static|unsafe)\s+)*class\s+(?:SurfaceChartView|WaterfallChartView|ScatterChartView)\b"
$oldViewViolations = Get-SourceCodeFiles |
    ForEach-Object { Select-CodeMatches $_ $oldViewPattern $regexOptions }

if ($oldViewViolations.Count -eq 0) {
    Write-Pass "No old chart view types found as public class declarations"
} else {
    Write-Fail "Old chart view types detected as public class declarations" $oldViewViolations
}

# Check 2: No direct Source property as public API on public chart controls
Write-Check "Direct Source property API"
$sourceViolations = @()
$chartFiles = Get-SourceCodeFiles |
    Where-Object { $_.FullName -like "*SurfaceCharts*" -and ($_.FullName -like "*Chart*" -or $_.FullName -like "*Plot*") }
$chartControlPattern = "\bpublic\s+(?:(?:new|partial|sealed|abstract|static|unsafe)\s+)*class\s+(?:VideraChartView|[A-Za-z_][A-Za-z0-9_]*ChartView)\b"
$sourcePropertyPattern = "\bpublic\s+(?!static\s+readonly\s+StyledProperty\b)[A-Za-z_][A-Za-z0-9_<>,\.\?\[\]\s]*\s+Source\s*\{[^}]*\bget\b[^}]*\bset\b[^}]*\}"
foreach ($file in $chartFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ([regex]::IsMatch($content, $chartControlPattern, $regexOptions)) {
        foreach ($match in [regex]::Matches($content, $sourcePropertyPattern, $regexOptions)) {
            $sourceViolations += "$($file.FullName):$(Get-LineNumber $content $match.Index)"
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
    $matches = Get-SourceCodeFiles |
        Select-String -Pattern "public\s+.*$pattern\b.*=\s*(true|enabled|implemented)" -CaseSensitive:$false
    if ($matches) {
        $pdfVectorViolations += $matches | ForEach-Object { "$($_.Path):$($_.LineNumber)" }
    }
    # Also check for class declarations implementing these features
    $classMatches = Get-SourceCodeFiles |
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
$exportServiceViolations = Get-SourceCodeFiles |
    Where-Object { $_.FullName -like "*SurfaceCharts*" -or $_.FullName -like "*Chart*" -or $_.FullName -like "*Plot*" } |
    Select-String -Pattern "VideraSnapshotExportService"
if ($exportServiceViolations) {
    Write-Fail "Chart-local code references viewer-level VideraSnapshotExportService" ($exportServiceViolations | ForEach-Object { "$($_.Path):$($_.LineNumber)" })
} else {
    Write-Pass "No viewer-level export service coupling from chart-local code"
}

# Check 5: No hidden compatibility, fallback, or downshift paths in chart public API cleanup paths
Write-Check "Hidden fallback/downshift in chart cleanup paths"
$fallbackIdentifierPattern = "\b(?:Fallback|Downshift|Downgrade|Legacy|Compatibility)[A-Za-z0-9_]*(?:Source|Chart|Control|View|Series|Plot|Path)\b|\b(?:Source|Chart|Control|View|Series|Plot|Path)[A-Za-z0-9_]*(?:Fallback|Downshift|Downgrade|Legacy|Compatibility)\b"
$fallbackScopeFiles = Get-SourceCodeFiles |
    Where-Object {
        $_.FullName -like "*Videra.SurfaceCharts.Avalonia\Controls\VideraChartView*" -or
        $_.FullName -like "*Videra.SurfaceCharts.Avalonia\Controls\Plot\*"
    }
$fallbackViolations = $fallbackScopeFiles |
    ForEach-Object { Select-CodeMatches $_ $fallbackIdentifierPattern $regexOptions }
if ($fallbackViolations.Count -gt 0) {
    Write-Fail "Hidden fallback/downshift patterns detected in chart cleanup paths" $fallbackViolations
} else {
    Write-Pass "No hidden fallback/downshift patterns in chart cleanup paths"
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
