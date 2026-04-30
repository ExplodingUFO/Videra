# Roadmap: v2.61 SurfaceCharts Cookbook Demo Runtime Continuity

**Goal:** Keep the SurfaceCharts cookbook demo and interaction handoff usable as
runtime examples after v2.60, with bounded validation and no compatibility,
fallback, backend, export, or generic workbench expansion.

**Phases:** 4 (409-412)
**Requirements:** 8 (DEMO-01..02, HANDOFF-01..02, VERIFY-01..02, SCOPE-01..02)
**Beads epic:** `Videra-uqv`

## Phases

- [ ] **Phase 409: Cookbook Demo Runtime Continuity Inventory** - Inventory demo runtime flows, launch/build expectations, interaction recipe discoverability, support evidence, and validation gaps.
- [ ] **Phase 410: Cookbook Demo Runtime QA** - Implement bounded runtime/demo QA hardening selected by Phase 409.
- [ ] **Phase 411: Interaction Recipe Runtime Handoff** - Implement bounded interaction recipe runtime handoff selected by Phase 409.
- [ ] **Phase 412: v2.61 Final Verification** - Close v2.61 with focused validation, Beads export, generated roadmap, archive, and cleanup.

## Phase Details

### Phase 409: Cookbook Demo Runtime Continuity Inventory

**Goal:** Establish real demo/runtime evidence before implementation.
**Depends on:** Nothing
**Requirements:** DEMO-01, HANDOFF-01, SCOPE-01
**Bead:** `Videra-63e`
**Success Criteria:**
1. Current SurfaceCharts cookbook demo launch/build/runtime flows are mapped.
2. Interaction recipe discoverability and support evidence gaps are scoped.
3. Phase 410 and Phase 411 dependency/write-set boundaries are recorded.
4. Validation commands and non-goals are recorded.
**Plans:** 409-PLAN.md

### Phase 410: Cookbook Demo Runtime QA

**Goal:** Implement bounded demo/runtime QA hardening selected by Phase 409.
**Depends on:** Phase 409
**Requirements:** DEMO-02, VERIFY-01, SCOPE-02
**Bead:** to be created by Phase 409
**Success Criteria:**
1. Demo runtime QA/docs/test hardening is implemented with focused validation.
2. Changes remain demo/test/support oriented unless Phase 409 proves a narrow API need.
3. No compatibility/fallback/backend/export/workbench scope is introduced.
**Plans:** 410-PLAN.md

### Phase 411: Interaction Recipe Runtime Handoff

**Goal:** Implement bounded interaction recipe runtime handoff selected by Phase 409.
**Depends on:** Phase 409
**Requirements:** HANDOFF-02, VERIFY-01, SCOPE-02
**Bead:** to be created by Phase 409
**Success Criteria:**
1. Runtime handoff polish is implemented with focused validation.
2. Existing interaction APIs are demonstrated without adding command framework,
   mouse remapping, chart-owned selection, or generic workbench scope.
3. Support evidence remains explicit and chart-local.
**Plans:** 411-PLAN.md

### Phase 412: v2.61 Final Verification

**Goal:** Close v2.61 with synchronized verification and handoff.
**Depends on:** Phase 410, Phase 411
**Requirements:** VERIFY-01, VERIFY-02
**Bead:** to be created by Phase 409
**Success Criteria:**
1. Focused checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase archive, branch/worktree
   cleanup, Git push, and Dolt Beads push are synchronized.
**Plans:** 412-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 409. Cookbook Demo Runtime Continuity Inventory | 0/1 | Ready | |
| 410. Cookbook Demo Runtime QA | 0/1 | Blocked by 409 | |
| 411. Interaction Recipe Runtime Handoff | 0/1 | Blocked by 409 | |
| 412. v2.61 Final Verification | 0/1 | Blocked by 410, 411 | |

---

<details>
<summary>v2.60 SurfaceCharts Cookbook QA and Interaction Handoff - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.60-phases`

v2.60 inventoried and hardened the cookbook/interaction handoff with structured
cookbook coverage, current consumer handoff wording, interaction host-wiring
docs/tests, probe/selection/draggable recipe coverage, focused validation,
Beads export, generated roadmap, and clean handoff.

</details>
