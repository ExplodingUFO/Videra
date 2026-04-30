# Phase 242 Summary: Viewer Instance Batch Contract

## Outcome

Completed the first viewer instance-batch public contract without runtime rendering or picking changes.

## Changed

- Added `InstanceBatchDescriptor` for same-mesh/same-material batches.
- Added `InstanceBatchEntry` as retained `SceneDocument` batch truth.
- Added `SceneDocument.InstanceBatches` and `SceneDocument.AddInstanceBatch(...)`.
- `SceneDocumentMutator.Clear(...)` now clears retained instance batches as well as object entries.
- Added validation for transform count, color/id count, empty ids, mesh/material mismatch, non-finite values, and transparent `Blend` material exclusion.
- Updated English and Chinese docs to state the supported contract and explicit exclusions.

## Verification

- `dotnet build src/Videra.Core/Videra.Core.csproj -c Release`: passed.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~InstanceBatchDescriptorTests"`: passed.
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~SceneDocumentTruthTests|FullyQualifiedName~SceneDocumentMutatorTests"`: passed.
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~RepositoryArchitectureTests|FullyQualifiedName~RepositoryLocalizationTests"`: passed.

## Deferred

- Runtime upload/rendering of batches, draw-call accounting, and picking are Phase 243.
- Benchmarks and Performance Lab remain later phases.
