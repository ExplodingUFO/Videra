# Phase 62 Research

## Problem

Telemetry and benchmark work exposed that `VideraViewRuntime` still owned too much scene coordination state even after `v1.7-v1.8` service extraction. The remaining publish/apply/enqueue/diagnostics flow was concentrated across runtime partials instead of behind a single scene-runtime coordinator.

## Findings

- `VideraViewRuntime` still directly held scene store, delta planner, residency registry, upload queue, items adapter, import service, engine applicator, and resource epoch state.
- Most of the remaining scene flow could be moved without widening the public viewer API because the types were already internal.
- Integration tests still reflected private runtime fields, so coordinator extraction needed to preserve enough compatibility to avoid gratuitous breakage.

## Decision

Phase 62 should extract a dedicated `SceneRuntimeCoordinator` that owns scene publication, delta application, residency mutation, upload queueing, import orchestration, resource-epoch changes, and scene diagnostics creation, leaving `VideraViewRuntime` as shell/orchestration glue.
