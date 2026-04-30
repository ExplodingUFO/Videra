---
phase: 18-demo-docs-and-repository-truth-for-professional-charts
plan: 01
subsystem: surface-charts-demo
tags: [surface-charts, demo, avalonia, rendering-status, probe]
requires:
  - phase: 16-rendering-host-seam-and-gpu-main-path
    provides: chart-local render-host seam plus `RenderingStatus` / `RenderStatusChanged`
  - phase: 17-large-dataset-residency-cache-evolution-and-optional-rust-spike
    provides: overview/detail truth and cache-backed data-path behavior
provides:
  - visible rendering-path onboarding inside the independent chart demo
  - explicit probe workflow and axis/legend truth in the sample UI
  - focused headless demo tests for rendering-status projection and visible copy
affects: [18-02, 18-03]
key-files:
  modified:
    - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml
    - samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs
    - tests/Videra.Core.Tests/Samples/SurfaceChartsDemoConfigurationTests.cs
    - tests/Videra.Core.Tests/Samples/SurfaceChartsDemoViewportBehaviorTests.cs
completed: 2026-04-14
---

# Phase 18 Plan 01 Summary

## Accomplishments
- Added visible demo panels for `Rendering path`, `Probe workflow`, and `Axes and legend` so the independent sample explains the shipped chart behavior instead of leaving it implicit.
- Projected `SurfaceChartView.RenderingStatus` into the sample UI through `RenderStatusChanged`, including backend, fallback, host path, readiness, and resident-tile text.
- Extended the headless sample tests so the demo must keep renderer-status truth and the new user-facing copy.

## Verification
- `dotnet test tests/Videra.Core.Tests/Videra.Core.Tests.csproj -c Release --filter "FullyQualifiedName~SurfaceChartsDemoConfigurationTests|FullyQualifiedName~SurfaceChartsDemoViewportBehaviorTests"`

## Notes
- The sample remains an independent chart demo and still does not pretend to ship a finished orbit / pan / dolly workflow.
- The rendering panel is intentionally lightweight; it exposes the real control status surface without turning the sample into a diagnostics matrix.
- `.planning/` artifacts remain local in this checkout (`commit_docs: false`).
