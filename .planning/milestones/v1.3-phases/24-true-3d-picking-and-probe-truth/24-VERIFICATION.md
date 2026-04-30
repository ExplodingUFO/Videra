---
phase: 24-true-3d-picking-and-probe-truth
verified: 2026-04-16T16:31:32.8015867+08:00
status: passed
score: 3/3 must-haves verified
---

# Phase 24: True 3D Picking and Probe Truth Verification Report

**Phase Goal:** 把 hover / pin / focus / dolly 锚点从 viewport-linear sample mapping 升级为 `screen point -> camera ray -> heightfield hit` 的真实 3D picking，同时保持 chart-local interaction boundary 不回灌到 `VideraView`。  
**Verified:** 2026-04-16T16:31:32.8015867+08:00  
**Status:** passed

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
| --- | --- | --- | --- |
| 1 | Surface charts now have first-class 3D picking contracts under `Videra.SurfaceCharts.Core`. | ✓ VERIFIED | `SurfacePickRay`, `SurfacePickHit`, `SurfaceHeightfieldPicker`, and richer `SurfaceProbeInfo` now exist; core tests passed `119/119`. |
| 2 | The real Avalonia overlay path now resolves hovered probes from camera-frame picking rather than viewport-linear math. | ✓ VERIFIED | `SurfaceProbeService` and `SurfaceProbeOverlayPresenter` gained camera-frame paths, and `SurfaceChartView` now feeds runtime camera frames into overlay invalidation. |
| 3 | Pinned probes and zoom anchors remain stable because they now consume upgraded hovered-probe truth instead of a screen-linear approximation. | ✓ VERIFIED | `SurfaceChartPinnedProbeTests` and `SurfaceChartInteractionTests` stayed green after the migration, with the same chart-local public/API boundary. |

### Behavioral Spot-Checks

| Behavior | Command | Result | Status |
| --- | --- | --- | --- |
| Full surface-chart core suite | `dotnet test tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj -c Release` | Passed: 119/119 | ✓ PASS |
| Full Avalonia integration suite | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release` | Passed: 78/78 | ✓ PASS |
| Probe/pin/interaction regression slice | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartProbeOverlayTests|FullyQualifiedName~SurfaceChartPinnedProbeTests|FullyQualifiedName~SurfaceChartInteractionTests"` | Passed: 19/19 | ✓ PASS |

### Requirements Coverage

| Requirement | Description | Status | Evidence |
| --- | --- | --- | --- |
| `PICK-01` | Hovered probes and focus anchors resolve from screen-ray -> heightfield intersection rather than viewport-linear mapping. | ✓ SATISFIED | Core picking contracts shipped; overlay presenter and view overlay now consume camera-frame picking truth. |
| `PICK-02` | Pinned probes and zoom/focus anchors stay stable through orbit/refine/tile upgrades and can upgrade from approximate to exact truth. | ✓ SATISFIED | Pinned probes remain sample-space anchors, the dolly path keeps using hovered-probe samples, and the interaction/pinned-probe regression suites stayed green. |

### Gaps Summary

Phase 24 is complete. Remaining `v1.3` work starts at camera-aware LOD/request planning (Phase 25), then GPU resident slimming, shader/backend color mapping, and professional overlay layout.
