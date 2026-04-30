---
phase: 373
plan: 373-PLAN
subsystem: surface-charts
tags: [probe, scatter, bar, contour, strategy-pattern, picking]
depends_on: []
provides: [ISeriesProbeStrategy, ScatterProbeStrategy, BarProbeStrategy, ContourProbeStrategy]
affects: [SurfaceProbeService, SurfaceProbeOverlayPresenter]
tech_stack:
  added: []
  patterns: [strategy-pattern, brute-force-search, aabb-hit-test, point-to-segment-distance]
key_files:
  created:
    - src/Videra.SurfaceCharts.Core/Picking/ISeriesProbeStrategy.cs
    - src/Videra.SurfaceCharts.Core/Picking/ScatterProbeStrategy.cs
    - src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs
    - src/Videra.SurfaceCharts.Core/Picking/ContourProbeStrategy.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SeriesProbeStrategyDispatcher.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Picking/SeriesProbeStrategyTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs
decisions:
  - Scatter probe uses brute-force nearest-point (defer KD-tree to profiling)
  - Bar probe uses AABB containment hit-test on render bars
  - Contour probe uses point-to-segment distance with configurable snap radius
  - ISeriesProbeStrategy interface in Core/Picking for extensibility
  - Dispatcher in Avalonia layer (needs Plot3DSeriesKind enum reference)
metrics:
  duration_seconds: 687
  completed_tasks: 8
  total_tasks: 8
  files_created: 6
  files_modified: 2
  tests_added: 12
completed: 2026-04-29T21:50:42Z
---

# Phase 373 Plan: Series Probe Strategies — Summary

## One-Liner

Extensible `ISeriesProbeStrategy` interface with brute-force scatter nearest-point, bar AABB hit-test, and contour segment distance probe resolution for all chart series types.

## What Was Built

### ISeriesProbeStrategy Interface
Single-method interface in `Videra.SurfaceCharts.Core/Picking` enabling per-series-kind probe strategies:
```csharp
public interface ISeriesProbeStrategy
{
    SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata);
}
```

### ScatterProbeStrategy
- Brute-force nearest-point search across point-object and columnar series
- Euclidean distance in XZ plane for snap-to-nearest probe
- Handles NaN gaps in columnar series gracefully

### BarProbeStrategy
- AABB containment hit-test against bar bounding boxes
- Returns bar value (Position.Y + Size.Y) and center coordinates
- Handles overlapping bars by preferring closest to cursor

### ContourProbeStrategy
- Point-to-segment distance calculation for all contour segments
- Configurable snap radius (default 0.05) for probe hit detection
- Projects cursor onto nearest segment for accurate sample position
- Returns iso-value from the nearest contour line

### SeriesProbeStrategyDispatcher
- Maps `Plot3DSeriesKind` to `ISeriesProbeStrategy`
- Routes probe resolution to correct strategy by series kind
- Located in Avalonia layer where `Plot3DSeriesKind` lives

### Integration Points
- `SurfaceProbeService` — new overload accepting dispatcher for non-surface series
- `SurfaceProbeOverlayPresenter` — new `CreateState` overload for Scatter/Bar/Contour series

## Tasks Completed

| Task | Name | Commit | Key Files |
|------|------|--------|-----------|
| 1 | ISeriesProbeStrategy Interface | 4827806 | ISeriesProbeStrategy.cs |
| 2 | ScatterProbeStrategy | 4d4cc5d | ScatterProbeStrategy.cs |
| 3 | BarProbeStrategy | bd26490 | BarProbeStrategy.cs |
| 4 | ContourProbeStrategy | 5a8216a | ContourProbeStrategy.cs |
| 5 | SeriesProbeStrategyDispatcher | aa5a5dd | SeriesProbeStrategyDispatcher.cs |
| 6 | SurfaceProbeService Integration | 787bc75 | SurfaceProbeService.cs |
| 7 | SurfaceProbeOverlayPresenter Wiring | 2b21dc9 | SurfaceProbeOverlayPresenter.cs |
| 8 | Unit Tests | bcda66d | SeriesProbeStrategyTests.cs |

## Verification Results

- [x] `ISeriesProbeStrategy` interface defined with single `TryResolve` method
- [x] `ScatterProbeStrategy` finds nearest point via brute-force XZ distance
- [x] `BarProbeStrategy` hit-tests bar AABB footprints
- [x] `ContourProbeStrategy` finds nearest contour segment within snap radius
- [x] `SeriesProbeStrategyDispatcher` maps series kinds to strategies
- [x] `SurfaceProbeService` has strategy-based resolution path
- [x] `SurfaceProbeOverlayPresenter` dispatches to strategies for non-surface series
- [x] All 12 unit tests pass
- [x] No existing tests broken

## Decisions Made

1. **Brute-force over KD-tree** — Scatter datasets expected <10k points in v2.54. Defer spatial indexing to profiling phase.
2. **AABB hit-test for bars** — Simple containment check; handles overlapping bars by distance-to-center preference.
3. **Snap radius for contours** — Configurable threshold (default 0.05) prevents false positives from distant segments.
4. **Dispatcher in Avalonia layer** — `Plot3DSeriesKind` enum lives in Avalonia; Core project cannot reference it.

## Deviations from Plan

None — plan executed exactly as written.

## Known Stubs

None — all probe strategies are fully wired and tested.

## Requirements Traceability

| Requirement | Status | Evidence |
|-------------|--------|----------|
| PROBE-01 | ✅ | ScatterProbeStrategy with nearest-point snap |
| PROBE-02 | ✅ | BarProbeStrategy with AABB hit-test |
| PROBE-03 | ✅ | ContourProbeStrategy with segment distance |
| PROBE-04 | ✅ | ISeriesProbeStrategy interface + dispatcher |

## Self-Check: PASSED

All created files exist. All commits verified. All tests pass.
