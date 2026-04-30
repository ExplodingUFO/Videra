---
phase: 44-asset-rehydration-and-rebind-truth
plan: 03
subsystem: rehydration-tests
tags: [viewer, integration-tests, rehydration]
provides:
  - deferred-upload rebind coverage
  - retained-asset regression tests
  - no-surprise upload behavior checks
key-files:
  modified:
    - tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs
requirements-completed: [ASSET-01, ASSET-02]
completed: 2026-04-17
---

# Phase 44 Plan 03 Summary

## Accomplishments
- Added a focused test that verifies deferred objects remain unuploaded until the runtime has an active resource factory.
- Covered the rehydration path so runtime state can rebuild scene resources after rebind without forcing a new import.
- Used the same test suite to confirm software fallback is no longer the steady-state upload story.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~GraphicsDeviceSurfaceIntegrationTests"` passed `29/29`

## Notes
Phase 44 finished the runtime side of the scene-pipeline contract before platform backend migration.
