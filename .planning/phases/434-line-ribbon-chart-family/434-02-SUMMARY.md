---
phase: 434-line-ribbon-chart-family
plan: 02
subsystem: avalonia
tags: [chart, line, ribbon, avalonia, integration]
depends_on: ["434-01"]
provides: [line-avalonia-ribbon-avalonia]
affects: [plot3d-series-model, legend-overlay, rendering-status, diagnostics]
tech_stack:
  added: []
  patterns: [discriminated-union-constructor, sealed-record-status, composition-merge]
key_files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/LinePlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/RibbonPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/LineChartRenderingStatus.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/RibbonChartRenderingStatus.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/ScatterPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/ContourPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/SurfacePlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/WaterfallPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesComposition.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
decisions:
  - "Used discriminated-union constructor pattern: LineChartData and RibbonChartData slots added to Plot3DSeries base constructor alongside existing null slots"
  - "Legend indicator: Line uses LegendIndicatorKind.Line (blue 0xFF4DA3FF), Ribbon uses LegendIndicatorKind.Swatch (purple 0xFF9B59B6)"
  - "CreateOutputEvidence extended with 6-param overload; existing 4-param overload chains through with nulls for backward compatibility"
  - "RibbonChartRenderingStatus.SegmentCount set to 0 since ribbon segment counting is not yet computed from data"
metrics:
  duration: "not-tracked"
  completed: "2026-04-30"
  tasks_completed: 3
  tasks_total: 3
  files_created: 4
  files_modified: 15
---

# Phase 434 Plan 02: Avalonia Integration for Line/Ribbon Chart Family Summary

Wire Line and Ribbon chart types into the Avalonia integration layer so `Plot.Add.Line(...)` and `Plot.Add.Ribbon(...)` work end-to-end with probe, legend, and diagnostics.

## Tasks Completed

### Task 1: Add Line/Ribbon enum values, series subclasses, and rendering status records
- Added `Line` and `Ribbon` values to `Plot3DSeriesKind` enum
- Extended `Plot3DSeries` constructor with `lineData` and `ribbonData` slots
- Created `LinePlot3DSeries` sealed class with `SetColor`/`SetWidth` mutable update methods
- Created `RibbonPlot3DSeries` sealed class with `SetColor`/`SetRadius` mutable update methods
- Created `LineChartRenderingStatus` sealed record with HasSource, IsReady, BackendKind, IsInteracting, SeriesCount, SegmentCount, ViewSize
- Created `RibbonChartRenderingStatus` sealed record matching LineChartRenderingStatus structure
- Updated all existing series subclass constructors (Bar, Scatter, Contour, Surface, Waterfall) to pass null for new slots
- **Commit:** `5b15ed5`

### Task 2: Wire Plot3DAddApi, Plot3D, and Plot3DSeriesComposition for Line/Ribbon
- Added `Plot3DAddApi.Line(xs, ys, zs, name)` and `Line(LineChartData, name)` convenience overloads
- Added `Plot3DAddApi.Ribbon(xs, ys, zs, radius, name)` and `Ribbon(RibbonChartData, name)` convenience overloads
- Added `ActiveLineData`, `ActiveLineSeries`, `ActiveRibbonData`, `ActiveRibbonSeries` properties to Plot3D
- Added `Plot3DSeriesComposition.CreateLineData` and `CreateRibbonData` composition methods
- Added `CreateScatterMetadata(IReadOnlyList<ScatterChartMetadata>)` overload for metadata merging
- **Commit:** `8285fdd`

### Task 3: Wire legend, rendering status, and diagnostics for Line/Ribbon
- Added Line/Ribbon cases to `SurfaceLegendOverlayPresenter` indicator kind and color switches
- Added `LineRenderingStatus` and `RibbonRenderingStatus` properties to `VideraChartView`
- Added `CreateLineRenderingStatus`/`CreateRibbonRenderingStatus` and update methods
- Added `CreateLineEvidence`/`CreateRibbonEvidence` to `Plot3DDatasetEvidence`
- Added `FromLineStatus`/`FromRibbonStatus` to `Plot3DRenderingEvidence`
- Extended `Plot3D.CreateOutputEvidence` with 6-param overload accepting line/ribbon statuses
- Included Line/Ribbon in `CreateColorMapStatus` NotApplicable branch
- Fixed `SurfaceTooltipOverlayTests` for updated `Plot3DSeries` constructor signature
- **Commit:** `5486280`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed test compilation for updated Plot3DSeries constructor**
- **Found during:** Task 3 build verification
- **Issue:** `SurfaceTooltipOverlayTests.cs` directly constructed `Plot3DSeries` with the old constructor signature missing `lineData` and `ribbonData` parameters
- **Fix:** Added `lineData: null, ribbonData: null` to the constructor call
- **Files modified:** `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceTooltipOverlayTests.cs`
- **Commit:** `5486280`

## Verification

- Build: 0 errors, pre-existing warnings only (both main project and test project)
- All 3 truth criteria from plan verified:
  - `Plot3DSeriesKind` includes `Line` and `Ribbon`
  - `Plot3DAddApi.Line` and `Plot3DAddApi.Ribbon` create and add series
  - `VideraChartView` exposes `LineRenderingStatus` and `RibbonRenderingStatus`
