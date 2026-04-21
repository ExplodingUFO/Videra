---
phase: 20-built-in-interaction-and-camera-workflow-recovery
plan: 01
subsystem: surface-charts-interaction
tags: [surface-charts, avalonia, interaction, orbit, pan, dolly]
requires:
  - phase: 19-surfacechart-runtime-and-view-state-recovery
    provides: authoritative `SurfaceViewState` and `SurfaceChartRuntime`
provides:
  - built-in orbit / pan / dolly gestures inside `SurfaceChartView`
  - pin-preserving gesture state machine with pointer capture and wheel routing
  - headless regression coverage for built-in navigation without host glue
affects: [20-02, 20-03]
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartView.Input.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartRuntime.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartInteractionTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartPinnedProbeTests.cs
requirements-completed: [INT-01, INT-02]
completed: 2026-04-16
---

# Phase 20 Plan 01 Summary

## Accomplishments
- Replaced the placeholder chart input path with a real chart-local gesture state machine for left-drag orbit, right-drag pan, wheel dolly, pointer capture, and capture-loss cleanup.
- Kept `Shift + LeftClick` pinning intact by explicitly reserving that gesture path while non-shift left drag now drives orbit through the recovered `SurfaceChartRuntime`.
- Added headless interaction coverage for orbit, pan, dolly, and pin preservation so the built-in navigation path is now locked by tests instead of demo-only behavior.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartInteractionTests|FullyQualifiedName~SurfaceChartPinnedProbeTests"`

## Notes
- Visible dolly remains data-window-driven rather than camera-distance-only so the current projection path shows zoom changes truthfully.
- No chart-specific glue was added to `VideraView`; the full gesture path stays inside `Videra.SurfaceCharts.Avalonia`.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
