# Roadmap

## v2.56 ScottPlot 5 Interaction and Cookbook Experience - In Progress

**Goal:** Make SurfaceCharts feel closer to ScottPlot 5 around interaction configuration, code ergonomics, live-view behavior, and cookbook-style demo discovery while preserving Videra's 3D chart boundaries.

**Phases:** 7 (383-389)
**Requirements:** 27 (INV-01..03, CODE-01..04, INT-01..04, PICK-01..04, AXIS-01..02, LIVE-01..02, COOK-01..03, DOC-01..02, VER-01..03)

## Phases

- [x] **Phase 383: ScottPlot 5 Interaction Inventory and Beads Coordination** - Confirm exact reference patterns, current implementation seams, non-goals, and bead/worktree boundaries.
- [x] **Phase 384: Plot Lifecycle and Code Experience Polish** - Add concise clear/remove/reorder/list affordances and keep typed plottable handle tweaks simple.
- [x] **Phase 385: Interaction Profile and Command Surface** - Add a small chart-local interaction profile plus bounded custom command/context-menu recipes.
- [x] **Phase 386: Selection, Probe, and Draggable Overlay Recipes** - Add pointer-to-probe helpers, host-owned selection, and bounded draggable marker/range overlay recipes.
- [x] **Phase 387: Axis Rules, Linked Views, and Live View Management** - Add axis limit rules, explicit view linking, and live latest-window/full-data behavior.
- [ ] **Phase 388: Cookbook Demo Gallery and Docs** - Restructure demo/docs into recipe groups with isolated setup and copyable snippets.
- [ ] **Phase 389: Integration, Guardrails, and Milestone Evidence** - Run focused regression, scope guardrails, Beads export, generated roadmap, and clean handoff.

## Phase Details

### Phase 383: ScottPlot 5 Interaction Inventory and Beads Coordination
**Goal:** The milestone starts from evidence, with precise ScottPlot 5 inspiration points, Videra-owned seams, non-goals, and dependency-aware Beads.
**Depends on**: Nothing
**Requirements**: INV-01, INV-02, INV-03
**Success Criteria**:
1. Current Plot, interaction, overlay, axis, live, demo, support evidence, and guardrail owners are documented.
2. Official ScottPlot 5 reference patterns are recorded as inspiration, not compatibility commitments.
3. Beads exist for every phase with parent-child links, blocking dependencies, validation notes, and safe parallelization boundaries.
4. `bd ready --json` shows only genuinely unblocked work.
**Plans**: 383-PLAN.md

### Phase 384: Plot Lifecycle and Code Experience Polish
**Goal:** Users can manage Plot-owned plottables with short, predictable code without touching internals.
**Depends on**: Phase 383
**Requirements**: CODE-01, CODE-02, CODE-03, CODE-04
**Success Criteria**:
1. `Plot` exposes concise clear/remove/reorder/list operations over Plot-owned plottables.
2. Returned handles and list/query APIs stay typed, read-only where appropriate, and deterministic.
3. Revision, support evidence, legend/probe participation, and snapshot behavior remain coherent after lifecycle operations.
4. No backend/runtime ownership expansion or compatibility wrapper types are introduced.
**Plans**: 384-PLAN.md

### Phase 385: Interaction Profile and Command Surface
**Goal:** Users can configure common mouse/keyboard behavior and attach bounded chart commands through a small API.
**Depends on**: Phase 383
**Requirements**: INT-01, INT-02, INT-03, INT-04
**Success Criteria**:
1. Built-in pan, zoom, reset/autoscale, keyboard, and focus behavior can be enabled or disabled explicitly.
2. Custom command/context-menu recipes reuse the same bounded command surface.
3. Unsupported or disabled behavior is visible in state/evidence and never silently downshifts.
4. Focused interaction tests cover default and customized behavior.
**Plans**: 385-PLAN.md

