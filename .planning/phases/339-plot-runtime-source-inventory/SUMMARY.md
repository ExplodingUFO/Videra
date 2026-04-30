# Phase 339 Summary: Plot Runtime Source Inventory

**Bead:** Videra-xdo  
**Status:** Complete  
**Date:** 2026-04-28  

## Goal

Inventory the remaining public `VideraChartView.Source` loading path before deleting it in v2.48. The milestone intent is to make `VideraChartView.Plot.Add.*` the only public chart data entrypoint while keeping runtime internals split by responsibility.

## Findings

The public direct-source API is concentrated in `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Properties.cs`:

- `SourceProperty`
- public `ISurfaceTileSource? Source`
- property change handler that calls `OnSourceChanged(...)`

The existing runtime activation path is still correct and should be reused:

1. public source assignment calls `OnSourceChanged(...)`
2. `OnSourceChanged(...)` resets overlay/projection/failure state
3. `_runtime.UpdateSource(source)` updates the chart-local runtime
4. controller/scheduler clear stale requests, schedule tiles, and invalidate render state
5. render host reads runtime state and loaded tiles

`Plot.Add.Surface(...)` and `Plot.Add.Waterfall(...)` already accept `ISurfaceTileSource`, but they only append passive `Plot3DSeries` entries. They do not currently activate runtime source state.

`Plot.Add.Scatter(...)` records scatter data through the Plot model and has a separate proof path. It must stay Plot-owned without using the old public `Source` property.

## Classification

Delete or migrate:

- public `VideraChartView.Source`
- public `VideraChartView.SourceProperty`
- sample/demo/smoke assignments such as `chartView.Source = source`
- README snippets using `new VideraChartView { Source = ... }`
- README snippets using `Plot.Add.Surface(chartView.Source, ...)`
- tests that assert public `chartView.Source`

Retain as internal implementation state:

- `SurfaceChartRuntime.Source`
- `SurfaceChartRuntime.UpdateSource(...)`
- `SurfaceChartController` source replacement and stale-generation behavior
- `SurfaceTileScheduler` private source state
- render input/state metadata and loaded tile consumption

The retained internal state is not a compatibility path. It is the chart-local runtime truth needed for scheduling, projection, overlays, and rendering.

## Active Plot Semantics

For v2.48, the bounded activation model is:

- `Plot.Add.Surface(...)` activates the latest surface series as the chart runtime source.
- `Plot.Add.Waterfall(...)` activates the latest waterfall series as the chart runtime source and preserves waterfall-specific semantics.
- `Plot.Add.Scatter(...)` activates Plot-owned scatter state and must not rely on public `Source`.
- `Plot.Clear()` clears active runtime state deterministically.
- Multi-series rendering is out of scope; the current model is last-added active series, not a general plotting engine.

## Test and CI Risks

Direct source usage appears across SurfaceCharts integration tests for lifecycle, scheduling, overlays, probes, axis labels, interaction, view-state, waterfall, GPU fallback, render-host snapshots, scene painter, and pinned probes.

The highest-risk behaviors to preserve are:

- stale tile requests do not repopulate the active view after replacement
- late failures from superseded generations are ignored
- replacing a failed source clears `LastTileFailure`
- clearing the active data path recomputes no-data overlay state
- cache-load failure in the demo keeps the previous working chart state

Current CI runtime filters do not cover all high-risk groups. Later phases should use focused local filters for touched contracts rather than treating the current CI subset as complete evidence.

## Handoff

Phase 340 should implement Plot-owned surface/waterfall activation inside `src/Videra.SurfaceCharts.Avalonia/Controls` and route activation through the existing reset/update path. It should not widen scheduler/render internals.

Phase 341 should align scatter status/evidence with Plot-owned scatter series state and avoid introducing a generic plotting engine.

Phase 342 should delete public `Source` / `SourceProperty` after activation is proven.

Phase 343 should migrate demo, smoke, and tests.

Phase 344 should update docs and add guardrails so direct public `Source` examples do not return.

## Verification

Read-only inventory used:

- source search across `src`
- test search across `tests`
- sample/smoke/docs search across `samples`, `smoke`, `docs`, and package README files
- three parallel read-only subagent inventories with disjoint focus areas

No product code was changed in this phase.
