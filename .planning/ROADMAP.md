# Roadmap: v2.67 ScottPlot5 Feature Parity — Charts, Axes, Annotations, Export

**Goal:** Push Videra's 3D chart module toward ScottPlot5 feature parity in chart
type breadth, axis flexibility, annotation/overlay richness, export capabilities,
and demo framework quality.

**Boundary:** This milestone does not add 2D-only chart families (2D heatmap,
polar axis, radar, smith chart), backend/renderer expansion beyond D3D11/Vulkan/Metal,
compatibility adapters, or fake validation evidence.

**Phases:** 12 (447-458)
**Requirements:** 30 (CHART-01..07, AXIS-01..06, ANNO-01..05, EXPORT-01..03, DEMO-01..04, COOK-05..08)
**Beads epic:** TBD

## Phases

- [ ] **Phase 447: Histogram & Function Plot** - Add Histogram and Function
  chart types with data model, renderer, probe strategy, and Plot3D API.
- [ ] **Phase 448: Pie/Donut Chart** - Add Pie/Donut chart type with slice
  rendering, labels, explode offset, and donut hole ratio.
- [ ] **Phase 449: Error Bar Overlay** - Add Error Bar overlay on scatter plots
  with symmetric/asymmetric X/Y error bars.
- [ ] **Phase 450: OHLC/Candlestick Chart** - Add OHLC/Candlestick chart type
  for financial data with up/down colors and wick rendering.
- [ ] **Phase 451: Violin & Polygon Chart** - Add Violin plot (KDE) and Polygon
  chart types with configurable rendering.
- [ ] **Phase 452: Axis System Upgrade** - Add DateTime axis, log scale, tick
  formatting, minor ticks, axis inversion, and multiple Y axes.
- [ ] **Phase 453: Text & Arrow Annotations** - Add text annotation and arrow
  annotation anchored to data coordinates.
- [ ] **Phase 454: Reference Lines, Spans & Shapes** - Add reference lines,
  reference regions, and shape annotations.
- [ ] **Phase 455: SVG & Batch Export** - Add SVG export and batch export API
  with JSON manifest for CI integration.
- [ ] **Phase 456: Demo Framework Refactor** - Refactor MainWindow into
  recipe-driven architecture with IChartRecipe, group navigation, and auto-generated
  parameter panels.
- [ ] **Phase 457: Cookbook Recipes for New Features** - Add cookbook recipes for
  all new chart types, axis features, annotations, and export.
- [ ] **Phase 458: Verification & Milestone Closure** - Final build verification,
  all requirements check, planning docs update, milestone closure.

## Phase Details

### Phase 447: Histogram & Function Plot

**Goal:** Add Histogram and Function chart types.
**Depends on:** Nothing
**Requirements:** CHART-01, CHART-06
**Success Criteria:**
1. Histogram with configurable bin count, density mode, cumulative mode.
2. Function plot evaluating `Func<double, double>` over a domain.
3. Both have data model, renderer, probe strategy, Plot3D API.
4. Cookbook recipe snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 447-01-PLAN.md — Add Histogram and Function chart types

### Phase 448: Pie/Donut Chart

**Goal:** Add Pie/Donut chart type.
**Depends on:** Nothing
**Requirements:** CHART-03
**Success Criteria:**
1. Pie chart with configurable slice colors, labels, explode offset.
2. Donut mode with configurable hole ratio.
3. Data model, renderer, probe strategy, Plot3D API.
4. Cookbook recipe snippet and live demo.

**Plans:** 1 plan
Plans:
- [ ] 448-01-PLAN.md — Add Pie/Donut chart type

### Phase 449: Error Bar Overlay

**Goal:** Add Error Bar overlay on scatter plots.
**Depends on:** Nothing
**Requirements:** CHART-02
**Success Criteria:**
1. Error bars with symmetric and asymmetric X/Y errors.
2. Configurable cap size, color, and width.
3. Integrates with existing ScatterPlot3DSeries.
4. Cookbook recipe snippet and live demo.

**Plans:** 1 plan
Plans:
- [ ] 449-01-PLAN.md — Add Error Bar overlay

### Phase 450: OHLC/Candlestick Chart

**Goal:** Add OHLC/Candlestick chart type for financial data.
**Depends on:** Nothing
**Requirements:** CHART-04
**Success Criteria:**
1. Candlestick with configurable up/down colors and wick rendering.
2. OHLC mode with line-style rendering.
3. Data model, renderer, probe strategy, Plot3D API.
4. Cookbook recipe snippet and live demo.

**Plans:** 1 plan
Plans:
- [ ] 450-01-PLAN.md — Add OHLC/Candlestick chart type

### Phase 451: Violin & Polygon Chart

**Goal:** Add Violin plot and Polygon chart types.
**Depends on:** Nothing
**Requirements:** CHART-05, CHART-07
**Success Criteria:**
1. Violin plot with kernel density estimation and configurable bandwidth.
2. Polygon chart for arbitrary 2D/3D filled/outlined polygons.
3. Both have data model, renderer, probe strategy, Plot3D API.
4. Cookbook recipe snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 451-01-PLAN.md — Add Violin and Polygon chart types

