---
phase: 24-true-3d-picking-and-probe-truth
plan: 01
subsystem: surface-charts-core-picking
tags: [surface-charts, core, picking, camera-frame]
provides:
  - shared `SurfacePickRay` and `SurfacePickHit` contracts
  - `SurfaceHeightfieldPicker` camera-ray -> heightfield intersection path
  - richer `SurfaceProbeInfo` world/tile/distance truth
key-files:
  modified:
    - src/Videra.SurfaceCharts.Core/Picking/SurfacePickRay.cs
    - src/Videra.SurfaceCharts.Core/Picking/SurfacePickHit.cs
    - src/Videra.SurfaceCharts.Core/Picking/SurfaceHeightfieldPicker.cs
    - src/Videra.SurfaceCharts.Core/Picking/SurfaceProbeInfo.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Picking/SurfaceHeightfieldPickerTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Picking/SurfaceProbeInfoTests.cs
requirements-completed: [PICK-01]
completed: 2026-04-16
---

# Phase 24 Plan 01 Summary

## Accomplishments
- Added first-class `SurfacePickRay` and `SurfacePickHit` contracts in `Videra.SurfaceCharts.Core`.
- Added `SurfaceHeightfieldPicker` with camera-frame ray creation, tile broad-phase bounds checks, triangle-level heightfield intersection, and a small vertex-snap fallback for shared-edge/vertex numeric gaps.
- Extended `SurfaceProbeInfo` so probe truth now carries `WorldPosition`, `TileKey`, and `DistanceToCamera` in addition to sample/axis/value truth.
- Locked the new contracts with core tests for multi-angle peak picking and coarse-tile approximate hits.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceHeightfieldPickerTests|FullyQualifiedName~SurfaceProbeInfoTests"`

## Notes
- Approximate coarse-tile assertions now intentionally allow small numerical tolerance because these hits come from interpolated heightfield triangles rather than integer sample snaps.
