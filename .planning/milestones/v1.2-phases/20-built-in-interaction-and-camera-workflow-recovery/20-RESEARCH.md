# Phase 20 Research: Built-in Interaction and Camera Workflow Recovery

**Phase:** 20  
**Name:** Built-in Interaction and Camera Workflow Recovery  
**Date:** 2026-04-16  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> How do we recover built-in orbit / pan / dolly / focus behavior and interactive-vs-refine quality switching on top of the shipped Phase-19 `SurfaceViewState` contract, without pushing chart input back into `VideraView` or destabilizing the renderer/data-path truth already shipped in Phases 15-19?

## Current Code Reality

### Phase 19 recovered the contract seam, not the end-user workflow

The current branch now has the right persisted/public state shape:

- `SurfaceChartView` exposes `ViewState`, `FitToData()`, `ResetCamera()`, and `ZoomTo(...)`.
- `SurfaceChartRuntime` owns source/view-size/view-state transitions.
- `Viewport` now bridges to `ViewState.DataWindow` instead of acting as the only public truth.

What is still missing:

- no built-in orbit / pan / dolly gesture workflow
- no built-in focus or box-zoom workflow
- no explicit `Interactive` / `Refine` quality contract

Phase 20 therefore needs to complete the user workflow on top of an already-recovered runtime seam, not reopen the Phase-19 contract work.

### The input path is still probe-only

Current implementation facts:

- `SurfaceChartView.Input.cs` only handles pointer press / move / release plus capture-loss reset.
- There is still no `OnPointerWheelChanged(...)`.
- `SurfaceChartInteractionController` only recognizes a `Shift + LeftClick` pin-toggle threshold.
- The shipped built-in interaction truth is still hover plus `Shift + LeftClick` pinned probes.

That means the control now has a real persisted `ViewState`, but the user still cannot navigate it without host-authored camera/data-window commands.

### Camera state is now persisted, but visible zoom still cannot rely on camera distance alone

Phase 19 introduced `SurfaceCameraPose`, but the current projection path matters:

- `SurfaceChartRuntime` stores the full camera pose.
- `SurfaceCameraController.ProjectionSettings` derives from `SurfaceCameraPose.ToProjectionSettings()`.
- `SurfaceChartProjection` currently uses only yaw and pitch from `SurfaceChartProjectionSettings`.
- The software projection path does **not** yet use camera distance or field-of-view to create visible perspective zoom.

Planning implication:

- **Orbit** can be visibly implemented now by mutating yaw/pitch.
- **Pan**, **dolly**, and **focus zoom** should primarily operate through `ViewState.DataWindow` and keep camera target synchronized, rather than depending on camera-distance-only zoom that the current projection ignores.

### Existing seams are already good enough for interaction anchors

The chart already has two useful hooks:

- `SurfaceChartView` tracks `_probeScreenPosition` and updates it on pointer move.
- overlay/probe state already resolves hovered probe truth from screen position, loaded tiles, and current viewport.

Planning implication:

- wheel-dolly can anchor on the hovered sample when hover resolution succeeds
- the fallback anchor can remain the current data-window center
- focus selection can be computed from screen-space drag against the current data window and control bounds, without adding 3D picking or renderer-specific hit testing

### The scheduler already has a practical lever for interactive-vs-refine quality

Current request selection flows through:

- `SurfaceChartController.UpdateViewSize(...)`
- `SurfaceTileScheduler.CreateRequestPlan(SurfaceViewport viewport, Size outputSize)`
- `SurfaceViewportRequest(metadata, viewport/dataWindow, outputWidth, outputHeight)`
- `SurfaceLodPolicy.Select(...)`

The key detail is that requested output size influences zoom density. Smaller effective output dimensions produce coarser selected detail.

Planning implication:

- Phase 20 can implement explicit `Interactive` / `Refine` quality by scaling the effective request size during motion, then restoring full-size refine requests after a short idle window
- this reuses the shipped scheduler/cache/data-path behavior instead of inventing a second tile-selection protocol

### Demo, docs, and repository guards still freeze the pre-Phase-20 truth

Current public truth is still:

- the demo uses host-driven `overview/detail` presets
- docs explicitly say built-in orbit / pan / dolly is not finished
- repository tests guard those exact phrases

Phase 20 must update code, demo, docs, and repository guards together. Shipping only the gesture code without flipping the public truth would leave the milestone internally inconsistent again.

## What This Phase Must Deliver

This phase is responsible for `INT-01`, `INT-02`, `INT-03`, and `INT-04`.

### Required user-visible outcomes

1. End users can orbit the chart around the active focus point through built-in pointer interaction.
2. End users can pan and dolly zoom through built-in interaction without host-authored viewport presets.
3. End users can focus a selected data region with built-in zoom behavior rather than the demo's current overview/detail combo-box flow.
4. During interaction the chart explicitly enters a lighter-quality mode and then returns to refine mode after input settles.

### Explicitly out of scope for this phase

- moving chart interaction semantics into `VideraView`
- reopening render-host ownership or GPU/software backend architecture
- broad renderer redesign to add full perspective or scene-camera abstractions
- 3D picking, contour/slice analysis, or other future chart features beyond built-in navigation

## Recommended Design Direction

### Keep all gesture semantics inside the surface-chart family

