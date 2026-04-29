---
phase: 384
title: "Plot Lifecycle and Code Experience Polish"
status: completed
bead: Videra-v256.2
---

# Phase 384 Plan

## Goal

Make Plot-owned plottable lifecycle code concise without widening the public model
or exposing mutable runtime internals.

## Scope

- Add the smallest useful reorder API to `Plot3D`.
- Add a typed read-only series query API to `Plot3D`.
- Cover deterministic revision, active-series, evidence, and null/invalid argument
  behavior in the focused Plot API tests.

## Non-Goals

- No compatibility wrapper types.
- No generic plotting abstraction.
- No backend/runtime ownership expansion.
- No interaction, overlay, sample demo, or broad documentation changes.

## Verification

status: passed

- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter VideraChartViewPlotApiTests --no-restore`
- `pwsh -NoProfile -File scripts/Test-SnapshotExportScope.ps1`
- `git diff --check`
