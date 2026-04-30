---
phase: 371
plan: crosshair-overlay
type: autonomous
depends_on: []
requirements: [XHAIR-01, XHAIR-02, XHAIR-03, XHAIR-04]
---

# Phase 371 Plan: Crosshair Overlay

## Objective

Add a crosshair overlay to VideraChartView that renders two projected ground-plane guidelines (X + Z) through the probe point, with axis-value pills at the guideline endpoints. The crosshair follows mouse movement via a lightweight overlay path that bypasses full overlay coordinator rebuilds.

## Context

- @CONTEXT.md — Key architectural decisions for crosshair implementation
- @.planning/research/SUMMARY.md — Research findings on overlay architecture
- Existing overlay pattern: `SurfaceAxisOverlayPresenter` / `SurfaceProbeOverlayPresenter` — static `CreateState()` + `Render()` methods
- `SurfaceChartProjection.Project(Vector3)` handles 3D→2D mapping
- `SurfaceChartOverlayCoordinator` orchestrates all overlay presenters
- `VideraChartView.Overlay.cs` handles overlay invalidation and rendering

## Tasks

### Task 1: Add ShowCrosshair to SurfaceChartOverlayOptions

**Type:** auto
**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs`

Add a `ShowCrosshair` property to control crosshair visibility per chart instance:

```csharp
/// <summary>
/// Gets a value indicating whether the crosshair overlay is rendered.
/// </summary>
public bool ShowCrosshair { get; init; } = true;
```

**Verification:** Property compiles, default is `true`.

**Done criteria:** `SurfaceChartOverlayOptions.ShowCrosshair` exists with correct default.

---

### Task 2: Create SurfaceCrosshairOverlayState

**Type:** auto
**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayState.cs` (new)

Create the immutable state class following the existing pattern (`SurfaceProbeOverlayState`, `SurfaceAxisOverlayState`):

```csharp
internal sealed class SurfaceCrosshairOverlayState
{
    public static SurfaceCrosshairOverlayState Empty { get; } = new(...);

    // Screen-space endpoints for the two projected guidelines
    public Point XGuidelineStart { get; }
    public Point XGuidelineEnd { get; }
    public Point ZGuidelineStart { get; }
    public Point ZGuidelineEnd { get; }

    // Axis value text for the pills
    public string XValueText { get; }
    public string ZValueText { get; }

    // Pill positions (at the outer endpoint of each guideline)
    public Point XPillPosition { get; }
    public Point ZPillPosition { get; }

    public bool IsVisible { get; }
}
```

**Verification:** Compiles, `Empty` singleton works.

**Done criteria:** State class exists with all properties and `Empty` singleton.

---

### Task 3: Create SurfaceCrosshairOverlayPresenter

**Type:** auto
**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayPresenter.cs` (new)

Create the static presenter following the existing pattern:

**`CreateState()` method:**
1. If `overlayOptions.ShowCrosshair` is false or projection is null, return `SurfaceCrosshairOverlayState.Empty`
2. Get the probe's world-space X and Z from the hovered probe info, or from the screen position via inverse projection
3. Project X guideline: `Project(new Vector3(xMin, yMin, probeZ))` to `Project(new Vector3(xMax, yMin, probeZ))`
4. Project Z guideline: `Project(new Vector3(probeX, yMin, zMin))` to `Project(new Vector3(probeX, yMin, zMax))`
5. Determine outer endpoints (farther from projected center) for pill positioning
6. Format axis values using `overlayOptions.FormatLabel()`

**`Render()` method:**
1. If not visible, return
2. Draw dashed lines for X and Z guidelines
3. Draw value pills (rounded rect + text) at outer endpoints

**Line style:** Dashed, semi-transparent white (`Color.FromArgb(160, 255, 255, 255)`), thickness 1.0

**Pill style:** Rounded rect with dark semi-transparent background (`Color.FromArgb(200, 30, 30, 30)`), white text, Consolas 11px

**Verification:** Compiles, renders correctly.

**Done criteria:** Presenter exists with `CreateState()` and `Render()` methods.

---

### Task 4: Wire Crosshair into SurfaceChartOverlayCoordinator

**Type:** auto
**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs`

