# Phase 379 Live Scatter Helpers Handoff

## Summary

- Added `DataLogger3D` as a thin live-scatter facade over `ScatterColumnarSeries`.
- The facade forwards append and replace operations to existing columnar semantics.
- Retained columns and streaming evidence counters remain available through the facade and through `ScatterChartData` via `DataLogger3D.Series`.

## Scope Notes

- No render loop, scheduler, background worker, fallback path, native backend work, old chart control work, or public `Source` API work was introduced.
- No Avalonia plot entrypoint was added; the core helper can be integrated by passing `DataLogger3D.Series` into existing scatter chart data paths.
- Bead `Videra-v255.4` remains open as requested.

## Verification

- `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --filter "Scatter"` passed: 30/30.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter BenchmarkContractRepositoryTests` passed: 2/2.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter PerformanceLabScenarioTests` passed: 5/5.
