---
phase: 45-backend-abstraction-v2-platform-migration
plan: 01
subsystem: software-backend-v2
tags: [graphics, backend, software]
provides:
  - direct software device/surface seam
  - software frame context
  - platform backend visibility for v2 internals
key-files:
  modified:
    - src/Videra.Core/Videra.Core.csproj
    - src/Videra.Core/Graphics/Software/SoftwareBackend.cs
requirements-completed: [GFX-03]
completed: 2026-04-17
---

# Phase 45 Plan 01 Summary

## Accomplishments
- Promoted the built-in software backend to implement `IGraphicsDevice` and `IRenderSurface` directly.
- Added the direct software frame-context path used by the orchestrator when no native backend is active.
- Extended `InternalsVisibleTo` so platform packages can participate in the internal migration without creating a new public seam.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests"`

## Notes
The software path became the first built-in backend to satisfy the v2 contract directly end to end.
