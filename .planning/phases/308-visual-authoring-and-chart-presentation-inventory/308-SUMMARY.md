# Phase 308 Summary: Visual Authoring and Chart Presentation Inventory

## Result

Completed the inventory and produced implementation-ready boundaries for Phases 309-312.

## Scene Authoring Seams

- `src/Videra.Core/Scene/SceneDocument.cs`: document root with retained entries and `InstanceBatches`; includes `AddInstanceBatch`.
- `src/Videra.Core/Scene/SceneDocumentEntry.cs`: retained entry identity, name, runtime objects, imported asset, and ownership.
- `src/Videra.Core/Scene/SceneDocumentMutator.cs`: internal mutation helper for current entry/object workflows.
- `src/Videra.Core/Scene/MeshPrimitive.cs`: mesh primitive id/name/mesh data/material id.
- `src/Videra.Core/Scene/MaterialInstance.cs`: material id/name/base color/metallic-roughness/alpha/emissive/texture bindings.
- `src/Videra.Core/Scene/InstanceBatchDescriptor.cs`: first-version same-mesh/same-material instancing contract with transforms, colors, object ids, pickability, and bounds.
- `src/Videra.Core/Scene/InstanceBatchEntry.cs`: retained document truth for instance batches.
- `src/Videra.Core/Selection/SceneHitTestService.cs`: instance-batch hit-test consumer.

## Runtime and Diagnostics Seams

- `src/Videra.Avalonia/Controls/VideraView.Scene.cs`: public viewer scene operations, including `AddInstanceBatch`.
- `src/Videra.Avalonia/Runtime/Scene/SceneRuntimeCoordinator.cs`: scene document coordinator and diagnostics source.
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`: scene document version, upload, resident, instance, and pickable metrics.
- `src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs`: diagnostics snapshot composition.
- `samples/Videra.Demo/Services/PerformanceLabEvidenceSnapshotBuilder.cs`: existing evidence path for performance truth.

## SurfaceCharts Presentation Seams

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`: public chart-local axis/grid/legend/formatter seam.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Properties.cs`: `OverlayOptions` and `ColorMap` Avalonia properties.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs`: axis title/tick/grid rendering and `FormatLabel` usage.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`: legend swatches and min/max label formatting.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs`: major/minor tick generation.
- `src/Videra.SurfaceCharts.Core/ColorMaps/SurfaceColorMap.cs` and `SurfaceColorMapPalette.cs`: current color-map range and palette contracts.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`: probe readout precision seam.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`: demo color-map and overlay option creation.

## Risks

- Instance batches are retained in `SceneDocument`, but runtime applicator paths are still mostly entry/runtime-object oriented. Phase 309 should stay Core-only where possible; Phase 310 should decide the minimum runtime bridge work.
- `PickableObjectCount` and draw-call intent need precise definitions before expanding diagnostics.
- Hard-coded numeric precision currently exists in both axis/legend and probe formatting.
- SurfaceCharts overlay state types are internal and tests inspect them through reflection; shape changes should be careful.
- Demo text tests are string-sensitive.

## Ownership Boundary

### Phase 309

Own Core scene-authoring contracts and tests:

- `src/Videra.Core/Scene/*`
- `src/Videra.Core/Geometry/*` only if typed geometry helpers need small additions
- `tests/Videra.Core.Tests/Scene/*`

Avoid Avalonia runtime and demo changes.

### Phase 310

Own instance-aware runtime/performance proof:

- `src/Videra.Avalonia/Runtime/Scene/*`
- `src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs`
- `src/Videra.Avalonia/Controls/VideraDiagnosticsSnapshotFormatter.cs`
- `samples/Videra.Demo/Services/*PerformanceLab*`
- focused tests under `tests/Videra.Avalonia.Tests`

Avoid SurfaceCharts presentation files.

### Phase 311

Own SurfaceCharts chart-local style and precision presets:

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs`
- `src/Videra.SurfaceCharts.Core/ColorMaps/*`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/SurfaceColorMapTests.cs`

Avoid viewer runtime files.

### Phase 312

Own proof and docs:

- `samples/Videra.Demo/*` for scene authoring proof
- `samples/Videra.SurfaceCharts.Demo/*` for chart styling proof
- README/docs/repository guardrails
- probe readout precision only if Phase 311 leaves it for closure
