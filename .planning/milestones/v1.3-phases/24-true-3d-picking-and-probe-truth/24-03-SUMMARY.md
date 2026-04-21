---
phase: 24-true-3d-picking-and-probe-truth
plan: 03
subsystem: surface-charts-interaction-anchor-stability
tags: [surface-charts, interaction, pinned-probe, zoom-anchor]
provides:
  - pinned-probe stability on top of 3D pick truth
  - wheel-dolly anchor stability through existing hovered-probe sample anchors
  - regression evidence that orbit / pin / wheel paths stay green after probe migration
requirements-completed: [PICK-02]
completed: 2026-04-16
---

# Phase 24 Plan 03 Summary

## Accomplishments
- Closed the pin/dolly anchor requirement without widening public APIs: `SurfaceChartInteractionController.ApplyDolly(...)` already used `hoveredProbe.SampleX/SampleY`, so migrating hovered-probe truth to camera-ray hits upgraded wheel anchors automatically.
- Kept pinned probes as sample-space requests, which means refine/tile-upgrade cycles continue to re-resolve them against the best loaded tile truth.
- Re-verified the chart-local interaction, probe, and pinned-probe suites against the new 3D picking path.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartPinnedProbeTests|FullyQualifiedName~SurfaceChartInteractionTests"`

## Notes
- No extra interaction-controller surgery was needed in this phase because the existing chart-local gesture code was already anchored on `hoveredProbe` truth.
