---
phase: 53-backend-rehydration-and-residency-diagnostics
plan: 02
subsystem: scene-diagnostics
tags: [diagnostics, scene, viewer]
provides:
  - backend diagnostics scene counts
  - session bridge residency projection
  - viewer-facing read-only counts
key-files:
  modified:
    - src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs
    - src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs
    - src/Videra.Avalonia/Runtime/Scene/SceneResidencyDiagnostics.cs
    - tests/Videra.Core.IntegrationTests/Rendering/VideraViewSessionBridgeIntegrationTests.cs
requirements-completed: [DIAG-01]
completed: 2026-04-17
---

# Phase 53 Plan 02 Summary

## Accomplishments
- Extended `BackendDiagnostics` with read-only scene residency counts for document version, pending uploads, resident objects, dirty objects, and failed uploads.
- Projected those values through the existing session bridge instead of adding a new public runtime API.
- Updated bridge integration tests to treat scene residency as part of the stable diagnostics shell.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSessionBridgeIntegrationTests"`

## Notes
Diagnostics stayed additive and read-only, matching the rule that public orchestration surface should not widen.
