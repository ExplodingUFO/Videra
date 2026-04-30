# Roadmap: v2.65 3D ScottPlot5 Analytics Chart Expansion

**Goal:** Expand Videra's 3D chart vocabulary toward ScottPlot5 parity with
analytics-focused chart types (Line/Ribbon, Vector Field, Heatmap Slice, Box
Plot), MultiPlot3D subplot grids, universal streaming, and promote Bar+Contour
from proof to production — while preserving the single `VideraChartView` +
`Plot.Add.*` route.

**Boundary:** This milestone does not add 2D chart families, compatibility
adapters, generic plotting platform scope, renderer/backend expansion, hidden
fallback/downshift behavior, PDF/vector export, or fake validation evidence.

**Phases:** 8 (432-439)
**Requirements:** 30 (LINE-01..04, VEC-01..04, HEAT-01..04, BOX-01..03,
PROMO-01..03, MULTI-01..05, STREAM-01..04, COOK-01..04, TRUTH-01..04)
**Beads epic:** `Videra-8krt`

## Phases

- [ ] **Phase 432: Chart Type Inventory and API Design** - Map current chart
  type surface, API patterns, and rendering seams before adding new chart
  families. Design the API contracts for Line, Ribbon, Vector Field, Heatmap
  Slice, and Box Plot.
- [ ] **Phase 433: Promote Bar+Contour to Production** - Move Bar and Contour
  chart families from proof-path to the public package API contract.
- [ ] **Phase 434: Line/Ribbon Chart Family** - Add 3D line plots and
  ribbon/pipe plots with colormap support and probe integration.
- [ ] **Phase 435: Vector Field Chart Family** - Add 3D vector field plots with
  arrow rendering, magnitude scaling, and probe integration.
- [ ] **Phase 436: 3D Heatmap Slice Chart Family** - Add heatmap slice plots
  for volumetric data visualization with interactive slice positioning.
- [ ] **Phase 437: Box Plot Chart Family** - Add 3D box plots for statistical
  distribution visualization with grouped layout.
- [ ] **Phase 438: MultiPlot3D Subplot Grid** - Add MultiPlot3D container for
  N×M subplot arrangements with optional shared camera/axis.
- [ ] **Phase 439: Extended DataLogger3D and v2.65 Verification** - Extend
  DataLogger3D streaming to Surface/Waterfall/Bar, add cookbook recipes, CI
  truth, and close v2.65 with verification.

## Phase Details

### Phase 432: Chart Type Inventory and API Design

**Goal:** Map the current chart type surface, API patterns, rendering seams,
and probe/overlay infrastructure before adding new chart families.
**Depends on:** Nothing
**Requirements:** (inventory phase — no specific REQ)
**Success Criteria:**
1. Current chart type API surface (Plot.Add.*, IPlottable3D, Plot3DSeriesKind)
   is mapped.
2. Rendering seams for each chart kind (kernel, geometry, overlay) are
   identified.
3. Probe/selection/overlay infrastructure integration points for new chart types
   are documented.
4. API contracts for Line, Ribbon, Vector Field, Heatmap Slice, and Box Plot
   are designed with type signatures and data models.


**Plans:** 1 plan
Plans:
- [x] 432-01-PLAN.md — Inventory current chart type API surface and design API contracts for new chart families

### Phase 433: Promote Bar+Contour to Production

**Goal:** Move Bar and Contour chart families from proof-path to the public
package API contract.
**Depends on:** Phase 432
**Requirements:** PROMO-01, PROMO-02, PROMO-03
**Success Criteria:**
1. `Plot.Add.Bar(...)` is in the public API contract and NuGet package surface.
2. `Plot.Add.Contour(...)` is in the public API contract and NuGet package
   surface.
3. Existing Bar and Contour tests, demo scenarios, and cookbook recipes pass
   without modification.
4. Package validation scripts confirm Bar+Contour are in the public surface.

**Plans:** 1 plan
Plans:
- [ ] 433-01-PLAN.md — Add BarPlot3DSeries and ContourPlot3DSeries to public API contract and verify existing tests pass

### Phase 434: Line/Ribbon Chart Family

