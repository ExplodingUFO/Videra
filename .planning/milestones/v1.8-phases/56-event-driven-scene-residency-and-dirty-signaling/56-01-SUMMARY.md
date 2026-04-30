---
phase: 56-event-driven-scene-residency-and-dirty-signaling
plan: 01
subsystem: scene-residency
tags: [scene, residency, runtime]
provides:
  - event-driven dirty transition API
  - resource-epoch-aware rehydrate marking
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/SceneResidencyRegistry.cs
requirements-completed: [RES-03]
completed: 2026-04-17
---

# Phase 56 Plan 01 Summary

## Accomplishments

- Replaced the blanket dirtying API with `MarkDirtyForResourceEpoch(...)`.
- Kept pending/uploading entries in their current state while only transitioning rehydratable resident entries to `Dirty`.
- Preserved residency diagnostics and approximate upload-byte tracking.

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~SceneResidencyRegistryTests"`

