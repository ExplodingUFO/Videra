---
phase: 19-surfacechart-runtime-and-view-state-recovery
plan: 01
subsystem: surface-charts-core
tags: [surface-charts, core, viewstate, camera, compatibility]
requires:
  - phase: 15-adaptive-axes-legend-and-probe-readout
    provides: stable chart-space/probe/value mapping semantics
provides:
  - authoritative `SurfaceDataWindow`, `SurfaceCameraPose`, and `SurfaceViewState` contracts
  - compatibility bridge between `SurfaceViewState.DataWindow` and legacy `SurfaceViewport`
  - request-level data-window support without changing existing zoom-density math
affects: [19-02, 19-03]
key-files:
  modified:
    - src/Videra.SurfaceCharts.Core/SurfaceDataWindow.cs
    - src/Videra.SurfaceCharts.Core/SurfaceCameraPose.cs
    - src/Videra.SurfaceCharts.Core/SurfaceViewState.cs
    - src/Videra.SurfaceCharts.Core/SurfaceViewport.cs
    - src/Videra.SurfaceCharts.Core/SurfaceViewportRequest.cs
    - src/Videra.SurfaceCharts.Core/SurfaceChartProjectionSettings.cs
    - tests/Videra.SurfaceCharts.Core.Tests/SurfaceViewportTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/SurfaceViewStateTests.cs
requirements-completed: [VIEW-01]
completed: 2026-04-16
---

# Phase 19 Plan 01 Summary

## Accomplishments
- Added `SurfaceDataWindow`, `SurfaceCameraPose`, and `SurfaceViewState` to `Videra.SurfaceCharts.Core`, giving the chart stack a persisted view contract with a separate data window and camera pose.
- Kept legacy `SurfaceViewport` alive as an explicit bridge through `ToDataWindow()` / `FromDataWindow(...)`, and updated `SurfaceViewportRequest` so it can accept authoritative data-window input without changing zoom-density behavior.
- Moved `SurfaceChartProjectionSettings` into the core assembly boundary while keeping its existing namespace, which removes the circular dependency trap without forcing downstream code to change `using` directives.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceViewportTests|FullyQualifiedName~SurfaceViewStateTests"`

## Notes
- Default camera pose creation intentionally centers on the active data-window semantics and current metadata value range; only yaw/pitch flow into the current projection path in Phase 19.
- The compatibility path stays additive: old viewport-based callers still work, but the new authoritative contract is `SurfaceViewState`.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