**Goal:** Add 3D line plots and ribbon/pipe plots with colormap support.
**Depends on:** Phase 433
**Requirements:** LINE-01, LINE-02, LINE-03, LINE-04
**Success Criteria:**
1. `Plot.Add.Line(xs, ys, zs, label)` renders 3D polyline with configurable
   color, width, and markers.
2. `Plot.Add.Ribbon(xs, ys, zs, radius, label)` renders tube/pipe geometry.
3. Per-segment colormap coloring works for both line and ribbon.
4. Line/ribbon plots participate in probe, selection, overlay, and legend.

### Phase 435: Vector Field Chart Family

**Goal:** Add 3D vector field plots with arrow rendering.
**Depends on:** Phase 433
**Requirements:** VEC-01, VEC-02, VEC-03, VEC-04
**Success Criteria:**
1. `Plot.Add.VectorField(xs, ys, zs, dxs, dys, dzs, label)` renders 3D arrows.
2. Arrow length proportional to magnitude with configurable scaling.
3. Colormap coloring by magnitude works.
4. Probe shows vector value and magnitude at probed point.

### Phase 436: 3D Heatmap Slice Chart Family

**Goal:** Add heatmap slice plots for volumetric data visualization.
**Depends on:** Phase 433
**Requirements:** HEAT-01, HEAT-02, HEAT-03, HEAT-04
**Success Criteria:**
1. `Plot.Add.HeatmapSlice(values, axis, position, label)` renders colored plane.
2. Configurable colormap, value range, and opacity work.
3. Interactive slice positioning (slider/drag) works.
4. Probe shows interpolated value on slice.

### Phase 437: Box Plot Chart Family

**Goal:** Add 3D box plots for statistical distribution visualization.
**Depends on:** Phase 433
**Requirements:** BOX-01, BOX-02, BOX-03
**Success Criteria:**
1. `Plot.Add.BoxPlot(data, label)` renders 3D box with whiskers.
2. Grouped/clustered layout works for multi-category comparison.
3. Probe shows statistical summary (min, Q1, median, Q3, max).

### Phase 438: MultiPlot3D Subplot Grid

**Goal:** Add MultiPlot3D container for N×M subplot arrangements.
**Depends on:** Phase 437
**Requirements:** MULTI-01, MULTI-02, MULTI-03, MULTI-04, MULTI-05
**Success Criteria:**
1. `MultiPlot3D(rows, cols)` creates grid layout with independent charts.
2. Optional shared camera/axis via link groups works.
3. `CaptureSnapshotAsync()` renders entire grid as single PNG.
4. Demo shows 2×2 grid with different chart types.

### Phase 439: Extended DataLogger3D and v2.65 Verification

**Goal:** Extend streaming to all chart types, add cookbook recipes, and close
v2.65 with verification.
**Depends on:** Phase 438
**Requirements:** STREAM-01..04, COOK-01..04, TRUTH-01..04
**Success Criteria:**
1. DataLogger3D pattern works for Surface, Waterfall, and Bar charts.
2. Cookbook recipes cover all new chart types, MultiPlot3D, and streaming.
3. CI truth tests cover new chart type filters.
4. Release-readiness confirms Bar+Contour in public package contract.
5. All tests pass, beads closed, roadmap synced, git pushed.

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 432. Chart Type Inventory and API Design | 1/1 | Complete    | 2026-04-30 |
| 433. Promote Bar+Contour to Production | 0/1 | Ready | |
| 434. Line/Ribbon Chart Family | 0/1 | Blocked | |
| 435. Vector Field Chart Family | 0/1 | Blocked | |
| 436. 3D Heatmap Slice Chart Family | 0/1 | Blocked | |
| 437. Box Plot Chart Family | 0/1 | Blocked | |
| 438. MultiPlot3D Subplot Grid | 0/1 | Blocked | |
| 439. Extended DataLogger3D and v2.65 Verification | 0/1 | Blocked | |

---

<details>
<summary>v2.64 Native SurfaceCharts Analysis Workspace and Streaming Evidence - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.64-phases`

v2.64 added native multi-chart analysis workspace, linked interaction with
CameraOnly/AxisOnly policies, interaction propagation, streaming evidence
tracking, cookbook recipes, CI truth tests, and release-readiness guardrails
while preserving the single native `VideraChartView` + `Plot.Add.*` route.

</details>
