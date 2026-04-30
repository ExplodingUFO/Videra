# Phase 416 Plan: Native Cookbook and Demo Code Simplification

## Ownership

Worker branch: `agents/v262-phase416-demo`

Write scope:

- `samples/Videra.SurfaceCharts.Demo/**`
- `tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs`
  or related sample tests only if required for the refactor

Do not edit `src/Videra.SurfaceCharts.Rendering/**`,
`VideraChartView.Rendering.cs`, `VideraChartView.Overlay.cs`, CI, or release
scripts.

## Tasks

1. Claim and execute child beads `Videra-dtc` and `Videra-b6e`.
2. Extract cookbook recipe record/catalog from `MainWindow.axaml.cs` into a
   bounded demo model/service.
3. Extract support summary and snapshot summary formatting helpers from
   `MainWindow.axaml.cs`.
4. Keep recipe content and support evidence output stable.
5. Run focused sample/demo tests and demo build if feasible.
6. Commit the worker branch and hand off changed files and validation results.

## Validation

- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsCookbook" --no-restore`
- `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore`
