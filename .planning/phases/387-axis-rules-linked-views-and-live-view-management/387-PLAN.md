---
phase: 387
title: "Axis Rules, Linked Views, and Live View Management"
status: completed
bead: Videra-v256.5
---

# Phase 387 Plan

## Goal

Make axis limit behavior, two-chart view linking, and live scatter view evidence explicit without changing dataset metadata ownership or adding global coordination.

## Success Criteria

1. `Plot.Axes` exposes chart-local lock and bounds rules that constrain explicit limits and autoscale calls.
2. Two `VideraChartView` instances can synchronize `ViewState` through an explicit disposable lifetime.
3. `DataLogger3D` exposes full-data/latest-window live view behavior with deterministic append, drop, visible-window, and autoscale-decision evidence.
4. Focused axis/Plot API and DataLogger3D tests pass, and snapshot export scope guardrails remain clean.

## Implementation Notes

- Keep rules in `PlotAxes3D`; do not write axis truth back into dataset metadata.
- Add a small `LinkViewWith` helper with no registry and no hidden fallback behavior.
- Keep live view evidence core-local in `DataLogger3D`.
- Avoid overlay, probe, selection, demo, and broad documentation files.

## Verification

- `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "Axis|PlotApi" --no-restore`
- `dotnet test tests\Videra.SurfaceCharts.Core.Tests\Videra.SurfaceCharts.Core.Tests.csproj --filter DataLogger3D --no-restore`
- `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
- `git diff --check`
