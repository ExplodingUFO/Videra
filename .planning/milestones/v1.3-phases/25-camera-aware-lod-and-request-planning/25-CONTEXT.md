# Phase 25: Camera-Aware LOD and Request Planning - Context

**Gathered:** 2026-04-16  
**Status:** Ready for planning and execution  
**Mode:** Autonomous

## Phase Boundary

Phase 25 upgrades surface-chart LOD and request planning from viewport-density heuristics to camera-aware projected-footprint and screen-error truth. It builds directly on Phase 24's camera-frame and picking spine.

This phase must keep the existing overview-first scheduler, batch-source support, bounded concurrency, and `Interactive` / `Refine` contract. It should not yet change GPU residency or color-map backend behavior.

## Decisions

- Keep the screen-error / projected-footprint contracts in `Videra.SurfaceCharts.Core`.
- Reuse the existing `SurfaceCameraFrame` and `SurfaceViewState` truth from Phase 23.
- Preserve overview-first request behavior and batch-request wiring in `SurfaceTileScheduler`.
- Let `Interactive` and `Refine` share one request spine with different quality/footprint thresholds, instead of inventing a second scheduler.

## Existing Code Insights

- `SurfaceLodPolicy.Select(...)` still consumes `SurfaceViewportRequest` and maps zoom density to a static target level.
- `SurfaceChartController.CreateCurrentRequestPlan()` still drives requests from `_cameraController.CurrentViewport` plus output size.
- `SurfaceChartController.UpdateViewState(...)` only refreshes the request pipeline when `DataWindow` changes; camera-only changes currently just invalidate the scene.
- `SurfaceTileScheduler.CreateRequestPlan(...)` still prioritizes tiles by visible/not-visible, center distance, and level penalty, with no camera-depth or projected-footprint awareness.
- `Interactive` quality already changes the effective output size, which gives the next phase a narrow existing seam for coarser-vs-refine request behavior.

## Deferred Ideas

- GPU-side LOD, resident-path slimming, and shader/backend color mapping stay in Phases 26-27.
- Any native/processing optimization beyond existing batch reads remains out of scope here.
