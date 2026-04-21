---
phase: 42-scenedocument-authoritative-scene-owner
plan: 02
subsystem: scene-mutation-flow
tags: [viewer, scene, items]
provides:
  - document-first scene mutation flow
  - ready-state-safe item binding updates
  - stable runtime-owned scene state
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
    - src/Videra.Avalonia/Controls/VideraView.cs
requirements-completed: [SCENE-04]
completed: 2026-04-17
---

# Phase 42 Plan 02 Summary

## Accomplishments
- Moved `AddObject`, `ReplaceScene`, `ClearScene`, and collection-change handling onto document-first semantics.
- Adjusted `VideraView.Items` property handling so scene ownership remains stable even before backend/session readiness.
- Fixed `ApplyViewState()` so runtime-owned scenes survive ready transitions instead of being cleared by unbound state sync.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
The control shell stayed thin; the ownership changes remained internal to the runtime.
