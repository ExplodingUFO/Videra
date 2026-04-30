---
phase: 45-backend-abstraction-v2-platform-migration
plan: 03
subsystem: orchestrator-backend-selection
tags: [graphics, orchestrator, integration-tests]
provides:
  - direct backend selection in orchestrator
  - narrowed adapter role
  - device/surface regression coverage
key-files:
  modified:
    - src/Videra.Avalonia/Rendering/RenderSessionOrchestrator.cs
    - tests/Videra.Core.IntegrationTests/Rendering/GraphicsDeviceSurfaceIntegrationTests.cs
requirements-completed: [GFX-03, GFX-04]
completed: 2026-04-17
---

# Phase 45 Plan 03 Summary

## Accomplishments
- Changed `RenderSessionOrchestrator` to prefer direct `IGraphicsDevice` backends and only fall back to the legacy adapter when necessary.
- Kept software-backend diagnostics and presentation-copy reporting truthful after the migration.
- Added a focused integration test that proves the software backend now satisfies the v2 contracts directly.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests"` passed `3/3`
- `pwsh -File ./scripts/verify.ps1 -Configuration Release`

## Notes
The adapter remains a compatibility seam, not the steady-state path for built-in backends.
