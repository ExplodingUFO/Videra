---
phase: 43-import-and-upload-pipeline-separation
plan: 01
subsystem: single-import-deferred-assets
tags: [viewer, import, scene-assets]
provides:
  - backend-neutral imported assets
  - deferred `Object3D` creation
  - runtime-stored imported-asset references
key-files:
  modified:
    - src/Videra.Core/Graphics/Object3D.cs
    - src/Videra.Core/Scene/SceneUploadCoordinator.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
requirements-completed: [IMPORT-01]
completed: 2026-04-17
---

# Phase 43 Plan 01 Summary

## Accomplishments
- Added deferred mesh preparation on `Object3D` so imported geometry can exist without immediate GPU buffers.
- Split `SceneUploadCoordinator` so it can create deferred objects first and upload later through an explicit resource-factory path.
- Changed `LoadModelAsync()` to retain backend-neutral imported assets instead of immediately forcing upload.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
Import now means “produce CPU-side truth,” not “immediately allocate backend resources.”