### Phase 452: Axis System Upgrade

**Goal:** Upgrade axis system with DateTime, log scale, tick formatting, and
multiple Y axes.
**Depends on:** Nothing
**Requirements:** AXIS-01, AXIS-02, AXIS-03, AXIS-04, AXIS-05, AXIS-06
**Success Criteria:**
1. DateTime axis with auto-formatted ticks.
2. Logarithmic axis scale with configurable base.
3. Tick label format strings and custom formatters.
4. Minor tick marks with configurable count.
5. Axis inversion.
6. Multiple independent Y axes (left/right).

**Plans:** 1 plan
Plans:
- [ ] 452-01-PLAN.md — Axis system upgrade

### Phase 453: Text & Arrow Annotations

**Goal:** Add text and arrow annotations.
**Depends on:** Phase 452 (axis system needed for coordinate anchoring)
**Requirements:** ANNO-01, ANNO-02
**Success Criteria:**
1. Text annotation anchored to data coordinates with font/color/rotation.
2. Arrow annotation from point to point with head style.
3. Both render in 3D space and respond to camera transforms.
4. Cookbook recipe snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 453-01-PLAN.md — Add text and arrow annotations

### Phase 454: Reference Lines, Spans & Shapes

**Goal:** Add reference lines, reference regions, and shape annotations.
**Depends on:** Phase 453 (builds on annotation infrastructure)
**Requirements:** ANNO-03, ANNO-04, ANNO-05
**Success Criteria:**
1. Horizontal/vertical reference lines at fixed values.
2. Horizontal/vertical spans between two values with fill.
3. Rectangle and ellipse shapes anchored to data coordinates.
4. All have configurable style, color, and label.
5. Cookbook recipe snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 454-01-PLAN.md — Add reference lines, spans, and shapes

### Phase 455: SVG & Batch Export

**Goal:** Add SVG export and batch export API.
**Depends on:** Nothing
**Requirements:** EXPORT-01, EXPORT-02, EXPORT-03
**Success Criteria:**
1. SVG export producing resolution-independent vector output.
2. Batch export API for multiple charts with naming patterns.
3. Export manifest (JSON) with metadata and timestamps.
4. Cookbook recipe snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 455-01-PLAN.md — Add SVG and batch export

### Phase 456: Demo Framework Refactor

**Goal:** Refactor MainWindow into recipe-driven architecture.
**Depends on:** Phase 447-455 (all features must exist before refactoring demo)
**Requirements:** DEMO-01, DEMO-02, DEMO-03, DEMO-04
**Success Criteria:**
1. Each recipe is a self-contained class implementing `IChartRecipe`.
2. Recipe group navigation with expand/collapse in sidebar.
3. Parameter panel auto-generated from recipe metadata.
4. MainWindow reduced to <500 lines.
5. All existing recipes continue to work.

**Plans:** 1 plan
Plans:
- [ ] 456-01-PLAN.md — Refactor demo into recipe-driven architecture

### Phase 457: Cookbook Recipes for New Features

**Goal:** Add cookbook recipes for all new features.
**Depends on:** Phase 456 (demo refactor provides the recipe infrastructure)
**Requirements:** COOK-05, COOK-06, COOK-07, COOK-08
**Success Criteria:**
1. Recipes for all 7 new chart types.
2. Recipe for DateTime axis and log scale.
3. Recipe for annotations (text, arrow, reference line, span, shape).
4. Recipe for SVG and batch export workflows.

**Plans:** 1 plan
Plans:
- [ ] 457-01-PLAN.md — Add cookbook recipes for new features

### Phase 458: Verification & Milestone Closure

**Goal:** Final verification and milestone closure.
**Depends on:** Phase 457
**Requirements:** All
**Success Criteria:**
1. All 30 requirements verified as met.
2. Demo builds and runs cleanly.
3. All planning docs updated.
4. Milestone closed.

**Plans:** 1 plan
Plans:
- [ ] 458-01-PLAN.md — Final verification and closure

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 447. Histogram & Function Plot | 0/1 | Not started | — |
| 448. Pie/Donut Chart | 0/1 | Not started | — |
| 449. Error Bar Overlay | 0/1 | Not started | — |
| 450. OHLC/Candlestick Chart | 0/1 | Not started | — |
| 451. Violin & Polygon Chart | 0/1 | Not started | — |
| 452. Axis System Upgrade | 0/1 | Not started | — |
| 453. Text & Arrow Annotations | 0/1 | Not started | — |
| 454. Reference Lines, Spans & Shapes | 0/1 | Not started | — |
| 455. SVG & Batch Export | 0/1 | Not started | — |
| 456. Demo Framework Refactor | 0/1 | Not started | — |
| 457. Cookbook Recipes for New Features | 0/1 | Not started | — |
| 458. Verification & Milestone Closure | 0/1 | Not started | — |

---

<details>
<summary>v2.66 Complete Cookbook Demo Gallery - Complete (2026-04-30)</summary>

v2.66 added 20 cookbook recipes covering all 10 chart types, MultiPlot3D,
DataLogger3D streaming, multi-chart workspace, and interactive parameter
controls. Phase 440 (framework refactor) was deferred.

</details>
