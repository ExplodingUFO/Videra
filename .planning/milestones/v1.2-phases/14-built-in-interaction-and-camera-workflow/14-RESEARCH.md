# Phase 14 Research: Built-in Interaction and Camera Workflow

**Phase:** 14  
**Name:** Built-in Interaction and Camera Workflow  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> How do we give `SurfaceChartView` built-in orbit / pan / zoom / focus behavior on top of the new `SurfaceViewState` contract, without pushing chart input back into `VideraView` or waiting for the future GPU renderer?

## Current Code Reality

### Phase 13 created the right seam, but there is still no built-in input

As implemented on branch `phase-13-runtime`:

- `SurfaceChartView` now owns a single `_runtime` field of type `SurfaceChartRuntime`.
- `SurfaceChartRuntime` owns chart-local orchestration, current `SurfaceViewState`, tile-cache coordination, and view-size/source updates.
- `SurfaceChartView` exposes `ViewState`, `FitToData()`, `ResetCamera()`, and `ZoomTo(...)`.
- `Viewport` is now a compatibility bridge to `ViewState.DataWindow`.

What still does **not** exist:

- no pointer press / move / release / wheel handling in `SurfaceChartView`
- no chart-local gesture state machine
- no built-in orbit / pan / dolly workflow
- no motion-vs-refine quality mode

The contract seam is ready; the interaction seam is not.

### The demo is still host-driven for navigation

Current `samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` still:

- computes an overview viewport and a fixed zoomed-detail viewport
- switches `_chartView.Viewport` through a combo-box-driven host flow
- does not exercise built-in interaction

This means the public API has improved, but the end-user interaction story is still not real yet.

### Camera pose exists, but the current software painter still owns the visible projection story

The persisted camera contract now exists in `SurfaceViewState.Camera`, but:

- `SurfaceScenePainter` is still a CPU pseudo-isometric painter
- current interactionless render path primarily follows sample-space data window changes
- camera-only changes in `SurfaceChartRuntime.UpdateViewState(...)` only invalidate the scene; they do not yet prove a visible orbit workflow by themselves

Planning implication:

- Phase 14 must be allowed to make the software path camera-aware enough to show orbit / pan / dolly behavior
- this must stay scoped to interaction correctness, not a premature GPU/render-host redesign

### Probe overlay already has a useful hook

`SurfaceChartView.Overlay.cs` already carries:

- `_probeScreenPosition`
- `UpdateProbeScreenPosition(Point probeScreenPosition)`

This is not a finished hover experience, but it means Phase 14 can wire pointer position into the control without inventing a second input path that later phases have to undo.

## What This Phase Must Deliver

This phase is responsible for `INT-01`, `INT-02`, `INT-03`, and `INT-04`.

### Required user-visible outcomes

1. End users can orbit around the chart's active focus point without host-authored camera glue.
2. End users can pan and dolly zoom through built-in interaction, with deterministic reset/preset behavior.
3. End users can focus a selected region through built-in zoom behavior instead of host-only viewport presets.
4. The chart explicitly switches between motion-friendly interactive quality and post-input refine quality.

### Explicitly out of scope for this phase

- axes, ticks, labels, legend, or probe readout productization
- GPU renderer / render-host package extraction
- large-dataset scheduler, residency, or cache-format redesign
- Rust or native acceleration work

## Recommended Design Direction

### Keep input entirely inside the surface-chart family

Do **not** reuse `VideraView` input routers or viewer-specific gesture abstractions.

Instead add chart-local input pieces such as:

```text
SurfaceChartView
  -> SurfaceChartInteractionController
      -> gesture state
      -> camera/data-window math
      -> quality-mode transitions
  -> SurfaceChartRuntime
      -> ViewState application
      -> tile/cache invalidation
```

This preserves the sibling boundary and keeps the chart's gesture semantics free to evolve independently.

### Split camera-only motion from data-window-changing motion

Not every gesture should trigger the same runtime behavior.

