---
phase: 22-batch-tile-read-adoption-for-live-scheduler
plan: 02
subsystem: surface-charts-scheduler
tags: [surface-charts, scheduler, batch-reads]
requirements-completed: []
provides:
  - batch-aware detail request path in `SurfaceTileScheduler`
  - unchanged single-tile fallback for non-batch sources
completed: 2026-04-16
---

# Phase 22 Plan 02 Summary

## Accomplishments
- Extended `SurfaceTileScheduler` to detect `ISurfaceTileBatchSource` and dispatch ordered detail batches through `GetTilesAsync(...)`.
- Kept overview requests on the existing single-tile path and preserved the per-key fallback for sources that only implement `ISurfaceTileSource`.
- Reused the current request-generation and tile-cache semantics, including per-key release/store behavior inside the batch path.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartTileSchedulingTests"`

## Notes
- Batch size follows the current scheduler concurrency budget, so this change does not introduce a new tuning surface.
