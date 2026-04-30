---
phase: 371-crosshair-overlay
plan: crosshair-overlay
subsystem: ui
tags: [avalonia, overlay, crosshair, 3d-chart, projection]

# Dependency graph
requires:
  - phase: existing infrastructure
    provides: SurfaceChartOverlayCoordinator, SurfaceChartProjection, VideraChartView
provides:
  - Crosshair overlay with projected ground-plane guidelines
  - Axis-value pills at guideline endpoints
  - Configurable crosshair visibility via SurfaceChartOverlayOptions
  - Lightweight crosshair update path bypassing full overlay rebuild
affects: [372-enhanced-tooltips, 373-series-probe-strategies, 375-integration-verification]

# Tech tracking
tech-stack:
  added: []
  patterns: [crosshair-overlay-presenter, lightweight-overlay-update]

key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayPresenter.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceCrosshairOverlayTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs

key-decisions:
  - "Crosshair uses projected ground-plane guidelines (not screen-space H/V lines) because Videra is 3D"
  - "Lightweight update path: UpdateCrosshairPosition() bypasses full coordinator Refresh() for pointer moves"
  - "Axis-value pills positioned at outer endpoints (farther from projected center) following ScottPlot pattern"
  - "Default crosshair visibility is ON — users see crosshair immediately on hover"

patterns-established:
  - "Crosshair presenter pattern: static CreateState() + Render() following existing axis/legend/probe presenters"
  - "Lightweight overlay update: separate state update method that bypasses full coordinator rebuild"

requirements-completed: [XHAIR-01, XHAIR-02, XHAIR-03, XHAIR-04]

# Metrics
duration: 15min
completed: 2026-04-29
---

# Phase 371: Crosshair Overlay Summary

**Projected ground-plane crosshair guidelines with axis-value pills following mouse position via lightweight overlay path**

## Performance

- **Duration:** 15 min
- **Started:** 2026-04-29T13:37:49Z
- **Completed:** 2026-04-29T13:53:00Z
- **Tasks:** 6
- **Files modified:** 6

## Accomplishments

- Crosshair overlay renders two projected ground-plane guidelines (X + Z) through the probe point
- Axis-value pills display formatted coordinates at guideline endpoints (chart edges)
- Crosshair visibility configurable per chart instance via `SurfaceChartOverlayOptions.ShowCrosshair`
- Lightweight update path established — `UpdateCrosshairPosition()` bypasses full overlay coordinator rebuild
- 6 integration tests covering position tracking, pill rendering, visibility toggle, and custom formatters
- All 48 overlay tests pass (6 new + 42 existing) with zero regressions

## Task Commits

Each task was committed atomically:

1. **Task 1: Add ShowCrosshair to SurfaceChartOverlayOptions** - `739cdb8` (feat)
2. **Task 2: Create SurfaceCrosshairOverlayState** - `f2df4ef` (feat)
3. **Task 3: Create SurfaceCrosshairOverlayPresenter** - `f20a9af` (feat)
4. **Task 4: Wire Crosshair into SurfaceChartOverlayCoordinator** - `9acd90d` (feat)
5. **Task 5: Wire Crosshair into VideraChartView.Overlay.cs** - `41ba3c8` (feat)
6. **Task 6: Add Crosshair Integration Tests** - `3f14411` (test)

**Warning fix:** `81d7d00` (fix: remove unused parameters)

## Files Created/Modified

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs` - Added `ShowCrosshair` property (default: true)
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayState.cs` - Immutable state for crosshair guidelines and pills
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayPresenter.cs` - Static presenter with CreateState() and Render()
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs` - Added CrosshairState, UpdateCrosshairPosition(), render call
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs` - Wired lightweight crosshair update path
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceCrosshairOverlayTests.cs` - 6 integration tests

## Decisions Made

- **Projected ground-plane guidelines:** Crosshair lines project onto the XZ ground plane (Y = value-range minimum), not screen-space H/V lines. This matches Videra's 3D nature and follows the existing axis overlay pattern.
- **Lightweight update path:** `UpdateCrosshairPosition()` updates crosshair state independently of the full `Refresh()` cycle. This establishes the pattern for future probe debouncing optimization.
- **Pill positioning:** Axis-value pills are positioned at the outer endpoint of each guideline (farther from projected center), following the ScottPlot axis-value readout pattern.
- **Default visibility ON:** Crosshair defaults to visible so users see it immediately on hover. Can be toggled off per chart instance.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Removed unused parameters in ResolveProbeWorldCoordinates**
- **Found during:** Task 3 (Create SurfaceCrosshairOverlayPresenter)
- **Issue:** `metadata` and `yMin` parameters were unused, causing compiler warnings
- **Fix:** Removed unused parameters from method signature
- **Files modified:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayPresenter.cs`
- **Verification:** Build passes with 0 warnings for crosshair files
- **Committed in:** `81d7d00`

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Minor cleanup — no scope creep.

## Issues Encountered

None — plan executed smoothly.

## Known Stubs

None — all crosshair functionality is fully wired and operational.

## User Setup Required

None — no external service configuration required.

## Next Phase Readiness

- Crosshair overlay complete and tested, ready for Phase 372 (Enhanced Tooltips)
- Lightweight overlay update pattern established for future phases
- All existing overlay tests pass — no regressions

---
*Phase: 371-crosshair-overlay*
*Completed: 2026-04-29*
