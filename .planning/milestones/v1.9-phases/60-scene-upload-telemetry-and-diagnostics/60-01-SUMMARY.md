---
phase: 60-scene-upload-telemetry-and-diagnostics
plan: 01
subsystem: scene-telemetry
tags: [scene, telemetry, upload]
provides:
  - richer upload flush telemetry
  - explicit queue pressure bytes
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/SceneUploadQueue.cs
    - src/Videra.Avalonia/Runtime/Scene/SceneResidencyDiagnostics.cs
requirements-completed: [OBS-01]
completed: 2026-04-17
---

# Phase 60 Plan 01 Summary

## Accomplishments

- Expanded `SceneUploadQueue.Drain(...)` so it now returns uploaded bytes, failure count, duration, and the resolved budget together with uploaded records.
- Extended `SceneResidencyDiagnostics` with queued-byte and last-upload metrics instead of only exposing simple state counts.
- Kept the telemetry shape internal-first so later diagnostics and benchmarks can consume the same contract.

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~RuntimeFramePreludeTests|FullyQualifiedName~SceneUploadQueueTests|FullyQualifiedName~SceneResidencyRegistryTests"`
