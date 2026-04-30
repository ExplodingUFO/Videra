---
phase: 434-line-ribbon-chart-family
plan: 03
subsystem: surfacecharts-core
tags: [colormap, per-point-color, line-chart, ribbon-chart, scatter-point-color]

# Dependency graph
requires:
  - phase: 434-line-ribbon-chart-family
    plan: 01
    provides: LineRenderer/RibbonRenderer with segment builders and ScatterPoint data model
  - phase: 434-line-ribbon-chart-family
    plan: 02
    provides: LinePlot3DSeries/RibbonPlot3DSeries Avalonia integration with ReplaceLineData/ReplaceRibbonData
provides:
  - Per-point color support in LineRenderer (start.Color ?? series.Color)
  - Per-point color support in RibbonRenderer (start.Color ?? series.Color)
  - SetColormap(SurfaceColorMap) on LinePlot3DSeries for value-mapped per-point coloring
  - SetColormap(SurfaceColorMap) on RibbonPlot3DSeries for value-mapped per-point coloring
  - Plot3DColorMapStatus correctly reports Applied/Unavailable for Line/Ribbon (not NotApplicable)
affects:
  - 434-04 (marker options build on per-point color infrastructure)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Per-point color override pattern: point.Color ?? series.Color (matching ScatterRenderer)"
    - "SetColormap pattern: rebuild series with new ScatterPoint array carrying colorMap.Map(value) per point"

key-files:
  created: []
  modified:
    - src/Videra.SurfaceCharts.Core/Rendering/LineRenderer.cs
    - src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderer.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/LinePlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/RibbonPlot3DSeries.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs

key-decisions:
  - "Segment color uses start.Color (not end.Color) -- each segment starts at point i, matching ScatterRenderer convention"
  - "SetColormap rebuilds entire series data via ReplaceLineData/ReplaceRibbonData to maintain immutability"
  - "Plot3DColorMapStatus returns Unavailable (not NotApplicable) for Line/Ribbon when no colormap is set, indicating colormap support exists"

patterns-established:
  - "Per-point color override: ScatterPoint.Color nullable uint? takes precedence over series.Color when present"
  - "Colormap application: SetColormap iterates points, maps value through SurfaceColorMap.Map(), creates new ScatterPoint with color"

requirements-completed: [LINE-03]

# Metrics
duration: 10min
completed: 2026-04-30
---

# Phase 434 Plan 03: Per-Segment Colormap Coloring Summary

Closed the LINE-03 gap: per-segment colormap coloring now works for both Line and Ribbon chart types.

## What Changed

### Task 1: Per-Point Color in Renderers

LineRenderer and RibbonRenderer now respect ScatterPoint.Color when present, falling back to series.Color. This matches the established ScatterRenderer pattern (line 46: point.Color ?? series.Color).

**LineRenderer.cs** -- Changed segment creation from  to 

**RibbonRenderer.cs** -- Same change: 

The segment from point[i] to point[i+1] uses point[i]'s color, consistent with how ScatterRenderer assigns per-point color.

### Task 2: SetColormap API and Status Logic

**LinePlot3DSeries.SetColormap(SurfaceColorMap)** -- Iterates all points in all series, maps each point's value through colorMap.Map(pt.Value), creates new ScatterPoint with the mapped color, and rebuilds the LineChartData via ReplaceLineData.

**RibbonPlot3DSeries.SetColormap(SurfaceColorMap)** -- Same pattern for ribbon series, rebuilding via ReplaceRibbonData.

**Plot3D.CreateColorMapStatus** -- Removed Line and Ribbon from the NotApplicable branch. When the active series is Line or Ribbon and no colormap evidence exists, status returns Unavailable (meaning "should use a colormap but none is applied") instead of NotApplicable (meaning "doesn't use a colormap at all"). When colormap evidence is present, status returns Applied.

## Deviations from Plan

None -- plan executed exactly as written.

## TDD Gate Compliance

Not applicable -- plan type is execute, not tdd.

## Known Stubs

None -- all implementations are fully wired with real data paths.

## Self-Check: PASSED
