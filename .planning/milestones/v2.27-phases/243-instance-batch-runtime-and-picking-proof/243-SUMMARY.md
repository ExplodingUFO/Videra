# Phase 243: Instance Batch Runtime and Picking Proof - Summary

**Completed:** 2026-04-27
**Status:** Complete

## Delivered

- Added `VideraView.AddInstanceBatch(InstanceBatchDescriptor)` and runtime/coordinator forwarding.
- Exposed retained instance batches through the Avalonia runtime for framing and interaction.
- `FrameAll()` now includes retained instance-batch bounds.
- `SceneHitTestService` now tests pickable instance batches and returns stable `ObjectId` plus `InstanceIndex`.
- Batch hits can have no backing `Object3D`; measurement snapping falls back cleanly to the hit world point for those cases.
- Diagnostics now include:
  - `InstanceBatchCount`
  - `RetainedInstanceCount`
  - `PickableObjectCount` including pickable batch instances
- Diagnostics still do not invent backend draw-call, vertex, or submitted-instance metrics.

## Verification

- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SceneHitTestServiceTests|FullyQualifiedName~PickingServiceTests|FullyQualifiedName~InstanceBatchDescriptorTests"`
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~VideraDiagnosticsSnapshotFormatterTests|FullyQualifiedName~VideraInteractionControllerTests"`
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~AddInstanceBatch_RetainsBatchForDiagnosticsWithoutCreatingRuntimeObjects|FullyQualifiedName~FrameAll_IncludesRetainedInstanceBatchBounds"`
- `dotnet build src/Videra.Avalonia/Videra.Avalonia.csproj -c Release`

## Deferred

- Real GPU instanced rendering and draw-call reduction.
- Benchmark evidence and thresholds.
- Performance Lab UI.
- Box selection and overlay outlines for per-instance selections.
