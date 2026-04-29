# Roadmap: v2.52 Professional Chart Snapshot Export

## Milestone Goal

Add a bounded chart-local PNG/bitmap snapshot export path for Plot-authored SurfaceCharts, with deterministic artifact manifest evidence and support tooling, without widening backend/runtime scope or restoring removed alpha APIs.

## Phase Plan

| Phase | Name | Goal | Requirements | Bead | Status |
|-------|------|------|--------------|------|--------|
| 361 | Chart Snapshot Export Inventory | Inventory current chart screenshot/export, output evidence, demo/support, Doctor, docs, tests, and guardrail surfaces before changing APIs. | INV-01, INV-02, INV-03, INV-04, VER-01, VER-03 | Videra-lu9.1 | Complete |
| 362 | Plot Snapshot Export Contract | Add a bounded Plot-owned snapshot request/result contract for PNG/bitmap outputs. | SNAP-01, SNAP-02, SNAP-03, SNAP-04, VER-01, VER-02, VER-03 | Videra-lu9.2 | Complete |
| 363 | Chart Snapshot Capture Implementation | Implement the minimal Avalonia-local bitmap snapshot path for `VideraChartView.Plot`. | CAP-01, CAP-02, CAP-03, CAP-04, VER-01, VER-02, VER-03 | Videra-lu9.3 | Complete |
| 364 | Demo Smoke Doctor Snapshot Evidence | Refresh demo, consumer smoke, support artifacts, and Doctor parsing around snapshot artifacts and manifests. | DEMO-01, DEMO-02, DEMO-03, DEMO-04, VER-01, VER-02, VER-03 | Videra-lu9.4 | Planned (plan ready) |
| 365 | Snapshot Export Guardrails and Docs | Close docs, public roadmap, guardrails, and Beads state around chart-local bitmap snapshot export scope. | GUARD-01, GUARD-02, GUARD-03, GUARD-04, VER-01, VER-02, VER-03 | Videra-lu9.5 | Planned (plan ready) |

## Phase Details

### Phase 361: Chart Snapshot Export Inventory

**Status:** complete
**Bead:** Videra-lu9.1
**Goal:** Map the current chart screenshot/export, output evidence, dataset evidence, demo/support, consumer smoke, Doctor, docs, tests, and guardrail surfaces before changing code.

**Success criteria:**
1. Inventory classifies bitmap snapshot/export gaps as implement, document, defer, or reject. ✅
2. Target examples show concise Plot-owned snapshot export and manifest usage. ✅
3. Non-goals explicitly reject old chart views, direct `Source`, PDF/vector export, generic plotting engine scope, backend expansion, hidden fallback/downshift, and god-code. ✅
4. Handoff identifies implementation owners and write boundaries. ✅

**Plans:** 1 plan

Plans:
- [x] 361-01-PLAN.md — Scan all chart export/snapshot surfaces, classify gaps, define non-goals and handoff

**Output:** `.planning/phases/361-chart-snapshot-export-inventory/SUMMARY.md`

### Phase 362: Plot Snapshot Export Contract

**Status:** complete
**Bead:** Videra-lu9.2
**Depends on:** Phase 361
**Goal:** Add a bounded chart-local snapshot request/result contract owned by Plot APIs.

**Success criteria:**
1. `VideraChartView.Plot` owns the snapshot request/result surface. ✅
2. Snapshot request captures dimensions, scale/DPI, background behavior, and target semantics without backend internals. ✅
3. Snapshot result includes deterministic manifest metadata linked to output and dataset evidence. ✅
4. Unsupported formats and invalid requests return explicit diagnostics rather than fallback behavior. ✅

**Plans:** 1 plan

Plans:
- [x] 362-01-PLAN.md — Create snapshot contract types (request/result/manifest/format/background/diagnostic) with unit tests

**Output:** `.planning/phases/362-plot-snapshot-export-contract/362-01-SUMMARY.md`

### Phase 363: Chart Snapshot Capture Implementation

**Status:** planned
**Bead:** Videra-lu9.3
**Depends on:** Phase 362
**Goal:** Implement a minimal Avalonia-local PNG/bitmap snapshot path for `VideraChartView.Plot`.

**Success criteria:**
1. Snapshot capture produces a PNG/bitmap artifact or stream through the Plot-owned contract.
2. Capture validates dimensions, target, and chart readiness with explicit errors.
3. The implementation stays chart-local and does not widen backend/runtime contracts.
4. Focused tests cover success, invalid request, unsupported format, and manifest determinism.

**Plans:** 1 plan

Plans:
- [ ] 363-01-PLAN.md — Add CaptureSnapshotAsync, render bridge, update diagnostics, and capture tests

**Output:** `.planning/phases/363-chart-snapshot-capture-implementation/SUMMARY.md`

### Phase 364: Demo Smoke Doctor Snapshot Evidence

**Status:** planned
**Bead:** Videra-lu9.4
**Depends on:** Phase 363
**Goal:** Refresh demo, consumer smoke, support summaries, and Doctor parsing around snapshot artifacts and manifests.

**Success criteria:**
1. Demo exposes a bounded snapshot action and support-summary fields for artifact and manifest state.
2. Consumer smoke can produce or validate snapshot manifest evidence.
3. Doctor parses snapshot present/missing/unavailable/failed states without launching UI.
4. Demo remains bounded and does not become a broad chart editor or workbench.

**Plans:** 1 plan

Plans:
- [ ] 364-01-PLAN.md — Add snapshot capture to demo, consumer smoke, support summaries, and Doctor parsing

**Output:** `.planning/phases/364-demo-smoke-doctor-snapshot-evidence/SUMMARY.md`

### Phase 365: Snapshot Export Guardrails and Docs

**Status:** planned (plan ready)
**Bead:** Videra-lu9.5
**Depends on:** Phase 364
**Goal:** Align docs, public roadmap, repository guardrails, and Beads state around the chart-local bitmap snapshot export contract.

**Success criteria:**
1. Repository guardrails block old chart controls and direct public `Source` APIs from returning.
2. Guardrails keep snapshot export work out of PDF/vector export, backend expansion, generic plotting engine scope, compatibility wrappers, hidden fallback/downshift behavior, and god-code.
3. Package/sample docs show concise Plot-owned snapshot export and manifest usage.
4. Beads/public roadmap and local planning state are clean.

**Plans:** 1 plan

Plans:
- [ ] 365-01-PLAN.md — Add scope guardrails, update docs, close Beads, mark milestone complete

**Output:** `.planning/phases/365-snapshot-export-guardrails-and-docs/SUMMARY.md`

## Dependencies

- Phase 361 blocks all implementation phases.
- Phase 362 waits for Phase 361.
- Phase 363 waits for Phase 362.
- Phase 364 waits for Phase 363.
- Phase 365 waits for Phase 364.

## Next

Phase 363 complete. Phase 364 planned. Proceed to execute Phase 364:

```bash
$gsd-execute-phase 364
```
