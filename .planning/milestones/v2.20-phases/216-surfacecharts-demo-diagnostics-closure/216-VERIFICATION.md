# Phase 216 Verification

## Commands

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~SurfaceChartsRepositoryArchitectureTests"` — passed, 29 tests.
- `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj -c Release --no-restore` — passed with 0 warnings and 0 errors.
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --no-restore --filter "FullyQualifiedName~SurfaceChartViewGpuFallbackTests"` — passed, 2 tests.
- `dotnet build Videra.slnx -c Release` — passed with 2 pre-existing warnings and 0 errors.
- `git diff --check` — passed; Git reported CRLF normalization warnings only.

## Notes

- `dotnet build Videra.slnx -c Release --no-restore` first failed because several projects in the fresh worktree had no `project.assets.json`; rerunning the solution build with restore passed.
- The demo reports the active runtime path. It does not force or exhaustively enumerate every GPU host/fallback combination.
