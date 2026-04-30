# Phase 244: Benchmark Evidence and Threshold Promotion - Plan

**Status:** Executed
**Created:** 2026-04-27

## Goal

Expose instance-batch performance evidence through the existing benchmark contract while leaving unstable metrics evidence-only.

## Tasks

1. Add Viewer benchmark class
   - Create `InstanceBatchBenchmarks`.
   - Use `[Params(1000, 10000)]` to capture meaningful object/instance counts.
   - Benchmark normal scene-document population, instance-batch scene-document population, normal hit-test, batch hit-test, and diagnostics snapshot evidence.

2. Update benchmark contract
   - Add an `InstanceBatch` family to the Viewer suite.
   - Keep `benchmark-thresholds.json` unchanged.

3. Update docs/tests
   - Update benchmark contract repository test expected names.
   - Document instance-batch benchmarks as evidence-only.

4. Verify
   - Build the Viewer benchmark project.
   - Run repository benchmark contract/threshold tests.
   - List BenchmarkDotNet benchmark names to confirm discovery.

## Non-Goals

- Full benchmark execution.
- CI threshold changes.
- Renderer draw-call accounting.
- Performance Lab UI.
