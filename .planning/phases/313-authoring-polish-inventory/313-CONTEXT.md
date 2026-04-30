# Phase 313 Context: Authoring Polish Inventory

## Bead

- `Videra-sge`

## Goal

Inventory the current authoring, SurfaceCharts precision, optional workbench, and public roadmap seams before v2.43 implementation starts.

## Current State

### Core Authoring

`Videra.Core` already has a static-scene authoring surface:

- `SceneAuthoring.Create(...)`
- `SceneAuthoringBuilder`
- `SceneGeometry`
- `SceneMaterials`
- `SceneAuthoringDiagnostic`
- `InstanceBatchDescriptor`

Supported helpers already include mesh buffers, triangle, quad, plane, grid, polyline, point cloud, cube, explicit instances, and explicit instance batches. Existing tests prove `SceneDocument` output, retained imported asset truth, diagnostics, duplicate ids, and instance-batch performance truth.

Important gap: authoring has no sphere helper, no axis helper, and no dedicated minimal authoring sample. `samples/Videra.MinimalSample` still demonstrates model loading/import rather than file-free authored scenes.

### SurfaceCharts Precision

SurfaceCharts already has chart-local formatting for axis and legend labels:

- `SurfaceChartOverlayOptions.FormatLabel(...)`
- `SurfaceChartNumericLabelFormat`
- `SurfaceChartNumericLabelPresets`
- `SurfaceChartOverlayPresets`

Axis and legend presenters consume the policy, but probe readouts still format numbers directly in `SurfaceProbeOverlayPresenter` and `SurfaceProbeOverlayState` with hardcoded `0.###` and delta formats.

### Optional Workbench

There is no `Workbench`, `DiagnosticsPanel`, or `SceneExplorer` surface. `Videra.Demo` already contains support capture and diagnostics-oriented UI, but it is a demo, not an optional reusable adoption surface. `Videra.Avalonia` has public snapshot/diagnostics services that a sample-first or optional tooling slice can compose.

### Public Roadmap

The repository documents Beads coordination in `docs/beads-coordination.md`, and tests cover coordination docs. There is no generated public roadmap artifact and no script that summarizes `.beads/issues.jsonl` for readers who do not run Docker/Dolt.

## Non-Goals

- Do not rename the existing authoring surface as part of inventory.
- Do not add ECS, update loops, physics, animation, material graphs, backend abstractions, hidden fallback, or compatibility layers.
- Do not move chart semantics into `VideraView`.
- Do not use GitHub Issues, Markdown TODOs, or another tracker as task truth.
