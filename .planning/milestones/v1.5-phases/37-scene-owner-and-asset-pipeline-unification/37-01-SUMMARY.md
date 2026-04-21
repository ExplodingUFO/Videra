---
phase: 37-scene-owner-and-asset-pipeline-unification
plan: 01
subsystem: core-scene-assets
tags: [viewer, scene, import]
provides:
  - backend-neutral `ImportedSceneAsset`
  - runtime-owned `SceneDocument` truth
  - separate upload coordinator for GPU resource creation
key-files:
  modified:
    - src/Videra.Core/Scene/SceneDocument.cs
    - src/Videra.Core/Scene/ImportedSceneAsset.cs
    - src/Videra.Core/Scene/SceneUploadCoordinator.cs
    - src/Videra.Core/IO/ModelImporter.cs
requirements-completed: [SCENE-01]
completed: 2026-04-17
---

# Phase 37 Plan 01 Summary

## Accomplishments
- Added `SceneDocument`, `ImportedSceneAsset`, and `SceneUploadCoordinator` so import and upload are no longer the same step.
- Extended `ModelImporter` with a backend-neutral import path that can later upload through a chosen resource factory.
- Kept compatibility by preserving the legacy importer API on top of the new asset path.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
- The import/upload split is intentionally internal and does not widen the public scene API.
