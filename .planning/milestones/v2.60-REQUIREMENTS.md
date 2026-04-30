# Requirements: v2.60 SurfaceCharts Cookbook QA and Interaction Handoff

## Goal

Audit and harden the v2.59 cookbook/interaction handoff with focused QA, docs,
support, and validation work. This milestone keeps the SurfaceCharts workflow
simple and direct without introducing ScottPlot compatibility, hidden fallbacks,
backend expansion, or a generic chart workbench.

## Active Requirements

### Cookbook QA

- **QA-01**: Inventory current cookbook docs, demo recipes, text-contract tests,
  support routes, and drift risks before selecting implementation slices.
- **QA-02**: Users can rely on cookbook docs/demo recipes that match current
  public APIs and are covered by focused validation.

### Interaction Handoff

- **HANDOFF-01**: Inventory current interaction profile, command, probe,
  selection, draggable overlay, and support-summary handoff surfaces.
- **HANDOFF-02**: Users can understand the selected interaction handoff through
  concise docs/demo/support evidence without a new command framework or mouse
  remapping surface.

### Verification

- **VERIFY-01**: Run focused tests and docs checks covering selected cookbook QA
  and interaction handoff changes.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase
  archive, branch/worktree cleanup, Git push, and Dolt Beads push.

### Scope Control

- **SCOPE-01**: Phase 405 must state non-goals and dependency boundaries before
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

- **Phase 405**: QA-01, HANDOFF-01, SCOPE-01
- **Phase 406**: QA-02, VERIFY-01, SCOPE-02
- **Phase 407**: HANDOFF-02, VERIFY-01, SCOPE-02
- **Phase 408**: VERIFY-01, VERIFY-02
