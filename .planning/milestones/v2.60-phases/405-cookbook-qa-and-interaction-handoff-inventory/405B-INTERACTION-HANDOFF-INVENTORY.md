---
phase: 405
bead: Videra-9pc
title: "Phase 405 Interaction Handoff Surface Inventory"
scope: "SurfaceCharts interaction handoff inventory only"
status: inventory
---

# 405B Interaction Handoff Surface Inventory

## Scope

This inventory covers the current v2.60 interaction handoff surfaces for
SurfaceCharts. It is evidence-only and does not modify product code, tests,
README files, Beads state, roadmap state, or other phase files.

## Current Handoff Surfaces

### Interaction Profile

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionProfile.cs`
  is the public chart-local switchboard.
- Default enables orbit, pan, dolly, reset camera, fit-to-data, keyboard
  shortcuts, Ctrl+left focus selection, Shift+left probe pinning, toolbar
  commands, and focus-on-pointer-press.
- `SurfaceChartInteractionProfile.Disabled` turns off all built-in gestures,
  command categories, toolbar handling, probe pinning, focus selection, and
  focus capture.
- `EnabledCommands` exposes the command list implied by the profile, grouped by
  dolly, pan, reset, and fit-to-data switches.

Handoff value: consumers can copy one small profile initializer and know which
built-in chart interactions remain active.

### Command Surface

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartCommand.cs`
  defines the public built-in commands: `ZoomIn`, `ZoomOut`, `PanLeft`,
  `PanRight`, `PanUp`, `PanDown`, `ResetCamera`, and `FitToData`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`
  exposes `VideraChartView.TryExecuteChartCommand(...)`.
- Keyboard shortcuts route through the same command implementation when
  `IsKeyboardShortcutsEnabled` is true: plus/minus for zoom, arrow keys for pan,
  Home for reset, and F for fit-to-data.
- Toolbar hit testing maps the visible toolbar actions to the same command set;
  disabling `IsToolbarEnabled` consumes toolbar hits without mutating view state.

Handoff value: host buttons, context menus, keyboard help, and cookbook snippets
can target one command API instead of reaching into camera or runtime internals.

### Pointer Gestures

- `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs`
  keeps built-in gesture interpretation internal.
- Left drag orbits when orbit is enabled.
- Right drag pans when pan is enabled.
- Wheel dolly zooms around the hovered probe when available, otherwise the
  current data-window center.
- Ctrl+left drag creates the built-in focus-selection rectangle and focuses the
  data window on release.
- Shift+left click toggles pinned probes when probe pinning is enabled and the
  press/release stays under the drag threshold.

Handoff value: hosts receive public profile switches and public helper APIs;
they do not own or duplicate gesture-mode state.

### Probe And Pinned-Probe Surface

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
  exposes `TryResolveProbe(Point, out SurfaceProbeInfo)` for pointer-to-probe
  resolution without changing hover or pinned-probe state.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs`
  resolves probes from camera-frame picking for 3D surfaces, viewport mapping
  for 2D-style series paths, and loaded tile truth.
- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartProbeEvidence.cs`
  exposes `SurfaceChartProbeEvidenceFormatter.Create(...)` and `Format(...)`
  for deterministic hovered/pinned probe support text.
- Pinned-probe overlay state remains internal; Shift+left click is the shipped
  chart-local UX for toggling pins.

Handoff value: support and sample surfaces can report probe status, pinned count,
readouts, exact/approximate flags, and delta-vs-first-pin without exposing
overlay internals.

### Selection Report Surface

- `VideraChartView.TryCreateSelectionReport(Point, out SurfaceChartSelectionReport)`
  creates a click report.
- `VideraChartView.TryCreateSelectionReport(Point, Point, out SurfaceChartSelectionReport)`
  creates click or rectangle reports from screen points.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartSelectionReport.cs`
  carries screen coordinates, normalized screen rectangle, sample-space start/end,
  axis-space start/end, and a `SurfaceDataWindow` for rectangle selections.

Handoff value: host selection workflows can own selected state while relying on
the chart for screen-to-sample and screen-to-axis mapping.

### Draggable Overlay Support

- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartDraggableOverlayRecipes.cs`
  defines immutable host-owned marker and range overlay records.
- `CreateMarker(...)` and `DragMarkerTo(...)` clamp sample coordinates to
  metadata bounds and expose mapped axis coordinates.
- `CreateRange(...)` and `DragRangeBy(...)` clamp a `SurfaceDataWindow` while
  preserving host ownership of the range state.
- `VideraChartView.TryCreateDraggableMarkerOverlay(...)` and
  `TryCreateDraggableRangeOverlay(...)` create bounded recipes from screen
  positions.
- `SurfaceChartInteractionRecipeEvidenceFormatter.CreateSupported()` reports
  deterministic support for probe resolution, selection reporting, draggable
  marker overlays, draggable range overlays, and `StateOwnership: HostOwned`.

Handoff value: Phase 407 can document or sample draggable overlays without
adding chart-owned selected or dragged mutable state.

### Demo, README, And Support Handoff

- `samples/Videra.SurfaceCharts.Demo/README.md` includes an `Interactions`
  recipe showing `SurfaceChartInteractionProfile`, `TryExecuteChartCommand`,
  and `TryResolveProbe`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` mirrors the
  same `Interactions` cookbook snippet in `CookbookRecipes`.
