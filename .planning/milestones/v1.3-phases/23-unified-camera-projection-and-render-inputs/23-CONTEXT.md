# Phase 23: Unified Camera, Projection, and Render Inputs - Context

**Gathered:** 2026-04-16  
**Status:** Ready for planning and execution  
**Mode:** Autonomous

## Phase Boundary

Phase 23 closes the current split between public chart view-state and the internal render/overlay projection path. The shipped chart already exposes `SurfaceViewState` / `SurfaceCameraPose`, but the renderer, overlay, and several tests still treat `SurfaceViewport + SurfaceChartProjectionSettings` as the real source of truth.

This phase does not introduce true 3D picking or camera-aware LOD yet. It only establishes the shared camera/projection contract that those later phases will consume.

## Decisions

### Locked for this phase

- Keep all new math and camera-frame contracts inside `Videra.SurfaceCharts.*`.
- Preserve the `VideraView` sibling boundary.
- Preserve `Viewport` and `SurfaceChartProjectionSettings` as compatibility shells where existing tests or GPU plumbing still need them.
- Migrate software rendering and overlay projection to the shared camera-frame math now.
- Leave true ray picking, request-plan changes, GPU resident slimming, and shader color mapping to later phases.

### Deferred to later phases

- Screen-ray -> heightfield picking truth
- Camera-aware LOD / scheduler refresh rules
- GPU-side frame uniform redesign
- Professional overlay layout features beyond projection migration

## Existing Code Insights

- `SurfaceCameraPose` already stores `Target`, `YawDegrees`, `PitchDegrees`, `Distance`, and `FieldOfViewDegrees`, but `ToProjectionSettings()` currently drops everything except yaw/pitch.
- `SurfaceChartRenderInputs` still carries `Viewport` and `ProjectionSettings` instead of a unified camera-frame contract.
- `SurfaceChartRenderState` marks projection dirtiness from viewport/projection/view-size changes, not from a first-class camera frame.
- `SurfaceChartView.Rendering.cs` and `SurfaceChartView.Overlay.cs` build render and overlay state from `_runtime.CurrentViewport` and `_runtime.ProjectionSettings`.
- `SurfaceChartProjection` owns a second projection math path that is disconnected from the new camera/view-state contract.
- Existing tests already cover view-state, render-host integration, incremental rendering, and axis overlay behavior, so Phase 23 can extend those seams without inventing new test infrastructure.

## Specific Ideas

- Add a public `SurfaceCameraFrame` contract plus shared `SurfaceProjectionMath`.
- Let `SurfaceChartRenderInputs` carry `SurfaceViewState` and `SurfaceCameraFrame`, while compatibility accessors keep old paths alive where needed.
- Route software painter and overlay projection through the shared camera-frame math so Phase 24 picking can reuse it directly.

## Deferred Ideas

- No demo/docs truth changes unless Phase 23 introduces a new public behavior that needs to be documented immediately.
