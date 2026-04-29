---
phase: 370-integration-and-evidence
plan: 02
subsystem: Demo/Smoke
tags: [demo, smoke, bar, contour, integration]
depends_on:
  requires: [370-01]
  provides: [demo-bar-contour, smoke-bar-contour]
  affects: []
tech_stack:
  added: []
  patterns: [source-selector, sample-data-factory]
key_files:
  created: []
  modified:
    - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml
    - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
    - smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs
    - .planning/REQUIREMENTS.md
decisions:
  - Bar uses ActiveSurfaceFamilyChartView path (has ViewState)
  - Contour gets its own support summary section (no ViewState)
metrics:
  duration: ~10m
  completed: 2026-04-29
---

# Phase 370 Plan 02: Demo and Smoke Integration Summary

Bar and Contour chart types added to demo app with sample data and UI, consumer smoke extended with multi-chart-family validation, and all INT/VER requirements marked complete.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Add Bar/Contour to demo | 3f77e79 | MainWindow.axaml, MainWindow.axaml.cs |
| 2 | Add Bar/Contour to smoke + requirements | 6a33974, 9fa9c99 | MainWindow.axaml.cs, REQUIREMENTS.md |

## Key Changes

### Demo MainWindow.axaml
- Added `BarChartPlotView` and `ContourPlotView` VideraChartView elements
- Added "Try next: Bar chart proof" and "Try next: Contour plot proof" to SourceSelector

### Demo MainWindow.axaml.cs
- Added `BarSourceIndex=5` and `ContourSourceIndex=6` constants
- Added `_barChartView` and `_contourPlotView` fields
- Added `ApplyBarSource()` and `ApplyContourSource()` methods
- Added `CreateSampleBarData()` — 3 series × 5 categories grouped bar chart
- Added `CreateSampleContourField()` — 32×32 radial scalar field
- Updated `SetActiveChartView`, `ActiveSurfaceFamilyChartView`, `IsContourProofActive`
- Updated all rendering/diagnostics/status/support summary text methods for Bar/Contour
- Updated `CreateOutputEvidence` to pass bar/contour rendering status
- Updated `CreateDatasetActiveSeriesMetadataSummary` for Bar/Contour kinds

### Smoke MainWindow.axaml.cs
- Added `AddBarSeries()` — 2 series × 4 categories
- Added `AddContourSeries()` — 16×16 radial field
- Updated `ConfigureChart` to add Bar and Contour after Surface
- Updated `CreateSupportSummary` to report BarRenderingStatus and ContourRenderingStatus
- Updated `CreateOutputEvidence` to pass bar/contour rendering status
- Updated `CreateDatasetActiveSeriesMetadataSummary` for Bar/Contour kinds

### REQUIREMENTS.md
- Marked INT-01 through INT-04 as complete
- Marked VER-01 through VER-03 as complete
- Updated traceability table

## Deviations from Plan

None — plan executed exactly as written.

## Verification

- `dotnet build samples/Videra.SurfaceCharts.Demo/Videra.SurfaceCharts.Demo.csproj --no-restore` — 0 errors
- `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore` — 0 errors
- `dotnet test --filter "Plot3D_CreateOutputEvidence|RegressionGuardrail|PlotAddContour|PlotAddBar"` — 15/15 pass
- Smoke project cannot be built without published NuGet packages (by design — consumer smoke validates packaged library)

## Known Stubs

None — all data sources are wired to real sample data.

## Self-Check: PENDING
