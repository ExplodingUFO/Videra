---
phase: 60-scene-upload-telemetry-and-diagnostics
plan: 03
subsystem: diagnostics-shell
tags: [scene, diagnostics, demo]
provides:
  - public backend diagnostics projection for scene uploads
  - narrow demo upload telemetry visibility
key-files:
  modified:
    - src/Videra.Avalonia/Controls/VideraBackendDiagnostics.cs
    - src/Videra.Avalonia/Controls/VideraViewSessionBridge.cs
    - samples/Videra.Demo/ViewModels/MainWindowViewModel.cs
requirements-completed: [OBS-01, OBS-02]
completed: 2026-04-17
---

# Phase 60 Plan 03 Summary

## Accomplishments

- Added scene upload bytes, object counts, failures, durations, and resolved budgets to the public backend-diagnostics shell without widening the viewer command surface.
- Mapped the new runtime telemetry through `VideraViewSessionBridge`.
- Reflected the richer scene queue/upload state in the narrow demo diagnostics panel and integration coverage.

## Verification

- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraViewSessionBridgeIntegrationTests"`
