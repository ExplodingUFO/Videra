---
phase: 50-incremental-items-adapter-and-mutation-flow
plan: 01
subsystem: items-adapter
tags: [runtime, items, scene]
provides:
  - incremental add/remove/replace/move handling
  - reset-only rebuild path
  - document identity preservation
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/SceneItemsAdapter.cs
    - tests/Videra.Avalonia.Tests/Scene/SceneItemsAdapterTests.cs
requirements-completed: [ITEMS-01]
completed: 2026-04-17
---

# Phase 50 Plan 01 Summary

## Accomplishments
- Introduced `SceneItemsAdapter` so collection-change logic lives in one runtime-scene service instead of the main runtime partial.
- Handled add/remove/replace/move incrementally while keeping reset as the only full rebuild path.
- Preserved document entry identity across incremental collection changes.

## Verification
- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release`

## Notes
The adapter isolates Avalonia collection semantics from the core scene document contract.
