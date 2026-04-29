# v2.54 Chart Interactivity — Stack Research

**Researched:** 2026-04-29
**Domain:** Chart interactivity — crosshair, tooltips, mouse-driven probe, zoom/pan controls
**Confidence:** HIGH

## Summary

Videra's SurfaceCharts module already has a robust interaction foundation: `VideraChartView` handles pointer events, `SurfaceChartInteractionController` manages orbit/pan/dolly/focus gestures, `SurfaceProbeService` resolves probes via ray picking, and `SurfaceProbeOverlayPresenter` renders readout bubbles. The v2.54 milestone needs to enhance this foundation with crosshair lines, improved tooltips, better mouse-driven probe UX, and zoom/pan UI controls.

**No new NuGet packages are needed.** All required capabilities exist in the current Avalonia 11.3.9 + SkiaSharp stack. The crosshair, tooltip, and toolbar are chart-local overlays rendered via the existing `SurfaceChartOverlayLayer` using Avalonia's `DrawingContext`. Keyboard shortcuts use Avalonia's built-in `OnKeyDown` handler. Zoom/pan buttons are lightweight Avalonia `Control` elements added to the chart's `_hostContainer` Grid.

**Primary recommendation:** Build all new interactivity features as chart-local overlay extensions to the existing `SurfaceChartOverlayCoordinator` / `SurfaceProbeOverlayPresenter` pattern. No backend, renderer, or Core changes are needed.

## Standard Stack

### Core (No Changes)

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Avalonia | 11.3.9 | UI framework, input events, overlay rendering | Already installed; provides `DrawingContext`, `PointerEventArgs`, `OnKeyDown` |
| SkiaSharp | (bundled with Avalonia) | 2D drawing primitives for crosshair/tooltips | Already available via Avalonia's `DrawingContext` |
| System.Numerics | (BCL) | Vector2/Vector3 for projection math | Already used throughout SurfaceCharts.Core |

### Supporting (No New Packages)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Avalonia.Controls.Primitives | (bundled) | `ToolTip`, `Popup` for enhanced tooltips | If using Avalonia-native tooltips instead of custom-drawn bubbles |
| Avalonia.Input | (bundled) | `KeyGesture`, `KeyBinding` for keyboard shortcuts | For zoom/pan keyboard accelerators |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Custom-drawn crosshair in overlay | Avalonia `Canvas` with `Line` elements | More Avalonia-native but harder to synchronize with 3D projection; overlay approach is simpler |
| Avalonia `ToolTip` | Enhanced `SurfaceProbeOverlayPresenter` bubbles | Avalonia ToolTip is 2D-only; custom bubbles integrate with 3D projection and can show axis-projected data |
| Separate toolbar UserControl | Inline buttons in `_hostContainer` Grid | Separate control adds complexity; inline buttons are simpler for chart-local scope |

**Installation:** No new packages to install.

**Version verification:**
```
Avalonia 11.3.9 — verified via `dotnet list package` output
```

## Architecture Patterns

### Existing Integration Points

The following existing classes are the primary integration points for v2.54 features:

```
src/Videra.SurfaceCharts.Avalonia/Controls/
├── VideraChartView.Input.cs          ← Add OnKeyDown, improve pointer tracking
├── VideraChartView.Overlay.cs        ← Add crosshair rendering call
├── VideraChartView.Rendering.cs      ← Add toolbar buttons to _hostContainer
├── VideraChartView.Core.cs           ← Add CrosshairVisible, ToolbarVisible properties
├── VideraChartView.Properties.cs     ← Add styled properties for crosshair/toolbar
├── Interaction/
│   └── SurfaceChartInteractionController.cs  ← Add keyboard gesture handling
└── Overlay/
    ├── SurfaceChartOverlayCoordinator.cs     ← Add crosshair state management
    ├── SurfaceProbeOverlayPresenter.cs       ← Enhance tooltip rendering
    └── (new) SurfaceCrosshairOverlayPresenter.cs  ← Crosshair line rendering
```

### Pattern 1: Crosshair Overlay

