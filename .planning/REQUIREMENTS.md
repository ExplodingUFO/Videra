# Requirements: v2.61 SurfaceCharts Cookbook Demo Runtime Continuity

## Goal

Keep the SurfaceCharts cookbook demo and interaction handoff usable as runtime
examples after v2.60. The milestone remains bounded to demo/docs/tests/support
evidence unless a narrow API need is proven by inventory.

## Active Requirements

### Demo Runtime

- **DEMO-01**: Inventory current demo launch/build/runtime flows, recipe
  selector behavior, support-summary route, and drift risks before selecting
  implementation slices.
- **DEMO-02**: Users can rely on cookbook demo runtime paths that match current
  public APIs and are covered by focused validation.

### Interaction Runtime Handoff

- **HANDOFF-01**: Inventory current runtime discoverability for interaction
  profile, command, probe, selection, draggable overlay, and support evidence.
- **HANDOFF-02**: Users can understand selected interaction recipes through
  concise runtime docs/demo/support evidence without new command framework,
  mouse-remapping, or chart-owned selection state.

### Verification

- **VERIFY-01**: Run focused tests and docs checks covering selected demo runtime
  QA and interaction handoff changes.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase
  archive, branch/worktree cleanup, Git push, and Dolt Beads push.

### Scope Control

- **SCOPE-01**: Phase 409 must state non-goals and dependency boundaries before
  implementation.
- **SCOPE-02**: Implementation phases must avoid ScottPlot compatibility,
  fallback/downshift behavior, old chart controls, backend expansion, PDF/vector
  export, and generic workbench scope.

## Future Requirements

- Broader chart-family expansion beyond current SurfaceCharts families.
- Full ScottPlot API parity or adapter behavior.
- Additional release/publication automation beyond existing manual-gated release
  boundaries.

## Out of Scope

- ScottPlot compatibility adapters or parity claims.
- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring a public direct `Source` API.
- Compatibility wrappers for removed alpha APIs.
- PDF/vector export or broad export-format expansion.
- Renderer/backend/platform expansion.
- Hidden fallback/downshift behavior.
- Generic plotting-engine or god-code demo/workbench expansion.

## Traceability

- **Phase 409**: DEMO-01, HANDOFF-01, SCOPE-01
- **Phase 410**: DEMO-02, VERIFY-01, SCOPE-02
- **Phase 411**: HANDOFF-02, VERIFY-01, SCOPE-02
- **Phase 412**: VERIFY-01, VERIFY-02
