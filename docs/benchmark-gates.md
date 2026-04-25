# Benchmark Gates

This runbook turns the viewer and surface-chart benchmarks into repeatable evidence instead of one-off local measurements.

## Suites

- `Viewer` -> `benchmarks/Videra.Viewer.Benchmarks/ScenePipelineBenchmarks.cs` and `benchmarks/Videra.Viewer.Benchmarks/InspectionBenchmarks.cs`
- `SurfaceCharts` -> `SurfaceChartsSelectionBenchmarks.cs`, `SurfaceChartsRenderStateBenchmarks.cs`, `SurfaceChartsCacheBenchmarks.cs`, `SurfaceChartsProbeBenchmarks.cs`, and `SurfaceChartsRenderHostContractBenchmarks.cs`

The source-controlled suite contract lives in `benchmarks/benchmark-contract.json`. It is the canonical list of supported suites, benchmark families, and benchmark method names for the current benchmark gate.

## Workflow entrypoints

- Manual: GitHub Actions -> `Benchmark Gates` -> `Run workflow`
- Pull request: benchmark gates run automatically on `opened`, `synchronize`, and `reopened`

The workflow uploads suite artifact directories for each suite as GitHub Actions artifacts:

- `benchmarks-viewer`
- `benchmarks-surfacecharts`

Each uploaded directory now includes:

- raw BenchmarkDotNet exporter output
- `SUMMARY.txt`
- `benchmark-manifest.json` — the machine-readable evidence manifest for that suite run

## Local run

```powershell
pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite Viewer -Configuration Release
pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite SurfaceCharts -Configuration Release
```

Artifacts are written under `artifacts/benchmarks/<suite>`.

## Evidence contract

- `benchmarks/benchmark-contract.json` defines the supported suites and benchmark names that make up the current evidence contract.
- `benchmarks/benchmark-thresholds.json` defines the committed hard-threshold slice for the current PR benchmark gate.
- `scripts/Run-Benchmarks.ps1` reads that contract and emits `benchmark-manifest.json` beside the raw exporter output.
- `scripts/Test-BenchmarkThresholds.ps1` reads the BenchmarkDotNet JSON reports plus `benchmark-manifest.json`, emits threshold-evaluation artifacts, and fails when committed mean-runtime budgets are exceeded.
- Review, CI artifacts, and support guidance should all refer to the manifest, threshold evaluation, and raw BenchmarkDotNet files, not to ad hoc exporter filenames alone.

## Current contract

- Pull requests run benchmark gates automatically.
- Each suite still uploads the full benchmark evidence set, but the hard gate currently enforces seven representative mean-runtime thresholds covering allocation, scene upload drain, rehydrate, inspection snapshot, and chart residency churn:
  - `ScenePipelineBenchmarks.SceneResidencyRegistry_ApplyDelta` (allocation)
  - `ScenePipelineBenchmarks.SceneUploadQueue_Drain` (scene upload drain)
  - `ScenePipelineBenchmarks.ScenePipeline_RehydrateAfterBackendReady` (rehydrate)
  - `InspectionBenchmarks.SceneHitTest_MeshAccurateDistance` (hit test)
  - `InspectionBenchmarks.SnapshotExport_LiveReadbackFastPath` (inspection snapshot)
  - `SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement` (chart residency churn)
  - `SurfaceChartsProbeBenchmarks.ProbeLatency` (probe latency)
- On the SurfaceCharts side, those committed names now describe the tightened interactive residency under camera movement and lower probe-path churn on the existing chart-local path.
- If one of those committed thresholds regresses beyond the allowed budget, the PR benchmark job fails and the uploaded artifact directory still includes the threshold evaluation details.
- This is now a hard numeric blocker for the thresholded slice set, not a label-gated review switch.

## What to watch

- `Mean` and `Allocated` are the primary quick signals.
- Viewer benchmarks are expected to show scene import, residency apply, upload drain, backend rehydrate costs, and inspection pick/clip/snapshot costs.
- Surface-chart benchmarks are expected to show LOD selection, resident render-state change sets, cache batch reads, cache lookup-miss filtering, probe latency, tile residency churn, and benchmark-local GPU contract-path recolor/orbit/resize-rebind behavior.
- The render-host contract benchmarks intentionally use a benchmark-local fake backend. They validate chart-local host/update contract cost and must stay on the GPU contract path without fallback; they do not measure driver, swapchain, or compositor overhead.
- Compare runs over time before reacting to a single noisy data point. This workflow is meant to build trend evidence across alpha iterations, not to reward one-off wins.

## Gate semantics

This workflow is now a hard numeric blocker for the committed threshold slice and a broader evidence feed for the rest of the benchmark suite.

Treat threshold failures as blocking regressions. Treat the remaining non-thresholded benchmark reports as review evidence for wider trend analysis, allocation spikes, or newly exposed hotspots.

## Future escalation

- Tighten the committed thresholds after enough stable CI history exists.
- Tighten the committed thresholds after enough stable CI history exists.
- Expand the hard-threshold slice only when the additional benchmarks show low enough noise to avoid false red builds.
