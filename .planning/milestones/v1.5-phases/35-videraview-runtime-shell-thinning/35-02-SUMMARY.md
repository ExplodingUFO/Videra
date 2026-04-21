---
phase: 35-videraview-runtime-shell-thinning
plan: 02
subsystem: avalonia-shell-forwarding
tags: [viewer, shell, compatibility]
provides:
  - public-shell forwarding through runtime
  - runtime-backed scene/overlay/input methods
  - compatibility-preserving `VideraView` API shape
key-files:
  modified:
    - src/Videra.Avalonia/Controls/VideraView.cs
    - src/Videra.Avalonia/Controls/VideraView.Scene.cs
    - src/Videra.Avalonia/Controls/VideraView.Overlay.cs
    - src/Videra.Avalonia/Controls/VideraView.Input.cs
requirements-completed: [SHELL-01, SHELL-02]
completed: 2026-04-17
---

# Phase 35 Plan 02 Summary

## Accomplishments
- Reworked `VideraView` into a forwarding shell that delegates scene, overlay, input, and diagnostics operations into the runtime.
- Kept `BackendDiagnostics`, `RenderCapabilities`, `SelectionState`, `Annotations`, and scene-loading APIs behaviorally compatible on the public shell.
- Preserved native-host composition and diagnostics publication on the control while removing direct session orchestration from the shell.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewExtensibilityIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraViewInteractionIntegrationTests"`

## Notes
- This keeps the shell thin enough for later scheduling, scene, and overlay work without widening public API.
