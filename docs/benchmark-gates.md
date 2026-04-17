# Benchmark Gates

This runbook turns the viewer and surface-chart benchmarks into repeatable evidence instead of one-off local measurements.

## Suites

- `Viewer` -> `benchmarks/Videra.Viewer.Benchmarks/ScenePipelineBenchmarks.cs`
- `SurfaceCharts` -> `benchmarks/Videra.SurfaceCharts.Benchmarks/SurfaceChartsBenchmarks.cs`

## Workflow entrypoints

- Manual: GitHub Actions -> `Benchmark Gates` -> `Run workflow`
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

## What to watch

- `Mean` and `Allocated` are the primary quick signals.
- Viewer benchmarks are expected to show scene import, residency apply, upload drain, and backend rehydrate costs.
- Surface-chart benchmarks are expected to show LOD selection, resident render-state change sets, cache batch reads, and pyramid-build costs.

## Gate semantics

This workflow is a regression gate in the alpha sense: it makes benchmark evidence visible and comparable before merge or release.

It is not yet a hard numeric blocker with automatic threshold enforcement. Use the artifacts to spot step-function regressions, then tighten thresholds once the team has enough historical runs to justify them.
