---
phase: 374
plan: 374-PLAN
type: auto
autonomous: true
wave: 4
depends_on: []
requirements: [KB-01, KB-02, KB-03, KB-04]
---

# Phase 374: Keyboard & Toolbar Controls — Plan

## Objective
Users can control zoom, pan, and camera reset via keyboard shortcuts and on-chart toolbar buttons, with cursor feedback during interactions.

## Context
- `VideraChartView` is an Avalonia `Decorator` hosting a Grid with native host + overlay layer
- `VideraChartView.Input.cs` already handles pointer events via `SurfaceChartInteractionController`
- `SurfaceChartRuntime` exposes `FitToData()`, `ResetCamera()`, `ZoomTo()`, and `BeginInteraction()`
- `SurfaceChartOverlayCoordinator` manages axis, legend, and probe overlay states
- `_overlayLayer.IsHitTestVisible = true` is already set
- Toolbar buttons follow the overlay presenter pattern (state record + static CreateState/Render)

## Tasks

### Task 1: Add keyboard shortcut handler to VideraChartView.Input.cs
**Type:** auto
**KB-01, KB-02, KB-03**

Add `OnKeyDown` override to `VideraChartView.Input.cs`:
- `+` / `=` → zoom in (apply dolly with positive delta)
- `-` → zoom out (apply dolly with negative delta)
- `ArrowLeft` → pan left
- `ArrowRight` → pan right
- `ArrowUp` → pan up
- `ArrowDown` → pan down
- `Home` → reset camera (`_runtime.ResetCamera()`)
- `F` → fit to data (`_runtime.FitToData()`)
- Gate on `_runtime.CanInteract` to prevent action without data
- Mark `e.Handled = true` for handled keys
- Pan step: 5% of current data window per keypress
- Zoom step: equivalent to one wheel click (WheelZoomStepFactor = 0.85)
- After each action: `SynchronizeViewStateProperties` + `InvalidateOverlay`

**Verification:**
- Code compiles without errors
- Keys are only processed when chart has focus and data is loaded

**Commit:** `feat(374-PLAN): add keyboard zoom, pan, and reset shortcuts`

---

### Task 2: Add toolbar overlay state and presenter
**Type:** auto
**KB-04**

Create `SurfaceChartToolbarOverlayState` record:
- `IsVisible` (bool) — toolbar visibility
- `Buttons` (list of toolbar button descriptors: icon, action, screen rect)
- `ViewSize` (Size) — for hit-testing
- Static `Empty` instance

Create `SurfaceChartToolbarOverlayPresenter`:
- `CreateState(viewSize, overlayOptions)` — compute button positions (bottom-right, vertical stack)
- `Render(context, state)` — draw toolbar buttons as rounded rectangles with icons
- Buttons: Zoom In (+), Zoom Out (−), Reset (⌂), Fit (⊞)
- Each button is a small rounded rect (28×28) with text icon
- Hover highlight via hit-test on pointer position
- Actions delegate to runtime methods

**Verification:**
- State record compiles
- Presenter renders buttons at correct positions

**Commit:** `feat(374-PLAN): add toolbar overlay state and presenter`

---

### Task 3: Wire toolbar into overlay coordinator and VideraChartView
**Type:** auto
**KB-04**

Update `SurfaceChartOverlayCoordinator`:
- Add `ToolbarState` property
- Update `Refresh()` to call `SurfaceChartToolbarOverlayPresenter.CreateState()`
- Update `Render()` to call `SurfaceChartToolbarOverlayPresenter.Render()`
- Update `ResetForSourceChange()` to reset toolbar state

Update `VideraChartView.Overlay.cs`:
- Pass toolbar overlay options to coordinator refresh

Add toolbar click handling in `VideraChartView.Input.cs`:
- On `OnPointerPressed`, check if click is within toolbar button rect
- If toolbar button hit: execute corresponding action, mark handled
- Use `_overlayCoordinator.ToolbarState` for hit-testing

**Verification:**
- Toolbar renders on chart
- Clicking toolbar buttons triggers correct actions

**Commit:** `feat(374-PLAN): wire toolbar into overlay coordinator and click handling`

---

### Task 4: Add cursor feedback during interactions
**Type:** auto
**KB-01, KB-02, KB-03**

Add cursor management to `VideraChartView.Input.cs`:
- `OnPointerEntered`: Set `Cursor = new Cursor(StandardCursorType.Cross)`
- `OnPointerExited`: Set `Cursor = Cursors.Default`
- On gesture start (orbit/pan): Set `Cursor = new Cursor(StandardCursorType.SizeAll)`
- On gesture end: Restore appropriate cursor
- During keyboard zoom: brief cursor change to `StandardCursorType.ZoomIn`/`ZoomOut`
- After 300ms settle, restore default cursor

Add cursor update hooks in `SurfaceChartInteractionController`:
- Expose `ActiveGestureMode` property (already has `_gestureMode`)
- VideraChartView checks gesture mode on pointer events

**Verification:**
- Cursor changes on hover
- Cursor changes during drag
- Cursor restores after gesture

**Commit:** `feat(374-PLAN): add cursor feedback during hover, drag, and zoom`

---

### Task 5: Add unit tests for keyboard shortcuts and toolbar
**Type:** auto
**KB-01, KB-02, KB-03, KB-04**

Create test file `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewKeyboardToolbarTests.cs`:
- Test keyboard zoom in/out changes data window
- Test keyboard pan changes data window start positions
- Test Home key resets camera
- Test F key fits to data
- Test toolbar button hit detection
- Test cursor changes on gesture
- Use `AvaloniaHeadlessTestSession.Run()` pattern from existing tests
- Use `SurfaceChartTestHelpers` for loading surface data

**Verification:**
- All tests compile and pass
- `dotnet test` for the integration test project passes

**Commit:** `test(374-PLAN): add keyboard, toolbar, and cursor feedback tests`

---

## Verification / Success Criteria

1. User can zoom in/out with +/- keys when chart has focus
2. User can pan with arrow keys
3. User can reset camera with Home and fit to data with F
4. Toolbar buttons render on chart and respond to clicks
5. Cursor changes during hover, drag, and zoom operations
6. All existing tests still pass (no regression)
7. New tests pass

## Output Spec

- Modified: `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs`
- New: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartToolbarOverlayState.cs`
- New: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartToolbarOverlayPresenter.cs`
- Modified: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs`
- New: `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/VideraChartViewKeyboardToolbarTests.cs`
