# Phase 334 Plan

## Tasks

1. Add `VideraChartView` as the new single chart View entry point.
2. Add `Plot3D`, `Plot3DAddApi`, `Plot3DSeries`, and `Plot3DSeriesKind`.
3. Cover `Plot.Add.Surface(...)`, `Plot.Add.Waterfall(...)`, and `Plot.Add.Scatter(...)` with focused tests.
4. Verify the touched package builds and the new API tests pass.

## Verification

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter FullyQualifiedName~VideraChartViewPlotApiTests`
- `dotnet build src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj -c Debug --no-restore`
