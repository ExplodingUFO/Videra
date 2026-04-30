# Phase 434: Line/Ribbon Chart Family - Context

**Gathered:** 2026-04-30
**Status:** Ready for planning

<domain>
## Phase Boundary

Add 3D line plots and ribbon/pipe plots with colormap support and probe
integration. Line plots render as connected 3D polylines. Ribbon plots render
as tube/pipe geometry around the polyline path. Both support per-segment
colormap coloring and participate in probe, selection, overlay, and legend.

</domain>

<decisions>
## Implementation Decisions

### API Contracts (locked from Phase 432)
- `Plot.Add.Line(xs, ys, zs, label)` — 3D polyline with configurable color, width, markers
- `Plot.Add.Ribbon(xs, ys, zs, radius, label)` — tube/pipe geometry with configurable cross-section
- Per-segment colormap coloring via colormap + value array
- Both participate in probe, selection, overlay, and legend infrastructure

### Data Models (locked from Phase 432 API-DESIGN.md)
- `LineChartData`: sealed class, ReadOnlyCollection arrays, LineChartPoint struct
- `RibbonChartData`: sealed class, ReadOnlyCollection arrays, radius parameter
- Both use `Array.AsReadOnly()` pattern for immutability

### Rendering (locked from Phase 432 API-DESIGN.md)
- `LineRenderer` / `RibbonRenderer`: public static class with `BuildScene(ChartData data)`
- `LineRenderScene` / `RibbonRenderScene`: readonly record struct with RenderSegments
- `LineRenderSegment` / `RibbonRenderSegment`: readonly record struct with Start/End positions
- Ribbon uses tube geometry (circle cross-section extruded along polyline)

### Probe (locked from Phase 432 API-DESIGN.md)
- `LineProbeStrategy` / `RibbonProbeStrategy`: implement `ISeriesProbeStrategy`
- `TryResolve` returns `SurfaceProbeInfo?` with nearest point on line/ribbon
- Both use standard `SurfaceProbeInfo` (no extension needed)

### Series (locked from Phase 432 API-DESIGN.md)
- `LinePlot3DSeries` / `RibbonPlot3DSeries`: sealed class extending `Plot3DSeries`
- Internal constructor with discriminated union data slots (per-kind, not generic)
- `LineChartRenderingStatus` / `RibbonChartRenderingStatus`: sealed record

### Integration Pattern (from Phase 432 INVENTORY.md)
Each chart type follows a 7-step integration pattern:
1. Add enum value to `Plot3DSeriesKind`
2. Create data model in Core
3. Create renderer in Core/Rendering
4. Create render scene in Core/Rendering
5. Create Plot3DSeries subclass in Avalonia/Controls/Plot
6. Add method to `Plot3DAddApi`
7. Create probe strategy in Core/Picking

</decisions>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Plot3DSeries` base class — all chart types extend this
- `ISeriesProbeStrategy` interface — probe strategies implement this
- `SurfaceProbeInfo` — standard probe info record
- `SeriesProbeStrategyDispatcher` — maps Plot3DSeriesKind to probe strategy
- `SurfaceLegendOverlayPresenter` — handles kind-specific legend indicators
- `Plot3DAddApi` — single authoring entry point for all chart types

### Established Patterns
- Immutable data models: sealed class + ReadOnlyCollection + Array.AsReadOnly()
- Renderers: public static class with BuildScene() entry point
- Render scenes: readonly record struct with element collections
- Render elements: readonly record struct with position/color data
- Probe strategies: sealed class implementing ISeriesProbeStrategy
- Series subclasses: sealed class with internal constructor
- Rendering status: sealed record with init auto-properties

### Integration Points
- `Plot3DSeriesKind` enum — add Line, Ribbon values
- `Plot3DAddApi` — add Line(), Ribbon() methods
- `SeriesProbeStrategyDispatcher` — register Line, Ribbon probe strategies
- `SurfaceLegendOverlayPresenter` — add Line, Ribbon legend indicators
- `SurfaceProbeOverlayPresenter` — add Line, Ribbon probe overloads
- NuGet package surface — add new types to public API contract

</code_context>

<specifics>
## Specific Ideas

Refer to `.planning/phases/432-chart-type-inventory-and-api-design/432-API-DESIGN.md`
for complete type signatures, data models, and implementation details.

Key implementation notes from API design:
- LineRenderSegment uses Point3F Start/End + float Width + Color4 Color
- RibbonRenderSegment uses Point3F Start/End + float Radius + int Sides + Color4 Color
- Ribbon tube geometry: 8-sided circle cross-section (configurable via Sides)
- Both support per-segment coloring via IReadOnlyList<Color4>? SegmentColors

</specifics>

<deferred>
## Deferred Ideas

None — all design decisions locked from Phase 432.

</deferred>
