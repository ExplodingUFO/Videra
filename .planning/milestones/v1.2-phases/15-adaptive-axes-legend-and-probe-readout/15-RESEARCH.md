# Phase 15 Research: Adaptive Axes, Legend, and Probe Readout

**Phase:** 15  
**Name:** Adaptive Axes, Legend, and Probe Readout  
**Date:** 2026-04-14  
**Status:** Ready for planning

## Objective

Answer the planning question for this phase:

> How do we add professional chart axes, a truthful color legend, and a real probe/pinned-readout workflow on top of the new Phase-14 interaction shell, without pushing chart overlay semantics back into `VideraView` or waiting for the future GPU renderer?

## Current Code Reality

### Planning baseline is the Phase-14 implementation branch, not the stale root checkout

The planning state now says Phase 14 is complete on branch `phase-13-runtime`. That branch already has:

- `SurfaceChartRuntime` owning chart-local orchestration
- `SurfaceViewState` as the public view contract
- built-in `orbit / pan / dolly / Ctrl+drag box zoom`
- explicit `Interactive` / `Refine` quality modes

The root checkout has not merged those code changes yet, so planning must follow the implemented Phase-14 branch truth rather than the older root-file snapshot.

### Axes and legend still do not exist as a product surface

Current chart metadata and rendering only provide raw ingredients:

- `SurfaceMetadata` exposes `HorizontalAxis`, `VerticalAxis`, and `ValueRange`
- `SurfaceAxisDescriptor` only carries `Label`, `Unit`, `Minimum`, and `Maximum`
- `SurfaceColorMap` only carries `Range` and `Palette`

What does **not** exist:

- no X/Y/Z overlay state
- no tick-generation model
- no legend presenter
- no camera-aware axis-edge selection
- no reusable chart projector for overlay layout

Phase 15 therefore needs both the presentation layer and the correctness contract.

### Probe exists, but only as an internal hover bubble

Current `SurfaceProbeOverlayPresenter` already:

- maps pointer position into sample space through `Viewport`
- remaps sample coordinates into loaded coarse-tile grids
- returns a `SurfaceProbeResult(sampleX, sampleY, value)`
- renders a small hover bubble in 2D overlay space

But it is still too primitive for Phase 15:

- compute and render are coupled in one class
- probe output only contains sample-space X/Y plus value
- there is no conversion to axis-space units
- there is no approximate/exact flag
- there is no pinned or highlighted probe workflow

This is a good seam, but not a finished feature.

### Camera-aware projection exists, but only inside the triangle painter

Phase 14 made `SurfaceScenePainter` camera-aware. That math is still trapped inside the software painter:

- `ProjectWithCamera(...)`
- raw projected coordinates
- centering/fit transform into view size

Axis layout and pinned probe markers need the same screen-space truth. If they duplicate the math, labels and markers will drift away from the actual surface projection.

Planning implication:

- Phase 15 should extract or centralize the projection helper before building axis/legend layout on top of it.

### The demo still does not exercise chart-product overlay features

`samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs` still:

- sets `ColorMap`
- switches preset viewports from host UI
- reports only dataset/source/viewport text

There is still no demo-level axis, legend, hover readout, or pinned-point story. That is acceptable for Phase 15 because demo/product truth is a later milestone, but the implementation plans must not assume that the sample already covers the new overlay workflows.

## What This Phase Must Deliver

This phase is responsible for:

- `AXIS-01`
- `AXIS-02`
- `PROBE-01`
- `PROBE-02`

### Required user-visible outcomes

1. X/Y/Z axes render with titles, units, adaptive ticks, and camera-aware placement.
2. The chart shows a built-in color legend consistent with the active color map range.
3. Hover readout exposes truthful X/Y/Z information without host-owned overlay code.
4. Users can pin or unpin probe points for persistent comparison.

### Explicitly out of scope for this phase

- GPU renderer / render-host redesign
- scheduler, residency, or cache-format rework
- streaming, contours, slices, or richer analysis layers
- broad host-facing chart options surface redesign

## Recommended Design Direction

### Keep axes, legend, and readout in the chart-local 2D overlay layer for now

Phase 15 does **not** need 3D text geometry or GPU label rendering.

Use the same high-level split already present in the chart control:

