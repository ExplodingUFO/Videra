# Phase 244: Benchmark Evidence and Threshold Promotion - Summary

**Completed:** 2026-04-27
**Status:** Complete

## Delivered

- Added `InstanceBatchBenchmarks` to the Viewer benchmark project.
- Benchmarks cover:
  - normal object scene-document population
  - instance-batch scene-document population
  - normal object hit-test pick latency
  - instance-batch hit-test pick latency
  - diagnostics snapshot evidence for retained instance counts
- Added the Viewer `InstanceBatch` family to `benchmark-contract.json`.
- Left `benchmark-thresholds.json` unchanged so the new metrics remain evidence-only.
- Updated benchmark gate docs to explain the evidence-only status.

## Verification

- `dotnet build benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj -c Release`
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~BenchmarkContractRepositoryTests|FullyQualifiedName~BenchmarkThresholdRepositoryTests"`
- `dotnet run -c Release --project benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj -- --list flat`

## Deferred

- Hard-threshold promotion for instance-batch metrics.
- Full benchmark execution and baseline capture.
- Draw-call and uploaded instance-buffer accounting.
