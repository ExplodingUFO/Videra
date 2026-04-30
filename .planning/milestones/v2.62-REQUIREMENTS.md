# Requirements: v2.62 Native SurfaceCharts Cleanup and Old-Code Removal

## Goal

Continue SurfaceCharts toward a native, cookbook-first 3D chart experience by
removing stale old-code paths and tightening the no-compatibility boundary. This
milestone must keep the shipped `VideraChartView` + `Plot.Add.*` model as the
only current chart authoring path and must not add compatibility layers,
downshift behavior, hidden fallback, migration shims, or broad workbench scope.

## Active Requirements

### Cleanup Inventory

- **CLEAN-01**: Inventory residual SurfaceCharts old-code, compatibility,
  downshift, fallback, old chart control, direct `Source`, stale plan/docs,
  demo, CI, and guardrail surfaces before implementation.
- **CLEAN-02**: Classify each finding as shipped behavior, stale documentation,
  intentional negative guardrail text, test fixture, or true cleanup candidate.

### Native Surface

- **NATIVE-01**: Users can rely on `VideraChartView` + `Plot.Add.*` as the only
  shipped SurfaceCharts authoring path for current cookbook/demo guidance.
- **NATIVE-02**: User-facing docs, samples, recipes, and support evidence avoid
  phrasing that implies ScottPlot compatibility, old chart-control support,
  direct public `Source` loading, or automatic scenario/data-path fallback.

### Old-Code Removal

- **REMOVE-01**: True stale old-code paths discovered by the inventory are
  deleted or directly replaced instead of wrapped, bridged, or preserved through
  compatibility shims.
- **REMOVE-02**: Any remaining references to old chart controls, direct
  `Source`, compatibility, fallback, or downshift are either necessary product
  truth outside the SurfaceCharts cleanup scope or explicit negative guardrail
  evidence.

### Demo and Cookbook Simplicity

- **DEMO-01**: The SurfaceCharts cookbook/demo remains copyable and Videra-native
  after cleanup, with recipe code traceable to chart-local `Plot` ownership.
- **DEMO-02**: Demo simplification avoids god-code expansion: split only
  bounded recipe/support responsibilities that reduce coupling or make behavior
  easier to verify.

### Guardrails and CI Truth

- **GUARD-01**: Repository tests/scripts catch reintroduction of old chart
  controls, direct public `Source`, compatibility wrappers, migration shims,
  hidden fallback/downshift behavior, and fake evidence in SurfaceCharts
  cookbook/demo/support surfaces.
- **GUARD-02**: CI/test gates remain focused and honest; no check is skipped,
  weakened, over-broadened, or turned into synthetic green status.

### Verification

- **VERIFY-01**: Focused tests and scripts prove cleanup behavior, cookbook/demo
  alignment, no-compat guardrails, generated roadmap sync, and snapshot/export
  scope boundaries.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase
  archive, branch/worktree cleanup, Git push, and Dolt Beads push.

## Future Requirements

- New chart families or generic chart-engine scope.
- Broader viewer/runtime/backend cleanup outside the SurfaceCharts cookbook/API
  line.
- Package publication, tag creation, or release cutover.
- Deep performance rewrites not directly required by old-code cleanup.

## Out of Scope

- ScottPlot compatibility adapters, parity claims, wrappers, or migration shims.
- Restoring `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- Restoring a public direct `VideraChartView.Source` API.
- Compatibility wrappers for removed alpha APIs.
- Hidden fallback/downshift behavior or automatic scenario/data-path fallback.
- Renderer/backend/platform expansion.
- PDF/vector export or broad export-format expansion.
- Generic plotting-engine or god-code demo/workbench expansion.
- Fake benchmark claims, fake CI success, skipped checks used as evidence, or
  synthetic support data presented as real runtime truth.

## Traceability

- **Phase 414**: CLEAN-01, CLEAN-02, NATIVE-01, NATIVE-02, GUARD-01
- **Phase 415**: REMOVE-01, REMOVE-02, NATIVE-01, GUARD-01
- **Phase 416**: DEMO-01, DEMO-02, NATIVE-02, REMOVE-02
- **Phase 417**: GUARD-01, GUARD-02, VERIFY-01, REMOVE-02
- **Phase 418**: VERIFY-01, VERIFY-02
