# Phase 348: Authoring Diagnostics and Failure Clarity

## Bead

`Videra-7di`

## Outcome

Invalid authored instance input now fails through explicit `SceneAuthoringDiagnostic` data instead of throwing before `TryBuild()` can report authoring failures. No fallback batches, substitute geometry, silent repair, or downshift path was added.

## Changes

- `SceneAuthoringBuilder` records input diagnostics for invalid `AddInstances(...)` calls.
- Covered invalid instance cases:
  - blank instance batch name
  - transparent `Blend` material
  - empty instance mesh
  - empty transforms
  - mismatched per-instance colors
  - mismatched per-instance object ids
  - non-finite transforms
  - non-finite per-instance colors
  - `Guid.Empty` object ids
- Added `SceneAuthoringDiagnosticsTests`.

## Verification

```bash
dotnet test tests\Videra.Core.Tests\Videra.Core.Tests.csproj --filter "FullyQualifiedName~SceneAuthoringDiagnosticsTests|FullyQualifiedName~SceneAuthoringBuilder|FullyQualifiedName~InstanceBatchDescriptorTests"
```

Result: passed, 26/26.

## Handoff

Phase 349 can surface these diagnostics in the minimal sample README or sample output. Phase 350 can rely on invalid instance inputs not creating fallback `InstanceBatchEntry` records.
