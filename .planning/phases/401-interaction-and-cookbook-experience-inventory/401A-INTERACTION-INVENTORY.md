# Phase 401A Interaction and Plot API Surface Inventory

Task bead: `Videra-imn`

## Scope and Assumptions

This inventory is read-only evidence for current `SurfaceCharts` interaction and
`Plot` API surfaces. It is inspired by ScottPlot 5 ergonomics only in the sense
of short, discoverable chart authoring paths and small interaction recipes; it
does not claim ScottPlot API compatibility and should not create adapter or
compatibility requirements.

The inspection focused on:

- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/*`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/*`
- relevant `VideraChartView` interaction bridge files
- focused interaction, overlay, Plot API, and sample contract tests
- Phase 376 and 383 prior inventory documents

## Current Evidence-Backed Surfaces

### Plot model and add API

- `Plot3D` is the chart-local model exposed from `VideraChartView.Plot`; it owns
  `Add`, `Axes`, `Series`, typed `GetSeries<TSeries>()`, `ActiveSeries`,
  `ColorMap`, `OverlayOptions`, `Revision`, dataset/output evidence, lifecycle
  operations, and snapshot helpers
  (`src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs:34`,
  `:39`, `:44`, `:49`, `:67`, `:163`, `:182`, `:201`, `:209`, `:217`,
  `:232`, `:250`, `:276`, `:286`, `:376`, `:466`).
- `Plot3DAddApi` currently covers `Surface(...)`, `Waterfall(...)`,
  `Scatter(...)`, `Bar(...)`, and `Contour(...)` overload families, including
  matrix/array convenience paths for surface-like series and coordinate-array
  scatter (`Plot3DAddApi.cs:21`, `:30`, `:39`, `:47`, `:56`, `:65`, `:73`,
  `:82`, `:92`, `:104`, `:115`, `:155`, `:166`).
- `Plot3DSeries` is the public plottable handle shape. It exposes `Kind`,
  `Name`/`Label`, `IsVisible`, and typed backing data/source properties for
  surface, scatter, bar, and contour series (`Plot3DSeries.cs:33`, `:38`,
  `:58`, `:76`, `:81`, `:86`, `:91`).
- Concrete returned handle types exist for surface, waterfall, and scatter
  (`SurfacePlot3DSeries.cs:8`, `WaterfallPlot3DSeries.cs:8`,
  `ScatterPlot3DSeries.cs:8`). Bar and contour currently return the base
  `Plot3DSeries` type from `Plot3DAddApi`.

### Axis, overlay options, and output helpers

- `Plot.Axes` exposes `X`, `Y`, `Z`, and all-axis `AutoScale`
  (`PlotAxes3D.cs:20`, `:25`, `:30`, `:35`).
- Per-axis APIs expose `Label`, `Unit`, `IsLocked`, `Bounds`, `SetBounds`,
  `ClearBounds`, `SetLimits`, `GetLimits`, and `AutoScale`
  (`PlotAxes3D.cs:55`, `:69`, `:89`, `:94`, `:101`, `:117`, `:127`, `:154`,
  `:167`).
- `Plot.OverlayOptions` is the public customization entry for axis, legend,
  crosshair, tooltip, and numeric formatting behavior
  (`Plot3D.cs:182`; `SurfaceChartOverlayOptions.cs:8`, `:283`, `:297`,
  `:311`; `SurfaceChartOverlayPresets` at `SurfaceChartOverlayOptions.cs:324`).
- Snapshot export is chart-local PNG/bitmap only through
  `CaptureSnapshotAsync(PlotSnapshotRequest)` and `SavePngAsync(...)`
  (`Plot3D.cs:376`, `:466`), with result/manifest/diagnostic request types in
  `Controls/Plot`.

### Interaction profile and commands

- `VideraChartView.InteractionProfile` is an Avalonia styled property whose
  default is `SurfaceChartInteractionProfile.Default`
  (`VideraChartView.Input.cs:20`, `:28`).
- `SurfaceChartInteractionProfile` exposes small boolean switches for orbit,
  pan, dolly, reset-camera, fit-to-data, keyboard shortcuts, focus selection,
  probe pinning, toolbar, and pointer-focus behavior, plus a `Disabled` profile
  (`SurfaceChartInteractionProfile.cs:11`, `:16`, `:33`, `:38`, `:43`, `:48`,
  `:53`, `:58`, `:63`, `:68`, `:73`, `:78`).
- `SurfaceChartCommand` is the public built-in command enum: zoom in/out, pan
  left/right/up/down, reset camera, and fit to data
  (`SurfaceChartCommand.cs:6`, `:11`, `:16`, `:21`, `:26`, `:31`, `:36`,
  `:41`, `:46`).
- `VideraChartView.TryExecuteChartCommand(...)` is the public host execution
  path; keyboard and toolbar handlers route to the same core command logic
  (`VideraChartView.Input.cs:209`, `:35`, `:54`, `:409`, `:429`).
- Pointer handling is built in: press updates probe/pointer state, optionally
  focuses the chart, handles toolbar clicks, and then delegates gestures to
  `SurfaceChartInteractionController`; move, release, wheel, and capture-lost
  handlers update probes, interaction state, pinned probes, and overlay invalidation
  (`VideraChartView.Input.cs:83`, `:92`, `:97`, `:122`, `:141`, `:176`,
  `:209`).

### Overlay, probe, selection, and draggable recipes

- Overlay state is coordinated internally by `SurfaceChartOverlayCoordinator`,
  which owns probe, axis, legend, crosshair, and toolbar overlay states
  (`SurfaceChartOverlayCoordinator.cs:15`, `:17`, `:19`, `:21`, `:23`, `:129`,
  `:152`).
- Public host-facing probe and selection helpers already exist on
  `VideraChartView`: `TryResolveProbe`, click/rectangle
  `TryCreateSelectionReport`, `TryCreateDraggableMarkerOverlay`, and
  `TryCreateDraggableRangeOverlay` (`VideraChartView.Overlay.cs:56`, `:89`,
  `:97`, `:142`, `:161`).
- `SurfaceProbeOverlayPresenter` resolves hovered and pinned probe states,
  supports multi-series tooltip content, and renders hovered/pinned readouts
  (`SurfaceProbeOverlayPresenter.cs:21`, `:57`, `:97`, `:138`, `:168`,
  `:185`, `:225`, `:240`, `:267`).
- `SurfaceProbeService` contains screen-to-probe and request-to-probe resolution
  paths (`SurfaceProbeService.cs:13`, `:39`, `:65`, `:95`).
- Draggable overlay recipes are currently bounded recipe helpers, not a generic
  interaction layer: marker and range records plus create/drag helpers
  (`SurfaceChartDraggableOverlayRecipes.cs:8`, `:17`, `:32`, `:51`, `:63`,
  `:81`).

### Test and documentation evidence

- Plot API tests cover add paths, label/visibility handles, remove/clear/move,
  typed `GetSeries<TSeries>()`, axes labels/limits/bounds/locks, overlay options,
  snapshot save, bar/contour paths, composed series, and interaction-quality
  evidence (`VideraChartViewPlotApiTests.cs:23`, `:108`, `:148`, `:201`,
  `:461`, `:507`, `:616`, `:716`, `:739`, `:893`).
- Interaction tests cover pointer navigation, wheel/keyboard behavior,
  interaction quality transitions, disabled/custom profile behavior, and
  routed view event paths (`SurfaceChartInteractionTests.cs:13`, `:243`,
  `:271`, `:308`, `:344`; `VideraChartViewKeyboardToolbarTests.cs:44`,
  `:83`, `:119`, `:193`, `:228`, `:277`, `:305`).
- Probe and overlay tests cover hovered probe state, same-position reuse,
  viewport remapping, exact/coarse tile probing, pinned probe toggling, and
  pinned probe survival across view/projection changes
  (`SurfaceChartProbeOverlayTests.cs:17`, `:48`, `:116`, `:203`, `:355`,
  `:442`, `:524`; `SurfaceChartPinnedProbeTests.cs:18`, `:78`, `:117`).
- Sample contract tests assert that docs/demo mention interaction recipes,
  `SurfaceChartInteractionProfile`, `TryResolveProbe`, `Plot.Add.Scatter`,
  `Plot.SavePngAsync`, interaction-quality text, and consumer smoke support
  fields (`SurfaceChartsDemoConfigurationTests.cs:71`, `:76`, `:78`, `:80`,
  `:186`, `:187`; `SurfaceChartsConsumerSmokeConfigurationTests.cs:46`,
  `:63`, `:81`).
- Prior inventories confirm the intended boundary: Phase 376 established the
  Plot API roadmap; Phase 383 explicitly treated ScottPlot 5 as inspiration
  only and excluded compatibility shims, old chart controls, direct public
  `Source`, PDF/vector export, backend expansion, hidden fallback, generic
  plugin/workbench commands, and god-code demo editors
  (`.planning/milestones/v2.55-phases/376-plot-api-inventory-and-beads-coordination/376-01-SUMMARY.md`;
  `.planning/phases/383-scottplot-5-interaction-inventory-and-beads-coordination/383-CONTEXT.md`).

## Ergonomics Gaps Inspired by ScottPlot 5, Without Compatibility Claims

- Public add ergonomics are mostly discoverable, but bar and contour still
  return `Plot3DSeries` instead of typed handles. This is an ergonomics gap only;
  it is not a compatibility bug.
- Interaction commands are host-callable through `TryExecuteChartCommand`, but
  there is no compact public description/list of enabled commands for menus,
  context menus, or cookbook snippets. Existing tests inspect behavior, not a
  stable recipe-friendly command catalog.
- Pointer gestures are built in and configurable through booleans, but custom
  mouse-action remapping is not exposed. The current profile is intentionally
  small; adding remapping would be a new design choice, not a Phase 402 default.
- Probe, selection, and draggable marker/range helpers exist, but the public
  route is spread across `VideraChartView` methods and recipe helper records.
  Cookbook users need one or two copyable examples that show host-owned state
  without implying a generic annotation editor.
- Overlay customization is powerful through `Plot.OverlayOptions`, but the
  common combinations are easier to discover via presets/tests than via a
  short interaction cookbook path.
- Snapshot and evidence APIs are present, but interaction/evidence snippets are
  not yet organized around "inspect current interaction support, then call a
  command/probe/selection helper" as a single recipe.

## Minimal Candidate Slices for Phase 402

1. **Typed handle polish for remaining add paths**
   - Candidate: introduce `BarPlot3DSeries` and `ContourPlot3DSeries` handles
     only if it can stay local to `Controls/Plot/*` and focused Plot API tests.
   - Success: `Plot.Add.Bar(...)` and `Plot.Add.Contour(...)` return typed
     handles while existing `Plot3DSeries` behavior remains intact.

2. **Recipe-friendly command surface discovery**
   - Candidate: add a small public read-only projection for built-in commands
     or a formatter that describes enabled commands from
     `SurfaceChartInteractionProfile`.
   - Success: hosts can build a context menu/help row from current command
     truth without reflecting over internals or duplicating hard-coded text.

3. **Host-owned probe/selection cookbook slice**
   - Candidate: add documentation/demo snippets around `TryResolveProbe`,
     `TryCreateSelectionReport`, `TryCreateDraggableMarkerOverlay`, and
     `TryCreateDraggableRangeOverlay`.
   - Success: examples demonstrate host-owned state and bounded overlay recipes
     without adding product code beyond missing tiny helper text if needed.

4. **Overlay/interaction evidence snippet**
   - Candidate: expose or document a compact support summary path combining
     interaction profile, command availability, overlay precision profile, and
     probe evidence.
   - Success: support/cookbook consumers can capture current interaction support
     with a small copyable snippet and existing formatter coverage.

5. **Demo/cookbook organization pass**
   - Candidate: group existing broad demo behavior into focused recipes:
     add data, axes/overlay options, interaction profile, commands, probe,
     selection, draggable recipe, snapshot.
   - Success: sample tests assert the recipe headings/snippets, not a generic
     chart editor.

## Non-Goals and Fallback Exclusions

- No ScottPlot compatibility layer, adapter shim, or API parity promise.
- No old `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`.
- No direct public `VideraChartView.Source`; data loading remains through
  `Plot.Add.*`.
- No PDF/vector export, backend expansion, or non-chart-local snapshot scope.
- No hidden fallback/downshift behavior; unsupported output remains explicit
  diagnostics.
- No generic plotting engine, plugin command framework, or workbench command
  system.
- No generic annotation editor or god-code demo surface.
- No custom mouse-action remapping unless a later phase explicitly accepts the
  design cost and tests the full input matrix.

## Validation Commands

Recommended focused validation for Phase 402 planning/execution:

```powershell
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewPlotApiTests|FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~VideraChartViewKeyboardToolbarTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests"
pwsh -NoProfile -File scripts/Test-SnapshotExportScope.ps1
git status --short
```

For this inventory-only bead, the required verification is:

```powershell
git status --short
```
