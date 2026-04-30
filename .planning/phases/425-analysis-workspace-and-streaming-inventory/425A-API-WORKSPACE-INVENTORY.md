# Phase 425A API Workspace Inventory

Task: Videra-7tqx.1.1 - inventory chart APIs and workspace seams for v2.64.

Scope: read-only inspection of current chart API/workspace seams around `VideraChartView`, `Plot.Add`, `Plot.Axes`, linked views, interaction profiles, selection/probe/measurement APIs, and support summary/status surfaces. No code changes and no Beads state changes were made for this inventory.

## Owners / Files

| Area | Current owner / file | Public surface found | Notes for v2.64 |
| --- | --- | --- | --- |
| Chart control shell | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` | `VideraChartView`, `Plot`, `ViewState`, `FitToData`, `ResetCamera`, `ZoomTo`, `Refresh`, `RenderingStatus`, `ScatterRenderingStatus`, `BarRenderingStatus`, `ContourRenderingStatus`, `RenderStatusChanged`, `InteractionQualityChanged` | Single shipped chart control. Keep workspace APIs additive around this control, not as old-control wrappers. |
| View state property | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Properties.cs` | `ViewState` styled property, `InteractionQuality` | `ViewState` is the current persisted camera/data-window contract and the only linked-view synchronization payload today. |
| Plot model | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` | `Plot3D`, `Add`, `Axes`, `Series`, `GetSeries<T>`, `ActiveSeries`, `ColorMap`, `OverlayOptions`, `Revision`, `CreateDatasetEvidence`, `CreateOutputEvidence`, `Clear`, `Remove`, `Move`, `CaptureSnapshotAsync`, `SavePngAsync` | Existing lifecycle and evidence hooks are reusable for per-chart inventory cards and status summaries. |
| Series authoring | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs` | `Plot.Add.Surface`, `Waterfall`, `Scatter`, `Bar`, `Contour` overloads | This is the canonical data-loading path. Do not reintroduce direct public `VideraChartView.Source`. |
| Axis facade | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotAxes3D.cs` | `Plot.Axes.X/Y/Z`, `Label`, `Unit`, `IsLocked`, `SetBounds`, `ClearBounds`, `SetLimits`, `GetLimits`, `AutoScale` | Good foundation for chart-local axis controls. No workspace-level axis group/link facade exists yet. |
| Linked views | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.LinkedViews.cs` | `LinkViewWith(VideraChartView peer)`, `VideraChartViewLink : IDisposable` | Supports exactly two charts by copying full `ViewState`. No N-chart group, axis-only link mode, lifecycle registry, or status surface. |
| Interaction profile / commands | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionProfile.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartCommand.cs` | `InteractionProfile`, `TryExecuteChartCommand`, profile booleans, `EnabledCommands`, built-in pointer and keyboard gestures | Reusable for per-chart command routing. Workspace-level active-chart routing and command broadcast are absent. |
| Probe / selection / measurement helpers | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs`; `SurfaceChartAnnotationAnchor.cs`; `SurfaceChartMeasurementReport.cs`; `SurfaceChartDraggableOverlayRecipes.cs` | `TryResolveProbe`, `SelectionReported`, `TryCreateSelectionReport`, `TryCreateProbeAnnotationAnchor`, `TryCreateSelectionAnnotationAnchors`, `TryCreateSelectionMeasurementReport`, `TryCreateDraggableMarkerOverlay`, `TryCreateDraggableRangeOverlay` | Host-owned chart-local recipes exist. There is no shared workspace selection/probe/measurement model across charts. |
| Rendering status records | `src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderingStatus.cs`; `src/Videra.SurfaceCharts.Avalonia/Controls/ScatterChartRenderingStatus.cs`; `BarChartRenderingStatus.cs`; `ContourChartRenderingStatus.cs` | Surface/waterfall status plus scatter/bar/contour status projections | Good per-chart status primitives. No multi-chart aggregate status or linked-view health record exists. |
| Support summary demo | `samples/Videra.SurfaceCharts.Demo/Services/SurfaceDemoSupportSummary.cs`; `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` | Demo support summary composes plot evidence, rendering statuses, snapshot status, active chart summaries | Useful pattern for workspace summary text, but should not become a broad workbench or product editor. |
| Cookbook seams | `samples/Videra.SurfaceCharts.Demo/Services/CookbookRecipeCatalog.cs`; `samples/Videra.SurfaceCharts.Demo/Recipes/axes-and-linked-views.md`; `waterfall.md` | Examples for `Plot.Add`, `Plot.Axes`, `InteractionProfile`, `TryResolveProbe`, `LinkViewWith`, selection and measurement reports | Existing recipes document host-owned boundaries and are good validation anchors for API docs. |
| Base 3D view inspection APIs | `src/Videra.Avalonia/Controls/VideraView.cs`; `src/Videra.Avalonia/Runtime/VideraViewRuntime.cs`; `VideraViewRuntime.Inspection.cs` | `SelectionState`, `Annotations`, `Measurements`, `ClippingPlanes`, `InteractionMode`, `InteractionOptions`, `CaptureInspectionState`, `ApplyInspectionState`, `ExportSnapshotAsync`, `BackendDiagnostics` | Mature base-view inspection pattern may guide workspace-owned chart analysis state, but chart API should not couple directly to `VideraView` internals. |

