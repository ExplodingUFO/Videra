# Benchmark Gates

This runbook turns the viewer and surface-chart benchmarks into repeatable evidence instead of one-off local measurements.

## Suites

- `Viewer` -> `benchmarks/Videra.Viewer.Benchmarks/ScenePipelineBenchmarks.cs` and `benchmarks/Videra.Viewer.Benchmarks/InspectionBenchmarks.cs`
- `SurfaceCharts` -> `SurfaceChartsSelectionBenchmarks.cs`, `SurfaceChartsRenderStateBenchmarks.cs`, `SurfaceChartsCacheBenchmarks.cs`, `SurfaceChartsProbeBenchmarks.cs`, and `SurfaceChartsRenderHostContractBenchmarks.cs`

The source-controlled suite contract lives in `benchmarks/benchmark-contract.json`. It is the canonical list of supported suites, benchmark families, and benchmark method names for the current benchmark gate.

## Workflow entrypoints

- Manual: GitHub Actions -> `Benchmark Gates (Label-Gated Review)` -> `Run workflow`
- Pull request: apply the `run-benchmarks` label to rerun the same workflow on the PR branch

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
- `scripts/Run-Benchmarks.ps1` reads that contract and emits `benchmark-manifest.json` beside the raw exporter output.
- Review, CI artifacts, and support guidance should all refer to the manifest plus the raw BenchmarkDotNet files, not to ad hoc exporter filenames alone.

## Current contract

- Default pull requests do not run benchmark evidence automatically.
- Manual runs and pull requests carrying the `run-benchmarks` label do run benchmark evidence.
- When a pull request opts into `run-benchmarks`, the uploaded artifacts become part of the review decision and should be checked before the PR is considered green.
- This is a label-gated review switch, not a hard numeric blocker with automatic threshold enforcement.
- Hard threshold enforcement and machine-enforced trend comparisons remain future options; they are not the current workflow contract.

## What to watch

- `Mean` and `Allocated` are the primary quick signals.
- Viewer benchmarks are expected to show scene import, residency apply, upload drain, backend rehydrate costs, and inspection pick/clip/snapshot costs.
- Surface-chart benchmarks are expected to show LOD selection, resident render-state change sets, cache batch reads, cache lookup-miss filtering, probe latency, tile residency churn, and benchmark-local GPU contract-path recolor/orbit/resize-rebind behavior.
- The render-host contract benchmarks intentionally use a benchmark-local fake backend. They validate chart-local host/update contract cost and must stay on the GPU contract path without fallback; they do not measure driver, swapchain, or compositor overhead.
- Compare runs over time before reacting to a single noisy data point. This workflow is meant to build trend evidence across alpha iterations, not to reward one-off wins.

## Gate semantics

This workflow is a regression gate in the alpha sense: it makes benchmark evidence visible and comparable before merge or release when the team explicitly asks for it.

Treat the artifacts as review evidence for step-function regressions, allocation trend spikes, or newly exposed hotspots. Do not treat the current workflow as proof that a PR passed a numeric benchmark threshold, because no numeric threshold is enforced yet.

## Future escalation

- Add soft threshold guidance only after enough stable historical runs exist.
- Consider automated trend comparisons before introducing hard blocking limits.
- Promote benchmark review into a harder blocker only when the workflow can explain failures without creating noisy false red builds.
