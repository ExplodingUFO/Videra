---
phase: 386
title: "Selection, Probe, and Draggable Overlay Recipes"
status: completed
bead: Videra-v256.4
---

# Phase 386 Plan

## Goal

Add concise chart-local helpers for pointer probe resolution, host-owned
click/rectangle selection reports, and bounded draggable marker/range overlay
recipes without introducing source-data mutation or a generic workbench system.

## Scope

- Public helper methods on `VideraChartView` for probe, selection report,
  marker overlay, and range overlay creation.
- Immutable recipe/evidence models under the overlay/control surface.
- Focused interaction/probe tests only.

## Non-Goals

- No compatibility layer or old chart controls.
- No direct public `Source` API.
- No sample demo, Plot API, axis/linking/live helper, or backend expansion work.
- No chart-owned selected state.

## Verification

1. Focused tests:
   `dotnet test tests\Videra.SurfaceCharts.Avalonia.IntegrationTests\Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "Probe|PinnedProbe|Interaction" --no-restore`
2. Snapshot scope guard:
   `pwsh -NoProfile -File scripts\Test-SnapshotExportScope.ps1`
3. Whitespace check:
   `git diff --check`
