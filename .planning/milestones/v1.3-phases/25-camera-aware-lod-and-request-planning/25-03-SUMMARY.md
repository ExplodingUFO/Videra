---
phase: 25-camera-aware-lod-and-request-planning
plan: 03
subsystem: surface-charts-scheduler-priority
tags: [surface-charts, scheduler, priority, benchmark]
provides:
  - camera-aware request priority derived from projected footprint, center distance, and depth
  - quantized priority buckets to reduce useless reorder churn on small camera motion
  - benchmark coverage for camera-aware LOD selection
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileRequestPriority.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs
    - benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsBenchmarks.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs
requirements-completed: [LOD-02]
completed: 2026-04-16
---

# Phase 25 Plan 03 Summary

## Accomplishments
- Reworked `SurfaceTileScheduler` so camera-aware plans rank detail tiles from projected footprint truth instead of only viewport-center distance.
- Quantized center-distance, depth, and projected-area signals into coarse priority buckets so small camera motion does not constantly reshuffle the whole request queue.
- Preserved overview-first and batch-read behavior while updating integration tests to assert the new camera-aware ordering truth.
- Added a `BenchmarkDotNet` entry for camera-aware LOD selection so later resident-path work can measure against the new baseline.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartTileSchedulingTests"`
- `dotnet build benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj -c Release`

## Notes
- The scheduler computes tile bounds locally from key/level metadata, so priority ranking does not need to materialize tiles before it can score them.
