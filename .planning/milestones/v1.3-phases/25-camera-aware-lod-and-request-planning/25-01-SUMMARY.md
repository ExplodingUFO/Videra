---
phase: 25-camera-aware-lod-and-request-planning
plan: 01
subsystem: surface-charts-core-lod
tags: [surface-charts, core, lod, camera-frame]
provides:
  - projected-footprint and screen-error contracts for camera-aware LOD
  - `SurfaceLodPolicy` overloads that consume `SurfaceCameraFrame`
  - core regression coverage for projected-footprint-driven selection
key-files:
  modified:
    - src/Videra.SurfaceCharts.Core/LOD/SurfaceTileProjectedFootprint.cs
    - src/Videra.SurfaceCharts.Core/LOD/SurfaceScreenErrorEstimator.cs
    - src/Videra.SurfaceCharts.Core/SurfaceLodPolicy.cs
    - tests/Videra.SurfaceCharts.Core.Tests/LOD/SurfaceScreenErrorEstimatorTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/SurfaceLodPolicyTests.cs
requirements-completed: [LOD-01]
completed: 2026-04-16
---

# Phase 25 Plan 01 Summary

## Accomplishments
- Added `SurfaceTileProjectedFootprint` and `SurfaceScreenErrorEstimator` so the core LOD path can measure projected tile/data-window footprint from `SurfaceCameraFrame`.
- Extended `SurfaceLodPolicy` with a camera-aware `Select(...)` overload that chooses detail levels from projected footprint rather than viewport density alone.
- Added core tests proving projected-footprint estimation is stable and that farther cameras shift the selected LOD.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceScreenErrorEstimatorTests|FullyQualifiedName~SurfaceLodPolicyTests"`

## Notes
- The new core contracts stay inside `Videra.SurfaceCharts.Core`; no Avalonia or viewer-specific dependency leaked into LOD selection.
