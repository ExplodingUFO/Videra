# Requirements: v2.66 Complete Cookbook Demo Gallery

**Milestone goal:** Make the SurfaceCharts Demo into a truly complete cookbook
gallery covering all 10 chart types, MultiPlot3D, streaming data, and upgraded
demo framework experience.

## Category 1: Missing Chart Type Recipes

- [ ] **RECIPE-01**: Line chart cookbook recipe with polyline data, configurable
  color/width, and probe integration. Demo shows `Plot.Add.Line(xs, ys, zs)`.
- [ ] **RECIPE-02**: Ribbon chart cookbook recipe with tube/pipe geometry,
  configurable radius, and colormap support. Demo shows `Plot.Add.Ribbon(...)`.
- [ ] **RECIPE-03**: VectorField chart cookbook recipe with arrow rendering,
  magnitude-based coloring, and scale control. Demo shows
  `Plot.Add.VectorField(xs, ys, zs, dxs, dys, dzs)`.
- [ ] **RECIPE-04**: HeatmapSlice chart cookbook recipe with volumetric slice
  visualization, axis/position control, and colormap. Demo shows
  `Plot.Add.HeatmapSlice(values, axis, position)`.
- [ ] **RECIPE-05**: BoxPlot chart cookbook recipe with statistical distribution
  visualization, grouped layout, and probe showing min/Q1/median/Q3/max. Demo
  shows `Plot.Add.BoxPlot(data)`.

## Category 2: MultiPlot3D Demo

- [ ] **MULTI-01**: MultiPlot3D cookbook recipe showing NxM subplot grid
  construction with `new MultiPlot3D(rows, cols)` and independent chart
  assignment per cell.
- [ ] **MULTI-02**: MultiPlot3D linked camera/axis demo showing `LinkAll()`,
  `LinkRow()`, `LinkColumn()` with SurfaceChartLinkPolicy.
- [ ] **MULTI-03**: MultiPlot3D grid snapshot recipe showing
  `CaptureSnapshotAsync()` rendering entire grid as single PNG.

## Category 3: DataLogger3D Streaming Demo

- [ ] **STREAM-01**: SurfaceDataLogger3D streaming demo showing append (new
  rows), replace (full matrix swap), and FIFO (row-cap) semantics with live
  counter display.
- [ ] **STREAM-02**: WaterfallDataLogger3D streaming demo showing the same
  append/replace/FIFO patterns delegated through WaterfallDataLogger3D.
- [ ] **STREAM-03**: BarDataLogger3D streaming demo showing append (new series),
  replace (full data swap), and series count tracking.
- [ ] **STREAM-04**: Unified streaming dashboard showing all three
  DataLogger3D types in a MultiPlot3D grid with per-chart streaming status
  evidence.

## Category 4: Demo Framework Upgrade

- [ ] **FRAMEWORK-01**: Refactor MainWindow code-behind into recipe-driven
  architecture where each recipe is a self-contained class with setup, teardown,
  and UI panel configuration.
- [ ] **FRAMEWORK-02**: Add recipe group navigation (sidebar or tabs) for
  chart type groups: Basics, Analytics, Composition, Streaming.
- [ ] **FRAMEWORK-03**: Add per-recipe parameter controls (sliders, dropdowns)
  for interactive configuration of chart properties (colormap, radius, slice
  position, etc.).
- [ ] **FRAMEWORK-04**: Add live preview panel that updates chart in real-time
  as recipe parameters change.

## Category 5: Cookbook Completeness

- [ ] **COOK-01**: All 10 chart types have at least one runnable cookbook recipe
  with code snippet and live demo.
- [ ] **COOK-02**: MultiPlot3D has at least one cookbook recipe demonstrating
  subplot grid with mixed chart types.
- [ ] **COOK-03**: DataLogger3D streaming has at least one cookbook recipe per
  streaming logger type (Surface, Waterfall, Bar).
- [ ] **COOK-04**: Cookbook recipe catalog updated with all new recipes, properly
  grouped and described.

## Out of Scope

- New chart types beyond the 10 already shipped in v2.65.
- 2D chart families or ScottPlot5 2D compatibility.
- Backend/renderer expansion.
- PDF/vector export.
- Generic plotting platform scope.
- Fake validation evidence.

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| RECIPE-01   | —     | —      |
| RECIPE-02   | —     | —      |
| RECIPE-03   | —     | —      |
| RECIPE-04   | —     | —      |
| RECIPE-05   | —     | —      |
| MULTI-01    | —     | —      |
| MULTI-02    | —     | —      |
| MULTI-03    | —     | —      |
| STREAM-01   | —     | —      |
| STREAM-02   | —     | —      |
| STREAM-03   | —     | —      |
| STREAM-04   | —     | —      |
| FRAMEWORK-01| —     | —      |
| FRAMEWORK-02| —     | —      |
| FRAMEWORK-03| —     | —      |
| FRAMEWORK-04| —     | —      |
| COOK-01     | —     | —      |
| COOK-02     | —     | —      |
| COOK-03     | —     | —      |
| COOK-04     | —     | —      |
