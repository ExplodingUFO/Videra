---
status: passed
---

# Phase 416 Verification

## Commands

- `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore`
  - Result: passed with 0 errors. Existing analyzer/source-link warnings remain.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~SurfaceChartsHighPerformancePathTests" --no-restore`
  - Result after rerun without concurrent demo build: passed 20, failed 0,
    skipped 0.

## Note

The first main-workspace test attempt ran concurrently with the demo build and
failed on a locked demo PDB file. The test command was rerun by itself and
passed.
