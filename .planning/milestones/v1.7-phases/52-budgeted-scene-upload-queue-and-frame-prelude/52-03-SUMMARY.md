---
phase: 52-budgeted-scene-upload-queue-and-frame-prelude
plan: 03
subsystem: frame-prelude
tags: [runtime, render-session, upload]
provides:
  - runtime frame prelude
  - before-render upload drain
  - no synchronous upload in publish path
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/RuntimeFramePrelude.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
    - src/Videra.Avalonia/Rendering/RenderSession.cs
requirements-completed: [RES-01, RES-02]
completed: 2026-04-17
---

# Phase 52 Plan 03 Summary

## Accomplishments
- Added `RuntimeFramePrelude` and wired it into `RenderSession` so scene uploads drain from the existing before-render hook.
- Removed direct upload from scene publication; runtime scene code now enqueues work and invalidates instead.
- Kept `RenderSession` generic by putting scene-specific queue logic in runtime-local coordination rather than in the render-session class itself.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionIntegrationTests|FullyQualifiedName~RenderSessionOrchestrationIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
This phase delivered the main user-visible behavior improvement: scene mutation no longer synchronously forces GPU upload on the API path.
