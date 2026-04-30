---
phase: 37-scene-owner-and-asset-pipeline-unification
plan: 02
subsystem: runtime-scene-owner
tags: [viewer, runtime, scene]
provides:
  - runtime-owned authoritative scene document
  - scene truth synchronized across public scene APIs and item binding
  - rebind-friendly CPU-side scene record
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
requirements-completed: [SCENE-01, SCENE-02]
completed: 2026-04-17
---

# Phase 37 Plan 02 Summary

## Accomplishments
- Updated runtime scene APIs so all object additions, replacements, clears, and item-driven changes keep `_sceneDocument` authoritative.
- Changed model loading to import CPU-side assets first and upload only when runtime resources are available.
- Positioned the runtime for backend rebind and resource recreation without forcing re-import.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
- This phase removes the old `Items` versus engine dual-truth problem from the runtime path.
