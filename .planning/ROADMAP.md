# Roadmap

## v2.55 ScottPlot-like Plot API - In Progress

**Goal:** Make SurfaceCharts first-chart usage feel ScottPlot-like: short `Plot.Add.*` code, typed plottable handles, discoverable axes, PNG convenience, live scatter helpers, and cookbook docs without widening renderer/backend scope.

**Phases:** 7 (376-382)
**Requirements:** 26 (INV-01..03, ADD-01..04, PLOT-01..03, AXIS-01..03, PNG-01..02, LIVE-01..03, MULTI-01..03, DOC-01, DEMO-01, VER-01..03)

## Phases

- [x] **Phase 376: Plot API Inventory and Beads Coordination** - Confirm exact API gaps, non-goals, dependency graph, and parallel worktree boundaries
- [ ] **Phase 377: Raw Add Overloads and Typed Plottables** - Add concise raw-array Plot.Add overloads and small typed plottable handles
- [ ] **Phase 378: Axes Facade and SavePng Convenience** - Add `Plot.Axes` and `Plot.SavePngAsync` on top of existing state/snapshot ownership
- [ ] **Phase 379: Live Scatter Helpers** - Add `DataLogger3D` / `ScatterStream` facade over existing columnar scatter append/FIFO semantics
- [ ] **Phase 380: Same-Type Multi-Series Composition** - Make visible same-type series compose deterministically instead of relying only on active last-series rendering
- [ ] **Phase 381: Cookbook Demo and Docs** - Refresh README/demo recipes around the new concise API without creating a workbench
- [ ] **Phase 382: Integration, Guardrails, and Milestone Evidence** - Run focused regression, guardrails, beads export, and generated roadmap closure

## Phase Details

### Phase 376: Plot API Inventory and Beads Coordination
**Goal:** The milestone starts from evidence, with precise implementation seams, non-goals, and bead/worktree boundaries.
**Depends on**: Nothing
**Requirements**: INV-01, INV-02, INV-03
**Success Criteria** (what must be TRUE):
  1. `Plot3D`, `Plot3DAddApi`, series handle, axis, snapshot, demo, smoke, and guardrail owners are documented.
  2. The milestone explicitly rejects old chart controls, public `Source`, compatibility wrappers, PDF/vector export, backend/runtime rewrite, hidden fallback/downshift, generic plotting-engine work, and god-code workbench scope.
  3. Beads exist for every phase with parent/child links, blocking dependencies, verification notes, and safe parallelization boundaries.
  4. `bd ready --json` shows only genuinely unblocked work.
**Plans**: 376-PLAN.md

### Phase 377: Raw Add Overloads and Typed Plottables
**Goal:** Users can add Surface, Waterfall, and Scatter data from raw arrays and receive typed plottable handles without constructing internal data models.
**Depends on**: Phase 376
**Requirements**: ADD-01, ADD-02, ADD-03, ADD-04, PLOT-01, PLOT-02, PLOT-03
**Success Criteria** (what must be TRUE):
  1. `Plot.Add.Surface(...)`, `Plot.Add.Waterfall(...)`, and `Plot.Add.Scatter(...)` expose concise raw numeric overloads.
  2. Existing advanced overloads remain available as the advanced path.
  3. Returned handles implement a small `IPlottable3D` contract with label and visibility.
  4. Existing Plot API and repository guardrail tests pass.
**Plans**: 377-PLAN.md

### Phase 378: Axes Facade and SavePng Convenience
**Goal:** Users can discover common axis and PNG export operations from `Plot` without moving runtime ownership into the facade.
**Depends on**: Phase 376
**Requirements**: AXIS-01, AXIS-02, AXIS-03, PNG-01, PNG-02
**Success Criteria** (what must be TRUE):
  1. `Plot.Axes.X/Y/Z` exposes label and limit affordances without making dataset metadata mutable source of truth.
  2. `SetLimits` and `AutoScale` bridge to existing `VideraChartView` viewport/runtime state.
  3. `Plot.SavePngAsync(...)` wraps chart-local snapshot export and writes caller-selected PNG output.
  4. Snapshot scope guardrails continue to reject PDF/vector export and backend expansion.
**Plans**: 378-PLAN.md

### Phase 379: Live Scatter Helpers
**Goal:** Users can stream 3D scatter points through a named first-class helper while reusing existing columnar append/FIFO behavior.
**Depends on**: Phase 376
**Requirements**: LIVE-01, LIVE-02, LIVE-03
**Success Criteria** (what must be TRUE):
  1. `DataLogger3D` or `ScatterStream` provides a concise API over existing scatter columnar series.
  2. Append, replace, FIFO trim, point count, batch count, and dropped-point evidence remain deterministic.
  3. The implementation does not introduce a render loop, scheduler, background worker, or hidden fallback.
**Plans**: 379-PLAN.md

### Phase 380: Same-Type Multi-Series Composition
**Goal:** Same-type series compose visibly and in evidence paths, instead of only the last active series determining runtime output.
**Depends on**: Phase 377
**Requirements**: MULTI-01, MULTI-02, MULTI-03
**Success Criteria** (what must be TRUE):
  1. Multiple visible same-type series compose deterministically for supported existing chart kinds.
  2. Legend, probe, snapshot, and support evidence identify visible composed series.
  3. No new chart types or generic plotting engine are introduced.
**Plans**: 380-PLAN.md

### Phase 381: Cookbook Demo and Docs
**Goal:** A new user can copy concise recipes for first chart, axes, snapshot PNG, and live scatter from repo docs/demo.
**Depends on**: Phase 377, Phase 378, Phase 379
**Requirements**: DOC-01, DEMO-01
**Success Criteria** (what must be TRUE):
  1. Root README and SurfaceCharts demo README show cookbook recipes using the new API.
  2. Demo exercises concise Plot.Add, Plot.Axes, SavePngAsync, and live scatter paths.
  3. Demo remains a bounded reference app, not a god-code workbench.
**Plans**: 381-PLAN.md

### Phase 382: Integration, Guardrails, and Milestone Evidence
**Goal:** The milestone closes with focused tests, guardrails, Beads export, generated roadmap, and clean handoff evidence.
**Depends on**: Phase 380, Phase 381
**Requirements**: VER-01, VER-02, VER-03
**Success Criteria** (what must be TRUE):
  1. Focused Plot API, demo/docs, snapshot, and repository guardrail tests pass.
  2. `scripts/Test-SnapshotExportScope.ps1` passes.
  3. Beads state and generated public roadmap are exported and synchronized.
  4. Worktree, branch, and handoff state are clean or blockers are explicitly reported.
**Plans**: 382-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 376. Plot API Inventory and Beads Coordination | 1/1 | Complete | 2026-04-29 |
| 377. Raw Add Overloads and Typed Plottables | 0/1 | Pending | - |
| 378. Axes Facade and SavePng Convenience | 0/1 | Pending | - |
| 379. Live Scatter Helpers | 0/1 | Pending | - |
| 380. Same-Type Multi-Series Composition | 0/1 | Pending | - |
| 381. Cookbook Demo and Docs | 0/1 | Pending | - |
| 382. Integration, Guardrails, and Milestone Evidence | 0/1 | Pending | - |

---

<details>
<summary>v2.54 Chart Interactivity - Complete (2026-04-29)</summary>

Shipped crosshair, tooltips, probe strategies, keyboard/toolbar controls. 5 phases, 22 requirements. Archived: `.planning/milestones/v2.54-ROADMAP.md`

</details>

<details>
<summary>v2.53 Chart Type Expansion and Axis Semantics - Complete (2026-04-29)</summary>

Shipped Bar/Contour chart types, Log/DateTime axes, custom formatters, multi-series legend. 5 phases, 25 requirements. Archived: `.planning/milestones/v2.53-ROADMAP.md`

</details>

<details>
<summary>v2.52 Professional Chart Snapshot Export - Complete (2026-04-29)</summary>

Shipped chart-local PNG/bitmap snapshot export. 5 phases, 23 requirements. Archived: `.planning/milestones/v2.52-ROADMAP.md`

</details>
