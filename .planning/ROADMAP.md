# Roadmap: Videra v2.53 Chart Type Expansion and Axis Semantics

## Overview

This milestone expands Videra SurfaceCharts from three chart families (Surface, Waterfall, Scatter) to five by adding Bar Chart and Contour Plot, while upgrading the axis model from linear-only to support log scale, DateTime, and custom tick formatters. An enhanced legend overlay completes the multi-series presentation story. The build order follows dependency logic: axis foundations first (everything depends on axes), then legend (new chart types need entries), then new chart types (Bar before Contour due to complexity), and finally integration evidence.

## Phases

**Phase Numbering:**
- Integer phases (366-370): Planned milestone work
- Decimal phases (366.1, 366.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 366: Axis Foundation** - Log scale, DateTime axis, and custom tick formatters
- [ ] **Phase 367: Enhanced Chart Legend** - Multi-series legend with per-kind indicators and position options
- [ ] **Phase 368: Bar Chart Series** - 3D bar chart rendering via Plot.Add.Bar
- [ ] **Phase 369: Contour Plot Series** - Contour plot with marching squares iso-line extraction
- [ ] **Phase 370: Integration and Evidence** - Demo, smoke, diagnostics, and guardrails for all new types

## Phase Details

### Phase 366: Axis Foundation
**Goal**: Chart axes support log scale, DateTime, and custom tick formatting — unblocking non-linear axis use across all chart types
**Depends on**: Nothing (first phase of milestone)
**Requirements**: AXIS-01, AXIS-02, AXIS-03, AXIS-04, AXIS-05
**Success Criteria** (what must be TRUE):
  1. User can set `SurfaceAxisScaleKind.Log` on Y axis and see powers-of-10 tick labels with correct spacing
  2. User can set `SurfaceAxisScaleKind.DateTime` on X axis and see human-readable time labels from UTC-seconds values
  3. User can assign a `Func<double, string>` formatter to any axis and see custom labels rendered in place of defaults
  4. User receives an explicit diagnostic when setting log scale with minimum <= 0 (zero or negative values rejected)
  5. DateTime axis values stored as UTC seconds (long) maintain full precision across zoom/pan without floating-point drift
**Plans**: 1 plan

Plans:
- [ ] 366-01-PLAN.md — Unblock Log scale, implement DateTime/log tick generation, wire per-axis custom formatters

### Phase 367: Enhanced Chart Legend
**Goal**: Chart displays a configurable, multi-series legend overlay with kind-specific visual indicators
**Depends on**: Phase 366
**Requirements**: LEG-01, LEG-02, LEG-03
**Success Criteria** (what must be TRUE):
  1. User sees a legend overlay listing all visible series with kind-specific indicators (rectangles for surface/waterfall, dots for scatter, bars for bar chart, lines for contour)
  2. User can configure legend position to any corner (top-left, top-right, bottom-left, bottom-right)
  3. Hidden series are automatically excluded from the legend display
**Plans**: TBD

Plans:
- [ ] 367-01: TBD

### Phase 368: Bar Chart Series
**Goal**: Users can add 3D bar chart series to plots with grouped and stacked layouts
**Depends on**: Phase 366
**Requirements**: BAR-01, BAR-02, BAR-03, BAR-04, BAR-05
**Success Criteria** (what must be TRUE):
  1. User can call `Plot.Add.Bar(double[] values)` and see 3D rectangular prisms rising from the base plane
  2. User can add multiple bar series and see them rendered side-by-side (grouped) or stacked vertically
  3. User can configure bar color per series
  4. User receives an explicit error when passing empty arrays or NaN values to Bar
**Plans**: TBD

Plans:
- [ ] 368-01: TBD

### Phase 369: Contour Plot Series
**Goal**: Users can add contour plot series from 2D scalar field data with configurable iso-lines
**Depends on**: Phase 366
**Requirements**: CTR-01, CTR-02, CTR-03, CTR-04, CTR-05
**Success Criteria** (what must be TRUE):
  1. User can call `Plot.Add.Contour(double[,] values)` and see iso-lines rendered as 3D line geometry in scene space
  2. Contour lines are extracted via marching squares algorithm at evenly-spaced value levels
  3. User can configure the number of contour levels (iso-line count)
  4. Contour extraction results are cached and only re-extracted when data changes (no redundant computation on re-render)
**Plans**: TBD

Plans:
- [ ] 369-01: TBD

### Phase 370: Integration and Evidence
**Goal**: All new chart types are wired into demo, diagnostics, evidence contracts, and repository guardrails
**Depends on**: Phase 367, Phase 368, Phase 369
**Requirements**: INT-01, INT-02, INT-03, INT-04, VER-01, VER-02, VER-03
**Success Criteria** (what must be TRUE):
  1. All new chart types (Bar, Contour) produce output evidence via `Plot3DOutputEvidence` contract
  2. All new chart types produce dataset evidence via `Plot3DDatasetEvidence` contract
  3. Demo exposes sample data and UI controls for Bar and Contour chart types
  4. Consumer smoke validates Bar and Contour chart types render without error
  5. Existing surface/waterfall/scatter behavior shows no regression (guardrail tests pass)
**Plans**: TBD

Plans:
- [ ] 370-01: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 366 → 367 → 368 → 369 → 370

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 366. Axis Foundation | 0/1 | Planned | - |
| 367. Enhanced Chart Legend | 1/1 | Complete | 2026-04-29 |
| 368. Bar Chart Series | 0/1 | Not started | - |
| 369. Contour Plot Series | 0/1 | Not started | - |
| 370. Integration and Evidence | 0/1 | Not started | - |
