# Roadmap: v2.66 Complete Cookbook Demo Gallery

**Goal:** Make the SurfaceCharts Demo into a truly complete cookbook gallery
covering all 10 chart types, MultiPlot3D, streaming data, and upgraded demo
framework experience.

**Boundary:** This milestone does not add new chart types beyond the 10 shipped
in v2.65, 2D chart families, compatibility adapters, backend/renderer expansion,
PDF/vector export, or fake validation evidence.

**Phases:** 7 (440-446)
**Requirements:** 20 (RECIPE-01..05, MULTI-01..03, STREAM-01..04, FRAMEWORK-01..04, COOK-01..04)
**Beads epic:** TBD

## Phases

- [ ] **Phase 440: Demo Framework Architecture Refactor** - Refactor
  MainWindow code-behind into recipe-driven architecture with self-contained
  recipe classes and group navigation.
- [x] **Phase 441: Line/Ribbon/VectorField Cookbook Recipes** - Add cookbook
  recipes for Line, Ribbon, and VectorField chart types with live demo.
- [x] **Phase 442: HeatmapSlice/BoxPlot Cookbook Recipes** - Add cookbook
  recipes for HeatmapSlice and BoxPlot chart types with live demo.
- [ ] **Phase 443: MultiPlot3D Cookbook Recipes** - Add MultiPlot3D subplot
  grid cookbook recipes with linked camera/axis and grid snapshot.
- [ ] **Phase 444: DataLogger3D Streaming Cookbook Recipes** - Add streaming
  cookbook recipes for Surface/Waterfall/Bar DataLogger3D with live counters.
- [ ] **Phase 445: Interactive Parameter Controls and Live Preview** - Add
  per-recipe parameter controls and real-time preview for chart properties.
- [ ] **Phase 446: Cookbook Catalog Integration and Completeness** - Integrate
  all recipes into catalog, verify completeness, and close milestone.

## Phase Details

### Phase 440: Demo Framework Architecture Refactor

**Goal:** Refactor the 1200+ line MainWindow code-behind into a recipe-driven
architecture where each recipe is a self-contained class.
**Depends on:** Nothing
**Requirements:** FRAMEWORK-01, FRAMEWORK-02
**Success Criteria:**
1. Each cookbook recipe is a self-contained class with setup, teardown, and UI
   panel configuration.
2. Recipe group navigation (Basics, Analytics, Composition, Streaming) works
   in the sidebar.
3. MainWindow code-behind is reduced to recipe dispatch and shared infrastructure.
4. All existing recipes continue to work after refactor.

**Plans:** 1 plan
Plans:
- [ ] 440-01-PLAN.md — Refactor demo into recipe-driven architecture

### Phase 441: Line/Ribbon/VectorField Cookbook Recipes

**Goal:** Add cookbook recipes for Line, Ribbon, and VectorField chart types.
**Depends on:** Phase 440
**Requirements:** RECIPE-01, RECIPE-02, RECIPE-03
**Success Criteria:**
1. Line chart recipe shows polyline with configurable color/width/markers.
2. Ribbon chart recipe shows tube geometry with configurable radius.
3. VectorField chart recipe shows arrows with magnitude-based coloring.
4. All three recipes have code snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 441-01-PLAN.md — Add Line/Ribbon/VectorField cookbook recipes

### Phase 442: HeatmapSlice/BoxPlot Cookbook Recipes

**Goal:** Add cookbook recipes for HeatmapSlice and BoxPlot chart types.
**Depends on:** Phase 440
**Requirements:** RECIPE-04, RECIPE-05
**Success Criteria:**
1. HeatmapSlice recipe shows volumetric slice with axis/position control.
2. BoxPlot recipe shows statistical distribution with grouped layout.
3. Both recipes have code snippets and live demo.

**Plans:** 1 plan
Plans:
- [ ] 442-01-PLAN.md — Add HeatmapSlice/BoxPlot cookbook recipes

### Phase 443: MultiPlot3D Cookbook Recipes

**Goal:** Add MultiPlot3D subplot grid cookbook recipes.
**Depends on:** Phase 441, Phase 442
**Requirements:** MULTI-01, MULTI-02, MULTI-03, COOK-02
**Success Criteria:**
1. MultiPlot3D recipe shows NxM grid with mixed chart types.
2. Linked camera/axis demo shows LinkAll/LinkRow/LinkColumn.
3. Grid snapshot recipe renders entire grid as single PNG.

**Plans:** 1 plan
Plans:
- [ ] 443-01-PLAN.md — Add MultiPlot3D cookbook recipes

### Phase 444: DataLogger3D Streaming Cookbook Recipes

**Goal:** Add streaming cookbook recipes for all DataLogger3D types.
**Depends on:** Phase 440
**Requirements:** STREAM-01, STREAM-02, STREAM-03, STREAM-04, COOK-03
**Success Criteria:**
1. SurfaceDataLogger3D recipe shows append/replace/FIFO with live counters.
2. WaterfallDataLogger3D recipe shows delegated streaming patterns.
3. BarDataLogger3D recipe shows append/replace with series tracking.
4. Unified streaming dashboard shows all three in MultiPlot3D grid.

**Plans:** 1 plan
Plans:
- [ ] 444-01-PLAN.md — Add DataLogger3D streaming cookbook recipes

### Phase 445: Interactive Parameter Controls and Live Preview

**Goal:** Add per-recipe parameter controls and real-time preview.
**Depends on:** Phase 441, Phase 442, Phase 443, Phase 444
**Requirements:** FRAMEWORK-03, FRAMEWORK-04
**Success Criteria:**
1. Slider controls for numeric parameters (radius, position, scale).
2. Dropdown controls for enum parameters (axis, colormap, layout).
3. Chart updates in real-time as parameters change.

**Plans:** 1 plan
Plans:
- [ ] 445-01-PLAN.md — Add interactive parameter controls and live preview

### Phase 446: Cookbook Catalog Integration and Completeness

**Goal:** Integrate all recipes into catalog and verify completeness.
**Depends on:** Phase 445
**Requirements:** COOK-01, COOK-04
**Success Criteria:**
1. All 10 chart types have at least one runnable cookbook recipe.
2. Cookbook recipe catalog is updated with all new recipes.
3. All recipes are properly grouped and described.
4. Demo builds and runs cleanly.

**Plans:** 1 plan
Plans:
- [ ] 446-01-PLAN.md — Final cookbook catalog integration and verification

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 440. Demo Framework Architecture Refactor | 0/1 | Not started | — |
| 441. Line/Ribbon/VectorField Cookbook Recipes | 1/1 | Complete    | 2026-04-30 |
| 442. HeatmapSlice/BoxPlot Cookbook Recipes | 1/1 | Complete    | 2026-04-30 |
| 443. MultiPlot3D Cookbook Recipes | 0/1 | Not started | — |
| 444. DataLogger3D Streaming Cookbook Recipes | 0/1 | Not started | — |
| 445. Interactive Parameter Controls and Live Preview | 0/1 | Not started | — |
| 446. Cookbook Catalog Integration and Completeness | 0/1 | Not started | — |

---

<details>
<summary>v2.65 3D ScottPlot5 Analytics Chart Expansion - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.65-phases`

v2.65 added 5 new chart families (Line/Ribbon, VectorField, HeatmapSlice,
BoxPlot), MultiPlot3D subplot grid, DataLogger3D streaming for
Surface/Waterfall/Bar, and promoted Bar+Contour to public API contract.

</details>
