---
phase: 25-camera-aware-lod-and-request-planning
plan: 02
subsystem: surface-charts-controller-runtime
tags: [surface-charts, avalonia, controller, scheduler]
provides:
  - controller/runtime request-plan migration to camera-aware inputs
  - camera-only view-state changes that can replan detail demand
  - retained-key pruning that stays correct even when the plan shape is unchanged
key-files:
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceTileRequestPlan.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartTileSchedulingTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceChartViewLifecycleTests.cs
requirements-completed: [LOD-01, LOD-02]
completed: 2026-04-16
---

# Phase 25 Plan 02 Summary

## Accomplishments
- Migrated `SurfaceChartController` request planning to build `SurfaceCameraFrame` inputs from the current `SurfaceViewState`, output size, and interaction quality.
- Removed the old camera-only no-op path: camera changes now flow through `RefreshRequestPipeline()` and can trigger new request plans when projected footprint changes.
- Added `SurfaceTileRequestPlan.IsEquivalentTo(...)` and tightened the controller so equivalent plans still prune to the current retained-key set before returning.
- Updated integration tests to validate camera-only replans and the new controller request counts under camera-aware truth.

## Verification
- `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~CameraAwareRequestPlan_FartherCameraChangesSelection|FullyQualifiedName~CameraOnlyViewStateChange_ReplansWhenProjectedFootprintChanges|FullyQualifiedName~ControllerSourceReplacement_DoesNotPublishLateFailuresFromSupersededGeneration"`

## Notes
- The runtime/controller migration still preserves the compatibility viewport bridge for callers that have not yet moved off `Viewport`.
