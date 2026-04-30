---
phase: 40-graphics-abstraction-v2-device-surface-split
plan: 02
subsystem: engine-orchestrator-v2-migration
tags: [viewer, graphics, runtime]
provides:
  - orchestrator bound to device/surface seam
  - engine frame lifecycle routed through render surface
  - capability truth preserved while internals migrate
key-files:
  modified:
    - src/Videra.Avalonia/Rendering/RenderSessionOrchestrator.cs
    - src/Videra.Core/Graphics/VideraEngine.cs
    - src/Videra.Core/Graphics/VideraEngine.Resources.cs
    - src/Videra.Core/Graphics/VideraEngine.Rendering.cs
requirements-completed: [GFX-01, GFX-02]
completed: 2026-04-17
---

# Phase 40 Plan 02 Summary

## Accomplishments
- Updated `RenderSessionOrchestrator` to resolve a legacy backend, wrap it as a device, create a render surface, and initialize the engine through the compatibility seam.
- Updated `VideraEngine` to begin and end frames through `IRenderSurface` while preserving capability and diagnostics behavior.
- Kept public viewer and engine APIs unchanged even though the internal graphics seam moved to v2.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests|FullyQualifiedName~RenderSessionOrchestrationIntegrationTests|FullyQualifiedName~VideraEngineExtensibilityIntegrationTests"`

## Notes
- Phase 40 intentionally stops at internal consumption plus adapter compatibility; backend-specific rewrites remain future work.
