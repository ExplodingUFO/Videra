---
status: passed
---

# Phase 418 Verification

## Commands

- `bd dep cycles --json`
  - Result: `[]`.
- `bd ready --json`
  - Result before closing Phase 418: `[]`.
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
  - Result: all scope checks passed.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore`
  - Result after regenerating `docs/ROADMAP.generated.md`: passed 1, failed 0,
    skipped 0.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~SurfaceChartsHighPerformancePathTests" --no-restore`
  - Result: passed 20, failed 0, skipped 0.
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~VideraChartViewGpuDiagnosticsTests|FullyQualifiedName~VideraChartViewPlotApiTests" --no-restore`
  - Result: passed 35, failed 0, skipped 0.
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~SurfaceChartsReleaseTruthRepositoryTests" --no-restore`
  - Result: passed 23, failed 0, skipped 0.
- `git diff --check`
  - Result: passed.

## Cleanup

- Removed local worktrees `.worktrees/v262-phase417-guardrails` and
  `.worktrees/v262-phase417-ci-truth`.
- Deleted local branches `agents/v262-phase417-guardrails` and
  `agents/v262-phase417-ci-truth`.
