---
phase: 24-true-3d-picking-and-probe-truth
plan: 02
subsystem: surface-charts-avalonia-probe-overlay
tags: [surface-charts, avalonia, overlay, picking]
provides:
  - camera-frame-based probe service resolution
  - overlay presenter path that consumes 3D pick truth
  - `SurfaceChartView` overlay invalidation wired to the new probe path
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartProbeOverlayTests.cs
requirements-completed: [PICK-01]
completed: 2026-04-16
---

# Phase 24 Plan 02 Summary

## Accomplishments
- Added a camera-frame-based `SurfaceProbeService.ResolveFromScreenPosition(...)` overload that resolves `screen -> ray -> heightfield hit -> SurfaceProbeInfo`.
- Added a new `SurfaceProbeOverlayPresenter.CreateState(...)` overload that consumes `SurfaceCameraFrame` instead of viewport-linear probe math.
- Switched the real `SurfaceChartView` overlay path to publish the runtime camera frame into probe resolution.
- Added integration coverage proving the overlay can hover a projected peak and recover world/tile truth.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartProbeOverlayTests"`

## Notes
- The legacy presenter overload was left in place as a compatibility test seam while the real control path now uses camera-frame picking.
