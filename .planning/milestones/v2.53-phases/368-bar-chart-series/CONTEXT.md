# Phase 368: Bar Chart Series — Context

## Goal
Users can add 3D bar chart series to plots with grouped and stacked layouts.

## Key Decisions

### API Design
- `Plot.Add.Bar(double[] values)` adds vertical bar chart series — simplest entry point
- `Plot.Add.Bar(BarChartData data)` adds bar chart from full data model (for grouped/stacked)
- Follows established `Plot3DSeriesKind` → `Plot3DAddApi` → renderer → overlay pattern exactly

### Data Model
- `BarChartData` — immutable container for bar series (mirrors `ScatterChartData` pattern)
- `BarSeries` — one bar series with values, color, label
- Each bar value maps to a category index (0-based) on the horizontal axis
- Bar values are `double[]` — validates no NaN, no empty arrays

### Rendering
- Bars render as axis-aligned quads (rectangular prisms in 3D scene space)
- Base at Y=0, top at Y=value — vertical bars rising from base plane
- Bar width = fraction of category spacing (0.8 default)
- Grouped bars: offset series horizontally within each category
- Stacked bars: accumulate values vertically per category
- Small gap between bars to prevent z-fighting (epsilon offset)

### Color
- Per-series configurable ARGB color (same pattern as `ScatterSeries.Color`)
- No color map dependency — bars use solid colors

### Integration Points
- `Plot3DSeriesKind.Bar` — new enum value
- `Plot3DAddApi.Bar()` — new method
- `Plot3DSeries` — add `BarData` property
- `Plot3D` — add `ActiveBarSeries` helper
- `SurfaceScenePainter` — render bar quads alongside surface triangles
- `Plot3DDatasetEvidence` / `Plot3DOutputEvidence` — handle Bar kind
- `BarChartRenderingStatus` — new rendering status type

## Files to Create
- `src/Videra.SurfaceCharts.Core/BarChartData.cs`
- `src/Videra.SurfaceCharts.Core/BarSeries.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/BarRenderBar.cs`
- `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/Rendering/BarRendererTests.cs`

## Files to Modify
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceScenePainter.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`

## Requirements Addressed
- BAR-01: `Plot.Add.Bar(double[] values)` adds vertical bar chart series
- BAR-02: Bar chart renders as 3D rectangular prisms with configurable color
- BAR-03: Bar chart supports grouped bars (multiple series side by side)
- BAR-04: Bar chart supports stacked bars (multiple series stacked vertically)
- BAR-05: Bar chart validates data (rejects empty arrays, NaN values)