**What:** Horizontal and vertical lines at cursor position, rendered in the overlay layer
**When to use:** When the user moves the pointer over the chart and crosshair is enabled
**Example (ScottPlot-inspired pattern):**
```csharp
// New file: SurfaceCrosshairOverlayPresenter.cs
// Renders horizontal + vertical lines at screen position using DrawingContext
internal static class SurfaceCrosshairOverlayPresenter
{
    private static readonly IBrush CrosshairBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
    private static readonly Pen CrosshairPen = new(CrosshairBrush, thickness: 1d, dashStyle: DashStyle.Dash);

    public static void Render(DrawingContext context, Point screenPosition, Size viewSize)
    {
        // Horizontal line
        context.DrawLine(CrosshairPen, new Point(0, screenPosition.Y), new Point(viewSize.Width, screenPosition.Y));
        // Vertical line
        context.DrawLine(CrosshairPen, new Point(screenPosition.X, 0), new Point(screenPosition.X, viewSize.Height));
    }
}
```

### Pattern 2: Enhanced Tooltip/Readout

**What:** Richer probe readout bubble with axis-value readouts on crosshair lines
**When to use:** When hovering over the chart with crosshair enabled
**Example:**
```csharp
// Enhanced SurfaceProbeOverlayPresenter.Render adds axis-label pills at crosshair edges
// Shows "X: 123.45" at right edge, "Z: 67.89" at top edge, "Value: 42.0" at probe position
```

### Pattern 3: Keyboard Shortcuts

**What:** Keyboard-driven zoom/pan/reset actions
**When to use:** When the chart has focus and the user presses keyboard shortcuts
**Example:**
```csharp
// In VideraChartView.Input.cs
protected override void OnKeyDown(KeyEventArgs e)
{
    switch (e.Key)
    {
        case Key.F: FitToData(); e.Handled = true; break;
        case Key.R: ResetCamera(); e.Handled = true; break;
        case Key.Add or Key.OemPlus: ZoomIn(); e.Handled = true; break;
        case Key.Subtract or Key.OemMinus: ZoomOut(); e.Handled = true; break;
        case Key.Left: PanLeft(); e.Handled = true; break;
        case Key.Right: PanRight(); e.Handled = true; break;
        case Key.Up: PanUp(); e.Handled = true; break;
        case Key.Down: PanDown(); e.Handled = true; break;
    }
}
```

### Pattern 4: Zoom/Pan Toolbar Buttons

**What:** Small overlay buttons for zoom in/out, fit-to-data, reset camera
**When to use:** Always visible in chart corner (configurable)
**Example:**
```csharp
// Rendered in SurfaceChartOverlayLayer.Render as clickable regions
// Uses DrawingContext.DrawRectangle + DrawText for button appearance
// Hit-testing done via OnPointerPressed with bounds checking
```

### Anti-Patterns to Avoid

- **Don't create a separate Avalonia UserControl for the toolbar.** Use the existing overlay layer's `DrawingContext` rendering for consistency with axis/legend/probe overlays.
- **Don't use Avalonia `ToolTip` for probe readouts.** The existing custom-drawn bubbles integrate with 3D projection and can show axis-projected data. Avalonia ToolTip is 2D-only.
- **Don't add new NuGet packages.** Everything needed is in Avalonia 11.3.9 + SkiaSharp.
- **Don't modify the render backend.** All interactivity features are overlay-only, rendered on top of the 3D scene.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Dash patterns | Custom dash drawing | `DashStyle.Dash` / `DashStyle.Dot` from Avalonia | Built-in, consistent across platforms |
| Keyboard key detection | Custom key mapping | `Avalonia.Input.Key` enum + `OnKeyDown` | Built-in, handles platform differences |
| Button hit testing | Custom hit-test math | Track button bounds in overlay state, check in `OnPointerPressed` | Simple bounds check, no framework needed |
| Text measurement | Custom text width calculation | `FormattedText.Width` / `FormattedText.Height` | Already used in `SurfaceProbeOverlayPresenter` |

**Key insight:** All v2.54 features are **presentation-layer additions** to the existing overlay system. No data processing, rendering backend, or Core contract changes are needed.

## Common Pitfalls

