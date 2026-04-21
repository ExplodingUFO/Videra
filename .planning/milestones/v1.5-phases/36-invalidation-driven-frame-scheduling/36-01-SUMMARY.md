---
phase: 36-invalidation-driven-frame-scheduling
plan: 01
subsystem: render-session-scheduler-core
tags: [viewer, runtime, scheduling]
provides:
  - invalidation-driven frame scheduler
  - interactive lease primitive
  - internal invalidation reason vocabulary
key-files:
  modified:
    - src/Videra.Avalonia/Rendering/FrameScheduler.cs
    - src/Videra.Avalonia/Rendering/InteractiveFrameLease.cs
    - src/Videra.Avalonia/Rendering/RenderInvalidationKinds.cs
requirements-completed: [FRAME-01]
completed: 2026-04-17
---

# Phase 36 Plan 01 Summary

## Accomplishments
- Added `FrameScheduler` to track pending dirty-frame reasons instead of assuming a permanent render loop.
- Added `InteractiveFrameLease` so continuous rendering exists only for active gesture windows.
- Renamed the invalidation vocabulary to `RenderInvalidationKinds` to keep the internal API clean while preserving behavior.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionIntegrationTests"`

## Notes
- This phase kept the new scheduling seam internal to Avalonia runtime code.