## Reusable Existing Surface for v2.64

- `VideraChartView` is already the single chart control with chart-local runtime, overlay, rendering status, and `Plot` ownership.
- `Plot.Add` already covers surface, waterfall, scatter, bar, and contour inputs with typed series handles and plot revision tracking.
- `Plot.Series`, `GetSeries<T>`, `ActiveSeries`, `Remove`, `Move`, `Clear`, `Revision`, `CreateDatasetEvidence`, and `CreateOutputEvidence` provide enough per-chart inventory data for a workspace panel without new data-loading APIs.
- `Plot.Axes.X/Y/Z` already supports labels, units, bounds, limits, lock flags, and autoscale for chart-local axis controls.
- `ViewState` already persists camera and data-window state, and `LinkViewWith` proves disposable state synchronization between two charts.
- `InteractionProfile.EnabledCommands` and `TryExecuteChartCommand` can drive host-owned toolbars or command palettes per active chart.
- Probe, selection, annotation-anchor, measurement, and draggable overlay recipe helpers already return host-owned immutable results instead of chart-owned product state.
- Rendering/status records already expose per-chart readiness, backend/fallback fields, scatter streaming counters, bar layout counts, contour extraction counts, and snapshot evidence.
- Demo support summary code already demonstrates how to join plot evidence, statuses, active series identity, and snapshot state into support-readable text.

## Gaps for Multi-Chart Analysis Workspace / Linked APIs

- No public workspace object owns a collection of `VideraChartView` instances, active chart identity, layout metadata, or per-chart labels.
- `LinkViewWith` only links exactly two views and always copies full `ViewState`. It does not express link groups, axis-only linking, camera-only linking, data-window-only linking, or per-axis link policies.
- There is no workspace-level axis group API that can apply `Plot.Axes` limits/bounds/autoscale to a selected set of charts while preserving per-chart locks.
- There is no aggregate status surface that reports chart count, linked group health, per-chart active series, and readiness across surface/scatter/bar/contour statuses.
- Probe/selection/measurement APIs are chart-local recipes. There is no shared workspace interaction state for "active probe", cross-chart probe compare, synchronized selection ranges, or measurements that reference multiple charts.
- Current linked view implementation has no public diagnostics for disposed links, sync direction, last sync source, or mismatch failures.
- Current support summary is sample-owned. A library-level workspace support summary would need a bounded type/formatter instead of expanding the demo into a workbench.
- Base `VideraView` has mature inspection state, but those types are object-scene oriented. Reusing the pattern for charts likely means new chart-specific state records rather than coupling chart workspaces to base 3D scene selection types.

## Risks / Non-Goals