- **Orbit** should change `SurfaceViewState.Camera` and invalidate rendering, but it should not trigger tile re-selection.
- **Pan / focus zoom / box zoom** should change `SurfaceViewState.DataWindow`, because they affect what data should be requested and drawn.
- **Dolly zoom** may touch both, depending on whether the current software path represents zoom as camera distance, data window, or a hybrid compatibility strategy.

This distinction already fits the Phase-13 runtime: camera-only changes can stay cheap, while data-window changes continue to drive LOD and tile requests.

### Make the software painter camera-aware enough for truthful interaction

Phase 16 will build the longer-term renderer seam and GPU path. That does **not** mean Phase 14 can ignore visible camera behavior.

Recommended bounded scope:

- replace hard-coded pseudo-isometric projection constants with projection derived from `SurfaceViewState.Camera`
- keep using the CPU fallback path, but make it obey yaw / pitch / distance / target truthfully enough that orbit/pan are visible and testable
- avoid introducing GPU/resource abstractions in this phase

This keeps interaction honest without front-running the renderer milestone.

### Represent motion-vs-refine as an explicit mode, not an implicit timing accident

Add an interaction-quality concept such as:

- `Interactive`
- `Refine`

Expected behavior:

- enter `Interactive` when drag / wheel / focus gestures are active
- keep updates lightweight and latency-oriented during motion
- switch back to `Refine` after a short idle window and request the fuller detail path again

Phase 14 does not need the final smart scheduler of Phase 17. It does need the contract and timing behavior now.

## Risks To Plan Around

### Risk 1: Orbit exists in state, but not on screen

If camera pose changes are stored but the painter still ignores them, the phase will look architecturally complete while failing the user's interaction goal.

Planning implication:

- at least one plan must cover visible camera-aware software rendering behavior
- acceptance criteria must verify end-user-visible view changes, not just `ViewState` mutation

### Risk 2: Input math leaks viewer assumptions back into `VideraView`

The repository already has a strong sibling-boundary truth. Reusing viewer routers would regress that.

Planning implication:

- interaction controller and pointer event wiring stay inside `Videra.SurfaceCharts.Avalonia`
- no `VideraView` public API growth is allowed to “help” the chart

### Risk 3: Interactive/refine mode gets coupled to final scheduler design

If Phase 14 tries to solve large-data scheduling perfectly, it will sprawl into Phase 17.

Planning implication:

- use a coarse, explicit quality-mode seam now
- keep the first implementation simple enough that later phases can swap in better residency/scheduler logic underneath it

## Suggested Plan Shape

The phase naturally decomposes into three execution plans:

1. **Built-in gestures + visible camera motion**
   - pointer/wheel routing in `SurfaceChartView`
   - chart-local interaction controller
   - camera-aware software projection so orbit/pan/dolly are visible
2. **Focus / reset / preset workflow**
   - box/focus zoom behavior
   - deterministic reset and fit behavior
   - tests for region-focus correctness and host-independence
3. **Interaction quality modes + contract verification**
   - explicit `Interactive` vs `Refine` state
   - idle transition back to refine
   - tests that lock gesture routing, quality-mode transitions, and sibling-boundary truth

## Planning Guidance

- Prefer direct Avalonia pointer events on `SurfaceChartView` over a reusable abstraction borrowed from `VideraView`.
- Keep gesture defaults simple and testable: orbit, pan, wheel, box zoom, reset.
- Use `SurfaceViewState` as the only authoritative interaction state; do not add a parallel “camera scratch” model on the view.
- Treat the software painter as a truthful fallback that must visibly obey the new camera workflow, even if it remains temporary.

## Recommendation

Phase 14 should make `SurfaceChartView` feel like a real chart control for the first time. The goal is not to finish the final renderer. The goal is to make input, camera state, and motion/refine behavior real enough that later axis/probe and GPU work can build on a stable interactive shell instead of host-driven presets.
