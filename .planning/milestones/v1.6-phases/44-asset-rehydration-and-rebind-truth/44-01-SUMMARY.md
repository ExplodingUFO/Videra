---
phase: 44-asset-rehydration-and-rebind-truth
plan: 01
subsystem: scene-rehydration-core
tags: [viewer, scene, rehydration]
provides:
  - active-resource upload of deferred objects
  - resource recreation for existing scene objects
  - no default software steady-state fallback
key-files:
  modified:
    - src/Videra.Core/Graphics/RenderWorld.cs
    - src/Videra.Core/Graphics/VideraEngine.cs
    - src/Videra.Core/Scene/SceneUploadCoordinator.cs
requirements-completed: [ASSET-01]
completed: 2026-04-17
---

# Phase 44 Plan 01 Summary

## Accomplishments
- Extended `SceneUploadCoordinator` with an upload path for pre-existing deferred `Object3D` instances.
- Updated `RenderWorld` / `VideraEngine` so re-adding retained scene objects recreates the graphics resources they need.
- Removed the assumption that software resources are the default upload truth when a real backend path is unavailable.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests"`

## Notes
The phase closed only when retained scene objects could be rehydrated through the active resource path instead of a silent software fallback.
