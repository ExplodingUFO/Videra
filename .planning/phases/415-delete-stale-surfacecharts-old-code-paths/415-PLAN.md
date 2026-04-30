# Phase 415 Plan: Delete Stale SurfaceCharts Old-Code Paths

## Ownership

Worker branch: `agents/v262-phase415-code`

Write scope:

- `src/Videra.SurfaceCharts.Rendering/**`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs`
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/**`

Do not edit `samples/Videra.SurfaceCharts.Demo/**`; that belongs to Phase 416.

## Tasks

1. Claim and execute child beads `Videra-sva`, `Videra-1wq`, and `Videra-avu`.
2. Remove automatic chart-local software fallback/downshift behavior from
   `SurfaceChartRenderHost`.
3. Replace silent GPU backend resolution failure with explicit failure or
   not-ready diagnostic behavior.
4. Remove compatibility camera-frame backfill from `SurfaceChartRenderInputs`.
5. Rename default color-map helper terminology away from fallback language.
6. Rename stale compatibility test vocabulary without weakening assertions.
7. Run focused integration tests and scope guardrail script.
8. Commit the worker branch and hand off changed files and validation results.

## Validation

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "FullyQualifiedName~SurfaceChartRenderHostIntegrationTests|FullyQualifiedName~VideraChartViewGpuFallbackTests|FullyQualifiedName~VideraChartViewPlotApiTests" --no-restore`
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
