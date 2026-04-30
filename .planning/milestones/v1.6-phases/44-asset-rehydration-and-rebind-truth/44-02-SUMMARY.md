---
phase: 44-asset-rehydration-and-rebind-truth
plan: 02
subsystem: runtime-scene-rebind
tags: [viewer, runtime, rehydration]
provides:
  - document-to-engine rebind sync
  - scene truth survival across ready transitions
  - retained asset rehydration path
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
requirements-completed: [ASSET-02]
completed: 2026-04-17
---

# Phase 44 Plan 02 Summary

## Accomplishments
- Changed runtime apply behavior so retained document entries are synchronized into the engine when the session/backend path becomes ready again.
- Preserved imported-asset references inside scene truth so the runtime can rebuild uploaded state after rebind without another import step.
- Kept runtime-owned scenes stable even when no `Items` collection is bound.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
This is the operational half of the new contract: retained CPU-side truth is enough to rebuild the active scene.
