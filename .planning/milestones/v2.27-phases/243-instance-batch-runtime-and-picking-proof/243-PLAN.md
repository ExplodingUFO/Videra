# Phase 243: Instance Batch Runtime and Picking Proof - Plan

**Status:** Executed
**Created:** 2026-04-27

## Goal

Prove instance batches are retained runtime scene state that participates in viewer framing, diagnostics, and click picking without expanding into a renderer rewrite.

## Tasks

1. Runtime entry point
   - Add `AddInstanceBatch(...)` from `VideraView` through `VideraViewRuntime` into `SceneRuntimeCoordinator`.
   - Expose current runtime `InstanceBatches`.

2. Framing and diagnostics
   - Include batch bounds in `FrameAll()`.
   - Add diagnostics fields for retained batch count and retained instance count.
   - Include pickable batch instances in `PickableObjectCount` while keeping draw-call/vertex/submitted-instance counters nullable.

3. Picking proof
   - Extend `SceneHitTestRequest` to accept instance batches.
   - Extend `SceneHitTestResult.SceneHit` with nullable `Object` and `InstanceIndex`.
   - Hit-test each pickable batch instance against the shared mesh payload and fallback bounds.
   - Route Avalonia click/annotation/measurement picking through batch-aware inputs.

4. Verification
   - Add Core tests for pickable and non-pickable instance batches.
   - Add Avalonia tests for diagnostics and framing.
   - Run focused builds/tests only; full CI is intentionally skipped per user instruction.

## Non-Goals

- GPU instanced rendering.
- Batching normal objects into draw-call-reducing renderer data.
- New benchmark gates.
- Demo UI.
- SurfaceCharts changes.
