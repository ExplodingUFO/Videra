---
phase: 35-videraview-runtime-shell-thinning
plan: 01
subsystem: avalonia-runtime-shell
tags: [viewer, runtime, shell]
provides:
  - internal `VideraViewRuntime` coordinator
  - runtime-owned session and bridge lifecycle
  - one place for shell-facing forwarding and runtime state
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Overlay.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Input.cs
requirements-completed: [SHELL-01]
completed: 2026-04-17
---

# Phase 35 Plan 01 Summary

## Accomplishments
- Added `VideraViewRuntime` partials so the control shell no longer directly owns session, bridge, overlay, native-host, and input coordination state.
- Moved render-session and bridge lifecycle into the runtime while keeping the existing public control surface intact.
- Created `VideraViewRuntimeTestAccess` so integration tests can verify runtime-owned state without reopening the shell.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewExtensibilityIntegrationTests|FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~VideraViewInteractionIntegrationTests"`

## Notes
- The new runtime seam stays internal to `Videra.Avalonia`; `VideraEngine` remains the public extensibility root.
