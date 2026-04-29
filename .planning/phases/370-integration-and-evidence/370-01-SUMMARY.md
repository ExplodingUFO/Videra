---
phase: 370-integration-and-evidence
plan: 01
subsystem: Plot
tags: [evidence, output-evidence, bar, contour, regression, tests]
depends_on:
  requires: [368, 369]
  provides: [output-evidence-bar-contour]
  affects: [370-02]
tech_stack:
  added: []
  patterns: [evidence-factory, backward-compatible-overload]
key_files:
  created:
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/RegressionGuardrailTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DOutputEvidence.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewPlotApiTests.cs
decisions:
  - Keep 2-param CreateOutputEvidence overload for backward compatibility
  - Fix missing UpdateBarRenderingStatus call in Refresh() (Rule 1 - Bug)
metrics:
  duration: ~15m
  completed: 2026-04-29
---

# Phase 370 Plan 01: Output Evidence Integration Summary

Bar and Contour chart types wired into Plot3DOutputEvidence contract with FromBarStatus/FromContourStatus factories, pre-existing test fixes for ImageExport capability change, and regression guardrail tests for all 5 chart families.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Wire Bar/Contour rendering evidence | e6e3993 | Plot3DOutputEvidence.cs, Plot3D.cs |
| 2 | Fix tests + add guardrail tests | e4f0381 | VideraChartViewPlotApiTests.cs, RegressionGuardrailTests.cs, VideraChartView.Core.cs |

## Key Changes

### Plot3DOutputEvidence.cs
- Added `FromBarStatus(BarChartRenderingStatus)` factory → `RenderingKind="bar-rendering-status"`
- Added `FromContourStatus(ContourChartRenderingStatus)` factory → `RenderingKind="contour-rendering-status"`, BackendKind=Software

### Plot3D.cs
- Added 4-param `CreateOutputEvidence` overload with bar/contour rendering status
- Kept 2-param overload as backward-compatible convenience (passes null for bar/contour)
- Updated `CreateRenderingEvidence` switch to handle Bar and Contour kinds

### VideraChartView.Core.cs
- Added `UpdateBarRenderingStatus()` call to `Refresh()` method (was missing, causing Bar rendering status to not update on plot changes)

### VideraChartViewPlotApiTests.cs
- Fixed 3 pre-existing tests: ImageExport is now correctly reported as supported
- Added `Plot3D_CreateOutputEvidence_ReportsBarChartContract` test
- Added `Plot3D_CreateOutputEvidence_ReportsContourContract` test
- Added `CreateContourField` helper

### RegressionGuardrailTests.cs (new)
- `RegressionGuardrail_AllChartFamilies_ProduceDatasetEvidence`: validates all 5 families
- `RegressionGuardrail_AllChartFamilies_ProduceOutputEvidence`: validates all 5 families with rendering evidence

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed missing UpdateBarRenderingStatus in Refresh()**
- **Found during:** Task 2
- **Issue:** `Refresh()` method called `UpdateScatterRenderingStatus()` and `UpdateContourRenderingStatus()` but not `UpdateBarRenderingStatus()`, causing Bar rendering status to not update when plot changes
- **Fix:** Added `UpdateBarRenderingStatus()` call to `Refresh()`
- **Files modified:** VideraChartView.Core.cs
- **Commit:** e4f0381

## Verification

- `dotnet build src/Videra.SurfaceCharts.Avalonia/Videra.SurfaceCharts.Avalonia.csproj --no-restore` — 0 errors
- `dotnet test --filter "Plot3D_CreateOutputEvidence|RegressionGuardrail"` — 7/7 pass
- All 5 chart families produce correct output evidence with rendering evidence
- All 5 chart families produce correct dataset evidence
- 3 previously failing tests now pass

## Self-Check: PENDING
