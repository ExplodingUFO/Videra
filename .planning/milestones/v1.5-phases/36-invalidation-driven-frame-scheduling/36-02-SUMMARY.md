---
phase: 36-invalidation-driven-frame-scheduling
plan: 02
subsystem: render-session-runtime-wiring
tags: [viewer, runtime, interaction]
provides:
  - invalidation-driven render session runtime
  - interactive-lease-aware host contract
  - dirty-frame-only software copy behavior
key-files:
  modified:
    - src/Videra.Avalonia/Rendering/RenderSession.cs
    - src/Videra.Avalonia/Controls/Interaction/IVideraInteractionHost.cs
    - src/Videra.Avalonia/Controls/Interaction/VideraView.InteractionHost.cs
    - src/Videra.Avalonia/Controls/Interaction/VideraInteractionController.cs
requirements-completed: [FRAME-01, FRAME-02]
completed: 2026-04-17
---

# Phase 36 Plan 02 Summary

## Accomplishments
- Reworked `RenderSession` so ready viewers render only when invalidated unless an interactive lease is active.
- Updated the interaction host and controller to acquire leases for gesture windows and invalidate frames after camera changes.
- Stopped the software copy path from running on a permanent loop when nothing is dirty.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionIntegrationTests|FullyQualifiedName~RenderSessionOrchestrationIntegrationTests"`

## Notes
- The render cadence changed materially while diagnostics and fallback shell truth stayed intact.
