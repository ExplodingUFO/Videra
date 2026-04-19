# Benchmark Gates

This runbook turns the viewer and surface-chart benchmarks into repeatable evidence instead of one-off local measurements.

## Suites

- `Viewer` -> `benchmarks/Videra.Viewer.Benchmarks/ScenePipelineBenchmarks.cs` and `benchmarks/Videra.Viewer.Benchmarks/InspectionBenchmarks.cs`
- `SurfaceCharts` -> `benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsBenchmarks.cs`

## Workflow entrypoints

- Manual: GitHub Actions -> `Benchmark Gates (Label-Gated Review)` -> `Run workflow`
- Pull request: apply the `run-benchmarks` label to rerun the same workflow on the PR branch

The workflow uploads raw BenchmarkDotNet artifacts for each suite as GitHub Actions artifacts:

- `benchmarks-viewer`
- `benchmarks-surfacecharts`

## Local run

```powershell
pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite Viewer -Configuration Release
pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite SurfaceCharts -Configuration Release
```

Artifacts are written under `artifacts/benchmarks/<suite>`.

## Current contract

- Default pull requests do not run benchmark evidence automatically.
- Manual runs and pull requests carrying the `run-benchmarks` label do run benchmark evidence.
- When a pull request opts into `run-benchmarks`, the uploaded artifacts become part of the review decision and should be checked before the PR is considered green.
- This is a label-gated review switch, not a hard numeric blocker with automatic threshold enforcement.
- Soft thresholds and machine-enforced trend comparisons remain future options; they are not the current workflow contract.

## What to watch

- `Mean` and `Allocated` are the primary quick signals.
- Viewer benchmarks are expected to show scene import, residency apply, upload drain, backend rehydrate costs, and inspection pick/clip/snapshot costs.
- Surface-chart benchmarks are expected to show LOD selection, resident render-state change sets, cache batch reads, and pyramid-build costs.
- compare runs over time before reacting to a single noisy data point. This workflow is meant to build trend evidence across alpha iterations, not to reward one-off wins.

## Gate semantics

This workflow is a regression gate in the alpha sense: it makes benchmark evidence visible and comparable before merge or release when the team explicitly asks for it.

Treat the artifacts as review evidence for step-function regressions, suspicious allocation growth, or newly exposed hotspots. Do not treat the current workflow as proof that a PR passed a numeric benchmark threshold, because no such threshold is enforced yet.

## Future escalation

- Add soft threshold guidance only after enough stable historical runs exist.
- Consider automated trend comparisons before introducing hard blocking limits.
- Promote benchmark review into a harder blocker only when the workflow can explain failures without creating noisy false red builds.
