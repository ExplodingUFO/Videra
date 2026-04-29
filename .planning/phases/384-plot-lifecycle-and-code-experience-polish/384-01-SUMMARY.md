---
phase: 384
title: "Plot Lifecycle and Code Experience Polish"
status: completed
bead: Videra-v256.2
---

# Phase 384 Summary

Phase 384 completed the Plot-owned lifecycle/code-experience polish for v2.56.

## Delivered

- Added `Plot3D.Move(Plot3DSeries series, int index)` for explicit draw-order
  reordering of attached plottables.
- Added `Plot3D.GetSeries<TSeries>()` to return a typed read-only snapshot in
  draw order without exposing the backing series list.
- Preserved existing handle behavior through `Plot3DSeries` and
  `IPlottable3D`; no wrapper compatibility types were introduced.
- Added focused integration coverage for typed queries, move semantics,
  revision stability, active-series changes, and dataset/output evidence
  identity order.

## Verification

status: passed

- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter VideraChartViewPlotApiTests --no-restore`
  - Passed: 26, Failed: 0, Skipped: 0.
- `pwsh -NoProfile -File scripts/Test-SnapshotExportScope.ps1`
  - Passed: all snapshot scope guardrails.
- `git diff --check`
  - Passed.

## Boundaries

No interaction, overlay, demo, public direct `Source`, PDF/vector export,
backend expansion, compatibility wrapper, or god-code workbench changes were
made.
