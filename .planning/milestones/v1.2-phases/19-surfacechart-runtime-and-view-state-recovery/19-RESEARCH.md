# Phase 19 Research: SurfaceChart Runtime and View-State Recovery

**Phase:** 19  
**Name:** SurfaceChart Runtime and View-State Recovery  
**Date:** 2026-04-16  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> What has to land now so `v1.2` recovers `SurfaceViewState` and `SurfaceChartRuntime` on top of the shipped Phase 15-18 chart stack, without regressing render-host, scheduler, probe, or repository truth?

## Current Shipped Reality

### The chart stack already shipped meaningful Phase 15-18 behavior

Current branch truth after the milestone audit:

- `SurfaceChartRenderHost` owns the chart-local renderer seam and projects `RenderingStatus`.
- `SurfaceTileScheduler`, `SurfaceTileRequestPlan`, persistent cache readers, and `SurfaceTileStatistics` are already shipped and verified.
- `SurfaceChartView` already exposes chart-local axis/legend overlays plus hover and `Shift + LeftClick` pinned probes.
- public docs, Chinese mirrors, and repository guards currently freeze a truthful but limited story: independent surface-chart family, visible renderer status, host-driven `overview/detail` presets, and no finished built-in orbit / pan / dolly workflow.

Phase 19 must recover missing `VIEW-*` scope without undoing any of that shipped baseline.

### `SurfaceChartView` still owns too much orchestration

Current implementation facts:

- `SurfaceChartView` constructs `_tileCache`, `_cameraController`, `_controller`, and `_renderHost`.
- `Source`, `Viewport`, and `ColorMap` property changes still route directly into control-owned orchestration.
- overlay/probe/render-host synchronization still reads `Viewport`, `_tileCache`, and `_cameraController.ProjectionSettings` directly from the control.
- current public contract remains `SurfaceViewport` in sample-space terms.

This is exactly the gap the audit recorded: the shipped stack works, but the authoritative public state is still `Viewport`, not `SurfaceViewState`.

### `Viewport` is deeply embedded in the shipped stack

The current chart baseline uses `SurfaceViewport` in many places:

- `SurfaceViewportRequest` and LOD selection
- tile scheduling and retained-neighborhood pruning
- probe mapping from screen to sample space
- render-state dirty tracking
- demo selectors and sample tests
- docs and repository guards that describe host-driven `overview/detail` presets

Phase 19 therefore cannot simply delete `Viewport` or replace all viewport math with a brand-new min/max model. It must create an authoritative `ViewState` while preserving a compatibility bridge for the existing shipped pathways.

## Gap-Recovery Design Constraints

### This phase closes `VIEW-01`, `VIEW-02`, and `VIEW-03` only

Phase 19 is responsible for:

1. `SurfaceViewState` as the persisted public chart-view contract
2. host-facing `FitToData()`, `ResetCamera()`, and `ZoomTo(...)` APIs
3. a chart-local runtime seam that owns state transitions and controller coordination

It is **not** responsible for:

- built-in orbit / pan / dolly gestures
- motion-vs-refine interaction-quality modes
- replacing the current demo's host-driven navigation story
- introducing new `VideraView` chart APIs

That remaining interaction work belongs to Phase 20.

### Phase 19 must preserve the shipped Phase 15-18 baseline

The recovery must not:

- regress `SurfaceChartRenderHost` ownership
- break `SurfaceProbeService` / pinned-probe / overlay truth
- change scheduler semantics away from sample-space span selection
- invalidate repository wording that correctly says free-camera interaction is not done yet

That means the recovery has to be additive and bridge-oriented, not a full semantic rewrite.

## Recommended Design Direction

### 1. Make `SurfaceDataWindow` viewport-isomorphic

Use a dedicated data-window contract, but keep its shape aligned with the current viewport math:

```csharp
public readonly record struct SurfaceDataWindow(
    double StartX,
    double StartY,
    double Width,
    double Height);
```

Recommended helpers:

- `EndXExclusive` / `EndYExclusive`
- `ClampTo(SurfaceMetadata metadata)`
- `Normalize(SurfaceMetadata metadata)`
- `ToViewport()`
- `SurfaceDataWindow.FromViewport(SurfaceViewport viewport)`

Why this shape is preferred over a new min/max contract:

- it maps directly onto the shipped LOD / scheduler / probe logic
- it minimizes churn across Phase 15-18 integration tests
- it keeps `Viewport` as a compatibility shell rather than forcing every shipped component through another translation model

### 2. Introduce a future-facing `SurfaceCameraPose`, but wire it conservatively

Recommended contract:

```csharp
public readonly record struct SurfaceCameraPose(
    Vector3 Target,
    double YawDegrees,
    double PitchDegrees,
    double Distance,
    double FieldOfViewDegrees);
```

Phase-19 guidance:

- keep `SurfaceCameraPose` public and persisted now, even if only `YawDegrees` / `PitchDegrees` feed the current software projection path
- provide explicit helpers such as `CreateDefault(SurfaceMetadata metadata, SurfaceDataWindow dataWindow)` and `ToProjectionSettings()`
- do **not** expose `OrbitCamera` itself as the public chart contract

`Videra.Core.Cameras.OrbitCamera` is a useful internal reference for naming and reset semantics, but the chart's public state must remain a chart-domain contract rather than a viewer runtime object.

### 3. Make `SurfaceViewState` the single source of truth

Recommended contract:

```csharp
public readonly record struct SurfaceViewState(
    SurfaceDataWindow DataWindow,
    SurfaceCameraPose Camera);
```

Phase-19 rule:

- `ViewState` is authoritative
- `Viewport` becomes a compatibility bridge to `ViewState.DataWindow`
- scheduler / probe / render-host inputs may continue consuming `SurfaceViewport` internally, but only through explicit conversions from the authoritative data window

This avoids a permanent dual-contract trap.

### 4. Extract a runtime seam without moving visual-host responsibilities

Recommended ownership split:

```text
SurfaceChartView
  -> visual shell
  -> Avalonia property wiring
  -> native host / overlay layer / render-host ownership
  -> delegates source/view-size/view-state transitions to runtime

SurfaceChartRuntime
  -> authoritative source + view-state
  -> command helpers (FitToData / ResetCamera / ZoomTo)
  -> controller/camera synchronization
  -> request-generation supersession + invalidation callbacks
```

Important nuance:

- `SurfaceChartRuntime` does **not** need to absorb `SurfaceChartRenderHost`
- render-host and native handle ownership are still UI-shell concerns and should remain in `SurfaceChartView`
- the runtime only needs to own orchestration and state transitions that are currently spread across the control and interaction classes

This is the smallest extraction that closes the audit gap without destabilizing Phase 16-18 behavior.

### 5. Keep the current projection path working through an adapter layer

Current software projection uses `SurfaceChartProjectionSettings` with yaw/pitch only. Phase 19 should:

- derive `SurfaceChartProjectionSettings` from `SurfaceViewState.Camera`
- keep current `SurfaceChartProjection` / overlay presenters / software scene rendering functional
- avoid promising visible free-camera behavior before Phase 20

This makes the camera contract real now without overclaiming interaction completeness.

### 6. Ship command APIs and document them truthfully

Recommended public shell surface on `SurfaceChartView`:

- `ViewState` property
- `Viewport` property retained as compatibility bridge
- `FitToData()`
- `ResetCamera()`
- `ZoomTo(SurfaceDataWindow dataWindow)`

Recommended behavior:

- `FitToData()` resets the active data window to full metadata bounds and refreshes the default camera pose
- `ResetCamera()` restores only the camera pose for the current data window
- `ZoomTo(...)` updates the authoritative data window without exposing tile-cache or scheduler internals
- if `Source` is null, command methods should no-op instead of throwing

Docs/demo truth after Phase 19 should say:

- `ViewState` is now the primary chart-view contract
- `Viewport` still exists as a compatibility bridge
- the demo still uses host-driven presets for navigation because built-in free-camera interaction is a Phase-20 concern

## Risks To Plan Around

### Risk 1: `ViewState` and `Viewport` diverge

If both become independently mutable, later phases will inherit a desynchronized shell.

Planning implication:

- there must be a single authoritative write path
- tests must verify `Viewport -> ViewState.DataWindow` and `ViewState -> Viewport` synchronization

### Risk 2: runtime extraction regresses shipped render/probe/scheduler behavior

If Phase 19 “finishes” `SurfaceChartRuntime` by moving or renaming too much, it can easily break the already-shipped Phase 15-18 stack.

Planning implication:

- keep render-host ownership on the control
- preserve existing tile-cache and overlay behavior through compatibility adapters
- regression tests must cover lifecycle, scheduling, render-host, and probe overlay flows

### Risk 3: docs overclaim Phase-20 interaction work

If docs or demo language imply built-in orbit / pan / dolly is done once `ViewState` lands, the repository goes back out of sync with shipped reality.

Planning implication:

- update wording to add `ViewState` / command truth
- keep the current “host-driven overview/detail” limitation explicit until Phase 20 ships

### Risk 4: public commands accept invalid persisted state

`SurfaceViewState`, `SurfaceDataWindow`, and `SurfaceCameraPose` are natural persistence surfaces, so non-finite values or impossible spans can leak in.

Planning implication:

- constructors/helpers must reject non-finite values
- data-window spans must stay positive
- command tests should cover null-source safety and invalid value rejection

## Suggested Plan Shape

The recovery still decomposes naturally into three plans, but they must be written against the shipped Phase 15-18 baseline instead of the original unshipped Phase-13 assumptions:

1. **Core contracts and compatibility math**
   - add `SurfaceDataWindow`, `SurfaceCameraPose`, `SurfaceViewState`
   - keep `SurfaceViewport` as the explicit compatibility model
   - extend core tests for round-trip, clamp, and zoom-density behavior
2. **Runtime extraction on the shipped stack**
   - introduce `SurfaceChartRuntime`
   - move state transitions and controller coordination into the runtime
   - keep render-host / probe / scheduler behavior working through the bridge
3. **Public shell, demo/docs truth, and repository guards**
   - expose `ViewState` and host command APIs
   - keep demo navigation host-driven for now, but surface the new contract explicitly
   - update English/Chinese docs and repo guards to reflect the new truth without overclaiming free-camera interaction

## Recommendation

Phase 19 should be treated as a compatibility-sensitive recovery, not a clean-sheet rewrite. The right outcome is:

- `SurfaceViewState` becomes real and authoritative
- `Viewport` remains available but clearly secondary
- `SurfaceChartRuntime` owns chart-local orchestration
- Phase 15-18 shipped behavior stays intact
- Phase 20 can then add built-in orbit / pan / dolly on top of a real persisted contract instead of a legacy viewport shell
