---
phase: 372
plan: 372-PLAN
subsystem: surface-charts
tags: [overlay, tooltip, multi-series, interactivity]
requires: []
provides: [TOOL-01, TOOL-02, TOOL-03, TOOL-04]
affects: [SurfaceProbeOverlayPresenter, SurfaceProbeOverlayState, SurfaceChartOverlayCoordinator]
tech-stack:
  added: []
  patterns: [immutable-state-records, static-presenter-methods, coordinator-orchestration]
key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceTooltipSeriesEntry.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceTooltipContent.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceTooltipOverlayTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeOverlayPresenter.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs
decisions:
  - Multi-series tooltip only activates when 2+ series present (single-series uses existing readout)
  - Series name defaults to "Series {N}" when unnamed
  - TooltipOffset defaults to (12, -12) for above-right cursor positioning
  - Edge avoidance uses existing ClampBubbleOrigin with configurable offset
metrics:
  duration: ~10m
  completed: 2026-04-29
  tasks: 9
  files: 7
---

# Phase 372 Plan: Enhanced Tooltips — Summary

## One-liner
Multi-series-aware tooltip system with configurable cursor offset and edge-avoidance positioning, extending the existing probe overlay presenter pattern.

## Deliverables

### What was built:
1. **Configurable TooltipOffset** — `SurfaceChartOverlayOptions.TooltipOffset` property (default 12px right, 12px above cursor)
2. **SurfaceTooltipSeriesEntry** — Immutable record struct pairing series name/kind with probe info
3. **SurfaceTooltipContent** — Aggregated tooltip model holding shared world coordinates and per-series entries
4. **Multi-series CreateState overload** — New presenter overload resolving probes across all surface series at hovered position
5. **Multi-series tooltip rendering** — Render method displays all series values when multiple series present
6. **Edge-avoidance with offset** — CreateHoveredBubbleOrigin uses configurable offset with existing clamping

### Requirements addressed:
- **TOOL-01** ✅ Tooltip shows series name, world coordinates, and data value
- **TOOL-02** ✅ Multi-series awareness — shows values from all series at same X/Z position
- **TOOL-03** ✅ Edge-avoidance positioning via ClampBubbleOrigin
- **TOOL-04** ✅ Configurable offset from cursor via TooltipOffset property

## Key Implementation Details

### Multi-series probe resolution:
- Only activates when `series.Count > 1` (single-series uses existing readout)
- Iterates through all series with `SurfaceSource` and resolves probes at same sample position
- Uses `SurfaceProbeService.Resolve` for each series independently
- Builds `SurfaceTooltipContent` from resolved entries

### Tooltip content format:
```
X 15.00  Z 150.00
Surface 1: 5.00
Surface 2: 8.00
```

### Edge avoidance:
- Default offset: (12, -12) — positions tooltip above-right of cursor
- ClampBubbleOrigin ensures tooltip stays within chart bounds
- Configurable via `SurfaceChartOverlayOptions.TooltipOffset`

## Testing

### Tests added:
- `SurfaceTooltipOverlayTests.cs` — 11 tests covering:
  - Multi-series probe resolution
  - TooltipContent aggregation and null handling
  - TooltipOffset default and custom values
  - Backward compatibility with single-series
  - Series kind preservation

### Test results:
- All 11 new tests pass
- Existing probe overlay tests pass (no regression)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Used viewport-based CreateState for simpler testing**
- **Found during:** Task 9
- **Issue:** Camera-frame-based CreateState requires complex setup for multi-series testing
- **Fix:** Used viewport-based overload for simpler test cases, camera-frame overload tested via coordinator integration
- **Files modified:** SurfaceTooltipOverlayTests.cs
- **Commit:** 84be60f

## Known Stubs

None — all tooltip functionality is fully wired and operational.

## Build Notes

There is a pre-existing build error in `VideraChartView.Input.cs` referencing `SurfaceChartToolbarAction` from Phase 374 (Keyboard & Toolbar Controls). This is not caused by Phase 372 changes. The tooltip library code compiles and tests pass independently.

## Self-Check: PASSED

- [x] SurfaceTooltipSeriesEntry.cs exists
- [x] SurfaceTooltipContent.cs exists
- [x] SurfaceTooltipOverlayTests.cs exists
- [x] TooltipOffset property added to SurfaceChartOverlayOptions
- [x] TooltipContent property added to SurfaceProbeOverlayState
- [x] Multi-series CreateState overload exists
- [x] All 11 tooltip tests pass
- [x] Git commits created for all tasks
