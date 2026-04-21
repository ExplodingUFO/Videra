# Phase 60 Research

## Problem

`v1.8` made scene upload budgeting and residency behavior correct, but the runtime still did not expose enough evidence to explain queue pressure, last-frame upload cost, or the resolved per-frame budget. Performance decisions were still being made by inference.

## Findings

- `SceneUploadQueue.Drain(...)` already knew uploaded bytes, uploaded object count, failures, duration, and the resolved budget.
- `RuntimeFramePrelude` applied ready scene objects but did not preserve the last meaningful upload result for diagnostics.
- `VideraViewSessionBridge` and `VideraBackendDiagnostics` were already the stable public diagnostics shell, so richer scene telemetry could flow there without widening the viewer API.

## Decision

Phase 60 should turn upload telemetry into first-class runtime diagnostics: capture the last meaningful drain result, retain the resolved budget choice, surface pending upload bytes, and project that truth through the existing backend diagnostics shell and narrow demo surfaces.