### Phase 386: Selection, Probe, and Draggable Overlay Recipes
**Goal:** Users can implement common interactive recipes with concise chart-local helpers while keeping application state ownership clear.
**Depends on**: Phase 385
**Requirements**: PICK-01, PICK-02, PICK-03, PICK-04
**Success Criteria**:
1. Pointer positions can be converted into deterministic probe/coordinate results.
2. Click or rectangle selection reports selected point, series, or range state without taking ownership from the host.
3. Draggable marker/range overlay recipes update explicit chart-local marker or axis state.
4. Selection and draggable overlay support evidence is deterministic and source data is not silently mutated.
**Plans**: 386-PLAN.md

### Phase 387: Axis Rules, Linked Views, and Live View Management
**Goal:** Axis behavior and live data views become discoverable, explicit, and easy to compose across charts.
**Depends on**: Phase 385
**Requirements**: AXIS-01, AXIS-02, LIVE-01, LIVE-02
**Success Criteria**:
1. `Plot.Axes` supports limit rules for locks, min/max bounds, and autoscale constraints.
2. Two `VideraChartView` instances can link axis/view limits with explicit lifetime and no global registry.
3. Live helpers expose latest-window or full-data view behavior explicitly.
4. Appended, dropped, visible-window, and autoscale-decision counters are deterministic.
**Plans**: 387-PLAN.md

### Phase 388: Cookbook Demo Gallery and Docs
**Goal:** The demo becomes a cookbook-style discovery surface for first chart, styling, interaction, live data, linked axes, and export recipes.
**Depends on**: Phase 384, Phase 386, Phase 387
**Requirements**: COOK-01, COOK-02, COOK-03, DOC-01, DOC-02
**Success Criteria**:
1. SurfaceCharts demo has recipe groups with isolated setup paths and visible results.
2. Recipes include copyable or matching README snippets without shared hidden mutable state.
3. Root README and demo README link the cookbook path and explain Videra's 3D-specific boundaries.
4. Demo remains a bounded reference app, not a generic chart editor or god-code workbench.
**Plans**: 388-PLAN.md

### Phase 389: Integration, Guardrails, and Milestone Evidence
**Goal:** The milestone closes with focused tests, guardrails, Beads export, generated roadmap, and clean handoff evidence.
**Depends on**: Phase 386, Phase 387, Phase 388
**Requirements**: VER-01, VER-02, VER-03
**Success Criteria**:
1. Focused tests cover lifecycle/code experience, interaction profile, selection/probe/draggable overlays, axis/live helpers, cookbook demo wiring, and docs guardrails.
2. Repository guardrails continue to reject old chart controls, direct public `Source`, compatibility wrappers, PDF/vector export, backend expansion, hidden fallback/downshift, and god-code workbench scope.
3. Beads state, generated public roadmap, phase evidence, branches, worktrees, and handoff notes are clean or blockers are explicitly reported.
**Plans**: 389-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 383. ScottPlot 5 Interaction Inventory and Beads Coordination | 1/1 | Complete | 2026-04-30 |
| 384. Plot Lifecycle and Code Experience Polish | 1/1 | Complete | 2026-04-30 |
| 385. Interaction Profile and Command Surface | 1/1 | Complete | 2026-04-30 |
| 386. Selection, Probe, and Draggable Overlay Recipes | 1/1 | Complete | 2026-04-30 |
| 387. Axis Rules, Linked Views, and Live View Management | 1/1 | Complete | 2026-04-30 |
| 388. Cookbook Demo Gallery and Docs | 0/1 | Pending | - |
| 389. Integration, Guardrails, and Milestone Evidence | 0/1 | Pending | - |

---

<details>
<summary>v2.55 ScottPlot-like Plot API - Complete (2026-04-30)</summary>

Shipped raw `Plot.Add.*` overloads, typed plottables, `Plot.Axes`, `Plot.SavePngAsync`, `DataLogger3D`, same-type multi-series composition, cookbook seed docs, and closure guardrails. 7 phases, 27 requirements. Archived: `.planning/milestones/v2.55-ROADMAP.md`

</details>

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