- `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml` exposes visible
  "Explore next: Probe workflow", "Cookbook gallery", and "Support summary"
  surfaces.
- `src/Videra.SurfaceCharts.Avalonia/README.md` documents probe evidence as
  chart-local and explicitly keeps overlay state internal.
- Existing demo support summary tests guard the support summary controls and
  current-plot/view-state copy behavior.

Handoff value: the current copyable story is real but uneven. The interaction
recipe is copyable; probe evidence and draggable support are stronger in tests
than in end-user docs or visible demo snippets.

## Copyability And Drift Risks

- Cookbook snippet drift: the `Interactions` snippet exists in both
  `samples/Videra.SurfaceCharts.Demo/README.md` and
  `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs`. The current
  contract tests check key terms, but the snippet text can still diverge in
  formatting or omissions.
- Namespace/import drift: the visible snippet shows public symbols but not the
  minimal `using` set. New consumers may miss
  `Videra.SurfaceCharts.Avalonia.Controls.Interaction`.
- Support-summary gap: deterministic probe and interaction-recipe evidence
  formatters exist, but the visible SurfaceCharts demo support summary should be
  audited before claiming it captures all interaction recipe support.
- Probe exactness risk: probe readouts depend on loaded tiles, masks, and
  camera-frame picking. Tests cover exact, approximate, masked, and no-hidden
  overview fallback behavior; docs should avoid promising a probe at every
  screen point.
- Selection ownership risk: `SurfaceChartSelectionReport` is host-owned output.
  Future snippets should avoid implying the chart stores product selection.
- Draggable overlay risk: current support is immutable recipe creation and
  clamping, not a built-in drag UI. Future cookbook wording should use "recipe"
  and "host-owned" consistently.
- Toolbar drift: toolbar actions currently cover zoom in, zoom out, reset camera,
  and fit-to-data, not the full command enum. Host-command docs should not imply
  the toolbar exposes pan commands.

## Residual Gaps For Phase 407

Candidate slices should stay small and should not widen runtime scope.

1. `407A - Interaction cookbook snippet parity`
   - Goal: keep README and demo `Interactions` snippets textually aligned.
   - Acceptance: a focused test compares the interaction snippet body or a shared
     snippet source so drift is detectable.

2. `407B - Interaction imports and minimal host wiring`
   - Goal: document the minimal usings and host button/context-menu wiring for
     `SurfaceChartInteractionProfile` plus `TryExecuteChartCommand`.
   - Acceptance: docs/demo sample compiles or is covered by a grep-friendly
     contract test for the public symbols and namespace.

3. `407C - Probe evidence support summary`
   - Goal: make the support summary visibly include deterministic probe evidence
     when a hovered or pinned probe exists, without exposing internal overlay
     state.
   - Acceptance: focused demo viewport test verifies `EvidenceKind:
     surface-chart-probe`, `ProbeStatus`, and `PinnedCount` appear only when
     available.

4. `407D - Selection and draggable recipe cookbook`
   - Goal: add a bounded recipe snippet for
     `TryCreateSelectionReport`, `TryCreateDraggableMarkerOverlay`, and
     `TryCreateDraggableRangeOverlay`.
   - Acceptance: docs/demo tests verify the snippet uses host-owned state wording
     and does not claim a built-in drag editor.

5. `407E - Interaction recipe evidence handoff`
   - Goal: expose `SurfaceChartInteractionRecipeEvidenceFormatter.Format(...)`
     in docs or support output as a stable handoff block.
   - Acceptance: support/docs contract test checks `ProbeResolution`,
     `SelectionReporting`, `DraggableMarkerOverlay`, `DraggableRangeOverlay`, and
     `StateOwnership: HostOwned`.

## Non-Goals And Fallback Exclusions

- No ScottPlot compatibility layer or parity promise.
- No reintroduction of `SurfaceChartView`, `WaterfallChartView`,
  `ScatterChartView`, direct public `Source` loading, or compatibility wrappers.
- No hidden fallback or downshift path when probe/selection/output support is
  unavailable.
- No PDF/vector export, export backend expansion, or broad renderer/backend
  work.
- No generic chart workbench, generic plotting engine, or editable demo shell.
- No chart-owned product selection or draggable state.

## Validation Commands

Focused inventory validation:

```powershell
git diff --check -- .planning/phases/405-cookbook-qa-and-interaction-handoff-inventory/405B-INTERACTION-HANDOFF-INVENTORY.md
git status --short
```

Recommended Phase 407 implementation validation:

```powershell
dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~VideraChartViewKeyboardToolbarTests|FullyQualifiedName~SurfaceChartInteractionRecipeTests|FullyQualifiedName~SurfaceChartProbeEvidenceTests|FullyQualifiedName~SurfaceChartProbeEvidenceCompatibilityTests|FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"
dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests"
scripts/Test-SnapshotExportScope.ps1
```
