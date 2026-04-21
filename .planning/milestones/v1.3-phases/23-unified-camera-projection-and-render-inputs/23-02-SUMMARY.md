---
phase: 23-unified-camera-projection-and-render-inputs
plan: 02
subsystem: surface-charts-render-host
tags: [surface-charts, rendering, runtime, render-host]
requires:
  - phase: 23-unified-camera-projection-and-render-inputs
    plan: 01
    provides: shared camera-frame math
provides:
  - render inputs led by `SurfaceViewState` + `SurfaceCameraFrame`
  - render-state dirty tracking based on view-state instead of the old split seam
  - render-host publication of camera-frame truth from `SurfaceChartView`
key-files:
  modified:
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderInputs.cs
    - src/Videra.SurfaceCharts.Rendering/SurfaceChartRenderState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartRuntime.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderHostTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Rendering/SurfaceChartRenderStateTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartRenderHostIntegrationTests.cs
requirements-completed: [CAM-01]
completed: 2026-04-16
---

# Phase 23 Plan 02 Summary

## Accomplishments
- Reworked `SurfaceChartRenderInputs` so `ViewState` and `CameraFrame` are the primary input seam, while `Viewport` and `ProjectionSettings` remain as compatibility fallbacks.
- Updated `SurfaceChartRenderState` to treat view-state changes as the authoritative projection-dirty signal instead of comparing the old viewport/projection split.
- Added a runtime helper for camera-frame creation and switched `SurfaceChartView.Rendering.cs` to publish that frame into the render host on every sync.
- Added render-host and integration coverage proving the host now carries `ViewState` and `CameraFrame` truth without regressing resident-tile delta behavior.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderStateTests|FullyQualifiedName~SurfaceChartRenderHostTests"`
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests"`

## Notes
- The GPU/backend side still reads compatibility projection data today; this phase only changes the owning input contract, not the later resident/uniform slimming work.
