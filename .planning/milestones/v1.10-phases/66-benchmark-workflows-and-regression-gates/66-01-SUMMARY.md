---
phase: 66-benchmark-workflows-and-regression-gates
plan: 01
subsystem: benchmark-runner
tags: [benchmark, tooling, performance]
provides:
  - shared benchmark runner script
  - viewer and surface-chart benchmark artifacts
key-files:
  added:
    - scripts/Run-Benchmarks.ps1
requirements-completed: [PERF-05]
completed: 2026-04-17
---

# Phase 66 Plan 01 Summary

## Accomplishments

- Added `Run-Benchmarks.ps1` so viewer and surface-chart suites now share one local runner.
- Standardized exporter output into JSON, CSV, Markdown, and a compact summary file.
- Verified both benchmark suites run for real rather than stopping at BenchmarkDotNet validation-only mode.

## Verification

- `pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite Viewer -Configuration Release -OutputRoot artifacts/benchmarks`
- `pwsh -File ./scripts/Run-Benchmarks.ps1 -Suite SurfaceCharts -Configuration Release -OutputRoot artifacts/benchmarks`
