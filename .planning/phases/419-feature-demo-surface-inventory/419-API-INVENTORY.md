# Phase 419A API Inventory: Native SurfaceCharts Feature/API Surface

Bead: `Videra-mula`

Branch/worktree: `agents/v263-phase419-api` at `.worktrees/v263-phase419-api`

Scope: docs-only inventory of current native SurfaceCharts API surface for later Phase 420/421 planning. No product code changes.

## Boundary Statement

No compatibility, downshift, or fallback route is recommended for Phase 420/421. The inventory supports continuing with `VideraChartView` + `VideraChartView.Plot` as the single host-facing chart surface. Do not reintroduce old chart controls, direct public `Source`, compatibility wrappers, hidden backend downshift, PDF/vector export, or a generic plotting engine.

## Current API Map By Owner/File

### Avalonia Host Control Owner

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs`
  - Public chart shell: `VideraChartView : Decorator`.
  - Plot ownership: `Plot3D Plot`.
  - Status projections: `RenderingStatus`, `ScatterRenderingStatus`, `BarRenderingStatus`, `ContourRenderingStatus`.
  - Events: `RenderStatusChanged`, `InteractionQualityChanged`.
  - View commands: `FitToData()`, `ResetCamera()`, `ZoomTo(SurfaceDataWindow)`, `Refresh()`.
  - Current behavior: `Plot.ActiveSurfaceSource` drives runtime source internally; scatter/bar/contour are routed through software chart-local status and drawing paths.

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Properties.cs`
  - Public persisted state: `ViewStateProperty`, `SurfaceViewState ViewState`.
  - Public diagnostic mode: `SurfaceChartInteractionQuality InteractionQuality`.
  - Current behavior: host owns persisted `ViewState`; control synchronizes runtime state back to the styled property.

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`
  - Public interaction profile: `InteractionProfileProperty`, `SurfaceChartInteractionProfile InteractionProfile`.
  - Public command API: `TryExecuteChartCommand(SurfaceChartCommand)`.
  - Built-in input mapping: left orbit, right pan, wheel dolly, keyboard zoom/pan/reset/fit, toolbar command hit testing, Ctrl+left focus selection, Shift+left probe pinning.

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
  - Public query/report APIs: `TryResolveProbe(Point, out SurfaceProbeInfo)`, `TryCreateSelectionReport(Point, out SurfaceChartSelectionReport)`, rectangle overload, `TryCreateDraggableMarkerOverlay(...)`, `TryCreateDraggableRangeOverlay(...)`.
  - Current limitation: these APIs create host-owned reports/recipes; there is no public selected-object state store, public annotation collection, or measurement model owned by SurfaceCharts.

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs`
  - Render path is public only through Avalonia rendering and Plot snapshot APIs. Offscreen render bridge is internal.
  - Native host creation is internal and only attempted when a GPU backend exists, a source exists, and render size is valid.

