# Requirements: v2.64 Native SurfaceCharts Analysis Workspace and Streaming Evidence

## Goal

Move SurfaceCharts from isolated chart recipes into native analysis workflows
with multi-chart composition, linked interaction, high-density and streaming
evidence, scenario cookbook templates, and CI/release-readiness truth. This
milestone must keep the single `VideraChartView` + `Plot.Add.*` route and must
not add compatibility adapters, old chart controls, hidden fallback/downshift
behavior, broad workbench scope, backend expansion, or external-library parity
claims.

## Active Requirements

### Inventory

- **INV-01**: Inventory current multi-chart, linked interaction, streaming,
  high-density, cookbook, package-smoke, CI, and release-readiness surfaces
  before implementation.
- **INV-02**: Classify desired improvements as native workflow API, demo/sample
  scenario, performance/evidence truth, CI/release truth, or out-of-scope
  workbench/runtime expansion.
- **INV-03**: Identify phase write sets, dependency order, and parallelizable
  child beads so implementation can use isolated worktrees safely.

### Analysis Workspace

- **WORK-01**: Users can compose a bounded multi-chart analysis layout from
  native SurfaceCharts panels without introducing a generic workbench shell.
- **WORK-02**: Users can inspect active panel identity, chart kind, recipe
  context, dataset scale, and rendering status through explicit workspace
  evidence.
- **WORK-03**: Demo/sample code keeps layout, scenario catalog, and support
  summary responsibilities separated rather than accumulating god-code in a
  window code-behind.

### Linked Interaction

- **LINK-01**: Users can link camera, axis, or view-state behavior across
  selected SurfaceCharts panels through bounded native contracts.
- **LINK-02**: Users can propagate probe, selection, or measurement context
  across linked panels while keeping selection ownership explicit in the host.
- **LINK-03**: Linked interaction support summaries explain which panels are
  linked, which interaction surfaces are active, and which evidence is runtime
  truth rather than benchmark truth.

### High-Density and Streaming

- **STREAM-01**: Users can run a high-density or streaming SurfaceCharts
  scenario with explicit ingestion/window/cache state and deterministic support
  evidence.
- **STREAM-02**: Streaming and large-data workflows expose bounded update,
  retention, and visible-window behavior without hidden data-path substitution.
- **STREAM-03**: Performance evidence uses real runnable scenarios and reports
  scope, dataset size, and limitations without fake benchmark claims.

### Cookbook and Package Templates

- **COOK-01**: Cookbook docs include native scenario recipes for multi-chart
  analysis, linked interaction, high-density data, streaming updates, and
  support evidence.
- **COOK-02**: Package-consumer templates demonstrate the supported workflow
  surface using public packages and copyable code paths.
- **COOK-03**: Repository tests verify cookbook snippets, package template
  claims, support artifact wording, and scope boundaries.

### CI and Release Truth

- **TRUTH-01**: Focused tests and CI filters include the new workspace, linked
  interaction, streaming, cookbook, package-template, and generated-roadmap
  checks.
- **TRUTH-02**: Release-readiness validation includes the new package-consumer
  evidence without counting skipped, unavailable, or unsupported checks as
  success.
- **TRUTH-03**: Guardrails reject old chart controls, direct public `Source`,
  hidden fallback/downshift, broad workbench scope, backend expansion,
  external-library compatibility/parity claims, and fake evidence.

### Verification

- **VERIFY-01**: Focused validation proves all v2.64 workflow, cookbook, CI, and
  release-readiness requirements before milestone closeout.
- **VERIFY-02**: Beads state, generated public roadmap, phase archive,
  branch/worktree cleanup, Git push, and Dolt Beads push are synchronized.

## Future Requirements

- Actual public package publication, public tag creation, or GitHub Release.
- Broad renderer/backend/platform expansion.
- Generic dashboard/workbench/plugin editor.
- New chart families beyond workflows needed by the selected analysis and
  streaming scenarios.
- Deep benchmark-driven renderer rewrites not justified by v2.64 evidence.

## Out of Scope

- Compatibility adapters, parity claims, wrappers, or migration shims for any
  external plotting library.
- Restoring old chart controls or direct public `Source` loading.
- Hidden fallback/downshift behavior or automatic data-path substitution.
- Generic chart-engine scope, broad workbench scope, plugin editor, or god-code
  demo shell.
- Backend expansion, OpenGL/WebGL work, or platform-hosting expansion.
- PDF/vector export or broad export-format expansion.
- Fake benchmark claims, fake CI success, skipped checks used as evidence, or
  synthetic support data presented as real runtime truth.

## Traceability

- **Phase 425**: INV-01, INV-02, INV-03
- **Phase 426**: WORK-01, WORK-02, WORK-03
- **Phase 427**: LINK-01, LINK-02, LINK-03
- **Phase 428**: STREAM-01, STREAM-02, STREAM-03
- **Phase 429**: COOK-01, COOK-02, COOK-03
- **Phase 430**: TRUTH-01, TRUTH-02, TRUTH-03, VERIFY-01
- **Phase 431**: VERIFY-01, VERIFY-02
