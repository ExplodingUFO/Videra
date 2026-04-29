# Requirements: v2.52 Professional Chart Snapshot Export

**Defined:** 2026-04-29
**Core Value:** 跨平台 3D 渲染引擎的可靠性

## Milestone Intent

Add a bounded, chart-local PNG/bitmap snapshot export path for Plot-authored SurfaceCharts.

This milestone starts from the v2.51 chart output and dataset evidence model. It should let users attach an actual bitmap chart snapshot plus deterministic manifest/support evidence while keeping the export path owned by `VideraChartView.Plot` and local to the Avalonia chart control. It must not restore old chart views, reintroduce direct public `Source`, create a generic plotting engine, add PDF/vector export, widen backend/runtime scope, or hide unsupported behavior behind fallback/downshift paths.

## v2.52 Requirements

### Inventory

- [x] **INV-01**: Inventory current chart screenshot/export, output evidence, dataset evidence, demo, consumer smoke, Doctor, docs, tests, and guardrail surfaces before changing APIs.
- [x] **INV-02**: Classify bitmap snapshot gaps as implement, document, defer, or reject.
- [x] **INV-03**: Define concise target examples for Plot-owned PNG/bitmap snapshot export and manifest evidence.
- [x] **INV-04**: Record non-goals: no old chart view compatibility, no direct `Source` API, no PDF/vector export, no backend/runtime rewrite, no generic plotting engine, no hidden fallback/downshift, and no god-code workbench.

### Snapshot Contract

- [x] **SNAP-01**: Users can request a chart-local snapshot export through a Plot-owned API.
- [x] **SNAP-02**: Snapshot requests can specify pixel dimensions, DPI or scale, background behavior, and output path or stream target without exposing backend internals.
- [x] **SNAP-03**: Snapshot results include deterministic manifest metadata linking chart output evidence, dataset evidence, active series identity, dimensions, and generated artifact identity.
- [x] **SNAP-04**: Unsupported formats and invalid requests are reported through explicit diagnostics rather than fallback behavior.

### Bitmap Capture

- [ ] **CAP-01**: `VideraChartView` can produce a PNG/bitmap snapshot through the Plot-owned contract on the Avalonia chart path.
- [ ] **CAP-02**: Snapshot capture validates dimensions, path/stream target, and chart readiness with explicit errors.
- [ ] **CAP-03**: Snapshot capture remains chart-local and does not require backend expansion, renderer rewrite, or a new export subsystem.
- [ ] **CAP-04**: Focused tests cover success, invalid request, unsupported format, and deterministic manifest behavior.

### Demo And Support Evidence

- [ ] **DEMO-01**: SurfaceCharts demo exposes a bounded snapshot/export action and support summary fields for the generated artifact and manifest.
- [ ] **DEMO-02**: Consumer smoke can produce or validate snapshot manifest evidence without becoming a broad chart editor.
- [ ] **DEMO-03**: Doctor can parse snapshot artifact/manifest evidence from saved support output without launching a UI.
- [ ] **DEMO-04**: Support evidence distinguishes present, missing, unavailable, and failed snapshot states explicitly.

### Guardrails And Documentation

- [ ] **GUARD-01**: Repository guardrails prevent old chart controls and direct public `Source` APIs from returning.
- [ ] **GUARD-02**: Repository guardrails prevent snapshot export work from becoming PDF/vector export, backend expansion, generic plotting engine scope, compatibility shims, or hidden fallback/downshift behavior.
- [ ] **GUARD-03**: Package/sample docs show concise Plot-owned snapshot export and manifest usage.
- [ ] **GUARD-04**: Public roadmap and Beads state identify v2.52 phases, dependencies, and status.

### Verification

- [ ] **VER-01**: Each phase records Beads ownership, dependencies, status, and handoff notes.
- [ ] **VER-02**: Implementation phases use isolated branch/worktree execution when they change code.
- [ ] **VER-03**: Each phase ends with clean Beads status, branch/worktree cleanup, and pushed repository state.

## Future Requirements

- PDF/vector export.
- Publication layout editor, visual-regression screenshot gates, or broad chart workbench.
- Additional chart families beyond surface, waterfall, and scatter.
- Generic plotting engine semantics beyond the current chart-local Plot model.
- Backend expansion, renderer rewrite, shader/material graph, or GPU-driven chart runtime.
- Compatibility shims for removed alpha chart APIs.

## Out of Scope

| Item | Reason |
|------|--------|
| Old `SurfaceChartView` / `WaterfallChartView` / `ScatterChartView` public views | `VideraChartView` is the single shipped chart control. |
| Direct public `Source` API | `Plot.Add.*` is the public runtime data-loading path. |
| PDF/vector export | This milestone is only a bounded PNG/bitmap snapshot vertical slice. |
| Generic plotting engine | Snapshot export is scoped to the current 3D chart model only. |
| Backend expansion or renderer rewrite | Snapshot capture should stay local to the Avalonia chart control. |
| Hidden fallback/downshift | Unsupported output behavior must remain explicit diagnostics. |
| God-code workbench | Demo/support work must stay bounded and sample-first. |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| INV-01 | Phase 361 | Complete |
| INV-02 | Phase 361 | Complete |
| INV-03 | Phase 361 | Complete |
| INV-04 | Phase 361 | Complete |
| SNAP-01 | Phase 362 | Complete |
| SNAP-02 | Phase 362 | Complete |
| SNAP-03 | Phase 362 | Complete |
| SNAP-04 | Phase 362 | Complete |
| CAP-01 | Phase 363 | Planned |
| CAP-02 | Phase 363 | Planned |
| CAP-03 | Phase 363 | Planned |
| CAP-04 | Phase 363 | Planned |
| DEMO-01 | Phase 364 | Planned |
| DEMO-02 | Phase 364 | Planned |
| DEMO-03 | Phase 364 | Planned |
| DEMO-04 | Phase 364 | Planned |
| GUARD-01 | Phase 365 | Planned |
| GUARD-02 | Phase 365 | Planned |
| GUARD-03 | Phase 365 | Planned |
| GUARD-04 | Phase 365 | Planned |
| VER-01 | Phases 361-365 | Planned |
| VER-02 | Phases 362-365 | Planned |
| VER-03 | Phases 361-365 | Planned |

**Coverage:**
- v2.52 requirements: 23 total
- Mapped to phases: 23
- Unmapped: 0

---
*Requirements defined: 2026-04-29*
