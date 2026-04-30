---
phase: 19-surfacechart-runtime-and-view-state-recovery
plan: 02
subsystem: surface-charts-runtime
tags: [surface-charts, avalonia, runtime, render-host, scheduler]
requires:
  - phase: 19-surfacechart-runtime-and-view-state-recovery
    plan: 01
    provides: authoritative view-state/core compatibility contracts
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: `SurfaceChartRenderHost` seam and renderer-status truth
provides:
  - chart-local `SurfaceChartRuntime` ownership for source/view-size/view-state transitions
  - authoritative `CurrentViewState` storage in `SurfaceCameraController`
  - runtime-driven scheduler/render/probe orchestration without moving visual-host ownership
affects: [19-03]
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartRuntime.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceCameraController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewLifecycleTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRenderHostIntegrationTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs
requirements-completed: [VIEW-03]
completed: 2026-04-16
---

# Phase 19 Plan 02 Summary

## Accomplishments
- Added `SurfaceChartRuntime` as the chart-local orchestration seam that now owns `Source`, arranged size, `ViewState`, and the command entrypoints used by the public shell.
- Updated `SurfaceCameraController` to keep `CurrentViewState` and derive projection settings from `SurfaceCameraPose`, then routed `SurfaceChartController` through `UpdateViewState(...)` / `UpdateDataWindow(...)`.
- Reduced `SurfaceChartView` to a visual shell for render-host, native-host, and overlay ownership while render-host sync and probe/axis/legend overlays now read runtime-derived compatibility data.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartViewLifecycleTests|FullyQualifiedName~SurfaceChartTileSchedulingTests|FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~SurfaceChartProbeOverlayTests"`

## Notes
- `SurfaceChartRenderHost` and native-handle lifecycle stayed in `SurfaceChartView`; Phase 19 only extracted orchestration.
- The scheduler still consumes `SurfaceViewport` math through `ViewState.DataWindow.ToViewport()`, which keeps Phase 15-18 render/probe/data-path behavior intact.
- Runtime command helpers intentionally stop short of built-in free-camera interaction; that remains Phase 20 work.
