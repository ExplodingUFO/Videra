---
phase: 36-invalidation-driven-frame-scheduling
plan: 03
subsystem: render-session-scheduling-guards
tags: [viewer, runtime, tests]
provides:
  - focused render-session scheduling coverage
  - orchestration regression guards
  - proof that interactive and idle paths diverge correctly
key-files:
  modified:
    - tests/Videra.Core.IntegrationTests/Rendering/RenderSessionIntegrationTests.cs
    - tests/Videra.Core.IntegrationTests/Rendering/RenderSessionOrchestrationIntegrationTests.cs
requirements-completed: [FRAME-02]
completed: 2026-04-17
---

# Phase 36 Plan 03 Summary

## Accomplishments
- Added tests that fail if a ready session starts looping before an interactive lease begins.
- Added tests proving non-interactive invalidation produces a single dirty frame without waking continuous scheduling.
- Kept orchestration integration tests green after the scheduler internals changed.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~RenderSessionIntegrationTests|FullyQualifiedName~RenderSessionOrchestrationIntegrationTests"`

## Notes
- The phase closes the ready-state polling debt without losing responsive gesture rendering.
