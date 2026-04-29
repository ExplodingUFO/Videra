# Roadmap

## v2.57 SurfaceCharts Release Readiness and Consumer Validation - In Progress

**Goal:** Prepare SurfaceCharts for a release-candidate decision by proving package metadata, public API boundaries, local package consumption, validation scripts, docs navigation, and support handoff without publishing a public package or tag.

**Phases:** 6 (390-395)
**Requirements:** 22 (INV-01..03, API-01..02, PKG-01..02, CONSUME-01..04, CI-01..04, DOC-01..04, VER-01..03)

## Phases

- [x] **Phase 390: Release Readiness Inventory and Beads Coordination** - Confirm package/docs/smoke/CI/guardrail surfaces, release-candidate boundaries, and Beads/worktree ownership.
- [x] **Phase 391: Public API and Package Metadata Review** - Catalog public SurfaceCharts API and package metadata while tightening release-readiness guardrails.
- [ ] **Phase 392: Local Package Consumer Smoke** - Prove a clean consumer can restore, compile, and exercise public package APIs for the cookbook path.
- [ ] **Phase 393: Release Validation Script and CI Alignment** - Add a single release-readiness validation path with clear local/manual boundaries.
- [ ] **Phase 394: Release Candidate Docs and Support Handoff** - Update README/demo docs, migration notes, and support artifact guidance for package consumers.
- [ ] **Phase 395: Integration, Guardrails, and Milestone Evidence** - Close with focused tests, release-readiness script, Beads export, generated roadmap, and clean handoff.

## Phase Details

### Phase 390: Release Readiness Inventory and Beads Coordination
**Goal:** The milestone starts from evidence, with clear release-candidate boundaries and dependency-aware Beads.
**Depends on**: Nothing
**Requirements**: INV-01, INV-02, INV-03
**Success Criteria**:
1. Current package, sample, docs, smoke, CI, guardrail, and Beads surfaces are documented.
2. Release-candidate boundaries distinguish local readiness from public publish/tag scope.
3. Beads exist for every phase with parent-child links, blocking dependencies, validation notes, and safe worktree/branch handoff notes.
4. `bd ready --json` shows only genuinely unblocked work.
**Plans**: 390-PLAN.md

### Phase 391: Public API and Package Metadata Review
**Goal:** SurfaceCharts public API and package metadata are reviewable before package consumer validation starts.
**Depends on**: Phase 390
**Requirements**: API-01, API-02, PKG-01, PKG-02
**Success Criteria**:
1. Public API catalog covers chart control, Plot, plottables, interactions, overlays, axes, live helpers, and snapshot export.
2. Guardrails reject old chart controls, direct public `Source`, compatibility wrappers, hidden fallback/downshift, and unexpected public API growth.
3. Package metadata, README links, assembly identity, and dependency boundaries are explicit.
4. Local package output can be inspected without public publish credentials.
**Plans**: 391-PLAN.md

### Phase 392: Local Package Consumer Smoke
**Goal:** A clean consumer can use the locally built SurfaceCharts package through public APIs only.
**Depends on**: Phase 391
**Requirements**: CONSUME-01, CONSUME-02, CONSUME-03, CONSUME-04
**Success Criteria**:
1. Consumer project restores and compiles against locally built package output.
2. Consumer smoke covers first chart, interaction profile, selection/probe, axis/live view, and snapshot entry points.
3. Consumer smoke uses package/public APIs only, not internal source references.
4. Consumer artifacts are deterministic and useful for support handoff.
**Plans**: 392-PLAN.md

### Phase 393: Release Validation Script and CI Alignment
**Goal:** Release-readiness checks become a single repeatable local path with explicit CI/manual boundaries.
**Depends on**: Phase 392
**Requirements**: CI-01, CI-02, CI-03, CI-04
**Success Criteria**:
1. A single script runs package build, consumer smoke, focused SurfaceCharts tests, demo/docs tests, and scope guardrails.
2. Output separates pass/fail, skipped publish/tag steps, and environment warnings.
3. CI docs explain mandatory checks before public release and local/manual checks.
4. Validation does not hide failures behind broad fallback behavior.
**Plans**: 393-PLAN.md

### Phase 394: Release Candidate Docs and Support Handoff
**Goal:** Package consumers can find cookbook recipes, migration notes, and support artifacts without reading internal planning docs.
**Depends on**: Phase 392
**Requirements**: DOC-01, DOC-02, DOC-03, DOC-04
**Success Criteria**:
1. Root README and demo README expose release-candidate package consumption and cookbook paths.
2. Migration notes summarize the Plot-owned model, removed old controls, and non-goals.
3. Support guidance names exact artifacts to attach for package consumption or rendering evidence failures.
4. Docs preserve the ScottPlot inspiration boundary without claiming compatibility.
**Plans**: 394-PLAN.md

### Phase 395: Integration, Guardrails, and Milestone Evidence
**Goal:** The milestone closes with release-readiness evidence, Beads export, generated roadmap, and clean handoff.
**Depends on**: Phase 393, Phase 394
**Requirements**: VER-01, VER-02, VER-03
**Success Criteria**:
1. Focused tests and release-readiness script cover public API guardrails, package consumer smoke, cookbook/demo docs, snapshot scope, and support artifacts.
2. Beads state, generated public roadmap, phase evidence, branch/worktree state, and milestone archive are synchronized.
3. Public publish/tag/package-release steps remain explicitly out of scope.
**Plans**: 395-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 390. Release Readiness Inventory and Beads Coordination | 1/1 | Complete | 2026-04-30 |
| 391. Public API and Package Metadata Review | 1/1 | Complete | 2026-04-30 |
| 392. Local Package Consumer Smoke | 0/1 | Pending | - |
| 393. Release Validation Script and CI Alignment | 0/1 | Pending | - |
| 394. Release Candidate Docs and Support Handoff | 0/1 | Pending | - |
| 395. Integration, Guardrails, and Milestone Evidence | 0/1 | Pending | - |

---

<details>
<summary>v2.56 ScottPlot 5 Interaction and Cookbook Experience - Complete (2026-04-30)</summary>

Shipped Plot lifecycle/code polish, interaction profile commands, selection/probe/draggable overlay recipes, axis rules, linked views, live view management, cookbook demo/gallery docs, and closure guardrails. 7 phases, 27 requirements. Archived: `.planning/milestones/v2.56-ROADMAP.md`

</details>
