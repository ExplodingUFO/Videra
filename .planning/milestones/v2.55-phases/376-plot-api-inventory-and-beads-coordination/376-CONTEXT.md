# Phase 376: Plot API Inventory and Beads Coordination - Context

**Gathered:** 2026-04-29
**Status:** Ready for closeout
**Mode:** Autonomous coordination with read-only explorer agents

<domain>
## Phase Boundary

v2.55 improves SurfaceCharts first-chart ergonomics around the existing `VideraChartView.Plot` surface. The scope is facade-first:

- Add concise `Plot.Add.*` overloads for common numeric inputs.
- Return typed plottable handles through a small contract.
- Add `Plot.Axes` and `Plot.SavePngAsync` convenience without moving runtime ownership into `Plot3D`.
- Add a live scatter helper over existing columnar scatter semantics.
- Compose same-type visible series.
- Refresh docs/demo around cookbook usage.

Rejected scope:

- old chart controls (`SurfaceChartView`, `WaterfallChartView`, `ScatterChartView`)
- public `VideraChartView.Source`
- compatibility wrappers
- PDF/vector export
- backend/runtime rewrite
- hidden fallback/downshift
- generic plotting-engine work
- god-code workbench expansion
</domain>

<decisions>
## Implementation Decisions

1. `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/` is the primary API facade write boundary.
2. `VideraChartView` remains the owner of runtime view state, data-window limits, render bridge, and offscreen snapshot rendering.
3. `src/Videra.SurfaceCharts.Core/` is touched only when a tiny reusable data helper is required, most likely for live scatter.
4. Demo/docs work stays separate from API implementation to avoid coupling sample churn with facade contracts.
5. Phases 377, 378, and 379 are parallelizable after Phase 376 because their write sets are separable. Each should use an isolated worktree/branch.
6. Phase 380 depends on Phase 377 because multi-series composition depends on plottable visibility/identity semantics.
7. Phase 381 depends on Phases 377-379 because cookbook examples should use the final public API shape.
8. Phase 382 closes integration, guardrails, beads export, and generated roadmap.
</decisions>

<code_context>
## Existing Code Insights

### Plot API and Plottables

- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` exposes `public Plot3D Plot`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` owns series ordering, active series, `Clear`, `Remove`, evidence, and `CaptureSnapshotAsync`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs` is the established sub-facade pattern for `Plot.Add.*`.
- `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs` is the current single public handle with `Kind`, `Name`, and nullable payloads.
- Surface and Waterfall have advanced inputs (`ISurfaceTileSource`, `SurfaceMatrix`) but not raw numeric array overloads.
- Scatter has advanced `ScatterChartData`, but not raw X/Y/Z array overloads.
- Bar and Contour already have convenience patterns (`Bar(double[])`, `Contour(double[,])`).

### Axes and Snapshot

- `VideraChartView.FitToData()`, `ResetCamera()`, and `ZoomTo(SurfaceDataWindow)` are the existing public viewport operations.
- `VideraChartView.ViewState.DataWindow` is the runtime visible-limit truth.
- Dataset axis descriptors are immutable metadata, not mutable plot-owned axis state.
- `PlotSnapshotRequest` has dimensions, scale, background, and format; `CaptureSnapshotAsync` currently chooses output path.
- `VideraChartView.Rendering.cs` owns the Avalonia `RenderTargetBitmap` bridge.

### Streaming and Demo

- `ScatterColumnarSeries` already supports `ReplaceRange`, `AppendRange`, optional FIFO trimming, counters, and default non-pickable high-volume behavior.
- `ScatterChartData` aggregates deterministic scatter evidence.
- Demo `ScatterStreamingScenarios` are deterministic batch scenarios, not a first-class public live helper.
- Demo paths generally clear then add one active chart path, so multi-series rendering needs explicit proof.
</code_context>

<specifics>
## Beads and Parallelization

Created phase beads:

- `Videra-v255.1` - Phase 376 coordination gate.
- `Videra-v255.2` - Phase 377 raw add overloads and typed plottables.
- `Videra-v255.3` - Phase 378 axes facade and SavePng convenience.
- `Videra-v255.4` - Phase 379 live scatter helpers.
- `Videra-v255.5` - Phase 380 same-type multi-series composition.
- `Videra-v255.6` - Phase 381 cookbook demo and docs.
- `Videra-v255.7` - Phase 382 integration guardrails and milestone evidence.

Parallel work after Phase 376:

| Bead | Worktree branch | Write boundary | Validation |
|------|-----------------|----------------|------------|
| `Videra-v255.2` | `v255-phase-377-plot-add-plottables` | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/`, `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs` | Plot API + guardrail tests |
| `Videra-v255.3` | `v255-phase-378-axes-savepng` | Plot facade files plus narrow `VideraChartView` bridge if needed | Plot snapshot, state, axis overlay, snapshot scope guardrail |
| `Videra-v255.4` | `v255-phase-379-live-scatter` | `src/Videra.SurfaceCharts.Core/` scatter helper files, focused Plot entrypoint only if required | scatter, demo config, benchmark contract tests |

Serialize or merge carefully if a worker must touch another lane's boundary.
</specifics>

<deferred>
## Known Workflow Note

`gsd-sdk query roadmap.get-phase 376` reads Phase 376 correctly, but `gsd-sdk query roadmap.analyze` returned an empty phases array immediately after writing the roadmap. Beads are therefore the authoritative work queue for v2.55 until that parser inconsistency is resolved or naturally clears after phase artifacts exist.
</deferred>
