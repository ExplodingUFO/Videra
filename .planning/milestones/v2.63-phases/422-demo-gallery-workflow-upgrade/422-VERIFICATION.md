# Phase 422 Verification

## Commands

```powershell
dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj -c Release --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoConfigurationTests --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookCoverageMatrixTests --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsCookbookBarContourSnapshotRecipeTests --no-restore
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj -c Release --filter FullyQualifiedName~SurfaceChartsConsumerSmokeConfigurationTests --no-restore
pwsh -File ./scripts/Invoke-ConsumerSmoke.ps1 -Configuration Release -Scenario SurfaceCharts -BuildOnly
git diff --check
bd dep cycles --json
```

## Results

- Demo build passed.
- Demo configuration, viewport behavior, coverage matrix, snapshot/cookbook,
  and consumer-smoke configuration tests passed in focused runs.
- Consumer smoke build-only validation passed on the worker branch before merge.
- `git diff --check` passed.
- `bd dep cycles --json`: passed with no cycles.

## Residuals

- Existing analyzer warnings remain in SurfaceCharts/demo files and were not
  introduced as release evidence.
- The packaged consumer smoke validation was build-only in this phase; live GUI
  smoke remains outside this bead's verified evidence.
