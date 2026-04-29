---
phase: 389
title: "Integration, Guardrails, and Milestone Evidence"
status: completed
bead: Videra-v256.7
---

# Phase 389 Plan

## Goal

Close v2.56 with integrated verification, repository guardrails, Beads export,
generated roadmap, milestone evidence, and clean branch/worktree state.

## Tasks

1. Confirm all prior v2.56 phase beads are closed and only Phase 389 remains in
   progress.
2. Re-run focused tests across Plot lifecycle, interaction profile,
   selection/probe overlays, axis/live helpers, and cookbook demo wiring.
3. Run snapshot/export scope guardrails.
4. Update requirements, roadmap, state, milestone summary, Beads export, and
   generated public roadmap.
5. Confirm temporary phase worktrees and branches are cleaned.

## Validation

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "VideraChartViewPlotApiTests|SurfaceChartInteractionTests|VideraChartViewKeyboardToolbarTests|Probe|PinnedProbe|Interaction|Axis|PlotApi" --no-restore`
- `dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter DataLogger3D --no-restore`
- `dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter SurfaceChartsDemoConfigurationTests --no-restore`
- `dotnet build samples\Videra.SurfaceCharts.Demo\Videra.SurfaceCharts.Demo.csproj --no-restore`
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
- `bd ready --json`
- `git status --short --branch`
