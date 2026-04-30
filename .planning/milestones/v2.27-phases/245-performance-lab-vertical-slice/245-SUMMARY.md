# Phase 245: Performance Lab Vertical Slice - Summary

**Completed:** 2026-04-27
**Status:** Complete

## Delivered

- Added a `PERFORMANCE LAB` section to `Videra.Demo`.
- Lab controls include object count, normal-object versus instance-batch mode, and pickable toggle.
- Normal mode generates initialized `Object3D` markers.
- Instance-batch mode records a retained `InstanceBatchDescriptor` through `VideraView.AddInstanceBatch(...)`.
- Lab report includes:
  - build/frame-time proxy
  - pick latency
  - pick object id and instance index where available
  - draw-call availability
  - upload bytes
  - resident bytes
  - retained instance count
  - pickable count
- Added copyable Performance Lab snapshot text.
- Adjusted `ClearScene()` so bound `Items` scenes also clear retained instance batches.

## Verification

- `dotnet build samples/Videra.Demo/Videra.Demo.csproj -c Release`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~DemoConfigurationTests"`
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~SceneDocumentMutatorTests|FullyQualifiedName~SceneDocumentTruthTests"`

## Deferred

- Real FPS instrumentation.
- GPU instanced rendering.
- Screenshot/pixel verification for the demo.
- Performance Lab benchmark baseline capture.
