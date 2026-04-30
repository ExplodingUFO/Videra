# Requirements: v2.61 Native SurfaceCharts Cookbook and CI Truth

## Goal

Push SurfaceCharts toward ScottPlot5-style cookbook usability while staying
native to Videra's 3D chart model. The milestone must produce detailed runnable
demo cookbook recipes, honest performance/CI evidence, and no compatibility
layer, hidden fallback, downshift path, or fake validation.

## Active Requirements

### Cookbook Experience

- **COOKBOOK-01**: Inventory current demo cookbook recipes, snippet sources,
  docs routes, and drift risks before selecting implementation slices.
- **COOKBOOK-02**: Users can follow detailed 3D chart cookbook recipes for the
  shipped Videra chart families and common workflows.
- **COOKBOOK-03**: Cookbook snippets document required imports, host wiring,
  data setup, interaction setup, support evidence, and snapshot routes without
  relying on unexplained hidden state.

### Native 3D Semantics

- **NATIVE-01**: Inventory ScottPlot5-inspired ergonomics and translate them to
  Videra-native 3D chart concepts without API parity or compatibility claims.
- **NATIVE-02**: Users can see native Videra chart ownership boundaries:
  `VideraChartView`, `Plot.Add.*`, chart-local interactions, host-owned
  selection/draggable state, and PNG-only chart-local snapshots.

### Performance Truth

- **PERF-01**: Inventory current demo/data paths and identify which examples are
  performance-sensitive.
- **PERF-02**: Users can trust that cookbook examples use existing efficient
  data paths and that performance evidence is truthful, bounded, and not a fake
  benchmark guarantee.

### CI and Validation Truth

- **CI-01**: Inventory current CI/test gates relevant to cookbook docs, demo
  runtime, interaction handoff, support evidence, Beads export, generated
  roadmap, and scope guardrails.
- **CI-02**: CI/test design for this milestone is focused, deterministic, and
  able to pass honestly without skipping, weakening, or fabricating checks.

### Verification

- **VERIFY-01**: Run focused tests and docs checks covering selected cookbook,
  native-performance, and CI truth changes.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase
  archive, branch/worktree cleanup, Git push, and Dolt Beads push.

### Scope Control

- **SCOPE-01**: All phases must avoid ScottPlot compatibility, fallback/downshift
  behavior, old chart controls, backend expansion, PDF/vector export, generic
  plotting workbench scope, and fake evidence.

## Future Requirements

- Broader chart-family expansion beyond current SurfaceCharts families.
- Full ScottPlot API parity or adapter behavior.
- Additional release/publication automation beyond existing manual-gated release
  boundaries.
- Native backend architecture work unrelated to cookbook/demo evidence.

## Out of Scope

- ScottPlot compatibility adapters, parity claims, wrappers, or migration shims.
- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring a public direct `Source` API.
- Compatibility wrappers for removed alpha APIs.
- PDF/vector export or broad export-format expansion.
- Renderer/backend/platform expansion.
- Hidden fallback/downshift behavior.
- Generic plotting-engine or god-code demo/workbench expansion.
- Fake benchmark claims, fake CI success, skipped checks used as evidence, or
  synthetic support data presented as real runtime truth.

## Traceability

- **Phase 409**: COOKBOOK-01, NATIVE-01, PERF-01, CI-01, SCOPE-01
- **Phase 410**: COOKBOOK-02, COOKBOOK-03, NATIVE-02, VERIFY-01, SCOPE-01
- **Phase 411**: PERF-02, NATIVE-02, VERIFY-01, SCOPE-01
- **Phase 412**: CI-02, VERIFY-01, VERIFY-02, SCOPE-01
- **Phase 413**: VERIFY-02
