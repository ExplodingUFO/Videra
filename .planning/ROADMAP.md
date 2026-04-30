# Roadmap: v2.59 ScottPlot5 Interaction and Cookbook Experience

**Goal:** Continue SurfaceCharts toward ScottPlot5-inspired interaction and code-experience optimization while turning demos into cookbook-style, copyable recipes. This milestone improves ergonomics and discoverability without claiming ScottPlot compatibility or adding compatibility layers.

**Phases:** 4 (401-404)
**Requirements:** 12 (INT-01..03, CODE-01..03, COOK-01..03, VERIFY-01..03)
**Beads epic:** `Videra-v259`

## Phases

- [x] **Phase 401: Interaction and Cookbook Experience Inventory** - Inventory current interaction APIs, demo flows, cookbook docs, and scoped ScottPlot5-inspired ergonomics gaps.
- [x] **Phase 402: Interaction and Code Experience Polish** - Implement the first bounded interaction/code-experience improvements without compatibility layers or broad rewrites.
- [x] **Phase 403: Cookbook Demo Conversion** - Restructure SurfaceCharts demos and docs toward cookbook-style, copyable recipes.
- [ ] **Phase 404: Interaction Cookbook Final Verification** - Close with cross-phase validation, docs checks, Beads export, generated roadmap, and clean handoff.

## Phase Details

### Phase 401: Interaction and Cookbook Experience Inventory

**Goal:** Establish the bounded implementation plan from real SurfaceCharts API/demo/doc surfaces.
**Depends on:** Nothing
**Requirements:** INT-01, CODE-01, COOK-01
**Bead:** `Videra-v259.1`
**Success Criteria:**
1. Current interaction APIs, demo flows, docs, and cookbook entry points are mapped with file-level evidence.
2. ScottPlot5 remains an inspiration boundary, not a compatibility or parity claim.
3. Dependencies identify whether Phase 402 and Phase 403 can run in parallel.
4. Validation commands and handoff expectations are recorded.
**Plans:** 401-PLAN.md

### Phase 402: Interaction and Code Experience Polish

**Goal:** Improve the first scoped set of SurfaceCharts interaction/code-experience surfaces.
**Depends on:** Phase 401
**Requirements:** INT-02, INT-03, CODE-02, CODE-03
**Bead:** `Videra-v259.2`
**Success Criteria:**
1. Changes are minimal, direct, and covered by focused tests.
2. Public API ergonomics improve without compatibility wrappers or hidden fallback behavior.
3. Existing chart boundaries remain intact.
4. Handoff records the exact changed API/demo paths.
**Plans:** 402-PLAN.md

### Phase 403: Cookbook Demo Conversion

**Goal:** Make SurfaceCharts demos and docs feel like a cookbook with copyable recipes.
**Depends on:** Phase 401
**Requirements:** COOK-02, COOK-03, CODE-03
**Bead:** `Videra-v259.3`
**Success Criteria:**
1. Cookbook entry points are discoverable from docs and demo README files.
2. Recipes use current public APIs and avoid obsolete chart controls or compatibility claims.
3. Demo/docs tests or grep checks cover the new cookbook structure.
4. Support handoff explains how to validate cookbook examples.
**Plans:** 403-PLAN.md

### Phase 404: Interaction Cookbook Final Verification

**Goal:** Close v2.59 with integrated verification and synchronized planning state.
**Depends on:** Phase 402, Phase 403
**Requirements:** VERIFY-01, VERIFY-02, VERIFY-03
**Bead:** `Videra-v259.4`
**Success Criteria:**
1. Focused tests and docs checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase artifacts, archive, branch/worktree cleanup, Git push, and Dolt Beads push are synchronized.
3. Remaining public release actions from v2.58 stay outside this milestone unless separately approved.
**Plans:** 404-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 401. Interaction and Cookbook Experience Inventory | 1/1 | Complete | 2026-04-30 |
| 402. Interaction and Code Experience Polish | 1/1 | Complete | 2026-04-30 |
| 403. Cookbook Demo Conversion | 1/1 | Complete | 2026-04-30 |
| 404. Interaction Cookbook Final Verification | 0/1 | Ready | |

---

<details>
<summary>v2.58 SurfaceCharts Controlled Release Cutover - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.58-phases`

v2.58 completed the controlled release cutover package for `0.1.0-alpha.7`: approval packet, package contracts, gated dry-run automation, release notes/support docs, final release-readiness validation, Beads export, generated roadmap, and clean Git/Dolt handoff. Public NuGet publish, public tags, and GitHub releases remain manual-gated and were not executed.

</details>
