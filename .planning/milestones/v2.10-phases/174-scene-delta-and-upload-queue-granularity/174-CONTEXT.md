# Phase 174 Context: Scene Delta and Upload Queue Granularity

## Goal

Refine scene-delta and upload-queue truth so the runtime no longer treats every retained-entry upload as one undifferentiated FIFO reupload.

## Problem

- `SceneDelta` still collapses all retained-entry upload work into one coarse reupload bucket.
- `SceneUploadQueue` still behaves like de-duplicated FIFO, so repeated dirty events only avoid duplicate ids but do not express priority.
- Interactive or already-attached scene entries currently have no explicit scheduling advantage over background pending imports.

## Scope

- `SceneDelta` may expose explicit change kinds.
- `SceneUploadQueue` may coalesce repeated requests by entry id and choose the next upload by priority instead of raw arrival order.
- Attached/dirty runtime entries may be prioritized over background pending imports during interactive frames.

## Non-Goals

- No new visibility solver, no camera-distance scheduler, and no scene culling system.
- No render-pipeline, diagnostics-doc, or support-wording work from Phase 175.
- No broader resource scheduler or background-worker abstraction.

## Assumption

For this phase, “visibility/interactive hot path” is approximated by already-attached runtime entries plus the existing interactive-frame signal; full camera-visibility awareness remains out of scope.
