---
phase: 50-incremental-items-adapter-and-mutation-flow
plan: 02
subsystem: runtime-items-wiring
tags: [runtime, items, wiring]
provides:
  - incremental item subscription flow
  - runtime scene orchestration simplification
  - document-first items publication
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/VideraViewRuntime.Scene.cs
requirements-completed: [ITEMS-01]
completed: 2026-04-17
---

# Phase 50 Plan 02 Summary

## Accomplishments
- Replaced runtime-side full rebuild logic with adapter-driven incremental mutations for item changes.
- Kept `VideraViewRuntime.Scene` in an orchestration role instead of continuing to parse collection diffs itself.
- Retained the existing external `Items` semantics while making the internal pipeline cheaper and cleaner.

## Verification
- `dotnet test tests/Videra.Core.IntegrationTests/Videra.Core.IntegrationTests.csproj -c Release --filter "FullyQualifiedName~VideraViewSceneIntegrationTests"`

## Notes
The runtime now delegates item-diff logic instead of embedding it directly.