- Do not add a compatibility layer, wrapper, adapter, or shim around removed alpha chart APIs.
- Do not reintroduce `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`; `VideraChartView` remains the single shipped chart control.
- Do not reintroduce direct public `VideraChartView.Source`; `Plot.Add.*` remains the data-loading path.
- Do not hide unsupported output paths behind fallback/downshift behavior. Unsupported output should remain explicit diagnostics.
- Do not widen chart-local snapshot export beyond the shipped PNG/bitmap scope.
- Do not build a broad workbench or generic chart editor as part of API inventory/workspace seams.
- Do not let workspace state own product-domain selection; current selection/probe/measurement APIs intentionally return host-owned reports.

## Candidate Write Scopes for Phase 426 / 427

| Candidate scope | Likely files | Safe to parallelize? | Notes |
| --- | --- | --- | --- |
| Workspace state/contracts | New files under `src/Videra.SurfaceCharts.Avalonia/Controls/Workspace/` or similar | Yes, if isolated from demo/docs changes | Define chart registration, active chart id, layout metadata, and aggregate snapshot records. Keep independent from `VideraChartView` internals at first. |
| Linked view group API | `VideraChartView.LinkedViews.cs` plus new linked group/link policy records | Partially | Touches existing link owner; should not run concurrently with other edits to linked-view code. Can parallelize with docs/demo if file sets stay separate. |
| Axis group facade | New workspace axis group files plus minimal tests; may call existing `Plot.Axes` | Yes | Can be built as host-owned helper over registered charts without changing `PlotAxes3D` initially. |
| Workspace interaction state | New chart-specific records for active probe/selection/measurement aggregation; possible tests around existing overlay report types | Yes | Keep as host-owned records. Avoid changing `VideraView` inspection types unless a later phase proves overlap. |
| Aggregate status/support formatter | New formatter over `VideraChartView`/`Plot` status snapshots; tests in repository/sample test projects | Yes | Can reuse `CreateOutputEvidence` and status records. Keep sample support summary separate. |
| Demo sample wiring | `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.*` and `Services/*` | No with other demo edits | Demo file is already broad and likely edited by other workers. Do this after contract tests stabilize. |
| Documentation/cookbook updates | `src/Videra.SurfaceCharts.Avalonia/README.md`, `samples/Videra.SurfaceCharts.Demo/Recipes/*.md`, repository docs tests | Yes with code if owners coordinate | Good parallel stream if code workers avoid docs files. Must preserve guardrail wording. |
| Validation/guardrail tests | `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/*`, `tests/Videra.Core.Tests/Repository/*`, `tests/Videra.Core.Tests/Samples/*` | Partially | Parallelize by test file ownership. Repository guardrail tests may conflict if multiple workers update expected terms. |

## Suggested Focused Validation

Targeted search / inventory commands:

```powershell
rg -n "class VideraChartView|partial class VideraChartView|VideraChartView" src samples tests smoke -g "*.cs" -g "*.axaml"
rg -n "Plot\.Add|Add\.Surface|Add\.Waterfall|Add\.Scatter|Plot\.Axes|Axes\." src samples tests smoke -g "*.cs" -g "*.axaml"
rg -n "LinkViewWith|VideraChartViewLink|InteractionProfile|TryExecuteChartCommand|TryResolveProbe|SelectionReported|TryCreateSelectionReport|TryCreateSelectionMeasurementReport" src samples tests -g "*.cs"
rg -n "RenderingStatus|ScatterRenderingStatus|BarRenderingStatus|ContourRenderingStatus|SupportSummary|CreateOutputEvidence|CreateDatasetEvidence" src samples tests -g "*.cs"
```

Focused tests for later implementation phases:

```powershell
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~SurfaceChartInteractionRecipeTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCookbookWaterfallLinkedRecipeTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests"
scripts/Test-SnapshotExportScope.ps1
git diff --check
```

For this inventory-only phase, `git diff --check` is sufficient after creating this file because no code or tests were modified.
