---
phase: 23-unified-camera-projection-and-render-inputs
plan: 03
subsystem: surface-charts-overlay
tags: [surface-charts, avalonia, overlay, projection]
requires:
  - phase: 23-unified-camera-projection-and-render-inputs
    plan: 02
    provides: render-host camera-frame truth
provides:
  - camera-frame-backed `SurfaceChartProjection`
  - shared projection math across software painting and axis overlay
  - integration coverage that binds overlay projection to core camera math
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Rendering.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Overlay.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceAxisOverlayTests.cs
requirements-completed: [CAM-02]
completed: 2026-04-16
---

# Phase 23 Plan 03 Summary

## Accomplishments
- Refactored `SurfaceChartProjection` into a thin wrapper over `SurfaceProjectionMath` instead of maintaining a second yaw/pitch projection implementation.
- Switched both the overlay path and the software render projection path to use runtime-generated camera frames.
- Added an integration test that compares `SurfaceChartProjection` screen positions directly against the shared core projection math.
- Kept the legacy size/settings overload as a compatibility wrapper so existing tests and non-runtime call sites still have a narrow bridge during the migration.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceAxisOverlayTests"`

## Notes
- This phase intentionally does not change probe picking or LOD behavior; it only establishes the projection contract that those later phases will consume.
