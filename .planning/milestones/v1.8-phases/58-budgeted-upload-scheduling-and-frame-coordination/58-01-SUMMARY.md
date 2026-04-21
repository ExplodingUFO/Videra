---
phase: 58-budgeted-upload-scheduling-and-frame-coordination
plan: 01
subsystem: upload-budget
tags: [scene, upload, budgeting]
provides:
  - queue-aware heuristic budget resolution
  - interactive vs idle budget split
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/SceneUploadBudget.cs
requirements-completed: [UPLD-02]
completed: 2026-04-17
---

# Phase 58 Plan 01 Summary

## Accomplishments

- Added `SceneUploadBudget.Resolve(...)` so budgets now react to runtime mode plus queue pressure.
- Kept interactive drain intentionally tighter than idle drain for the same backlog.
- Bound the milestone to heuristic/runtime-aware tuning instead of pretending hardware telemetry is already part of the design.

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~SceneUploadBudgetTests"`

