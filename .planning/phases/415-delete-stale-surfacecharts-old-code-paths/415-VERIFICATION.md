---
status: passed
---

# Phase 415 Verification

## Commands

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~VideraChartViewGpuDiagnosticsTests|FullyQualifiedName~VideraChartViewPlotApiTests" --no-restore`
  - Result: passed 35, failed 0, skipped 0.
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
  - Result: all scope checks passed.

## Worker Validation

The isolated Phase 415 worktree also passed:

- `dotnet test ... --filter "FullyQualifiedName~SurfaceChartProbeEvidenceSharedInputTests|FullyQualifiedName~SurfaceChartIncrementalRenderingTests"`
  - Result: passed 11, failed 0.
- `git diff --check`
  - Result: clean in the worker branch.