### Pitfall 1: Crosshair Lines Outside Chart Bounds
**What goes wrong:** Crosshair lines extend beyond the chart area when pointer is near edges
**Why it happens:** Drawing without clamping to chart bounds
**How to avoid:** Always clip crosshair rendering to `viewSize` bounds
**Warning signs:** Lines visible outside chart area in demo

### Pitfall 2: Probe Readout Disappears During Interaction
**What goes wrong:** Tooltip vanishes when user starts dragging (orbit/pan)
**Why it happens:** `OnPointerMoved` is consumed by gesture controller during active drag
**How to avoid:** Continue showing crosshair/tooltip during gestures; only suppress probe resolution when gesture is active
**Warning signs:** Tooltip flickers during drag operations

### Pitfall 3: Keyboard Shortcuts Conflict with Host Application
**What goes wrong:** Chart captures keyboard events meant for the host application
**Why it happens:** `OnKeyDown` handles keys without checking focus state
**How to require focus:** Only handle keyboard shortcuts when `IsFocused` is true; use `Focusable = true` on the chart control
**Warning signs:** Host application can't receive keyboard input when chart is visible

### Pitfall 4: Toolbar Buttons Not Accessible
**What goes wrong:** Custom-drawn buttons have no accessibility support
**Why it happens:** Overlay-rendered buttons are not real Avalonia controls
**How to avoid:** Add `AutomationProperties` or use real Avalonia `Button` controls for the toolbar
**Warning signs:** Screen readers can't identify toolbar buttons

### Pitfall 5: Crosshair Performance on Large Datasets
**What goes wrong:** Crosshair rendering causes frame drops during rapid mouse movement
**Why it happens:** Probe resolution triggers expensive ray picking on every mouse move
**How to avoid:** Throttle probe resolution to 60fps max; reuse last probe result when pointer moves less than 1px
**Warning signs:** Visible lag when moving mouse quickly over chart

## Code Examples

### Crosshair State Model

```csharp
// New file: SurfaceCrosshairOverlayState.cs
internal readonly record struct SurfaceCrosshairOverlayState(
    bool IsVisible,
    Point ScreenPosition,
    double AxisXValue,
    double AxisZValue,
    string? AxisXFormatted,
    string? AxisZFormatted);
```

### Crosshair Presenter Render

```csharp
// In SurfaceCrosshairOverlayPresenter.cs
public static void Render(
    DrawingContext context,
    SurfaceCrosshairOverlayState state,
    Size viewSize,
    SurfaceChartOverlayOptions options)
{
    if (!state.IsVisible) return;

    var dashPen = new Pen(new SolidColorBrush(Color.FromArgb(160, 200, 200, 200)), 1d, dashStyle: DashStyle.Dash);

    // Horizontal line
    context.DrawLine(dashPen, new Point(0, state.ScreenPosition.Y), new Point(viewSize.Width, state.ScreenPosition.Y));
    // Vertical line
    context.DrawLine(dashPen, new Point(state.ScreenPosition.X, 0), new Point(state.ScreenPosition.X, viewSize.Height));

    // Axis-value pills at edges
    if (state.AxisXFormatted is not null)
    {
        DrawAxisPill(context, state.AxisXFormatted, new Point(state.ScreenPosition.X, viewSize.Height - 20));
    }
    if (state.AxisZFormatted is not null)
    {
        DrawAxisPill(context, state.AxisZFormatted, new Point(viewSize.Width - 60, state.ScreenPosition.Y));
    }
}
```

### Keyboard Handler Pattern

```csharp
// In VideraChartView.Input.cs
protected override void OnKeyDown(KeyEventArgs e)
{
    ArgumentNullException.ThrowIfNull(e);

    if (!IsFocused)
    {
        base.OnKeyDown(e);
        return;
    }

    var handled = e.Key switch
    {
        Key.F => HandleFitToData(),
        Key.R => HandleResetCamera(),
        Key.Add or Key.OemPlus => HandleZoomIn(),
        Key.Subtract or Key.OemMinus => HandleZoomOut(),
        Key.Left => HandlePan(-1, 0),
        Key.Right => HandlePan(1, 0),
        Key.Up => HandlePan(0, -1),
        Key.Down => HandlePan(0, 1),
        _ => false,
    };

    if (handled)
    {
        e.Handled = true;
    }
    else
    {
        base.OnKeyDown(e);
    }
}
```

