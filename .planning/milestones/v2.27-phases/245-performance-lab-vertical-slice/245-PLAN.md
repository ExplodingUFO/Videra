# Phase 245: Performance Lab Vertical Slice - Plan

**Status:** Executed
**Created:** 2026-04-27

## Goal

Add a focused Performance Lab panel to `Videra.Demo` that can generate normal-object and retained instance-batch datasets, display diagnostics, and copy a support snapshot.

## Tasks

1. ViewModel state
   - Add selectable object counts.
   - Add normal-object versus instance-batch mode.
   - Add pickable toggle.
   - Add diagnostics and snapshot text state.

2. Demo UI
   - Add one `PERFORMANCE LAB` section in the existing side panel.
   - Keep existing support, scene pipeline, scene graph, and transform flows reachable.

3. Dataset generation
   - In normal mode, create regular initialized `Object3D` markers.
   - In instance mode, add one retained `InstanceBatchDescriptor`.
   - Keep counts bounded and generation direct.

4. Evidence and copy support
   - Report build/frame-time proxy, pick latency, draw-call availability, upload bytes, resident bytes, retained instance count, and pickable count.
   - Add a copyable Performance Lab snapshot.

5. Verification
   - Build `Videra.Demo`.
   - Run Demo configuration tests.
   - Run scene-document tests for the clear-scene adjustment.

## Non-Goals

- Full benchmark execution.
- Real GPU instancing.
- Replacing the existing demo layout.
- New chart or SurfaceCharts work.