Add crosshair state management to the coordinator:

1. Add `CrosshairState` property (default: `SurfaceCrosshairOverlayState.Empty`)
2. Add `UpdateCrosshairPosition(Point screenPosition, SurfaceChartProjection? projection, SurfaceChartOverlayOptions overlayOptions, SurfaceMetadata? metadata)` — lightweight method that only updates crosshair state
3. In `ResetForSourceChange()`, reset `CrosshairState` to `Empty`
4. In `Render()`, add `SurfaceCrosshairOverlayPresenter.Render(context, CrosshairState)` call

**Verification:** Compiles, crosshair state updates independently of full `Refresh()`.

**Done criteria:** Coordinator has crosshair state, lightweight update method, and render call.

---

### Task 5: Wire Crosshair into VideraChartView.Overlay.cs

**Type:** auto
**Files:** `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs`

Update the chart view to use the lightweight crosshair path:

1. In `UpdateProbeScreenPosition()`, after the coordinator position update, also call `_overlayCoordinator.UpdateCrosshairPosition()` with the current projection and metadata. This is the **lightweight path** — no full `InvalidateOverlay()` for crosshair-only updates.
2. Modify the approach: `UpdateProbeScreenPosition` should NOT call `InvalidateOverlay()` for crosshair-only moves. Instead, it should update crosshair state directly and only invalidate the overlay visual (not rebuild state).

**Key insight:** The current `UpdateProbeScreenPosition()` calls `InvalidateOverlay()` which rebuilds ALL overlay state. We need to split this:
- Crosshair position update → lightweight path (just update crosshair state + invalidate visual)
- Probe position update → still triggers `InvalidateOverlay()` for probe resolution

**Implementation:**
```csharp
internal bool UpdateProbeScreenPosition(Point probeScreenPosition)
{
    if (!_overlayCoordinator.UpdateProbeScreenPosition(probeScreenPosition))
    {
        return false;
    }

    // Lightweight crosshair update — bypasses full overlay rebuild
    _overlayCoordinator.UpdateCrosshairPosition(
        probeScreenPosition,
        _chartProjection,
        Plot.OverlayOptions,
        _runtime.Source?.Metadata);

    // Full rebuild for probe resolution (axis/legend/probe state)
    InvalidateOverlay();
    return true;
}
```

**Note:** For now, we keep `InvalidateOverlay()` for probe resolution. The lightweight crosshair path is established for future optimization where probe resolution can be debounced.

**Verification:** Compiles, crosshair follows pointer.

**Done criteria:** Crosshair updates on pointer move, uses lightweight path.

---

### Task 6: Add Crosshair Integration Tests

**Type:** auto
**Files:** `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceCrosshairOverlayTests.cs` (new)

Create integration tests following the existing pattern in `SurfaceAxisOverlayTests.cs`:

1. **Crosshair_FollowsPointerPosition** — Move pointer, verify crosshair guideline endpoints change
2. **Crosshair_DisplaysAxisValuePills** — Verify pill text contains formatted X and Z values
3. **Crosshair_CanBeToggledOff** — Set `ShowCrosshair = false`, verify state is empty
4. **Crosshair_UpdatesWithoutFullRebuild** — Verify crosshair state updates independently

**Verification:** All tests pass.

**Done criteria:** Tests exist and pass.

---

## Verification / Success Criteria

1. User sees two projected guidelines (X + Z) on the ground plane that follow the mouse cursor in real time
2. Axis coordinate values are displayed as pills at the guideline endpoints (chart edges)
3. User can toggle crosshair visibility on/off per chart instance via `SurfaceChartOverlayOptions`
4. Crosshair updates on pointer move without triggering a full overlay coordinator rebuild

## Output

- `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs` (modified)
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayState.cs` (new)
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceCrosshairOverlayPresenter.cs` (new)
- `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartOverlayCoordinator.cs` (modified)
- `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs` (modified)
- `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/SurfaceCrosshairOverlayTests.cs` (new)
