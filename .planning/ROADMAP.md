# Roadmap: v2.64 Native SurfaceCharts Analysis Workspace and Streaming Evidence

**Goal:** Move SurfaceCharts from isolated chart recipes into native analysis
workflows with multi-chart composition, linked interaction, high-density and
streaming evidence, scenario cookbook templates, and CI/release-readiness truth.

**Boundary:** This milestone does not add compatibility adapters, parity claims,
old chart controls, direct public `Source` loading, hidden fallback/downshift
behavior, renderer/backend expansion, generic workbench scope, package
publication, or fake validation evidence.

**Phases:** 7 (425-431)
**Requirements:** 20 (INV-01..03, WORK-01..03, LINK-01..03,
STREAM-01..03, COOK-01..03, TRUTH-01..03, VERIFY-01..02)
**Beads epic:** `Videra-7tqx`

## Phases

- [x] **Phase 425: Analysis Workspace and Streaming Inventory** - Map current
  multi-chart, linked interaction, streaming/high-density, cookbook,
  package-smoke, and CI truth surfaces before implementation.
- [x] **Phase 426: Native Multi-Chart Analysis Workspace** - Add bounded native
  multi-chart layout/workspace affordances without creating a generic workbench.
- [ ] **Phase 427: Linked Interaction and Selection Propagation** - Add linked
  camera/axis/probe/selection workflows with host-owned state and explicit
  support evidence.
- [ ] **Phase 428: High-Density and Streaming Data Evidence** - Strengthen
  streaming and large-data workflows with real ingestion/window/cache evidence.
- [ ] **Phase 429: Scenario Cookbook and Package Templates** - Turn the richer
  workflows into detailed native cookbook scenarios and package-consumer
  templates.
- [ ] **Phase 430: CI Performance and Release-Readiness Truth** - Align focused
  tests, CI, consumer smoke, release-readiness filters, generated roadmap, and
  no-fake-evidence guardrails.
- [ ] **Phase 431: v2.64 Final Verification** - Close v2.64 with composed
  validation, Beads export, generated roadmap sync, phase archive, push, and
  cleanup.

## Phase Details

### Phase 425: Analysis Workspace and Streaming Inventory

**Goal:** Establish the real v2.64 workflow surface before changing APIs, demo,
or validation behavior.
**Depends on:** Nothing
**Requirements:** INV-01, INV-02, INV-03
**Bead:** `Videra-7tqx.1`
**Success Criteria:**
1. Current multi-chart, linked interaction, streaming/high-density, cookbook,
   package-smoke, CI, and release-readiness owners are mapped.
2. Candidate work is classified as native workflow API, demo/sample scenario,
   performance evidence, CI/release truth, or out-of-scope expansion.
3. Phase 426-430 write sets, dependencies, validation commands, and safe
   worktree split points are identified.
**Plans:** 425-PLAN.md
**Completed:** 2026-04-30

### Phase 426: Native Multi-Chart Analysis Workspace

**Goal:** Add bounded native analysis layout affordances for comparing multiple
SurfaceCharts panels.
**Depends on:** Phase 425
**Requirements:** WORK-01, WORK-02, WORK-03
**Bead:** `Videra-7tqx.2`
**Success Criteria:**
1. Users can compose selected SurfaceCharts panels into a bounded analysis
   layout using native APIs and demo paths.
2. Workspace evidence reports active panel identity, chart kinds, recipe
   context, dataset scale, and rendering status.
3. Demo/sample code keeps layout, scenario catalog, and support summary
   responsibilities separated.
Plans:
- [ ] 426-01-PLAN.md — Workspace contracts and core registration
- [ ] 426-02-PLAN.md — Link group, aggregate status, evidence, and demo integration

### Phase 427: Linked Interaction and Selection Propagation

**Goal:** Make linked panel interaction useful and explicit without hiding state
ownership.
**Depends on:** Phase 426
**Requirements:** LINK-01, LINK-02, LINK-03
**Bead:** `Videra-7tqx.3`
**Success Criteria:**
1. Users can link camera, axis, or view-state behavior across selected panels.
2. Probe, selection, and measurement context can propagate across linked panels
   while host-owned state remains explicit.
