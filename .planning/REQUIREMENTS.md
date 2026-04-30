# Requirements: v2.63 Native SurfaceCharts Feature and Demo Expansion

## Goal

Make SurfaceCharts feel like a richer native 3D plotting surface by adding
focused feature affordances and a stronger recipe-first demo while preserving
the single `VideraChartView` + `Plot.Add.*` route. This milestone must not add
compatibility adapters, old chart controls, hidden fallback/downshift behavior,
broad workbench scope, or external-library parity claims.

## Active Requirements

### Inventory

- **INV-01**: Inventory current SurfaceCharts feature APIs, demo gallery
  behavior, cookbook coverage, interaction affordances, support evidence, tests,
  and CI gates before implementation.
- **INV-02**: Classify each desired improvement as native chart feature,
  demo-only workflow, documentation/test truth, or out-of-scope workbench/runtime
  expansion.

### Native Feature APIs

- **FEAT-01**: Users can create common 3D chart scenarios through concise,
  native `Plot.Add.*` or `Plot`-owned convenience APIs without constructing
  internal data models manually.
- **FEAT-02**: Users can apply common chart styling and data-shaping options
  directly through bounded native APIs that remain chart-local and testable.
- **FEAT-03**: New feature affordances return typed handles or explicit result
  objects where users need to inspect, update, or validate chart state.

### Annotation and Measurement

- **MARK-01**: Users can add bounded chart-local labels, markers, or reference
  elements that attach to 3D chart coordinates and render consistently with the
  current camera/projection model.
- **MARK-02**: Users can inspect or measure meaningful chart relationships
  through explicit support evidence without inventing benchmark or precision
  claims.

### Demo Gallery

- **DEMO-01**: Users can browse a richer recipe-first SurfaceCharts demo gallery
  with clear categories, scenario selection, copyable snippets, and visible chart
  outcomes.
- **DEMO-02**: Demo code remains bounded by recipe/catalog/support
  responsibilities and does not become a generic plotting workbench or god-code
  shell.
- **DEMO-03**: Demo support summaries describe the active recipe, feature
  surface, dataset scale, and rendering evidence truthfully.

### Cookbook and CI Truth

- **TRUTH-01**: Cookbook docs and repository tests cover every shipped v2.63
  feature/demo workflow with direct native examples.
- **TRUTH-02**: CI and release-readiness filters include the new feature,
  demo, generated-roadmap, and no-fake-evidence checks.
- **TRUTH-03**: Guardrails prevent reintroducing compatibility claims, old chart
  controls, hidden fallback/downshift behavior, or skipped checks presented as
  evidence.

### Verification

- **VERIFY-01**: Focused tests prove feature APIs, annotations/measurements,
  demo gallery behavior, cookbook truth, and CI guardrails.
- **VERIFY-02**: Synchronize Beads state, generated public roadmap, phase
  archive, branch/worktree cleanup, Git push, and Dolt Beads push.

## Future Requirements

- New renderer/backend/platform architecture.
- Generic plotting workbench or plugin editor.
- Package publication, public tag creation, or release cutover.
- Broad performance rewrites not required by the selected feature workflows.

## Out of Scope

- Compatibility adapters, parity claims, wrappers, or migration shims for any
  external plotting library.
- Restoring old chart controls or direct public `Source` loading.
- Hidden fallback/downshift behavior or automatic data-path substitution.
- Generic chart-engine scope, broad workbench scope, or god-code demo shell.
- PDF/vector export or broad export-format expansion.
- Fake benchmark claims, fake CI success, skipped checks used as evidence, or
  synthetic support data presented as real runtime truth.

## Traceability

- **Phase 419**: INV-01, INV-02, FEAT-01, MARK-01, DEMO-01, TRUTH-01
- **Phase 420**: FEAT-01, FEAT-02, FEAT-03
- **Phase 421**: MARK-01, MARK-02, FEAT-03
- **Phase 422**: DEMO-01, DEMO-02, DEMO-03
- **Phase 423**: TRUTH-01, TRUTH-02, TRUTH-03, VERIFY-01
- **Phase 424**: VERIFY-01, VERIFY-02
