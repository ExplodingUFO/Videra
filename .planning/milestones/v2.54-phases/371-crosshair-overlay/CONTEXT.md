# Phase 371: Crosshair Overlay — Context

## Goal

Users see projected ground-plane guidelines with axis coordinate values at the probe position, following mouse movement with minimal overlay overhead.

## Key Decisions

### 1. Crosshair = Projected Ground-Plane Guidelines (Not Screen-Space Lines)

Videra is a 3D chart. Crosshair lines must project onto the ground plane (XZ plane at value-range minimum), not be simple screen-space H/V lines. This follows the existing axis overlay pattern — `SurfaceAxisOverlayPresenter` already projects 3D ground-plane lines to screen coordinates.

**How it works:**
- Given a probe point in world coordinates (X, Y, Z), project two lines on the ground plane:
  - **X guideline**: from (X_min, Y_min, Z) to (X_max, Y_min, Z) — horizontal line through probe's Z
  - **Z guideline**: from (X, Y_min, Z_min) to (X, Y_min, Z_max) — vertical line through probe's X
- Use `SurfaceChartProjection.Project(Vector3)` to map 3D endpoints to screen points
- Render as dashed overlay lines via Avalonia `DrawingContext`

### 2. Axis-Value Pills at Endpoints

At the endpoints of each guideline (where they reach the chart edges), render small "pill" labels showing the axis coordinate value. This is the ScottPlot-style axis-value readout.

**Pill rendering:**
- Small rounded rectangle with semi-transparent dark background
- White text showing the formatted axis value
- Positioned at the outer endpoint of each guideline (the end farther from chart center)
- Uses `SurfaceChartOverlayOptions.FormatLabel()` for consistent formatting

### 3. Lightweight Overlay Path — Bypass Full Coordinator Rebuild

The critical performance insight from research: `InvalidateOverlay()` calls `_overlayCoordinator.Refresh()` which rebuilds ALL overlay state (axis, legend, probe). On every pointer move, this is expensive.

**Solution:** The crosshair has its own lightweight render path:
- Crosshair state is stored separately from the coordinator's `Refresh()` path
- On pointer move, only the crosshair position updates — axis/legend/probe state is NOT rebuilt
- Crosshair renders in the overlay layer's `Render()` call, but its state is updated via a separate fast path
- The coordinator's `Render()` method calls `SurfaceCrosshairOverlayPresenter.Render()` directly with the current crosshair state

**Implementation pattern:**
```csharp
// In SurfaceChartOverlayCoordinator:
public SurfaceCrosshairOverlayState CrosshairState { get; private set; } = SurfaceCrosshairOverlayState.Empty;

// Lightweight update — no full rebuild
public void UpdateCrosshairPosition(Point screenPosition, SurfaceChartProjection? projection, SurfaceChartOverlayOptions options, SurfaceMetadata? metadata)
{
    CrosshairState = SurfaceCrosshairOverlayPresenter.CreateState(screenPosition, projection, options, metadata);
}

// Render includes crosshair
public void Render(DrawingContext context, SurfaceChartProjection? chartProjection)
{
    SurfaceAxisOverlayPresenter.Render(context, AxisState);
    SurfaceLegendOverlayPresenter.Render(context, LegendState);
    SurfaceProbeOverlayPresenter.Render(context, ProbeState, _viewSize, chartProjection);
    SurfaceCrosshairOverlayPresenter.Render(context, CrosshairState); // NEW
}
```

### 4. Visibility Configurable via SurfaceChartOverlayOptions

Add `ShowCrosshair` property to `SurfaceChartOverlayOptions`:
```csharp
public bool ShowCrosshair { get; init; } = true; // Default on
```

When `ShowCrosshair` is false, the crosshair state is empty and nothing renders.

### 5. Uses Existing SurfaceChartProjection.Project()

The crosshair presenter uses the same `SurfaceChartProjection.Project(Vector3)` method that `SurfaceAxisOverlayPresenter` uses for grid lines. No new projection math needed.

### 6. Rendered in Overlay Pass via Avalonia DrawingContext

Follows the exact same pattern as axis/legend/probe presenters:
- Static `CreateState()` method builds immutable state
- Static `Render()` method draws to `DrawingContext`
- State is a sealed class with `Empty` singleton

## Architecture

### New Files

1. **`SurfaceCrosshairOverlayState.cs`** — Immutable state record
   - `SurfaceCrosshairOverlayState.Empty` static singleton
   - Properties: `IsVisible`, `XGuidelineStart/End` (screen points), `ZGuidelineStart/End`, `XValueText`, `ZValueText`, `XPillPosition`, `ZPillPosition`

2. **`SurfaceCrosshairOverlayPresenter.cs`** — Static presenter
   - `CreateState()` — projects 3D guidelines to screen, calculates pill positions
   - `Render()` — draws dashed lines and value pills

### Modified Files

1. **`SurfaceChartOverlayCoordinator.cs`**
   - Add `CrosshairState` property
   - Add `UpdateCrosshairPosition()` lightweight method
   - Add crosshair render call in `Render()`
   - Add crosshair reset in `ResetForSourceChange()`

2. **`SurfaceChartOverlayOptions.cs`**
   - Add `ShowCrosshair` property (default: true)

3. **`VideraChartView.Overlay.cs`**
   - In `UpdateProbeScreenPosition()`, also update crosshair position via lightweight path
   - In `InvalidateOverlay()`, pass metadata to coordinator for crosshair state rebuild

### Test Files

4. **`SurfaceCrosshairOverlayTests.cs`** — Integration tests
   - Crosshair follows pointer position
   - Axis-value pills show correct formatted values
   - Crosshair visibility toggle works
   - Crosshair updates without full overlay rebuild

## Success Criteria

1. User sees two projected guidelines (X + Z) on the ground plane that follow the mouse cursor in real time
2. Axis coordinate values are displayed as pills at the guideline endpoints (chart edges)
3. User can toggle crosshair visibility on/off per chart instance via `SurfaceChartOverlayOptions`
4. Crosshair updates on pointer move without triggering a full overlay coordinator rebuild

## Requirements Traceability

- **XHAIR-01**: Crosshair overlay follows mouse position showing projected ground-plane guidelines through the probe point
- **XHAIR-02**: Crosshair displays axis coordinate values at the guideline endpoints (axis-value pills)
- **XHAIR-03**: Crosshair visibility is configurable (on/off per chart instance)
- **XHAIR-04**: Crosshair uses lightweight overlay path — bypasses full overlay rebuild on pointer move
