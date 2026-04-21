# Phase 56 Research

## Problem

`v1.7` left one important inefficiency in place: `RuntimeFramePrelude.Execute()` still performed a global dirty sweep by calling the residency registry every frame. That made steady-state render cadence look like new upload work even when the scene and device epoch had not changed.

## Findings

- The residency registry already tracked enough state to distinguish `PendingUpload`, `Uploading`, `Resident`, `Dirty`, and `Failed`.
- The upload queue already deduped enqueues by entry id, so the remaining waste was mostly caused by dirty transitions, not by queue structure.
- Backend-ready / resource-epoch changes are the right trigger for broad rehydrate behavior; frame cadence itself is not.

## Decision

Phase 56 should replace the global dirty sweep with an event-driven API that only dirties entries when scene truth or resource epoch actually changes, and then keep frame prelude focused on draining already-queued work.

