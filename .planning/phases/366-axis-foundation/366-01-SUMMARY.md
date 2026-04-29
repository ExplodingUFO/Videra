---
phase: 366-axis-foundation
plan: "01"
subsystem: surface-charts
tags: [axis, log-scale, datetime, formatters, tick-generation]
dependency_graph:
  requires: []
  provides: [log-scale-axes, datetime-axes, per-axis-formatters]
  affects: [bar-chart, contour-plot, legend-enhancement]
tech_stack:
  added: []
  patterns: [tick-generation, scale-kind-dispatch, per-axis-formatter-priority]
key_files:
  created:
    - tests/Videra.SurfaceCharts.Core.Tests/Axis/LogScaleAxisDescriptorTests.cs
    - tests/Videra.SurfaceCharts.Core.Tests/Axis/DateTimeAxisTickGeneratorTests.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/Axis/CustomFormatterIntegrationTests.cs
  modified:
    - src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisOverlayPresenter.cs
decisions:
  - "Log validation uses ArgumentOutOfRangeException (not ArgumentException) for min/max <= 0"
  - "Y axis defaults to Linear scale kind (value axis has no ScaleKind in metadata)"
  - "DateTime ticks filtered to exact axis range to prevent out-of-range labels"
  - "DateTime formatting uses span-aware format: HH:mm:ss (<1hr), MM-dd HH:mm (<1day), yyyy-MM-dd (>=1day)"
metrics:
  duration: "~5 minutes"
  completed: "2026-04-29T19:15:00Z"
  tasks_completed: 2
  tasks_total: 2
  tests_added: 25
---

# Phase 366 Plan 01: Axis Foundation Summary

Log-scale axis unblock with validation, DateTime tick generation with nice time intervals, and per-axis custom formatter priority wiring across the SurfaceCharts axis pipeline.

## Tasks Completed

### Task 1: Unblock Log Scale + Log/DateTime Tick Generation
- Removed `throw` on `SurfaceAxisScaleKind.Log` in `SurfaceAxisDescriptor`
- Added validation: Log axis requires `minimum > 0` and `maximum > 0`
- Added `CreateLogTickValues` — produces powers-of-10 tick values using existing `ComputeNiceStep` on log-space
- Added `CreateDateTimeTickValues` — produces ticks at nice time intervals (1s, 2s, 5s, 10s, 15s, 30s, 1min, 5min, 1hr, etc.)
- Added 17 unit tests (7 Log descriptor + 10 tick generators)
- Commit: `74ef3f3`

### Task 2: DateTime Formatting + Per-Axis Formatters + Overlay Wiring
- Added `XAxisFormatter`, `YAxisFormatter`, `ZAxisFormatter` properties to `SurfaceChartOverlayOptions`
- Modified `FormatLabel` to dispatch per-axis formatters first, then `LabelFormatter`, then numeric defaults
- Added `FormatDateTimeLabel` with span-aware formatting (time-only, date+time, date-only)
- Modified `SurfaceAxisOverlayPresenter.CreateAxisState` to accept `SurfaceAxisScaleKind` and dispatch to appropriate tick generator
- Modified `CreateTicks` to format DateTime labels via `FormatDateTimeLabel`
- Updated all three `CreateAxisState` call sites to pass axis `ScaleKind`
- Added 8 integration tests
- Commit: `61660fd`

## Key Decisions

1. **ArgumentOutOfRangeException for Log validation** — More specific than ArgumentException; matches the pattern used for non-finite values
2. **Y axis defaults to Linear** — `SurfaceMetadata` doesn't carry a value-axis ScaleKind; future phases can add it
3. **DateTime tick range filtering** — Loop tolerance `+ (step * 0.5d)` can produce ticks past max; added explicit range check
4. **Span-aware DateTime formatting** — Different format based on axis span: HH:mm:ss (<1hr), MM-dd HH:mm (<1day), yyyy-MM-dd (>=1day)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] DateTime tick generator producing out-of-range values**
- **Found during:** Task 1
- **Issue:** Loop condition `tick <= axisMaximum + (step * 0.5d)` allowed ticks past the axis maximum
- **Fix:** Added `if (rounded >= axisMinimum && rounded <= axisMaximum)` filter when adding ticks
- **Files modified:** `SurfaceAxisTickGenerator.cs`
- **Commit:** `74ef3f3`

## Known Stubs

None — all implementations are fully wired and functional.

## Test Summary

| Suite | Count | Status |
|-------|-------|--------|
| LogScaleAxisDescriptorTests | 7 | ✅ All passing |
| DateTimeAxisTickGeneratorTests | 10 | ✅ All passing |
| CustomFormatterIntegrationTests | 8 | ✅ All passing |
| **Total** | **25** | ✅ |

## Self-Check: PASSED

- ✅ All 7 created/modified files exist
- ✅ Commit `74ef3f3` exists (Task 1)
- ✅ Commit `61660fd` exists (Task 2)
- ✅ 25/25 tests passing
- ✅ Both projects build with 0 errors