Do **not** route chart interaction through viewer routers or shared `VideraView` abstractions.

The intended ownership remains:

```text
SurfaceChartView
  -> Avalonia pointer/wheel capture
  -> chart-local overlay feedback
  -> delegates to SurfaceChartInteractionController

SurfaceChartInteractionController
  -> gesture state machine
  -> delta-to-ViewState math
  -> interaction-quality mode transitions

SurfaceChartRuntime
  -> authoritative ViewState application
  -> request-pipeline refresh
  -> render/overlay invalidation
```

This keeps the sibling boundary intact and prevents chart-specific input semantics from leaking back into viewer code.

### Use gesture defaults that preserve the shipped pin workflow

Recommended built-in defaults:

- `Left drag`: orbit
- `Right drag`: pan
- `Mouse wheel`: dolly zoom around hovered sample or data-window center
- `Ctrl + Left drag`: box/focus zoom
- `Shift + LeftClick`: keep the existing pinned-probe toggle

Why this mapping fits the current codebase:

- it preserves the already-shipped `Shift + LeftClick` contract
- it avoids modifier conflicts between orbit and box zoom
- it stays simple enough for headless Avalonia integration tests

### Keep `ViewState` authoritative for every gesture

The new interaction path should not add a second camera or viewport scratch model.

Recommended state split:

- **Orbit** mutates `ViewState.Camera` (yaw/pitch, and target if needed)
- **Pan** mutates `ViewState.DataWindow` and keeps the camera target aligned to the shifted window
- **Dolly** mutates `ViewState.DataWindow` around an anchor sample and refreshes the compatible camera pose
- **Focus/box zoom** produces a new `SurfaceDataWindow` from the selected region and writes it through the same runtime path as `ZoomTo(...)`

That keeps Phase 20 compatible with the Phase-19 contract instead of bypassing it.

### Make focus selection pure sample-space math, not renderer picking

The current branch already knows:

- control bounds
- current data window
- current hover/probe sample coordinates when hover succeeds

That is enough to implement focus selection conservatively:

- map the drag rectangle from screen coordinates into data-window coordinates
- clamp the resulting `SurfaceDataWindow` to metadata
- enforce a minimum span to avoid degenerate focus windows
- render the active selection rectangle on the chart overlay layer

This is materially safer than introducing a new hit-test or projection-unprojection system inside the renderer.

### Make `Interactive` / `Refine` an explicit, observable contract

Phase 20 should not rely on timing accidents or incidental stale-tile behavior.

Recommended behavior:

- enter `Interactive` when drag or wheel interaction starts
- while interactive, request coarser tiles by using a reduced effective request size
- cancel or supersede stale refine transitions when fresh input arrives
- return to `Refine` after a short idle window and rerun the full request path

If demo/tests need to project this truth, expose the current interaction-quality mode through a narrow read-only control surface rather than forcing tests to infer it from request counts alone.

## Risks To Plan Around

### Risk 1: Dolly exists in state, but not on screen

If Phase 20 implements dolly only by changing camera distance, the current projection path will not show a visible zoom change.

Planning implication:

- implement visible dolly primarily through data-window scaling
- keep camera pose compatible with that new window

### Risk 2: New gestures break the already-shipped pin contract

`Shift + LeftClick` is already shipped and documented. Regressing it would be a behavior regression, not a refactor casualty.

Planning implication:

- keep an explicit pin gesture branch in the controller
- add regression coverage that proves pinning still works after orbit/pan/dolly ship

### Risk 3: Interactive/refine mode becomes untestable timing noise

If mode transitions are buried inside ad hoc `Task.Delay` code with no observable state, the repository cannot lock the new contract.

Planning implication:

- introduce one explicit quality-mode concept
- make transitions cancellable/deterministic
- give tests one truthful observation seam

### Risk 4: Demo/docs keep telling the old story

If the control ships built-in interaction but the demo still centers on host-driven overview/detail presets, the milestone remains publicly inconsistent.

Planning implication:

- Phase 20 must include demo/docs/repository-guard updates as part of the execution plan, not as optional cleanup

## Suggested Plan Shape

The recovery still decomposes cleanly into three plans:

1. **Built-in orbit / pan / dolly gesture pipeline**
   - pointer and wheel routing in `SurfaceChartView`
   - chart-local gesture state machine
   - visible orbit/pan/dolly behavior on top of the Phase-19 runtime
2. **Focus zoom and persisted reset workflow**
   - marquee selection
   - selected-region focus behavior through `ViewState`
   - deterministic `FitToData()` / `ResetCamera()` behavior after built-in gestures
3. **Interactive/refine quality contract and truth updates**
   - explicit quality-mode transitions
   - request-path degradation/refine behavior
   - demo/docs/repository-guard migration from host-driven preset truth to built-in interaction truth

## Recommendation

Phase 20 should finish the interaction shell on top of the already-shipped runtime/view-state seam instead of reopening renderer architecture.

The right outcome is:

- built-in gestures write only through `ViewState`
- panning/zooming stay compatible with the existing data-window/scheduler math
- interactive/refine quality is explicit and observable
- demo/docs/tests stop claiming host-driven presets are the real workflow

That closes the last milestone gap without undoing the verified Phase 15-19 baseline.
