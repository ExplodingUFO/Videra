---
phase: 58-budgeted-upload-scheduling-and-frame-coordination
plan: 02
subsystem: upload-queue
tags: [scene, upload, queue]
provides:
  - budget-respecting drain
  - pending entries retained across frames
key-files:
  modified:
    - src/Videra.Avalonia/Runtime/Scene/SceneUploadQueue.cs
requirements-completed: [UPLD-01]
completed: 2026-04-17
---

# Phase 58 Plan 02 Summary

## Accomplishments

- Kept upload queue drain bounded by both object count and byte budget.
- Left remaining entries pending when the active frame budget is exhausted instead of bulk-uploading them.
- Preserved dedupe semantics so already queued ids do not multiply in the pending queue.

## Verification

- `dotnet test tests/Videra.Avalonia.Tests/Videra.Avalonia.Tests.csproj -c Release --filter "FullyQualifiedName~SceneUploadQueueTests"`

