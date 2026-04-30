# Phase 374: Keyboard & Toolbar Controls — Context

## Goal
Users can control zoom, pan, and camera reset via keyboard shortcuts and on-chart toolbar buttons, with cursor feedback during interactions.

## Requirements
- **KB-01**: Keyboard zoom in/out with +/- keys
- **KB-02**: Keyboard pan with arrow keys (left/right/up/down)
- **KB-03**: Keyboard camera reset with Home key, fit-to-data with F key
- **KB-04**: Zoom/pan toolbar buttons rendered as overlay controls on the chart

## Key Decisions

### Keyboard Shortcuts
- `+` / `=`: Zoom in (dolly in)
- `-`: Zoom out (dolly out)
- `ArrowLeft` / `ArrowRight` / `ArrowUp` / `ArrowDown`: Pan in respective direction
- `Home`: Reset camera to default pose
- `F`: Fit to data (reset data window to full bounds)
- Focus-gated: Only active when VideraChartView has keyboard focus
- Uses Avalonia's `OnKeyDown` handler in `VideraChartView.Input.cs`

### Toolbar
- Overlay-rendered buttons (not real Avalonia controls) for consistency with existing overlay pattern
- Buttons: Zoom In (+), Zoom Out (-), Reset Camera (⌂), Fit to Data (⊞)
- Positioned bottom-right corner, vertically stacked
- Follows `SurfaceChartOverlayCoordinator` / presenter pattern
- Hit-testing via `_overlayLayer.IsHitTestVisible = true` (already set)
- Toolbar has its own state record and render method

### Cursor Feedback
- Default: Arrow cursor
- Hover over chart: Cross cursor
- During orbit (left-drag): SizeAll cursor
- During pan (right-drag): SizeAll cursor
- During zoom (scroll/+/-): ZoomIn/ZoomOut cursor feedback
- Uses Avalonia's `Cursor` property on VideraChartView

### Architecture
- Extends `VideraChartView.Input.cs` with `OnKeyDown`/`OnKeyUp` handlers
- New `SurfaceChartToolbarOverlayState` + `SurfaceChartToolbarOverlayPresenter`
- Toolbar state added to `SurfaceChartOverlayCoordinator`
- Cursor updates via `_interactionController` gesture state changes
- All features are presentation-layer — no Core changes needed
