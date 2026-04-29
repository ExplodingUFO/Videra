# Phase 373: Series Probe Strategies — Verification

## Status: PASSED

## Verification Checklist

### Interface Contract
- [x] `ISeriesProbeStrategy` interface defined in `Videra.SurfaceCharts.Core/Picking/`
- [x] Single method: `SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)`
- [x] XML documentation complete

### ScatterProbeStrategy
- [x] Brute-force nearest-point search in XZ plane
- [x] Handles point-object series (`ScatterSeries`)
- [x] Handles columnar series (`ScatterColumnarSeries`)
- [x] Skips NaN values in columnar data
- [x] Returns `SurfaceProbeInfo` with correct coordinates and value

### BarProbeStrategy
- [x] AABB containment hit-test on bar footprints
- [x] Returns bar value (Position.Y + Size.Y)
- [x] Handles overlapping bars (prefers closest to cursor)
- [x] Returns null when cursor outside all bars

### ContourProbeStrategy
- [x] Point-to-segment distance calculation
- [x] Configurable snap radius (default 0.05)
- [x] Projects cursor onto nearest segment
- [x] Returns iso-value from nearest contour line
- [x] Returns null when outside snap radius

### SeriesProbeStrategyDispatcher
- [x] Maps `Plot3DSeriesKind` to `ISeriesProbeStrategy`
- [x] `TryResolve` dispatches to correct strategy
- [x] `HasStrategy` checks for registered strategy
- [x] Returns null for unregistered series kinds

### Integration
- [x] `SurfaceProbeService.ResolveFromScreenPosition` overload for dispatcher
- [x] `SurfaceProbeOverlayPresenter.CreateState` overload for non-surface series
- [x] Screen-to-chart-space coordinate mapping

### Tests
- [x] 12 unit tests covering all strategies
- [x] All tests passing
- [x] No existing tests broken

## Test Results

```
测试总数: 12
通过: 12
失败: 0
```

## Commits

| Hash | Message |
|------|---------|
| 4827806 | feat(373-PLAN): add ISeriesProbeStrategy interface |
| 4d4cc5d | feat(373-PLAN): implement ScatterProbeStrategy |
| bd26490 | feat(373-PLAN): implement BarProbeStrategy |
| 5a8216a | feat(373-PLAN): implement ContourProbeStrategy |
| aa5a5dd | feat(373-PLAN): add SeriesProbeStrategyDispatcher |
| 787bc75 | feat(373-PLAN): add strategy-based probe to SurfaceProbeService |
| 2b21dc9 | feat(373-PLAN): wire strategy dispatcher into SurfaceProbeOverlayPresenter |
| bcda66d | test(373-PLAN): add comprehensive unit tests |

## Files Created/Modified

### New Files
- `src/Videra.SurfaceCharts.Core/Picking/ISeriesProbeStrategy.cs`
- `src/Videra.SurfaceCharts.Core/Picking/ScatterProbeStrategy.cs`
- `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs`
- `src/Videra.SurfaceCharts.Core/Picking/ContourProbeStrategy.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SeriesProbeStrategyDispatcher.cs`
- `tests/Videra.SurfaceCharts.Core.Tests/Picking/SeriesProbeStrategyTests.cs`

### Modified Files
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs`
