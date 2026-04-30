# Roadmap: v2.61 Native SurfaceCharts Cookbook and CI Truth

**Goal:** Push SurfaceCharts toward ScottPlot5-style cookbook usability while
staying native to Videra's 3D chart model: detailed runnable demo recipes,
high-performance data paths, truthful CI, and no compatibility or fallback
layers.

**External reference:** ScottPlot 5's cookbook emphasizes short `Plot.Add.*`
recipes, returned plottable handles for customization, axis/export examples,
and high-performance data shapes such as Signal/SignalXY for large ordered
datasets. v2.61 uses those cookbook ergonomics as inspiration only, not API
compatibility or parity.

**Phases:** 5 (409-413)
**Requirements:** 12 (COOKBOOK-01..03, NATIVE-01..02, PERF-01..02,
CI-01..02, VERIFY-01..02, SCOPE-01)
**Beads epic:** `Videra-uqv`

## Phases

- [x] **Phase 409: Native Cookbook and CI Inventory** - Inventory current demo cookbook, native 3D chart API surfaces, performance-sensitive paths, CI gates, and anti-fake validation gaps.
- [x] **Phase 410: Detailed 3D Cookbook Demo Recipes** - Expand the demo/docs cookbook into detailed runnable recipes for the shipped 3D chart model.
- [x] **Phase 411: Native High-Performance Demo Paths** - Tighten cookbook/demo examples around native Videra data paths and performance evidence without downshift/fallback behavior.
- [x] **Phase 412: CI Truth and Validation Hardening** - Ensure focused CI/test design is reasonable, deterministic, and catches docs/demo/support drift.
- [ ] **Phase 413: v2.61 Final Verification** - Close v2.61 with composed validation, Beads export, generated roadmap, archive, push, and cleanup.

## Phase Details

### Phase 409: Native Cookbook and CI Inventory

**Goal:** Establish real surfaces and validation boundaries before implementation.
**Depends on:** Nothing
**Requirements:** COOKBOOK-01, NATIVE-01, PERF-01, CI-01, SCOPE-01
**Bead:** `Videra-63e`
**Success Criteria:**
1. Current SurfaceCharts demo cookbook recipes are mapped against shipped 3D chart APIs.
2. ScottPlot5-inspired ergonomics are translated into Videra-native concepts without compatibility claims.
3. Performance-sensitive demo/data paths and current evidence gaps are identified.
4. CI/test gaps are scoped with anti-fake validation rules.
**Plans:** 409-PLAN.md
**Completed:** 2026-04-30

### Phase 410: Detailed 3D Cookbook Demo Recipes

**Goal:** Make the demo cookbook detailed enough for users to copy and adapt real 3D chart recipes.
**Depends on:** Phase 409 (complete)
**Requirements:** COOKBOOK-02, COOKBOOK-03, NATIVE-02, VERIFY-01, SCOPE-01
**Bead:** `Videra-2de`
**Success Criteria:**
1. Cookbook recipes cover first chart, surface, waterfall, scatter, bar, contour, axes, styling, interaction, live data, linked views, and PNG snapshot routes.
2. Recipes expose Videra-native setup, imports, host wiring, and support evidence without pretending to be ScottPlot.
3. Demo/docs snippets stay aligned through focused tests.
**Plans:** 410-PLAN.md
**Completed:** 2026-04-30

### Phase 411: Native High-Performance Demo Paths

**Goal:** Keep cookbook examples high-performance and native to Videra's 3D chart architecture.
**Depends on:** Phase 409 (complete)
**Requirements:** PERF-02, NATIVE-02, VERIFY-01, SCOPE-01
**Bead:** `Videra-s6h`
**Success Criteria:**
1. Demo recipes use existing efficient chart-local APIs and data builders.
2. Performance evidence is truthful: no fake benchmark claims, no hidden fallback, and no unsupported backend promises.
3. Any added tests validate behavior/evidence instead of synthetic success text.
**Plans:** 411-PLAN.md
**Completed:** 2026-04-30

### Phase 412: CI Truth and Validation Hardening

**Goal:** Ensure CI/test design is reasonable, targeted, and able to pass honestly.
**Depends on:** Phase 410, Phase 411
**Requirements:** CI-02, VERIFY-01, VERIFY-02, SCOPE-01
**Bead:** `Videra-79n`
**Success Criteria:**
1. Cookbook/demo/interaction CI checks are focused and deterministic.
2. Validation covers generated roadmap, Beads export, snapshot scope guardrails, and docs/demo drift.
3. No CI check is weakened, skipped, or faked to make the milestone appear green.
**Plans:** 412-PLAN.md
**Completed:** 2026-04-30

### Phase 413: v2.61 Final Verification

**Goal:** Close v2.61 with synchronized verification and handoff.
**Depends on:** Phase 412
**Requirements:** VERIFY-02
**Bead:** `Videra-q10`
**Success Criteria:**
1. Focused checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase archive, branch/worktree
   cleanup, Git push, and Dolt Beads push are synchronized.
**Plans:** 413-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 409. Native Cookbook and CI Inventory | 1/1 | Complete | 2026-04-30 |
| 410. Detailed 3D Cookbook Demo Recipes | 1/1 | Complete | 2026-04-30 |
| 411. Native High-Performance Demo Paths | 1/1 | Complete | 2026-04-30 |
| 412. CI Truth and Validation Hardening | 1/1 | Complete | 2026-04-30 |
| 413. v2.61 Final Verification | 0/1 | Ready | |

---

<details>
<summary>v2.60 SurfaceCharts Cookbook QA and Interaction Handoff - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.60-phases`

v2.60 inventoried and hardened the cookbook/interaction handoff with structured
cookbook coverage, current consumer handoff wording, interaction host-wiring
docs/tests, probe/selection/draggable recipe coverage, focused validation,
Beads export, generated roadmap, and clean handoff.

</details>
