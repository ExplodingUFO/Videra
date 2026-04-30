---
phase: 40-graphics-abstraction-v2-device-surface-split
plan: 01
subsystem: graphics-abstraction-v2-core
tags: [viewer, graphics, backend]
provides:
  - internal device/surface/frame abstractions
  - legacy compatibility adapter for existing backends
  - future offscreen/shared-device seam without new public APIs
key-files:
  modified:
    - src/Videra.Core/Graphics/Abstractions/IGraphicsDevice.cs
    - src/Videra.Core/Graphics/Abstractions/IRenderSurface.cs
    - src/Videra.Core/Graphics/Abstractions/IFrameContext.cs
    - src/Videra.Core/Graphics/Abstractions/LegacyGraphicsBackendAdapter.cs
requirements-completed: [GFX-01]
completed: 2026-04-17
---

# Phase 40 Plan 01 Summary

## Accomplishments
- Added internal `IGraphicsDevice`, `IRenderSurface`, and `IFrameContext` abstractions for the v2 seam.
- Added `LegacyGraphicsBackendAdapter` so existing native and software backends can participate without immediate rewrites.
- Kept the new seam internal, preserving `VideraEngine` as the only public extensibility root.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests"`

## Notes
- This is the minimal viable seam for future shared-device and offscreen work.
