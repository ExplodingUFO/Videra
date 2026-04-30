# Roadmap

## v2.58 SurfaceCharts Controlled Release Cutover - Active

**Goal:** Turn the completed v2.57 SurfaceCharts release-readiness evidence into a controlled release cutover package: approval packet, version/package contract finalization, gated dry-run automation, release notes/support docs, and final handoff without public publish/tag/release actions unless separately approved.

**Phases:** 5 (396-400)
**Requirements:** 19 (APPROVAL-01..04, PKG-01..04, GATE-01..04, DOC-01..04, VERIFY-01..03)
**Beads epic:** `Videra-v258`

## Phases

- [x] **Phase 396: Release Cutover Inventory and Approval Packet** - Convert v2.57 evidence into an approval packet, abort/hold criteria, and Beads/worktree coordination map.
- [ ] **Phase 397: Version and Package Cutover Contracts** - Finalize version/package metadata, package asset evidence, README/package links, and public API/package guardrails.
- [ ] **Phase 398: Gated Publish and Tag Dry-Run Automation** - Harden the non-interactive release dry-run path while keeping publish/tag/GitHub release actions manual-gated and fail-closed.
- [ ] **Phase 399: Release Notes Docs and Support Cutover** - Prepare consumer-facing release notes, cookbook/migration paths, and support artifact guidance.
- [ ] **Phase 400: Final Cutover Verification and Handoff** - Close with validation, Beads export, generated roadmap, archive, pushed state, and clean branch/worktree handoff.

## Phase Details

### Phase 396: Release Cutover Inventory and Approval Packet

**Goal:** The milestone starts from evidence, with explicit cutover gates and dependency-aware Beads.
**Depends on:** Nothing
**Requirements:** APPROVAL-01, APPROVAL-02, APPROVAL-03, APPROVAL-04
**Bead:** `Videra-v258.1`
**Success Criteria:**
1. v2.57 validation, package-consumer, docs/support, Beads, generated-roadmap, and archive artifacts are inventoried.
2. Approval packet distinguishes proven evidence from gated publish/tag/release actions.
3. Abort/hold criteria cover package, docs, validation, Beads, Git, Dolt, and remote-state failures.
4. Beads record ownership, dependencies, validation commands, and safe worktree/branch handoff expectations.
**Plans:** 396-PLAN.md

### Phase 397: Version and Package Cutover Contracts

**Goal:** SurfaceCharts package version, metadata, assets, and public API boundaries are reviewable before gated dry-run automation.
**Depends on:** Phase 396
**Requirements:** PKG-01, PKG-02, PKG-03, PKG-04
**Bead:** `Videra-v258.2`
**Success Criteria:**
1. Package version, metadata, dependency surface, symbols/assets, and README/package links are confirmed.
2. Local package assets build and inspect without public-feed mutation.
3. Public API guardrails reject unexpected API/package-scope drift.
4. `VideraChartView` and `Plot.Add.*` remain the shipped chart control/data-loading contract.
**Plans:** 397-PLAN.md

### Phase 398: Gated Publish and Tag Dry-Run Automation

**Goal:** Release automation can prove the cutover path without publishing, tagging, or creating a GitHub release.
**Depends on:** Phase 397
**Requirements:** GATE-01, GATE-02, GATE-03, GATE-04
**Bead:** `Videra-v258.3`
**Success Criteria:**
1. One non-interactive dry-run path exercises package build, package inspection, consumer smoke, focused tests, docs checks, and scope guardrails.
2. Publish, tag, and GitHub release actions require explicit approval inputs and fail closed by default.
3. Output and evidence separate pass, fail, skipped, and manual-gate states.
4. Validation keeps unsupported or blocked actions explicit and does not hide failures behind fallback behavior.
**Plans:** 398-PLAN.md

### Phase 399: Release Notes Docs and Support Cutover

**Goal:** Package consumers can understand the cutover surface and attach useful support evidence.
**Depends on:** Phase 397
**Requirements:** DOC-01, DOC-02, DOC-03, DOC-04
**Bead:** `Videra-v258.4`
**Success Criteria:**
1. Release notes/changelog content reflects the current package surface and v2.55-v2.57 SurfaceCharts outcomes.
2. Public-facing docs expose package consumption, cookbook, migration, and support paths.
3. Support guidance names exact artifacts and commands for package restore, rendering, snapshot, and cookbook smoke failures.
4. Docs preserve the ScottPlot inspiration boundary without compatibility claims.
**Plans:** 399-PLAN.md

### Phase 400: Final Cutover Verification and Handoff

**Goal:** The milestone closes with validated gated-release evidence, synchronized Beads/docs, and clean handoff.
**Depends on:** Phase 398, Phase 399
**Requirements:** VERIFY-01, VERIFY-02, VERIFY-03
**Bead:** `Videra-v258.5`
**Success Criteria:**
1. Focused validation covers package build/inspection, consumer smoke, public API guardrails, cookbook/demo docs, snapshot scope, and support artifacts.
2. Beads state, generated public roadmap, phase evidence, milestone archive, branch/worktree cleanup, Git push, and Dolt Beads push are synchronized.
3. Public publish/tag/GitHub release steps remain recorded as gated follow-up unless separately approved.
**Plans:** 400-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 396. Release Cutover Inventory and Approval Packet | 1/1 | Complete | 2026-04-30 |
| 397. Version and Package Cutover Contracts | 0/1 | Ready | |
| 398. Gated Publish and Tag Dry-Run Automation | 0/1 | Blocked by 397 | |
| 399. Release Notes Docs and Support Cutover | 0/1 | Blocked by 397 | |
| 400. Final Cutover Verification and Handoff | 0/1 | Blocked by 398, 399 | |

---

<details>
<summary>v2.57 SurfaceCharts Release Readiness and Consumer Validation - Complete (2026-04-30)</summary>

Prepared SurfaceCharts release-candidate evidence without public publish/tag scope: release-readiness inventory, public API/package metadata review, package-only consumer smoke, single validation script, release-candidate docs/support handoff, and final validation/archive evidence. 6 phases, 22 requirements. Archived: `.planning/milestones/v2.57-ROADMAP.md`

</details>

<details>
<summary>v2.56 ScottPlot 5 Interaction and Cookbook Experience - Complete (2026-04-30)</summary>

Shipped Plot lifecycle/code polish, interaction profile commands, selection/probe/draggable overlay recipes, axis rules, linked views, live view management, cookbook demo/gallery docs, and closure guardrails. 7 phases, 27 requirements. Archived: `.planning/milestones/v2.56-ROADMAP.md`

</details>
