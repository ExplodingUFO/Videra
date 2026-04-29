---
phase: 385
title: "Interaction Profile and Command Surface"
status: completed
bead: Videra-v256.3
---

# Phase 385 Plan

## Goal

Add a small chart-local public interaction profile and bounded command surface
for the built-in `VideraChartView` interactions without introducing a generic
plugin, workbench command registry, compatibility layer, or fallback behavior.

## Scope

- Add public profile switches for orbit, pan, dolly/zoom, reset camera,
  fit-to-data, keyboard shortcuts, toolbar commands, probe pinning, focus
  selection, and pointer focus requests.
- Add bounded built-in chart commands for zoom, pan, reset camera, and fit to
  data.
- Route existing mouse, wheel, keyboard, and toolbar actions through the profile.
- Preserve the default behavior by making the default profile fully enabled.
- Add focused interaction tests for default behavior and disabled behavior.

## Non-Goals

- No old chart view controls.
- No public direct `Source` API.
- No generic plugin or workbench command system.
- No hidden fallback/downshift behavior.
- No overlay/probe, axis, live, demo, or Plot lifecycle changes.

## Verification

status: passed

- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj --filter "SurfaceChartInteractionTests|VideraChartViewKeyboardToolbarTests" --no-restore`
- `pwsh -NoProfile -File scripts/Test-SnapshotExportScope.ps1`
- `git diff --check`
