---
phase: 368
plan: 368-01
subsystem: surface-charts
tags: [bar-chart, 3d-rendering, plot-add-api]
dependency_graph:
  requires: [Phase 366]
  provides: [BAR-01, BAR-02, BAR-03, BAR-04, BAR-05]
  affects: [Phase 370]
tech_stack:
  added: []
  patterns: [Plot3DSeriesKind, Plot3DAddApi, Renderer, RenderScene]
key_files:
  created:
    - src/Videra.SurfaceCharts.Core/BarSeries.cs
    - src/Videra.SurfaceCharts.Core/BarChartData.cs
    - src/Videra.SurfaceCharts.Core/Rendering/BarRenderBar.cs
    - src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs
    - src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/BarRendererTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
decisions:
  - Bar layout uses 80% category width with epsilon gap for z-fighting prevention
  - Bar depth is 60% of bar width for 3D appearance
  - Default bar color is 0xFF4488CC (steel blue)
  - Bars normalize heights to max value for visual consistency
  - Stacked bars accumulate values and normalize to max stacked total
metrics:
  duration_seconds: 1155
  completed: "2026-04-29T11:42:09Z"
  tasks_completed: 9
  tasks_total: 9
---

# Phase 368: Bar Chart Series — Summary

3D bar chart rendering via Plot.Add.Bar() with grouped/stacked layouts, per-series color, and full validation pipeline following the established Plot3DSeriesKind pattern.

## What Was Built

### Core Data Model
- **BarSeries** — Immutable bar series with values, color, label. Validates non-empty, no NaN.
- **BarChartData** — Container for one or more BarSeries with layout mode (Grouped/Stacked). Validates matching category counts.
- **BarChartLayout** — Enum: Grouped (side-by-side) and Stacked (vertical accumulation).

### Rendering Pipeline
- **BarRenderBar** — Record struct with Position (Vector3), Size (Vector3), Color (uint).
- **BarRenderScene** — Immutable container for render-ready bars with metadata.
- **BarRenderer** — Static builder that produces BarRenderScene from BarChartData:
  - Grouped: bars offset horizontally per series within each category
  - Stacked: bars accumulate vertically per category
  - Bar width = 80% of category spacing with epsilon gap for z-fighting prevention
  - Bar depth = 60% of bar width for 3D appearance

### API Surface
- **Plot3DSeriesKind.Bar** — New enum value for bar chart series.
- **Plot3DAddApi.Bar(double[] values)** — Adds bar series from value array.
- **Plot3DAddApi.Bar(BarChartData data)** — Adds bar series from full data model.
- **Plot3DSeries.BarData** — Property exposing bar data.
- **Plot3D.ActiveBarSeries** — Helper for bar series detection.

### Integration
- **VideraChartView.BarRenderingStatus** — Bar chart rendering diagnostics.
- **BarChartRenderingStatus** — Record with series count, category count, bar count, layout.
- **Plot3DDatasetEvidence** — Handles Bar kind with sampling profile.
- Color map status returns NotApplicable for Bar kind (solid colors, not color maps).

### Tests
- 19 tests covering BAR-01 through BAR-05:
  - BarSeries validation (empty, NaN, immutability)
  - BarChartData (empty series, mismatched categories, layouts)
  - BarRenderer (grouped, stacked, proportional heights, width gaps)
  - Plot3DAddApi.Bar() integration

## Requirements Satisfied

| Requirement | Status | Evidence |
|-------------|--------|----------|
| BAR-01 | ✅ | `Plot.Add.Bar(double[] values)` adds vertical bar chart series |
| BAR-02 | ✅ | Bars render as 3D rectangular prisms with configurable color |
| BAR-03 | ✅ | Grouped layout positions series side-by-side |
| BAR-04 | ✅ | Stacked layout accumulates values vertically |
| BAR-05 | ✅ | Empty arrays and NaN values rejected with ArgumentException |

## Commits

| Hash | Message |
|------|---------|
| eeab02d | feat(368-01): add Bar enum value to Plot3DSeriesKind |
| 742ff4f | feat(368-01): add BarSeries data model |
| 9ee8a8d | feat(368-01): add BarChartData container with layout enum |
| d03e3de | feat(368-01): add BarRenderBar and BarRenderScene render types |
| 619a26a | feat(368-01): add BarRenderer with grouped and stacked layout |
| 7ccd578 | feat(368-01): add Plot3DAddApi.Bar() and Plot3DSeries.BarData |
| 649d4d7 | feat(368-01): wire bar chart into Plot3D and VideraChartView |
| fcb0f99 | feat(368-01): update Plot3DDatasetEvidence for bar chart series |
| 0b8e479 | test(368-01): add comprehensive bar chart tests |

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None — all bar chart functionality is fully wired and tested.

## Self-Check: PASSED
