---
phase: 374
plan: 374-PLAN
subsystem: SurfaceCharts.Avalonia
tags: [keyboard, toolbar, cursor, overlay, interaction]
requirements: [KB-01, KB-02, KB-03, KB-04]
depends_on: []
provides: [keyboard-shortcuts, toolbar-overlay, cursor-feedback]
affects: [VideraChartView.Input, SurfaceChartOverlayCoordinator, SurfaceChartInteractionController]
tech-stack:
  added: []
  patterns: [overlay-presenter, keyboard-handler, toolbar-hit-test]
key-files:
  created:
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartToolbarOverlayState.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartToolbarOverlayPresenter.cs
    - tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewKeyboardToolbarTests.cs
  modified:
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs
    - src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs
decisions:
  - Keyboard shortcuts use Avalonia OnKeyDown, focus-gated to chart with data
  - Toolbar rendered as overlay presenter (not real Avalonia controls) for consistency
  - Toolbar positioned bottom-right with vertical button stack
  - Cursor feedback differentiated by gesture type (orbit/pan/focus)
  - Keyboard zoom/pan uses same dolly/pan math as mouse interactions
metrics:
  duration: ~10 minutes
  completed: 2026-04-29
  tasks: 5
  files: 7
  tests: 12
---

# Phase 374: Keyboard & Toolbar Controls — Summary

Keyboard shortcuts (+/- zoom, arrow pan, Home reset, F fit-to-data) and overlay toolbar buttons with gesture-aware cursor feedback.

## Tasks Completed

| Task | Name | Commit | Key Changes |
|------|------|--------|-------------|
| 1 | Keyboard shortcut handler | `cba16ee` (previous agent) | OnKeyDown with zoom/pan/reset/fit keys |
| 2 | Toolbar overlay state & presenter | `cba16ee` (previous agent) | SurfaceChartToolbarOverlayState + Presenter |
| 3 | Wire toolbar into coordinator | `cba16ee` | Coordinator integration, pointer tracking, click handling |
| 4 | Cursor feedback | `c28ff16` | ActiveGestureMode, gesture-aware cursor |
| 5 | Tests | `36e861b` | 12 tests covering all features |

## What Was Built

### Keyboard Shortcuts (KB-01, KB-02, KB-03)
- `+`/`=`: Zoom in (dolly in)
- `-`: Zoom out (dolly out)
- Arrow keys: Pan in respective direction (5% window step)
- `Home`: Reset camera to default pose
- `F`: Fit to data (full bounds)
- Focus-gated: Only active when chart has data loaded
- Uses same dolly/pan math as mouse wheel and drag

### Toolbar Overlay (KB-04)
- 4 buttons: Zoom In (+), Zoom Out (−), Reset Camera (⌂), Fit to Data (⊞)
- Positioned bottom-right corner, vertically stacked
- Rounded rect buttons with hover highlight
- Tooltip appears on hover
- Hit-test for click detection
- Follows overlay presenter pattern (state record + static CreateState/Render)

### Cursor Feedback
- Pointer enter: Shows cursor appropriate for current state
- Pointer exit: Restores default cursor
- During orbit: DragCopy cursor
- During pan: SizeAll cursor
- During focus selection: Cross cursor
- Keyboard zoom/pan: Brief cross/size-all flash, then auto-restore

## Decisions Made

1. **Toolbar as overlay (not real controls)**: Follows existing overlay presenter pattern for consistency. Accessibility can be added in a future milestone.

2. **Keyboard zoom/pan reuses interaction math**: Same `WheelZoomStepFactor` (0.85) and pan ratio (5% window) ensures consistent behavior between mouse and keyboard.

3. **Gesture-aware cursor**: Different cursors for orbit vs pan vs focus selection provide clear visual feedback about the active interaction mode.

4. **Toolbar pointer tracking**: `UpdatePointerPosition` added to coordinator to support hover highlighting without triggering full probe rebuild.

## Verification

- [x] All 12 new tests pass
- [x] All 189 integration tests pass (no regression)
- [x] Keyboard shortcuts only active when chart has data
- [x] Toolbar renders and responds to clicks
- [x] Cursor changes during interactions

## Known Stubs

None — all features are fully wired and functional.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Avalonia cursor API mismatch**
- **Found during:** Task 1
- **Issue:** `Cursors.Default` doesn't exist in Avalonia 11 (use `Cursor.Default`); `StandardCursorType.ZoomIn`/`ZoomOut` don't exist
- **Fix:** Changed to `Cursor.Default` and `StandardCursorType.Cross` for zoom feedback
- **Files modified:** `VideraChartView.Input.cs`
- **Commit:** included in task 1 commit

**2. [Rule 3 - Blocking] Missing Overlay namespace import**
- **Found during:** Task 3
- **Issue:** `SurfaceChartToolbarOverlayPresenter` and `SurfaceChartToolbarAction` not accessible without Overlay namespace
- **Fix:** Added `using Videra.SurfaceCharts.Avalonia.Controls.Overlay;`
- **Files modified:** `VideraChartView.Input.cs`
- **Commit:** included in task 3 commit
