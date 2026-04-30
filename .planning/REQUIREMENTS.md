# Requirements: v2.65 3D ScottPlot5 Analytics Chart Expansion

**Milestone:** v2.65
**Scope:** Expand Videra's 3D chart vocabulary toward ScottPlot5 parity with
analytics-focused chart types, MultiPlot3D subplot grids, and universal
streaming — while preserving the single `VideraChartView` + `Plot.Add.*` route
and no-compatibility/no-fallback boundary.

## Validated Requirements

### Line/Ribbon Chart Family (LINE-01..04)

**REQ-LINE-01:** Users can add 3D line plots via `Plot.Add.Line(xs, ys, zs, label)`.
Line plots render as connected 3D polylines with configurable color, width, and
marker options.

**REQ-LINE-02:** Users can add ribbon/pipe plots via
`Plot.Add.Ribbon(xs, ys, zs, radius, label)`. Ribbon plots render as tube/pipe
geometry around the polyline path with configurable cross-section.

**REQ-LINE-03:** Line and ribbon plots support per-segment color mapping via
colormap + value array, enabling trajectory coloring by time, speed, or other
scalar.

**REQ-LINE-04:** Line and ribbon plots participate in the existing probe,
selection, and overlay infrastructure (crosshair, tooltip, legend).

### Vector Field Chart Family (VEC-01..04)

**REQ-VEC-01:** Users can add 3D vector fields via
`Plot.Add.VectorField(xs, ys, zs, dxs, dys, dzs, label)`. Vector fields render
as 3D arrows at each grid point showing direction and magnitude.

**REQ-VEC-02:** Arrow length is proportional to vector magnitude, with
configurable scaling factor. Arrow color can be mapped by magnitude via colormap.

**REQ-VEC-03:** Vector fields support both regular-grid and irregular-point
layouts. For regular grids, the grid spacing is inferred from the coordinate
arrays.

**REQ-VEC-04:** Vector fields participate in the existing probe infrastructure
— probing shows the vector value (dx, dy, dz) and magnitude at the probed point.

### 3D Heatmap Slice Chart Family (HEAT-01..04)

**REQ-HEAT-01:** Users can add 3D heatmap slices via
`Plot.Add.HeatmapSlice(values, axis, position, label)`. Heatmap slices render
as a colored plane at the specified position along the specified axis (X, Y, or Z).

**REQ-HEAT-02:** Heatmap slices support configurable colormap, value range, and
opacity. Multiple slices can be composed to show volumetric data.

**REQ-HEAT-03:** Users can interactively move the slice position via a slider or
drag gesture, enabling "slicing through" volumetric data.

**REQ-HEAT-04:** Heatmap slices participate in the probe infrastructure —
probing shows the interpolated value at the probed point on the slice.

### Box Plot Chart Family (BOX-01..03)

**REQ-BOX-01:** Users can add 3D box plots via
`Plot.Add.BoxPlot(data, label)` where `data` contains min, Q1, median, Q3, max,
and optional outliers for each category. Box plots render as 3D rectangular prisms
with whiskers.

**REQ-BOX-02:** Box plots support grouped/clustered layout for comparing
distributions across categories. Each box can have a distinct color.

**REQ-BOX-03:** Box plots participate in the probe infrastructure — probing
shows the statistical summary (min, Q1, median, Q3, max) for the probed box.

### Promote Bar+Contour to Production (PROMO-01..03)

**REQ-PROMO-01:** `Plot.Add.Bar(...)` moves from proof-path to the public
package API contract. The Bar chart family is included in the public NuGet
package surface.

**REQ-PROMO-02:** `Plot.Add.Contour(...)` moves from proof-path to the public
package API contract. The Contour chart family is included in the public NuGet
package surface.

**REQ-PROMO-03:** Existing Bar and Contour cookbook recipes, demo scenarios, and
tests continue to pass without modification after promotion.

### MultiPlot3D Subplot Grid (MULTI-01..05)

**REQ-MULTI-01:** Users can create a `MultiPlot3D(rows, cols)` container that
arranges multiple `VideraChartView` instances in a grid layout.

**REQ-MULTI-02:** Each cell in the grid can hold an independent chart with its
own data, chart type, axes, and styling.

**REQ-MULTI-03:** Users can optionally share camera/axis state across selected
rows, columns, or the entire grid via link groups.

**REQ-MULTI-04:** `MultiPlot3D` provides a `CaptureSnapshotAsync()` method that
renders the entire grid as a single PNG image.

**REQ-MULTI-05:** The demo includes a MultiPlot3D scenario showing a 2×2 grid
with different chart types.

### Extended DataLogger3D Streaming (STREAM-01..04)

**REQ-STREAM-01:** `DataLogger3D` pattern extends to Surface charts —
`Plot.Add.Surface(logger, label)` where logger provides append/replace/FIFO
semantics for surface matrix data.

**REQ-STREAM-02:** `DataLogger3D` pattern extends to Waterfall charts —
`Plot.Add.Waterfall(logger, label)` with row-level append/FIFO.

**REQ-STREAM-03:** `DataLogger3D` pattern extends to Bar charts —
`Plot.Add.Bar(logger, label)` with category-level append/replace.

**REQ-STREAM-04:** Extended streaming charts participate in workspace streaming
status tracking from Phase 428.

### Cookbook and Demo (COOK-01..04)

**REQ-COOK-01:** Cookbook recipes cover all new chart types (Line, Ribbon, Vector
Field, Heatmap Slice, Box Plot) with copyable code snippets.

**REQ-COOK-02:** Cookbook recipes cover MultiPlot3D with shared-axis examples.

**REQ-COOK-03:** Cookbook recipes cover extended streaming (Surface/Waterfall/Bar
DataLogger3D).

**REQ-COOK-04:** Demo scenarios include all new chart types and MultiPlot3D.

### CI and Release Readiness (TRUTH-01..04)

**REQ-TRUTH-01:** CI truth tests cover new chart type test filters.

**REQ-TRUTH-02:** Release-readiness tests verify Bar+Contour are in the public
package contract after promotion.

**REQ-TRUTH-03:** Generated roadmap reflects v2.65 phases.

**REQ-TRUTH-04:** Scope guardrails reject: 2D chart family, generic plotting
platform, renderer/backend expansion, compatibility layers, hidden fallback.

## Deferred

- 2D chart family (ScottPlot5 is 2D; Videra is 3D-native)
- Animation/skeletal/morph
- Shadows/environment maps on chart geometry
- Wayland-native rendering
- OpenGL/WebGL backends
- PDF/vector export (bitmap-only)
- Generic plugin/workbench architecture
- Public package publication (stays repo-only alpha)

---

*Requirements gathered: 2026-04-30*
*Milestone: v2.65 3D ScottPlot5 Analytics Chart Expansion*
