# Phase 13 Research: SurfaceChart Runtime and View-State Contract

**Phase:** 13  
**Name:** SurfaceChart Runtime and View-State Contract  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> What has to change first so the surface-chart stack can gain professional interaction and a GPU main path later, without pushing chart semantics back into `VideraView`?

## Current Code Reality

### `SurfaceChartView` owns too much orchestration today

Current implementation facts from `Videra.SurfaceCharts.Avalonia`:

- `SurfaceChartView` constructs and owns `_tileCache`, `_cameraController`, and `_controller`.
- `Source`, `Viewport`, and `ColorMap` property changes immediately drive controller/cache/render-scene work inside the control.
- `SurfaceChartView.Rendering.cs` rebuilds `_renderScene` directly from loaded tiles and renders through `SurfaceScenePainter`.
- `ArrangeOverride(...)` feeds view size directly into the controller; there is no chart-local runtime seam between UI shell and orchestration.

This makes the Avalonia control the effective runtime host, which is the wrong ownership boundary for the rest of the milestone.

### `SurfaceViewport` is a data-window contract, not a true camera contract

From `SurfaceViewport` and `SurfaceViewportRequest`:

- `SurfaceViewport` models a sample-space rectangle with `StartX`, `StartY`, `Width`, `Height`.
- `SurfaceViewportRequest` derives `ZoomDensity` from viewport span over output pixels.
- `SurfaceCameraController` currently only stores `CurrentViewport`.

This is enough for overview/detail LOD selection, but it is not enough for professional orbit/pan/zoom. The public model must split:

- data-space window used for LOD and prefetch
- actual 3D camera pose used for interaction and projection

### Current phase should not try to solve rendering yet

Current rendering still depends on:

- `SurfaceRenderer.BuildScene(...)` creating a CPU-side scene snapshot
- `SurfaceScenePainter` projecting triangles into a pseudo-isometric 2D view
- tile loads invalidating and rebuilding the scene in the control

That is a later-phase concern. Phase 13 only needs to create the runtime and contract seams that let rendering be replaced safely in Phase 16.

## What This Phase Must Deliver

This phase is responsible for `VIEW-01`, `VIEW-02`, and `VIEW-03` only.

### Required contract outcomes

1. Introduce a persisted chart view-state model that separates:
   - `SurfaceDataWindow`
   - `SurfaceCameraPose`
   - combined `SurfaceViewState`
2. Move control-level orchestration into a chart-local runtime/service layer.
3. Keep `SurfaceChartView` as the Avalonia shell and public entry point.
4. Preserve sibling separation from `VideraView`; no viewer-side API expansion is allowed here.
5. Maintain a backward-compatibility bridge from the existing `Viewport` API to the new data-window semantics.

### Explicitly out of scope for this phase

- built-in orbit/pan/zoom gesture behavior
- adaptive axes, legend, or probe UX
- GPU renderer or native host integration
- scheduler/residency/cache throughput upgrades
- Rust or FFI work

## Recommended Design Direction

### New core contracts

Add chart-domain contracts in `Videra.SurfaceCharts.Core`:

```csharp
public sealed record SurfaceDataWindow(double XMin, double XMax, double YMin, double YMax);

public sealed record SurfaceCameraPose(
    Vector3 Target,
    double Yaw,
    double Pitch,
    double Distance,
    double FieldOfView,
    SurfaceProjectionMode ProjectionMode);

public sealed record SurfaceViewState(
    SurfaceDataWindow DataWindow,
    SurfaceCameraPose Camera);
```

Design intent:

- `SurfaceDataWindow` is the descendant of today's `SurfaceViewport`.
- `SurfaceCameraPose` is future-facing and can remain mostly inert in this phase, but it must exist now so later phases do not overload `Viewport`.
- `SurfaceViewState` becomes the long-term persisted public model.

### New runtime seam

Introduce a chart-local runtime in the surface-chart family:

```text
SurfaceChartView
  -> SurfaceChartRuntime
      -> source/update/view-size handling
      -> view-state synchronization
      -> tile scheduler / cache coordination
      -> render invalidation callbacks
```

Minimum required outcome in this phase:

- `SurfaceChartRuntime` owns orchestration that is currently split across `SurfaceChartView`, `SurfaceChartController`, and `SurfaceCameraController`.
- `SurfaceChartView` keeps property wiring, size changes, visual invalidation, and future input hookup.
- existing render/tile/cache behavior may keep working through the new seam until later phases replace it.

### Compatibility strategy

Do not break the current shell abruptly. Instead:

- keep `Viewport` as a compatibility property for now
- map it internally to `SurfaceViewState.DataWindow`
- introduce `ViewState` alongside it
- let `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` target the new model

This phase should end with the public contract stable enough that later phases can build on it without another semantic rewrite.

## Risks To Plan Around

### Risk 1: “Runtime” becomes a renamed controller

If `SurfaceChartRuntime` only renames `SurfaceChartController` but still leaves cache/camera/render ownership tangled in the control, the phase will look complete while failing its goal.

Planning implication:

- tasks must move ownership, not just names
- acceptance criteria must prove `SurfaceChartView` no longer constructs/coordinates the old stack directly

### Risk 2: `Viewport` and `ViewState` drift

If the compatibility bridge is vague, later phases will not know which contract is authoritative.

Planning implication:

- choose one source of truth now: `SurfaceViewState`
- define how `Viewport` is projected into `DataWindow`
- add tests for property synchronization and persistence behavior

### Risk 3: chart semantics leak into `VideraView`

This phase will touch public contracts and hosting assumptions. It must not “solve reuse” by reaching into viewer APIs.

Planning implication:

- read existing repository/tests/docs boundary files before editing
- acceptance criteria should assert no new `VideraView` chart API surface is introduced

## Suggested Plan Shape

The phase naturally decomposes into three execution plans:

1. **Core view-state contract**
   - add `SurfaceDataWindow`, `SurfaceCameraPose`, `SurfaceViewState`
   - define compatibility helpers from `SurfaceViewport`
   - add tests for mapping and persistence semantics
2. **Runtime extraction**
   - introduce `SurfaceChartRuntime`
   - move orchestration responsibilities out of `SurfaceChartView`
   - keep current rendering/tile path functioning through the runtime seam
3. **Public shell + compatibility bridge**
   - add `ViewState` and command-style APIs on `SurfaceChartView`
   - keep `Viewport` as a bridge
   - update docs/tests to lock sibling boundary and contract language

## Planning Guidance

- Prefer one runtime seam over several tiny helper types.
- Keep the camera contract simple in this phase; it only needs to be real, not feature-complete.
- Do not invent GPU/resource abstractions here just because later phases need them.
- Make test coverage part of the phase, especially around property synchronization and boundary preservation.

## Recommendation

The phase should end with a stable contract, not a flashy feature. If it succeeds, later phases can add interaction, axes/probe, and rendering on top of a clean `SurfaceViewState` + `SurfaceChartRuntime` foundation. If it fails, every later chart feature will keep paying down the same ownership confusion.
