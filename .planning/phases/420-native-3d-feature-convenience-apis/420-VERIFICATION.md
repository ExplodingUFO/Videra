# Phase 420 Verification

## Commands

```powershell
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter Bar --no-restore
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter Contour --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Bar --no-restore
dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter Contour --no-restore
git diff --check
bd dep cycles --json
```

## Notes

The final verification commands must be run sequentially per test project. A
parallel run against the same project can lock generated build outputs and is
not valid pass/fail evidence.
