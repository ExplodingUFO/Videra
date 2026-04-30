# Roadmap: v2.62 Native SurfaceCharts Cleanup and Old-Code Removal

**Goal:** Continue SurfaceCharts toward a native, cookbook-first 3D chart
experience by deleting stale old-code paths and tightening the no-compatibility
boundary around the shipped `VideraChartView` + `Plot.Add.*` model.

**Boundary:** This milestone does not add ScottPlot compatibility, old chart
controls, direct public `Source` loading, compatibility wrappers, migration
shims, hidden fallback/downshift behavior, backend expansion, generic workbench
scope, or fake validation evidence.

**Phases:** 5 (414-418)
**Requirements:** 12 (CLEAN-01..02, NATIVE-01..02, REMOVE-01..02,
DEMO-01..02, GUARD-01..02, VERIFY-01..02)
**Beads epic:** `Videra-6zx`

## Phases

- [x] **Phase 414: Native Cleanup and Old-Code Inventory** - Classify residual
  SurfaceCharts old-code, compatibility, fallback/downshift, docs/demo/test, and
  guardrail surfaces before deletion.
- [x] **Phase 415: Delete Stale SurfaceCharts Old-Code Paths** - Remove true
  stale old-code and misleading user-facing leftovers without wrappers, bridges,
  or migration shims.
- [x] **Phase 416: Native Cookbook and Demo Code Simplification** - Keep recipes
  copyable and native while reducing demo coupling only where it directly serves
  recipe ownership and verification.
- [ ] **Phase 417: No-Compat Guardrails and CI Truth** - Harden tests/scripts/CI
  so forbidden SurfaceCharts old paths and fake evidence cannot drift back.
- [ ] **Phase 418: v2.62 Final Verification** - Close v2.62 with composed
  validation, Beads export, generated roadmap, archive, push, and cleanup.

## Phase Details

### Phase 414: Native Cleanup and Old-Code Inventory

**Goal:** Establish the real cleanup surface before deleting or renaming code.
**Depends on:** Nothing
**Requirements:** CLEAN-01, CLEAN-02, NATIVE-01, NATIVE-02, GUARD-01
**Bead:** `Videra-2wb`
**Success Criteria:**
1. Residual old-code/compat/downshift/fallback hits are mapped by owner and
   classified as true cleanup, intentional negative guardrail, test fixture,
   stale docs, or product truth outside this milestone.
2. SurfaceCharts user-facing docs/demo/API risks are separated from historical
   planning docs and viewer/runtime fallback truth.
3. Phase 415, 416, and 417 write sets and validation commands are identified so
   independent beads can run in isolated worktrees where safe.
**Plans:** 414-PLAN.md
**Completed:** 2026-04-30

### Phase 415: Delete Stale SurfaceCharts Old-Code Paths

**Goal:** Remove true stale old-code paths without compatibility scaffolding.
**Depends on:** Phase 414
**Requirements:** REMOVE-01, REMOVE-02, NATIVE-01, GUARD-01
**Bead:** `Videra-9vg`
**Success Criteria:**
1. True stale SurfaceCharts old-view/direct-Source/compat leftovers are deleted
   or directly replaced with current native `VideraChartView.Plot` usage.
2. No wrapper, adapter, migration shim, hidden fallback, or downshift path is
   introduced to preserve removed behavior.
3. Focused tests prove old public chart controls and direct public `Source`
   loading remain absent.
**Plans:** 415-PLAN.md
**Completed:** 2026-04-30

### Phase 416: Native Cookbook and Demo Code Simplification

**Goal:** Keep the cookbook/demo native and maintainable after cleanup.
**Depends on:** Phase 414
**Requirements:** DEMO-01, DEMO-02, NATIVE-02, REMOVE-02
**Bead:** `Videra-w2t`
**Success Criteria:**
1. Cookbook/demo code remains traceable from recipe to `VideraChartView.Plot`
   ownership.
2. Any extracted demo support code has bounded responsibility and removes real
   coupling or verification friction.
3. The demo does not become a generic plotting workbench or compatibility layer.
**Plans:** 416-PLAN.md
**Completed:** 2026-04-30

### Phase 417: No-Compat Guardrails and CI Truth

**Goal:** Make the no-compat/no-old-code boundary durable.
**Depends on:** Phase 415, Phase 416
**Requirements:** GUARD-01, GUARD-02, VERIFY-01, REMOVE-02
**Bead:** `Videra-r9q`
**Success Criteria:**
1. Tests/scripts distinguish forbidden shipped behavior from intentional
   negative guardrail wording.
2. CI covers the focused cleanup/cookbook/support drift risks without broad fake
   checks.
3. Generated roadmap and Beads export truth still pass after all cleanup.
**Plans:** 417-PLAN.md

### Phase 418: v2.62 Final Verification

**Goal:** Close v2.62 with synchronized verification and handoff.
**Depends on:** Phase 417
**Requirements:** VERIFY-01, VERIFY-02
**Bead:** `Videra-73a`
**Success Criteria:**
1. Focused cleanup, cookbook/demo, guardrail, generated-roadmap, and scope
   checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase archive, branch/worktree
   cleanup, Git push, and Dolt Beads push are synchronized.
**Plans:** 418-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 414. Native Cleanup and Old-Code Inventory | 1/1 | Complete | 2026-04-30 |
| 415. Delete Stale SurfaceCharts Old-Code Paths | 1/1 | Complete | 2026-04-30 |
| 416. Native Cookbook and Demo Code Simplification | 1/1 | Complete | 2026-04-30 |
| 417. No-Compat Guardrails and CI Truth | 0/1 | Ready | |
| 418. v2.62 Final Verification | 0/1 | Blocked on 417 | |

---

<details>
<summary>v2.61 Native SurfaceCharts Cookbook and CI Truth - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.61-phases`

v2.61 pushed SurfaceCharts toward ScottPlot5-style cookbook usability while
staying Videra-native. It added detailed 3D cookbook/demo recipes, native
high-performance demo evidence, focused CI truth gates, Beads/generated-roadmap
sync, scope guardrails, and final clean handoff. It did not add ScottPlot
compatibility, old chart controls, hidden fallback/downshift behavior, backend
expansion, or fake validation evidence.

</details>
