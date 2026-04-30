# Roadmap: v2.60 SurfaceCharts Cookbook QA and Interaction Handoff

**Goal:** Audit and harden the v2.59 cookbook/interaction handoff with focused
QA, docs, support, and validation work. This milestone remains a Videra
SurfaceCharts workflow and does not add ScottPlot compatibility, hidden
fallbacks, backend expansion, or a generic chart workbench.

**Phases:** 4 (405-408)
**Requirements:** 8 (QA-01..02, HANDOFF-01..02, VERIFY-01..02, SCOPE-01..02)
**Beads epic:** `Videra-tqz`

## Phases

- [x] **Phase 405: Cookbook QA and Interaction Handoff Inventory** - Inventory v2.59 cookbook QA, interaction handoff docs, support artifacts, and validation gaps.
- [ ] **Phase 406: Cookbook QA Hardening** - Implement bounded cookbook QA or docs/test hardening selected by Phase 405.
- [ ] **Phase 407: Interaction Handoff Polish** - Implement bounded interaction handoff polish selected by Phase 405.
- [ ] **Phase 408: v2.60 Final Verification** - Close v2.60 with focused validation, Beads export, generated roadmap, archive, and cleanup.

## Phase Details

### Phase 405: Cookbook QA and Interaction Handoff Inventory

**Goal:** Establish a real-surface plan before any v2.60 changes.
**Depends on:** Nothing
**Requirements:** QA-01, HANDOFF-01, SCOPE-01
**Bead:** `Videra-b53`
**Success Criteria:**
1. Current cookbook docs/demo/tests/support handoff surfaces are mapped with file-level evidence.
2. Residual QA and interaction-handoff gaps are scoped to minimal implementation slices.
3. Phase 406 and Phase 407 dependency/write-set boundaries are recorded.
4. Validation commands and non-goals are recorded.
**Plans:** 405-PLAN.md
**Completed:** 2026-04-30

### Phase 406: Cookbook QA Hardening

**Goal:** Implement bounded cookbook QA hardening selected by Phase 405.
**Depends on:** Phase 405 (complete)
**Requirements:** QA-02, VERIFY-01, SCOPE-02
**Bead:** `Videra-1h1`
**Success Criteria:**
1. Cookbook QA/docs/test hardening is implemented with focused tests.
2. Changes remain docs/demo/test/support oriented unless Phase 405 proves a narrow API need.
3. No compatibility/fallback/backend expansion scope is introduced.
**Plans:** 406-PLAN.md

### Phase 407: Interaction Handoff Polish

**Goal:** Implement bounded interaction handoff polish selected by Phase 405.
**Depends on:** Phase 405 (complete)
**Requirements:** HANDOFF-02, VERIFY-01, SCOPE-02
**Bead:** `Videra-xq1`
**Success Criteria:**
1. Interaction handoff polish is implemented with focused validation.
2. Existing interaction APIs are documented or demonstrated without adding a command framework or mouse remapping scope.
3. Support evidence remains explicit and chart-local.
**Plans:** 407-PLAN.md

### Phase 408: v2.60 Final Verification

**Goal:** Close v2.60 with synchronized verification and handoff.
**Depends on:** Phase 406, Phase 407
**Requirements:** VERIFY-01, VERIFY-02
**Bead:** `Videra-448`
**Success Criteria:**
1. Focused checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase archive, branch/worktree cleanup, Git push, and Dolt Beads push are synchronized.
**Plans:** 408-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 405. Cookbook QA and Interaction Handoff Inventory | 1/1 | Complete | 2026-04-30 |
| 406. Cookbook QA Hardening | 0/1 | Ready | |
| 407. Interaction Handoff Polish | 0/1 | Ready | |
| 408. v2.60 Final Verification | 0/1 | Blocked by 406, 407 | |

---

<details>
<summary>v2.59 ScottPlot5 Interaction and Cookbook Experience - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.59-phases`

v2.59 completed SurfaceCharts interaction/code-experience and cookbook polish:
typed Bar/Contour plot handles, recipe-friendly enabled command discovery,
Bar/Contour cookbook recipes, docs alignment, text-contract coverage, Beads
export, generated roadmap, and clean handoff.

</details>
