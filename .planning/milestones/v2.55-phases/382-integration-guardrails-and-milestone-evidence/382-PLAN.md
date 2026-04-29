# Phase 382 Plan: Integration, Guardrails, and Milestone Evidence

## Goal

Close v2.55 with focused regression evidence, snapshot scope guardrails, synchronized beads export, generated public roadmap output, and clean branch/worktree handoff.

## Scope

- Verify the new Plot API, axes, snapshot, same-type composition, demo/docs, and existing Core paths with focused tests.
- Run `scripts/Test-SnapshotExportScope.ps1`.
- Export beads state and generated roadmap artifacts.
- Close the Phase 382 bead and v2.55 epic after evidence is written.
- Clean temporary v2.55 worktrees and local feature branches when safe.

## Non-Goals

- No new product code.
- No compatibility wrappers, old chart controls, public `Source` API, PDF/vector export, backend expansion, hidden fallback/downshift, or god-code demo workbench.
- No unrelated warning cleanup.

## Validation

1. `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "VideraChartViewPlotApiTests|RegressionGuardrailTests|VideraChartViewStateTests|SurfaceAxisOverlayTests|SurfaceLegendOverlayTests|SurfaceChartProbeEvidenceTests|PlotSnapshot" --no-restore`
2. `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj --no-restore`
3. `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj --filter "SurfaceChartsDemoConfigurationTests|PerformanceLabScenarioTests" --no-restore`
4. `pwsh -File scripts/Test-SnapshotExportScope.ps1`
5. `bd export -o .beads/issues.jsonl`
6. `pwsh -File scripts/Export-BeadsRoadmap.ps1`

