# Phase 24: True 3D Picking and Probe Truth - Context

**Gathered:** 2026-04-16  
**Status:** Ready for planning and execution  
**Mode:** Autonomous

## Phase Boundary

Phase 24 replaces the current viewport-linear probe path with camera-aware 3D picking. Hover, pin, and wheel-zoom anchors should resolve from `screen point -> camera ray -> heightfield hit`, not from a normalized screen coordinate projected back into the current data window rectangle.

This phase builds on Phase 23's shared camera-frame/projection spine. It does not yet change request planning or GPU residency.

## Decisions

- Keep the picking contracts in `Videra.SurfaceCharts.Core`.
- Reuse the existing `SurfaceViewState` / `SurfaceCameraFrame` math from Phase 23.
- Preserve pinned probes as durable anchors that can survive orbit and refine.
- Keep focus-selection rectangle UX unless a camera-hit-based replacement is needed for correctness inside this phase.

## Existing Code Insights

- `SurfaceProbeService.ResolveFromScreenPosition(...)` still maps screen position linearly through `SurfaceViewport`.
- `SurfaceProbeOverlayPresenter` and `SurfaceChartView.Input.cs` both consume that viewport-based probe truth.
- `SurfaceChartInteractionController.ApplyDolly(...)` already zooms around `hoveredProbe.SampleX/SampleY`, so changing hovered-probe resolution changes wheel-anchor truth without another public API change.
- Pinned probes are currently stored as `SurfaceProbeRequest(sampleX, sampleY)`, which is a useful stable anchor if the 3D pick resolves an exact sample-space hit.

## Deferred Ideas

- Camera-aware request planning and refine heuristics stay in Phase 25.
- GPU-side depth-assisted picking is out of scope here.
