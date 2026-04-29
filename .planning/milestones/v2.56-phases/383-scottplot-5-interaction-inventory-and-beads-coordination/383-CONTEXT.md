---
phase: 383
title: "ScottPlot 5 Interaction Inventory and Beads Coordination"
status: completed
completed_at: "2026-04-30T01:17:44+08:00"
bead: Videra-v256.1
---

# Phase 383 Context

## Scope

Phase 383 is an inventory and coordination phase. It does not change product
code. Its output is the evidence and dependency map used to keep the remaining
v2.56 work small, Beads-backed, and safe to parallelize.

## ScottPlot 5 References

These references are inspiration points only. They do not create an API parity
or compatibility requirement for Videra.

- ScottPlot 5 Cookbook: https://scottplot.net/cookbook/5/
- ScottPlot 5 Demo: https://scottplot.net/demo/5/
- ScottPlot 5 Scatter Cookbook: https://scottplot.net/cookbook/5/Scatter/

Reference patterns selected for v2.56:

- short `Plot.Add.*` code paths with typed returned handles
- plottable management: add, remove, reorder, and inspect plot contents
- configurable mouse actions and context-menu recipes
- interactive recipes for selection, probes, and draggable overlays
- explicit axis rules and linked-axis examples
- live data logger/streamer view behavior
- cookbook/demo organization around small, copyable examples

## Current Videra Inventory

| Area | Current owner | Evidence | Phase owner |
|------|---------------|----------|-------------|
| Plot lifecycle and code experience | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs`, `Plot3DAddApi.cs`, `IPlottable3D.cs` | `Plot3D` already owns add, clear, remove, index, axes, snapshot, support evidence, and revision changes. Reorder and a typed public plottable list/query API are still missing. | 384 |
| Plottable handles | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs` and concrete family handles | Handles already expose label/visibility and typed source data. Keep changes handle-local and avoid wrapper compatibility types. | 384 |
| Input and gestures | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs` | Mouse, wheel, keyboard, focus box, pin toggle, toolbar, and reset/autoscale behavior are hard-coded. A small public profile/command surface is missing. | 385 |
| Overlay, probe, and selection | `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/*` | Probe and pinned probe rendering exist. Focus rectangle exists through the interaction controller, but host-owned selection/probe helpers and draggable marker/range recipes are missing. | 386 |
| Axes and linked views | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/PlotAxes3D.cs`, `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` | `Plot.Axes` exposes labels, units, explicit limits, and autoscale. Axis rules, explicit link lifetime, and linked view helper APIs are missing. | 387 |
| Live helpers | `src/Videra.SurfaceCharts.Core/DataLogger3D.cs` | `DataLogger3D` wraps deterministic append/FIFO semantics. Latest-window/full-data view behavior and autoscale-decision evidence are missing. | 387 |
| Demo and cookbook docs | `samples/Videra.SurfaceCharts.Demo/*`, `README.md`, `samples/Videra.SurfaceCharts.Demo/README.md` | Current demo is useful but broad. Cookbook grouping, isolated recipes, and copyable recipe snippets are still needed. | 388 |
| Guardrails and closure | `scripts/Test-SnapshotExportScope.ps1`, integration tests, Beads export, generated roadmap | Guardrails already reject old chart controls, direct public `Source`, PDF/vector scope creep, backend expansion, hidden fallback/downshift, and god-code demo scope. | 389 |

## Non-Goals

- no ScottPlot compatibility layer or adapter shim
- no old `SurfaceChartView`, `WaterfallChartView`, or `ScatterChartView`
- no direct public `VideraChartView.Source`
- no PDF/vector export or backend expansion
- no hidden fallback/downshift behavior
- no generic plugin/workbench command system
- no god-code demo editor

## Coordination Notes

- Beads is the source of truth for task state, ownership, dependencies, and handoff.
- Worktree isolation is required for independent implementation beads.
- After Phase 383, `Videra-v256.2` and `Videra-v256.3` are the only independent ready implementation beads.
- Phase 386 and Phase 387 both depend on Phase 385. They can be reconsidered for parallel work after Phase 385 defines the public interaction seam.
- `gsd-sdk query roadmap.analyze` returned zero phases while `roadmap.get-phase 383` succeeded. Phase execution should rely on Beads plus direct phase lookup until that analyzer inconsistency is fixed.
