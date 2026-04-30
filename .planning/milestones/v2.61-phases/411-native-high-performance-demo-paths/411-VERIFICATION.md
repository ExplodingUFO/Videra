---
phase: 411
bead: Videra-s6h
title: "Native High-Performance Demo Paths Verification"
status: complete
created_at: 2026-04-30
---

# Phase 411 Verification

## Child Verification

- `411A`: worker reported `git diff --check`,
  `SurfaceChartsHighPerformancePathTests`, and
  `SurfacePyramidBuilderTests|InMemorySurfaceTileSourceTests` passing, commit
  `e7dfc2d`.
- `411B`: worker reported `git diff --check`,
  `ScatterStreamingScenarioEvidenceTests`, and
  `ScatterDataLogger3DTests|ScatterRendererTests` passing, commit `cc23943`.
- `411C`: local worktree first failed on an over-strict negative assertion for
  documented deferred `GPU-driven culling`; the assertion was narrowed to avoid
  treating explicit non-goals as violations. The final
  `SurfaceChartsPerformanceTruthTests` run passed, commit `baeb6a7`.

Fresh worktrees needed one restore before requested `--no-restore` focused test
commands could run; the requested commands passed after restore.

## Mainline Verification

Executed after integrating child commits:

```powershell
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCookbookFirstSurfaceRecipeTests|FullyQualifiedName~SurfaceChartsCookbookWaterfallLinkedRecipeTests|FullyQualifiedName~SurfaceChartsCookbookScatterLiveRecipeTests|FullyQualifiedName~SurfaceChartsCookbookBarContourSnapshotRecipeTests|FullyQualifiedName~SurfaceChartsHighPerformancePathTests|FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~SurfaceChartsPerformanceTruthTests" --no-restore
dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter "FullyQualifiedName~ScatterDataLogger3DTests|FullyQualifiedName~ScatterRendererTests|FullyQualifiedName~PlotSnapshotContractTests|FullyQualifiedName~PlotSnapshotCaptureTests|FullyQualifiedName~SurfacePyramidBuilderTests|FullyQualifiedName~InMemorySurfaceTileSourceTests" --no-restore
git diff --check
```

Results:

- Core focused integration test run: passed, 19/19 tests.
- SurfaceCharts.Core focused integration test run: passed, 75/75 tests.
- `git diff --check`: passed.
