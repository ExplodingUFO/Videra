# Phase 23 Research

## Problem

The chart runtime already persists a richer camera contract than the renderer and overlay consume. That produces a durable split:

- Public truth: `SurfaceViewState` + `SurfaceCameraPose`
- Internal truth: `SurfaceViewport` + `SurfaceChartProjectionSettings`

If Phase 24 or Phase 25 builds on the current internal path, the project will accumulate two incompatible camera/projection systems.

## Codebase Findings

### Core contracts

- `SurfaceCameraPose` keeps target, yaw, pitch, distance, and FOV, but rendering only consumes yaw/pitch through `ToProjectionSettings()`.
- `SurfaceViewState` and `SurfaceDataWindow` already exist and are the correct public seam to keep.

### Rendering

- `SurfaceChartRenderInputs` still takes `Viewport` and `ProjectionSettings`.
- `SurfaceChartRenderState` uses viewport/projection/view-size changes to mark `ProjectionDirty`.
- GPU frame uniforms still consume viewport rectangles, so a compatibility bridge is required for Phase 23.

### Overlay/software path

- `SurfaceChartProjection` computes its own screen transform from model vertices plus projection settings.
- `SurfaceAxisOverlayPresenter` and `SurfaceScenePainter` depend on that secondary projection helper.

### Runtime

- `SurfaceCameraController` and `SurfaceChartRuntime` already centralize `SurfaceViewState`.
- Interaction code writes yaw/pitch/data-window changes through the runtime, so once a shared camera frame exists the later phases can consume it without reopening the public API.

## Chosen Direction

1. Introduce shared camera-frame and projection math contracts in `Videra.SurfaceCharts.Core`.
2. Update render inputs/state to accept the shared contracts as the primary source of truth.
3. Rebuild software/overlay projection on top of the shared math, while keeping compatibility projection/settings properties for the GPU path and existing narrow tests.

## Constraints

- No `VideraView` surface-area changes.
- No silent deletion of `Viewport` or `SurfaceChartProjectionSettings` this phase.
- Preserve existing interaction workflows and render-host ownership.

## Verification Shape

- Core tests:
  - project/unproject round-trip
  - default camera frame keeps plot bounds in-frame
- Render tests:
  - camera-frame changes mark projection dirty without rebuilding resident geometry
- Avalonia integration tests:
  - render host consumes `ViewState` + camera frame
  - axis overlay still reacts correctly when camera yaw changes
