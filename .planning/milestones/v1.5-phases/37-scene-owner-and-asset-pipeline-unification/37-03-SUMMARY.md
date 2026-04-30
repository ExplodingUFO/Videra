---
phase: 37-scene-owner-and-asset-pipeline-unification
plan: 03
subsystem: scene-owner-regression-guards
tags: [viewer, scene, tests]
provides:
  - scene-document-aware integration tests
  - coverage for import/add/replace/clear flows
  - regression guards for unified scene truth
key-files:
  modified:
    - tests/Videra.Core.IntegrationTests/Rendering/VideraViewSceneIntegrationTests.cs
requirements-completed: [SCENE-02]
completed: 2026-04-17
---

# Phase 37 Plan 03 Summary

## Accomplishments
- Added tests proving runtime scene document truth tracks scene mutations and imports.
- Updated scene helpers to stop reflecting the old raw engine field and instead use the new internal scene view.
- Locked the phase so backend-neutral assets and runtime scene ownership stay aligned.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
- The tests focus on behavior and internal truth, not on exposing new scene APIs.