### Plot API Owner

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`
  - Public entrypoint: `Plot3D`.
  - Authoring facades: `Add`, `Axes`.
  - Series model: `Series`, `GetSeries<TSeries>()`, `ActiveSeries`, `Revision`.
  - Lifecycle: `Clear()`, `Remove(Plot3DSeries)`, `Move(Plot3DSeries, int)`, `IndexOf(Plot3DSeries)`.
  - Presentation: `SurfaceColorMap? ColorMap`, `SurfaceChartOverlayOptions OverlayOptions`.
  - Evidence: `CreateDatasetEvidence()`, `CreateOutputEvidence(...)`.
  - Snapshot export: `CaptureSnapshotAsync(PlotSnapshotRequest)`, `SavePngAsync(...)`.
  - Current behavior: last visible series is active; visible series of the same active kind can be composed. Mixed-family rendering is not a general scene compositor.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
  - Surface: `Surface(ISurfaceTileSource)`, `Surface(SurfaceMatrix)`, `Surface(double[,])`.
  - Waterfall: `Waterfall(ISurfaceTileSource)`, `Waterfall(SurfaceMatrix)`, `Waterfall(double[,])`.
  - Scatter: `Scatter(ScatterChartData)`, `Scatter(double[] x, double[] y, double[] z, ...)`.
  - Bar: `Bar(double[] values)`, `Bar(BarChartData)`.
  - Contour: `Contour(double[,])`, `Contour(SurfaceScalarField)`, `Contour(ContourChartData)`.
  - Current limitation: convenience overloads are intentionally small. There are no category-label, error-bar, per-series style builder, annotation, or measurement overloads.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs` and typed handles
  - Base handle: `Plot3DSeries : IPlottable3D`.
  - Shared mutable handle surface: `Label`, `IsVisible`.
  - Read-only identity/data links: `Kind`, `Name`, `SurfaceSource`, `ScatterData`, `BarData`, `ContourData`.
  - Typed handles: `SurfacePlot3DSeries`, `WaterfallPlot3DSeries`, `ScatterPlot3DSeries`, `BarPlot3DSeries`, `ContourPlot3DSeries`.
  - Current limitation: handles do not expose native styling knobs beyond label/visibility.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotAxes3D.cs`
  - Axis facade: `PlotAxes3D.X/Y/Z`.
  - Axis operations: `Label`, `Unit`, `IsLocked`, `Bounds`, `SetBounds`, `ClearBounds`, `SetLimits`, `GetLimits`, `AutoScale`.
  - Current behavior: uses plot-owned view-state bridge; labels/units are copied into `OverlayOptions` overrides.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotSnapshot*.cs`
  - PNG-only bitmap contract: `PlotSnapshotRequest`, `PlotSnapshotResult`, `PlotSnapshotManifest`, `PlotSnapshotDiagnostic`, `PlotSnapshotFormat.Png`, `PlotSnapshotBackground`.
  - Current limitation: no PDF/vector route is present or recommended.

### Avalonia Overlay/Interaction Owner

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`
  - Axis/grid/legend/crosshair formatting: minor ticks, grid plane, axis side mode, label formats, precision, `LabelFormatter`, per-axis formatters, title/unit overrides, `ShowCrosshair`, `TooltipOffset`, `LegendPosition`.
  - Presets: `SurfaceChartNumericLabelPresets`, `SurfaceChartOverlayPresets`.
  - Current limitation: no host-facing annotation style contract, callout style, measurement style, or per-series legend customization beyond source labels/colors.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionProfile.cs`
  - Switches: orbit, pan, dolly, reset, fit, keyboard shortcuts, focus selection, probe pinning, toolbar, focus-on-press.
  - `EnabledCommands` exposes currently enabled command set.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartCommand.cs`
  - Built-in command enum: zoom in/out, pan left/right/up/down, reset camera, fit to data.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs`
  - Host-owned selection report with screen, sample, axis, and rectangle `DataWindow` fields.
  - `SurfaceChartSelectionKind`: click or rectangle.

- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartDraggableOverlayRecipes.cs`
  - Host-owned overlay recipe helpers: `SurfaceChartDraggableMarkerOverlay`, `SurfaceChartDraggableRangeOverlay`, `CreateMarker`, `DragMarkerTo`, `CreateRange`, `DragRangeBy`.
  - Current limitation: recipes produce immutable bounded state only; no built-in persistent draggable visual collection.

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartProbeEvidence.cs`
  - Public evidence surface for probe resolution, selection reporting, draggable marker/range recipe support, and pinned probe counts.

### Core Data/Feature Owner

- `src/Videra.SurfaceCharts.Core/Surface*.cs`
  - Surface data contract: `SurfaceMetadata`, `SurfaceMatrix`, `SurfaceScalarField`, `SurfaceMask`, `SurfaceTile`, `SurfaceTileKey`, `SurfaceTileBounds`, `SurfaceTileStatistics`, `SurfaceValueRange`.
  - View/viewport contract: `SurfaceViewState`, `SurfaceViewport`, `SurfaceDataWindow`, `SurfaceNormalizedViewport`, `SurfaceViewportRequest`, `SurfaceCameraPose`, `SurfaceChartProjectionSettings`, `SurfaceDisplaySpace`.
  - Tiling/LOD: `ISurfaceTileSource`, `ISurfaceTileBatchSource`, `InMemorySurfaceTileSource`, `SurfacePyramidBuilder`, `SurfacePyramidLevel`, `SurfaceLodPolicy`, `SurfaceLodSelection`.
  - Axes/grids: `SurfaceAxisDescriptor`, `SurfaceAxisScaleKind`, `SurfaceGeometryGrid`, `SurfaceRegularGrid`, `SurfaceExplicitGrid`.

