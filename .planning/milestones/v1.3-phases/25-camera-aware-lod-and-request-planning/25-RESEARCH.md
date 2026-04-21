# Phase 25 Research

## Current Request Spine

- `SurfaceChartController` owns request-pipeline supersession and invalidation.
- `SurfaceTileScheduler` owns request ordering, overview-first behavior, batch-vs-single fetch routing, and bounded concurrency.
- `SurfaceLodPolicy` currently selects levels from `HorizontalZoomDensity` / `VerticalZoomDensity`, which still assumes the viewport rectangle is the authoritative detail signal.

## Why Phase 25 Exists

After Phase 23-24, surface charts now have:

- `SurfaceViewState`
- `SurfaceCameraFrame`
- camera-true probe/pin/zoom anchors

But the request pipeline still behaves as if detail selection only depends on the current sample-space window and output size. In practice, orbit and perspective change how much screen area a tile covers even when the `DataWindow` stays fixed, so the request pipeline needs a camera-aware update trigger and a richer priority model.

## Planning Direction

### 25-01 Core Screen-Error Contracts

Add a core estimator layer that can answer:

- how large a tile footprint is in screen space
- whether a tile is currently visible or near the center of attention
- what level/error budget should be used in `Interactive` vs `Refine`

This should stay independent of Avalonia and scheduler concerns.

### 25-02 Controller / Runtime Migration

Move the request-plan inputs from:

- `SurfaceViewport`
- output size

to:

- `SurfaceCameraFrame`
- `SurfaceViewState`
- `InteractionQuality`

The controller should refresh the pipeline not only on data-window changes but also when camera movement materially changes projected footprint/error truth.

### 25-03 Scheduler Priority and Regression Coverage

Keep:

- overview-first
- batch-source support
- bounded concurrency
- retained-neighborhood pruning

But re-rank detail requests with camera-aware signals such as:

- on-screen / off-screen bucket
- projected footprint or screen error
- center-of-view bias
- depth / level penalty

Regression coverage should prove:

- small camera moves do not thrash requests
- larger orbit/zoom changes do re-target detail requests
- `Interactive` remains coarser/faster than `Refine`