3. Support summaries describe linked panels, active interaction surfaces, and
   evidence boundaries truthfully.
Plans:
- [x] 427-01-PLAN.md — Filtered link classes and link group policy implementation
- [x] 427-02-PLAN.md — Interaction propagator, linked interaction evidence, and demo scenario

### Phase 428: High-Density and Streaming Data Evidence

**Goal:** Prove streaming and high-density workflows with real runtime evidence.
**Depends on:** Phase 427
**Requirements:** STREAM-01, STREAM-02, STREAM-03
**Bead:** `Videra-7tqx.4`
**Success Criteria:**
1. A high-density or streaming SurfaceCharts scenario runs with explicit
   ingestion/window/cache evidence.
2. Update, retention, and visible-window behavior stay bounded and do not
   perform hidden data-path substitution.
3. Performance evidence reports real scenario scope, dataset size, limits, and
   non-goals without benchmark overclaims.
**Plans:** 428-PLAN.md

### Phase 429: Scenario Cookbook and Package Templates

**Goal:** Convert the v2.64 workflows into copyable native recipes and package
consumer templates.
**Depends on:** Phase 428
**Requirements:** COOK-01, COOK-02, COOK-03
**Bead:** `Videra-7tqx.5`
**Success Criteria:**
1. Cookbook docs cover multi-chart analysis, linked interaction, high-density
   data, streaming updates, and support evidence.
2. Package-consumer templates demonstrate supported public package workflows
   with copyable code.
3. Repository tests verify snippets, template claims, support wording, and
   scope boundaries.
**Plans:** 429-PLAN.md

### Phase 430: CI Performance and Release-Readiness Truth

**Goal:** Keep the larger workflow surface honest in CI and release-readiness.
**Depends on:** Phase 429
**Requirements:** TRUTH-01, TRUTH-02, TRUTH-03, VERIFY-01
**Bead:** `Videra-7tqx.6`
**Success Criteria:**
1. Focused tests and CI filters cover workspace, linked interaction, streaming,
   cookbook, package templates, and generated roadmap checks.
2. Release-readiness validation includes the new package-consumer evidence
   without counting skipped or unavailable checks as success.
3. Guardrails continue to reject compatibility claims, old chart paths, hidden
   fallback/downshift, broad workbench scope, backend expansion, and fake
   evidence.
**Plans:** 430-PLAN.md

### Phase 431: v2.64 Final Verification

**Goal:** Close v2.64 with synchronized verification and handoff.
**Depends on:** Phase 430
**Requirements:** VERIFY-01, VERIFY-02
**Bead:** `Videra-7tqx.7`
**Success Criteria:**
1. Focused workflow, streaming, cookbook, CI, generated-roadmap, release
   readiness, and scope checks pass or are explicitly reported.
2. Beads state, generated public roadmap, phase archive, branch/worktree
   cleanup, Git push, and Dolt Beads push are synchronized.
**Plans:** 431-PLAN.md

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 425. Analysis Workspace and Streaming Inventory | 1/1 | Complete | 2026-04-30 |
| 426. Native Multi-Chart Analysis Workspace | 2/2 | Complete | 2026-04-30 |
| 427. Linked Interaction and Selection Propagation | 2/2 | Complete | 2026-04-30 |
| 428. High-Density and Streaming Data Evidence | 1/1 | Complete | 2026-04-30 |
| 429. Scenario Cookbook and Package Templates | 0/1 | Blocked | |
| 430. CI Performance and Release-Readiness Truth | 0/1 | Blocked | |
| 431. v2.64 Final Verification | 0/1 | Blocked | |

---

<details>
<summary>v2.63 Native SurfaceCharts Feature and Demo Expansion - Complete (2026-04-30)</summary>

Archived phase artifacts: `.planning/milestones/v2.63-phases`

v2.63 added focused native Bar/Contour/styling/data-shaping affordances,
bounded annotation/measurement workflows, richer recipe-first demo behavior,
and real package-consumer release-readiness truth while preserving the single
native `VideraChartView` + `Plot.Add.*` route.

</details>
