# Phase 418 Plan: v2.62 Final Verification

## Success Criteria

1. Focused SurfaceCharts cleanup, cookbook/demo, guardrail, generated-roadmap,
   and dependency checks pass.
2. Phase 417 worktrees and local branches are cleaned after integration.
3. `Videra-73a` and the v2.62 epic close only after validation evidence is
   recorded.
4. Git push and Beads Dolt push complete; if direct Dolt push fails with the
   known Windows path issue, use the host fallback script.

## Validation Commands

- `bd dep cycles --json`
- `bd ready --json`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter FullyQualifiedName~BeadsPublicRoadmapTests --no-restore`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests|FullyQualifiedName~ScatterStreamingScenarioEvidenceTests|FullyQualifiedName~SurfaceChartsHighPerformancePathTests" --no-restore`
- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~VideraChartViewGpuDiagnosticsTests|FullyQualifiedName~VideraChartViewPlotApiTests" --no-restore`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SurfaceChartsCiTruthTests|FullyQualifiedName~ReleaseDryRunRepositoryTests|FullyQualifiedName~SurfaceChartsReleaseTruthRepositoryTests" --no-restore`
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
- `git diff --check`
