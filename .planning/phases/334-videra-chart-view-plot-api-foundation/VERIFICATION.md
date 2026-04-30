# Phase 334 Verification

## Commands

```powershell
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Debug --no-restore -m:1 --filter FullyQualifiedName~VideraChartViewPlotApiTests
```

Result: passed, 2 tests.

```powershell
dotnet build src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj -c Debug --no-restore
```

Result: passed, 0 warnings, 0 errors.
