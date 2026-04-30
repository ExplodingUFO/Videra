---
phase: 53-backend-rehydration-and-residency-diagnostics
plan: 01
subsystem: backend-rehydration
tags: [scene, backend, rehydration]
provides:
  - scene resource epoch
  - mark-all-dirty on rebind
  - dirty requeue flow
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Overlay.cs
    - src/Videra.Avalonia/Runtime/Scene/SceneResidencyRegistry.cs
requirements-completed: [RECOVERY-01]
completed: 2026-04-17
---

# Phase 53 Plan 01 Summary

## Accomplishments
- Added a resource epoch and backend-ready hook so retained imported/runtime-owned entries can be marked dirty and requeued after backend or surface recovery.
- Fixed the residency rule so previously resident entries still become dirty on a new device even if their old buffers are non-null.
- Kept recovery document-first: runtime requeues retained truth instead of reimporting files or rebuilding the scene document.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
The dirty-on-rebind fix closed the most important correctness gap in the new residency model.
