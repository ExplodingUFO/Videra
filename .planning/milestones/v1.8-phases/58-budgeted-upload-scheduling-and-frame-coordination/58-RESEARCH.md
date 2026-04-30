# Phase 58 Research

## Problem

`v1.7` already moved upload into frame prelude, but budgets were still relatively static and cadence-driven dirtying made budgeting less meaningful than it should have been. Once Phase 56 removed the steady-state dirty churn, the next gap was to make budget selection and drain behavior align with actual queue pressure and runtime mode.

## Findings

- `SceneUploadQueue` already had deduped pending-id tracking and could stop cleanly once object/byte limits were reached.
- The runtime already knew whether it was in interactive or steady-state mode.
- Pending object count and pending upload bytes are enough to drive a first-pass heuristic budget without adding hardware telemetry.

## Decision

Phase 58 should keep the upload queue simple, but make budget resolution queue-aware and use frame prelude as the single coordination seam for drain plus ready-add application.