- `src/Videra.SurfaceCharts.Core/ColorMaps/*`
  - `SurfaceColorMap`, `SurfaceColorMapPalette`, `SurfaceColorMapPresets`.
  - Current limitation: color map is plot-level for surface/waterfall; no per-surface color map handle.

- `src/Videra.SurfaceCharts.Core/Scatter*.cs` and `DataLogger3D.cs`
  - Scatter data: `ScatterChartData`, `ScatterChartMetadata`, `ScatterSeries`, `ScatterPoint`.
  - High-volume/live path: `ScatterColumnarSeries`, `ScatterColumnarData`, `DataLogger3D`, `DataLogger3DLiveViewEvidence`.
  - Existing native feature shape: FIFO capacity, sorted X validation, NaN gaps, pickable flag, per-point size/color columns, append/replace counters.
  - Current limitation: no public live chart binding object that automatically refreshes a `VideraChartView`; hosts still construct/update data and refresh through existing plot flow.

- `src/Videra.SurfaceCharts.Core/Bar*.cs`
  - Bar data: `BarChartData`, `BarSeries`, `BarChartLayout.Grouped/Stacked`.
  - Current limitation: no category labels, no per-category metadata, no error bars, and no convenience API for multiple named bar series.

- `src/Videra.SurfaceCharts.Core/Contour*.cs`
  - Contour data/extraction: `ContourChartData`, `ContourExtractor`, `ContourLine`, `ContourSegment`, `ContourRenderScene`, `ContourSceneCache`.
  - Current limitation: level count is the only public contour styling/level control; no explicit contour level list or line style contract.

- `src/Videra.SurfaceCharts.Core/Picking/*`
  - Probe/picking: `SurfaceHeightfieldPicker`, `SurfaceProbeInfo`, `SurfaceProbeRequest`, `SurfaceProbeResult`, `SurfacePickRay`, `SurfacePickHit`, `ISeriesProbeStrategy`, `ScatterProbeStrategy`, `BarProbeStrategy`, `ContourProbeStrategy`.
  - Current limitation: probe resolution exists, but measurement primitives such as point-to-point distance, delta readouts, area/range statistics, and annotation anchors are not first-class SurfaceCharts API.

### Rendering Owner

- `src/Videra.SurfaceCharts.Rendering/*`
  - Render backend contract: `ISurfaceChartRenderBackend`, `SurfaceChartRenderBackendKind`.
  - Public render host/state: `SurfaceChartRenderHost`, `SurfaceChartRenderInputs`, `SurfaceChartRenderSnapshot`, `SurfaceChartRenderingStatus`, `SurfaceChartRenderState`, `SurfaceChartRenderChangeSet`, `SurfaceChartResidentTile`.
  - Backends: `SurfaceChartGpuRenderBackend`, `SurfaceChartSoftwareRenderBackend`.
  - Current limitation: rendering APIs are low-level support surfaces; Phase 420/421 should avoid expanding these unless a feature needs a narrow, tested state/status projection.

## Test Evidence Map

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`
  - Guards single `Plot.Add` API, raw-array overloads, typed plottable handles, revision lifecycle, remove/clear/move, typed queries, same-kind composition, no public `Source`, and no old chart controls.

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/RegressionGuardrailTests.cs`
  - Confirms all chart families produce dataset/output evidence and snapshot mode suppresses interaction chrome.

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionTests.cs`
  - Exercises built-in orbit/pan/dolly/keyboard/quality behavior.

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionRecipeTests.cs`
  - Exercises selection reports and draggable marker/range overlay recipes.

- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs` and `SurfaceChartProbeOverlayTests.cs`
  - Exercise hover/pinned probes, axis truth stability, no-data probe overlay state, and probe formatting.

- `tests/Videra.SurfaceCharts.Core.Tests/*`
  - Covers surface metadata, view state, tiles, LOD, color maps, contour extraction/cache, scatter live data, picking, axis scale/tick behavior, and render-scene contracts.

- `tests/Videra.SurfaceCharts.Core.Tests/PlotSnapshot*.cs`
  - Covers Plot snapshot contract and capture behavior.

## Feature Gaps And Candidate Improvements

### Native Feature Candidates

- Per-series style handles.
  - Gap: `Plot3DSeries` handles expose only `Label` and `IsVisible`; series colors mostly live in immutable data objects and `Plot.ColorMap` is plot-level.
  - Candidate: add small typed style surfaces for existing families, starting with read/write handle properties that map to already-rendered concepts: scatter default marker size/color, contour line color/thickness, bar series color/category labels, surface/waterfall opacity or color map only if renderer support is already narrow.
  - Risk: broad style systems can become a generic plotting engine. Keep each bead per family and backed by one testable visual/evidence contract.

- Bar chart category metadata.
  - Gap: `BarChartData` has series/count/layout but no category labels or category axis formatter contract.
  - Candidate: add category labels as data metadata and overlay/evidence support.
  - Risk: avoid full 2D chart parity. Keep to labels and existing grouped/stacked layout.

- Contour level/style control.
  - Gap: `ContourChartData` exposes `LevelCount` but not explicit level values or per-level styling.
  - Candidate: support explicit contour levels and deterministic evidence for extracted level values.
  - Risk: line styling should be minimal and not introduce a separate annotation layer.

- Live scatter refresh ergonomics.
  - Gap: `DataLogger3D` and `ScatterColumnarSeries` track live data counters, but no small binding/helper connects updates to `Plot.Refresh()`.
  - Candidate: add a narrow live scatter recipe/helper if demo/support inventory confirms user friction.
  - Risk: avoid background scheduler or hidden auto-refresh; keep host-triggered and explicit.

- Public output/feature capability evidence refinement.
  - Gap: output evidence exists, but feature support is spread across dataset/output/probe/status types.
  - Candidate: add or document a single feature capability evidence projection only if later planning needs it.
  - Risk: do not use evidence to claim validation that did not run.

### Annotation/Measurement Support Candidates

- Host-owned annotation anchor contracts.
  - Gap: probe, selection, marker, and range recipes exist, but no first-class annotation anchor DTO with chart-space, axis-space, and optional label.
  - Candidate: add small immutable `SurfaceChartAnnotationAnchor`/`SurfaceChartAnnotationRange` contracts and creation methods from probe/selection reports.
  - Dependency: selection/probe APIs remain the source of truth.
  - Risk: do not add a persistent annotation manager unless a later bead explicitly owns it.

- Measurement primitives from existing reports.
  - Gap: `SurfaceChartSelectionReport` exposes endpoints and `DataWindow`, but no helper computes deltas, Euclidean sample/axis distances, value deltas, or range summary.
  - Candidate: add `SurfaceChartMeasurementReport` helpers for point-to-point deltas and rectangle extents using existing selection/probe state.
  - Risk: value statistics over selected regions need loaded tile/source semantics; split that into a separate bead if needed.

- Public selection change/event bridge.
  - Gap: focus selection exists as built-in interaction, and host can ask for reports, but there is no public `SelectionReported` event.
  - Candidate: add event-based reporting only if demo/support inventory shows hosts need push notifications from built-in gestures.
  - Risk: host-owned state remains the architecture; event should carry immutable report and not store selected state inside chart.

- Annotation/measurement evidence.
  - Gap: `SurfaceChartProbeEvidence` reports support booleans, but measurement/annotation support is not represented.
  - Candidate: extend evidence after contracts exist.
  - Risk: keep evidence as truth about implemented APIs, not roadmap intent.

### Out Of Scope For Phase 420/421

- Restoring `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView`, compatibility wrappers, or direct public `Source`.
- Hidden fallback/downshift path or viewer/backend fallback broadening. Existing chart-local software rendering status is part of current runtime truth, not a planning recommendation to add new fallback routes.
- PDF/vector export or generic export subsystem; current snapshot contract is PNG/bitmap only.
- Generic plotting engine, full ScottPlot parity, or mixed-family arbitrary compositor.
- Backend expansion such as OpenGL, WebGPU, or new native host embedding path.
- Demo workbench/god-code expansion; API changes should be small and independently validated.

## Recommended Small Child Beads For Phase 420/421

### Phase 420A: Bar Category Labels

- Type: native feature.
- Dependency: `Videra-mula`; should also wait for demo inventory if demo recipes need this exposed.
- Proposed write scope:
  - `src/Videra.SurfaceCharts.Core/BarChartData.cs`
  - `src/Videra.SurfaceCharts.Core/BarSeries.cs` only if category metadata belongs per series after design review
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
  - focused tests under `tests/Videra.SurfaceCharts.Core.Tests` and `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests`
- Validation commands:
  - `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter Bar`
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Bar`
  - `git diff --check`
- Handoff notes: keep labels as metadata/evidence and overlay formatting only. Do not add new chart control types or a general categorical plotting framework.

### Phase 420B: Contour Explicit Levels

- Type: native feature.
- Dependency: `Videra-mula`; independent of 420A.
- Proposed write scope:
  - `src/Videra.SurfaceCharts.Core/ContourChartData.cs`
  - `src/Videra.SurfaceCharts.Core/ContourExtractor.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DDatasetEvidence.cs`
  - `tests/Videra.SurfaceCharts.Core.Tests/Contour*`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs`
- Validation commands:
  - `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter Contour`
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Contour`
  - `git diff --check`
- Handoff notes: explicit levels should be deterministic and finite. Avoid introducing contour-label placement or annotation rendering in this bead.

### Phase 420C: Minimal Series Style Handles

- Type: native feature.
- Dependency: `Videra-mula`; should follow whichever single family is selected first by Phase 420 planning.
- Proposed write scope:
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs`
  - one typed series file only for the chosen family
  - the renderer/evidence file for that chosen family only
  - focused plot API and rendering/evidence tests
- Validation commands:
  - selected family `dotnet test` filter, for example `--filter Scatter` or `--filter Bar`
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Plot`
  - `git diff --check`
- Handoff notes: one family per bead. Keep mutable handle properties narrow and evidence-backed. Do not create a broad style inheritance system.

### Phase 421A: Annotation Anchor DTOs From Probe/Selection

- Type: annotation/measurement support.
- Dependency: `Videra-mula`; can run after or parallel with native-feature beads if write scopes stay disjoint.
- Proposed write scope:
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs`
  - new small annotation contract file under `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionRecipeTests.cs`
- Validation commands:
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter SurfaceChartInteractionRecipe`
  - `git diff --check`
- Handoff notes: output immutable host-owned anchor/range records. Do not add a chart-owned annotation collection or visual editor in this bead.

### Phase 421B: Measurement Report Helpers

- Type: annotation/measurement support.
- Dependency: 421A if measurement consumes annotation anchors; otherwise only `Videra-mula`.
- Proposed write scope:
  - new measurement contract file under `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionRecipeTests.cs`
  - optional core-only tests if pure math is placed in Core
- Validation commands:
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter SurfaceChartInteractionRecipe`
  - `git diff --check`
- Handoff notes: start with screen/sample/axis deltas and rectangle extents. If value statistics over source data are requested, split into a separate bead because it crosses into tile/source sampling semantics.

### Phase 421C: Selection Report Event

- Type: annotation/measurement support.
- Dependency: 421A recommended; demo inventory should confirm event-driven host UX need.
- Proposed write scope:
  - `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
  - `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs`
  - `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionTests.cs`
- Validation commands:
  - `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter SurfaceChartInteraction`
  - `git diff --check`
- Handoff notes: event should report immutable selection data from built-in gestures and leave selected state host-owned.

## Verification Performed For This Inventory

- Read-only inspection of SurfaceCharts Core/Avalonia/Rendering APIs and SurfaceCharts API/interaction tests.
- Planned validation: `git diff --check` after this document is written.
