---
phase: 49-scenedocument-contract-hardening
plan: 02
subsystem: scene-document-mutation
tags: [scene, mutation, ownership]
provides:
  - scene document mutator
  - runtime-owned vs external ownership
  - immutable mutation flow
key-files:
  modified:
    - src/Videra.Core/Scene/SceneOwnership.cs
    - src/Videra.Core/Scene/SceneDocumentMutator.cs
    - src/Videra.Core/Scene/SceneDocument.cs
requirements-completed: [SCENE-06]
completed: 2026-04-17
---

# Phase 49 Plan 02 Summary

## Accomplishments
- Added explicit `SceneOwnership` semantics so runtime-owned imported entries and external objects can be handled differently during removal and recovery.
- Moved document mutation into `SceneDocumentMutator`, keeping `SceneDocument` itself immutable.
- Gave runtime scene code a pure mutation layer instead of embedding document rewrite logic in a view-local partial.

## Verification
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release`

## Notes
Ownership semantics stayed internal, matching the architectural rule that new orchestration seams are not public API.
