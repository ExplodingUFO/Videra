---
phase: 58-budgeted-upload-scheduling-and-frame-coordination
plan: 03
subsystem: frame-prelude-coordination
tags: [scene, upload, runtime]
provides:
  - frame-prelude budget resolution
  - no synchronous GPU realization on scene mutation APIs
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/RuntimeFramePrelude.cs
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.cs
requirements-completed: [UPLD-03]
completed: 2026-04-17
---

# Phase 58 Plan 03 Summary

## Accomplishments

- Moved queue-aware budget resolution into `RuntimeFramePrelude`.
- Kept upload drain and ready-add application entirely inside frame cadence.
- Preserved the `v1.7` rule that public scene mutation APIs stay free of synchronous GPU resource creation.

## Verification

- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests|FullyQualifiedName~RenderSessionIntegrationTests|FullyQualifiedName~RenderSessionOrchestrationIntegrationTests"`

