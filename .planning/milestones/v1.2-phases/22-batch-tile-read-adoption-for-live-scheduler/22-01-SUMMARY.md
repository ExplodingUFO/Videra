---
phase: 22-batch-tile-read-adoption-for-live-scheduler
plan: 01
subsystem: surface-charts-scheduler-tests
tags: [surface-charts, scheduler, batch-reads, tests]
requirements-completed: []
provides:
  - failing-then-passing regression tests for batch-capable scheduler sources
completed: 2026-04-16
---

# Phase 22 Plan 01 Summary

## Accomplishments
- Added batch-capable scheduling regressions to `SurfaceChartTileSchedulingTests`.
- Covered both flattened visible-first ordering and batch-size partitioning against the scheduler concurrency budget.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartTileSchedulingTests"`

## Notes
- The new tests fail on the old code path because all detail tiles still went through per-key `GetTileAsync(...)`.
