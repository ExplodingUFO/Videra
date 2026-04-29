# Phase 369: Contour Plot Series — Context

## Goal
Users can add contour plot series from 2D scalar field data with configurable iso-lines.

## Key Decisions

1. **API Design**: `Plot.Add.Contour(double[,] values)` adds contour plot from 2D scalar field — follows existing `Plot3DSeriesKind` pattern
2. **Algorithm**: Iso-lines extracted via marching squares algorithm (~100 lines of C#, no external math library needed)
3. **Rendering**: Contour lines render as 3D line geometry in scene space (consistent with Videra's 3D engine model)
4. **Configuration**: Configurable iso-level count (number of contour lines) via parameter
5. **Performance**: Results cached, re-extracted only when data changes (not re-extracted every frame)
6. **Masking**: Contour lines respect `SurfaceMask` — masked regions produce no contour segments
7. **Scope**: Iso-lines only for v2.53 (not filled regions — filled requires triangulation, deferred)

## Architecture

### Data Flow
```
User code → Plot.Add.Contour(double[,]) → ContourChartData (data model)
    → MarchingSquaresExtractor → ContourLine[] (cached)
    → ContourRenderScene → 3D line geometry in scene
```

### Files to Create
- `ContourChartData.cs` — Data model for contour scalar field
- `MarchingSquaresExtractor.cs` — Iso-line extraction algorithm
- `ContourLine.cs` — Individual contour line segment representation
- `ContourRenderScene.cs` — Render-ready contour scene

### Files to Modify
- `Plot3DSeriesKind.cs` — Add `Contour` enum value
- `Plot3DAddApi.cs` — Add `Contour()` method overloads
- `Plot3DSeries.cs` — Add `ContourData` property
- `Plot3D.cs` — Add `ActiveContourSeries` property
- `VideraChartView.Core.cs` — Wire contour rendering status
- `SurfaceScenePainter.cs` — Add contour line drawing

### Dependencies
- Phase 366 (Axis Foundation) — complete
- Existing `SurfaceScalarField`, `SurfaceMask`, `SurfaceValueRange` types

## Requirements Addressed
- CTR-01: `Plot.Add.Contour(double[,] values)` adds contour plot series
- CTR-02: Contour plot extracts iso-lines using marching squares algorithm
- CTR-03: Contour lines render as 3D line geometry in scene space
- CTR-04: Contour plot supports configurable iso-level count
- CTR-05: Contour extraction caches results, re-extracts only when data changes