```text
SurfaceChartView
  -> software surface scene
  -> chart-local 2D overlay presenters
       - axes
       - legend
       - probe hover/pins
```

This matches the repository's existing overlay patterns and keeps the work compatible with the later GPU renderer phase.

### Extract a reusable projector from `SurfaceScenePainter`

Do not let axis/legend/probe overlay code re-implement camera projection by hand.

Recommended direction:

- extract camera projection + view-fit transform into a reusable chart-local helper
- let both `SurfaceScenePainter` and overlay presenters consume the same helper
- keep it inside the surface-chart module family

That gives Phase 15 camera-aware placement without touching `VideraView`.

### Treat X/Y axis values and sample-space coordinates as different truths

Current probe code reports sample-space coordinates only. Professional chart readout needs both:

- sample-space coordinates for tile/bounds correctness
- axis-space values derived from `SurfaceAxisDescriptor.Minimum..Maximum`

Planning implication:

- introduce a richer probe model that carries `SampleX`, `SampleY`, `AxisX`, `AxisY`, `Value`, and `IsApproximate`
- convert sample-space to axis-space by linear interpolation across metadata width/height

### Approximate vs exact must be explicit

Current probe resolver already knows when a loaded tile spans more source samples than its grid cells.

That gives a concrete first-pass rule:

- `IsApproximate = true` when `tile.Bounds.Width != tile.Width` or `tile.Bounds.Height != tile.Height`
- `IsApproximate = false` when the loaded tile grid matches its source-space span 1:1

This rule is simple, testable, and truthful enough until a future exact-hover refinement path exists.

### Pinned probes should compose with the Phase-14 gesture set

Phase 14 already uses:

- left drag = orbit
- right drag = pan
- wheel = dolly
- Ctrl + left drag = box zoom

Pinned probes therefore need a non-conflicting gesture. The cleanest bounded option is:

- `Shift + LeftClick` with travel under the click threshold toggles pin/unpin for the current hovered probe

That keeps the pin workflow built in without inventing a host-owned tool mode.

## Risks To Plan Around

### Risk 1: Axis overlay drifts away from the rendered surface

If the axis layer uses different projection math than the painter, ticks and labels will look plausible but be spatially wrong.

Planning implication:

- one plan must centralize projector math
- axis tests must verify camera-dependent edge switching rather than only static text presence

### Risk 2: Probe remains coupled to one bubble implementation

If `SurfaceProbeOverlayPresenter` keeps both compute and render responsibilities, pinned probes and approximate/exact semantics will become fragile.

Planning implication:

- split probe computation into a reusable service
- keep overlay rendering focused on presentation only

### Risk 3: Legend and probe disagree about value truth

If legend labels come from `Metadata.ValueRange` while colors come from `ColorMap.Range`, users will see inconsistent value stories.

Planning implication:

- use `ColorMap.Range` when `ColorMap` is present
- only fall back to `Metadata.ValueRange` when the chart is rendering with the fallback color map

## Suggested Plan Shape

The phase decomposes cleanly into three execution plans:

1. **Adaptive axes + legend contract**
   - reusable projector
   - camera-aware axis layout
   - legend/value-range overlay
2. **Probe/readout workflow**
   - richer probe model
   - hover readout with axis values + approximate/exact
   - built-in pin toggle workflow
3. **Correctness regression matrix**
   - sample-to-axis mapping tests
   - coarse-tile / clamped-viewport coverage
   - boundary guard to keep overlay logic chart-local

## Planning Guidance

- Keep the new overlay pieces inside `Videra.SurfaceCharts.Avalonia`; do not reuse `VideraView` overlay presenters or annotation contracts.
- Prefer deterministic overlay-state tests over screenshot-heavy verification.
- Derive Z-axis and legend labels from the active color/value range, not from ad-hoc strings.
- Make pinned-probe interaction concrete and minimal instead of inventing a broad annotation system in this phase.

## Recommendation

Phase 15 should treat axes, legend, and probe readout as the first real productization layer on top of the Phase-14 interaction shell. The key is not visual polish first; it is truthfulness. If projection, axis values, legend range, and probe state all line up now, later renderer and large-data phases can optimize underneath a stable chart contract instead of re-litigating overlay semantics.