### Toolbar Button Hit-Testing

```csharp
// In VideraChartView.Input.cs - extend OnPointerPressed
private bool TryHandleToolbarClick(Point position)
{
    if (!_toolbarState.IsVisible) return false;

    foreach (var button in _toolbarState.Buttons)
    {
        if (button.Bounds.Contains(position))
        {
            ExecuteToolbarAction(button.Action);
            return true;
        }
    }
    return false;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| No crosshair | Crosshair overlay at cursor | v2.54 (planned) | Better spatial awareness for probe |
| Basic probe bubbles | Enhanced readout with axis pills | v2.54 (planned) | Richer data display |
| Mouse-only zoom/pan | Keyboard + toolbar buttons | v2.54 (planned) | Better accessibility and power-user UX |

**Deprecated/outdated:**
- None — v2.54 adds new features without deprecating existing ones

## Open Questions

1. **Should crosshair be enabled by default?**
   - What we know: ScottPlot defaults to crosshair off; most chart libraries default to on
   - What's unclear: User preference for Videra's 3D chart context
   - Recommendation: Default to `true` for surface/waterfall, `false` for scatter/bar/contour (3D context makes crosshair more useful for surface probing)

2. **Should toolbar be overlay-rendered or real Avalonia controls?**
   - What we know: Overlay rendering is simpler and consistent with axis/legend pattern; real controls offer accessibility
   - What's unclear: Whether accessibility is a priority for v2.54
   - Recommendation: Start with overlay rendering for consistency; add accessibility in a follow-up milestone

3. **Should keyboard shortcuts be configurable?**
   - What we know: ScottPlot uses fixed shortcuts; some chart libraries allow remapping
   - What's unclear: Whether Videra users need configurable shortcuts
   - Recommendation: Fixed shortcuts for v2.54; add configurability if users request it

## Environment Availability

> Step 2.6: SKIPPED — This is a code-only research task with no external tool dependencies. All features are implemented in C# using existing Avalonia/SkiaSharp APIs.

## Validation Architecture

> Skipped — `workflow.nyquist_validation` is explicitly `false` in `.planning/config.json`.

## Sources

### Primary (HIGH confidence)
- Videra source code — `src/Videra.SurfaceCharts.Avalonia/Controls/` — existing interaction, overlay, and probe infrastructure
- Avalonia 11.3.9 — installed in project; provides `DrawingContext`, `PointerEventArgs`, `OnKeyDown`, `DashStyle`, `FormattedText`
- ScottPlot 5 `Crosshair.cs` — GitHub source — crosshair pattern: HorizontalLine + VerticalLine + marker
- ScottPlot 5 `Tooltip.cs` — GitHub source — tooltip pattern: bubble with tail pointing to data location

### Secondary (MEDIUM confidence)
- Avalonia documentation — `docs.avaloniaui.net` — input event handling patterns (404 on specific URL, but APIs verified via source)

### Tertiary (LOW confidence)
- None — all findings verified against existing codebase

## Project Constraints (from AGENTS.md)

- **bd (beads) for issue tracking** — All task tracking via `bd create`, `bd update`, `bd close`
- **Non-interactive shell commands** — Use `-f` flags for file operations
- **Session completion** — Must `git push` before ending session
- **Snapshot export scope boundaries** — Chart-local bitmap only; no PDF/vector export; no backend expansion
- **Single shipped chart control** — `VideraChartView` is the only public control; no old `SurfaceChartView`/`WaterfallChartView`/`ScatterChartView`
- **Plot.Add.* is the data-loading path** — No direct public `Source` API
- **No hidden fallback/downshift** — Unsupported output = explicit diagnostics

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — All libraries already installed; no new dependencies needed
- Architecture: HIGH — Integration points clearly identified in existing codebase
- Pitfalls: MEDIUM — Based on ScottPlot patterns and general chart library experience; Videra-specific edge cases may emerge during implementation

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (30 days — stable stack, no external dependencies)
