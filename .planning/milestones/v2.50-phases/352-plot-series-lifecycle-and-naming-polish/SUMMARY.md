# Phase 352 Summary: Plot Series Lifecycle and Naming Polish

## Result

Completed.

## Scope

- Added public Plot-owned series inspection through `ActiveSeries`.
- Added explicit `Remove(Plot3DSeries)` and `IndexOf(Plot3DSeries)` lifecycle helpers.
- Kept public chart usage on `VideraChartView.Plot.Add.*`.

## Verification

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter FullyQualifiedName~VideraChartViewPlotApiTests`
- Integrated into `master` with focused combined Plot API tests passing.

## Beads

- Bead: `Videra-z44.2`
- Status: closed
- Branch: `v2.50-phase-352-plot-lifecycle`
