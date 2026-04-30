# Phase 318 Context: Professional Visualization Quality Inventory

## Bead

- `Videra-5eu`

## Goal

Inventory authoring visual semantics, SurfaceCharts professional presentation quality, workbench evidence, and guardrail gaps before v2.44 implementation.

## Current State

### Authoring

Core authoring has a clean static-scene surface:

- `SceneAuthoring.Create(...)`
- `SceneAuthoringBuilder`
- `SceneGeometry`
- `SceneMaterials`
- `SceneAuthoringDiagnostic`
- `InstanceBatchDescriptor`

Current geometry helpers include buffer, triangle, quad, plane, grid, polyline, point cloud, cube, sphere, direct instances, and instance batches. Existing tests cover retained `SceneDocument` output, invalid mesh diagnostics, duplicate ids, primitive topologies, sphere geometry, and instance-batch truth.

Primary quality gap: professional visualization scenes still need authors to assemble common semantic helpers manually. Good candidates are axes/triads, tick/scale lines, or small named marker/measurement helper methods. These should remain static mesh/line authoring helpers and should not become an ECS, editor, scripting system, material graph, or runtime behavior layer.

### SurfaceCharts

SurfaceCharts already has chart-local precision/presentation contracts:

- `SurfaceChartOverlayOptions`
- `SurfaceChartNumericLabelPresets`
- `SurfaceChartOverlayPresets`
- `SurfaceColorMapPresets`
- axis/legend/probe consistency

Current color-map presets are minimal: default, cool-warm, and grayscale. The SurfaceCharts demo still has a local five-stop palette and local `LabelFormatter` in `CreateColorMap(...)` / `CreateOverlayOptions()`. That is a good signal for Phase 320: lift one professional presentation improvement into tested chart-local presets instead of keeping demo-only styling logic.

### Workbench

`samples/Videra.AvaloniaWorkbenchSample` now provides a sample-first optional adoption surface. It loads authored scenes, OBJ evidence, captures diagnostics on explicit events, copies support capture text, and demonstrates chart precision presets.

Primary evidence gap: support capture is string-only and not structured beyond section headings. It can become more useful by including named fields for authored-scene counts, instance counts, selected object, chart precision profile, and diagnostics status while keeping refresh explicit/snapshot-based.

### Guardrails

Existing repository tests cover SurfaceCharts independence from `VideraView`, Beads public roadmap generation, release/readiness wording, and multiple docs contracts. `BeadsPublicRoadmapTests` still asserts v2.43-specific roadmap content and should be updated during docs/guardrail closure so the generated roadmap can evolve with v2.44.

## Non-Goals

- No backend expansion.
- No animation, shadows, environment maps, post-processing, or material graph.
- No ECS, scripting, scene graph editor, or update loop.
- No hidden fallback/downshift, compatibility layer, or old-code rescue path.
- No broad chart-family expansion.
- No default workbench/tooling dependency in `Videra.Core` or base `Videra.Avalonia`.
