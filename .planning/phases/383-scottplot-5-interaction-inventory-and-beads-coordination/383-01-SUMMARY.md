---
phase: 383
title: "ScottPlot 5 Interaction Inventory and Beads Coordination"
status: completed
bead: Videra-v256.1
---

# Phase 383 Summary

Phase 383 completed the v2.56 inventory and coordination pass. No product code
was changed.

## Evidence Summary

### Plot Lifecycle and Code Experience

`Plot3D` already owns the chart-local surface area needed for lifecycle work:

- `Plot.Add.*` overloads are in `Plot3DAddApi.cs`.
- `Plot3D.Clear`, `Plot3D.Remove`, `Plot3D.IndexOf`, `Plot3D.Series`, and
  revision changes are in `Plot3D.cs`.
- `IPlottable3D` exists as the minimal handle contract.
- `VideraChartViewPlotApiTests` already covers add, labels, visibility,
  clear/remove, axes, `SavePngAsync`, and old-control guardrails.

Phase 384 should stay in `Controls/Plot/*` and focused Plot API tests. The
smallest useful next step is reorder plus typed read-only inspection. Do not
touch interaction, overlay, or demo files from that bead.

### Interaction Profile and Commands

Input is currently chart-local and hard-coded:

- `VideraChartView.Input.cs` maps keyboard, pointer, wheel, toolbar, focus box,
  and pinned probe gestures.
- `SurfaceChartInteractionController.cs` maps left drag, right drag, wheel,
  Shift+left pin toggle, and Ctrl+left focus selection.
- `SurfaceChartInteractionTests` and `VideraChartViewKeyboardToolbarTests`
  cover the existing behavior.

Phase 385 should introduce a small public profile/command surface without a
generic plugin or workbench command system. Disabled behavior must be explicit.

### Selection, Probe, and Draggable Overlays

Overlay and probe behavior already exists but is mostly internal:

- `VideraChartView.Overlay.cs` owns overlay coordinator wiring, projection
  state, hovered probe, and pinned probes.
- Focus selection currently exposes rectangle state through the interaction
  controller.
- Probe tests already verify deterministic probe and pinned readout behavior.

Phase 386 should build on the Phase 385 interaction seam and keep selected
state host-owned. It should not silently mutate source data.

### Axes, Linked Views, and Live View Management

Axis and live foundations are present:

- `PlotAxes3D` exposes `X`, `Y`, `Z`, labels, units, `SetLimits`, `GetLimits`,
  and `AutoScale`.
- `VideraChartView.Core.cs` bridges plot view state through
  `SetViewStateBridge`.
- `DataLogger3D` is in `Videra.SurfaceCharts.Core` and already wraps
  deterministic append/FIFO scatter semantics.

Phase 387 should add explicit axis rules, bounded linked-view lifetime, and
latest-window/full-data live view behavior. Avoid global registries.

### Demo, Docs, and Guardrails

The current sample app and docs already seed cookbook snippets but are not yet
organized as a cookbook:

- `samples/Videra.SurfaceCharts.Demo/MainWindow.axaml(.cs)` owns the sample UI.
- `samples/Videra.SurfaceCharts.Demo/README.md` and `README.md` contain current
  recipe snippets.
- `scripts/Test-SnapshotExportScope.ps1` and integration tests are the primary
  scope guardrails.

Phase 388 should restructure the demo as recipe groups with isolated setup
paths. It must not become a generic chart editor.

## Beads Handoff

| Bead | Phase | Ready after | Ownership | Validation |
|------|-------|-------------|-----------|------------|
| `Videra-v256.2` | 384 | 383 | `Controls/Plot/*`, Plot API tests | focused Plot API tests, guardrail script |
| `Videra-v256.3` | 385 | 383 | `VideraChartView.Input.cs`, `Controls/Interaction/*`, interaction tests | focused interaction/keyboard tests |
| `Videra-v256.4` | 386 | 385 | overlay/probe/selection files and tests | focused probe/selection tests |
| `Videra-v256.5` | 387 | 385 | `PlotAxes3D`, view-linking/live helper files and tests | focused axis/live tests |
| `Videra-v256.6` | 388 | 384, 386, 387 | sample app, README/demo docs | demo/docs tests and build |
| `Videra-v256.7` | 389 | 386, 387, 388 | guardrails, Beads export, generated roadmap, closure docs | full focused suite, guardrails, clean state |

## Parallelization Decision

After Phase 383 closes, `Videra-v256.2` and `Videra-v256.3` are independent and
safe to dispatch in separate worktrees and branches. Their write sets are
disjoint by design.

After Phase 385 closes, Phase 386 and Phase 387 can be reconsidered for
parallel execution. If both need the same core chart file, serialize or split
the bridge change into a separate bead first.
