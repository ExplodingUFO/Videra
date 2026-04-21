---
phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike
plan: 01
subsystem: surface-charts-interaction
tags: [surface-charts, scheduler, residency, concurrency, cancellation]
requires:
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: chart-local render-host seam and resident render-state baseline from phase 16
provides:
  - view-aware tile request plans with visible-first ordering
  - bounded concurrent request execution with generation-aware stale-work suppression
  - retention-based cache pruning for the current viewport neighborhood
affects: [17-02-persistent-cache-and-statistics, 17-03-benchmark-and-native-boundary-guard]
tech-stack:
  added: []
  patterns: [request planning, visible-first prioritization, bounded concurrency, generation-aware residency]
key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileRequestPlan.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileRequestPriority.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileScheduler.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileCache.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewLifecycleTests.cs
requirements-completed: [REND-03, DATA-01]
completed: 2026-04-14
---

# Phase 17 Plan 01 Summary

## Accomplishments
- Added `SurfaceTileRequestPlan` so the controller now prunes and requests from one explicit scheduling decision instead of re-deriving row-major work ad hoc.
- Reworked `SurfaceTileScheduler` to prioritize visible tiles before the outer neighborhood, keep the overview tile available first when needed, and cap detail request concurrency at `4`.
- Replaced prune-all detail behavior with `PruneToKeys(...)`, so viewport changes keep the active neighborhood resident instead of dropping every non-overview tile.
- Preserved generation-aware stale-work suppression so canceled or late pipelines cannot repopulate the active source generation.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartTileSchedulingTests|FullyQualifiedName~SurfaceChartViewLifecycleTests"`

## Notes
- The visible-vs-neighborhood ordering now uses `SurfaceTileRequestPriority` with Manhattan distance from the viewport-center tile, which keeps refinement tied to what the user is actually looking at.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
