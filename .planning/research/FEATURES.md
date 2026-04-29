# v2.54 Chart Interactivity — Feature Research

**Researched:** 2026-04-29
**Domain:** 3D chart interactivity — crosshair, tooltips, mouse-driven probe, zoom/pan controls
**Confidence:** HIGH (architecture from source code, patterns from ScottPlot docs + training data)

## Summary

Videra's SurfaceCharts already has a mature interaction foundation: orbit/pan/dolly/focus-selection gestures, a hover+pinned probe system with readout bubbles, a 3D→2D projection engine, and an overlay coordinator that composes axes/grid/legend/probe layers. The v2.54 milestone needs to **enhance and expose** these capabilities through a polished public API, not rebuild from scratch.

The key insight from researching ScottPlot, LiveCharts2, OxyPlot, and SciChart: professional chart interactivity has a small set of **table-stakes features** (crosshair, tooltip, zoom, pan) and a larger set of **differentiators** (rubber-band zoom, axis-specific zoom, snap-to-data, multi-series probe, export of interaction state). Videra's 3D context makes some 2D patterns (like axis-line crosshairs) need adaptation — the crosshair becomes projected ground-plane guidelines, not screen-space lines.

## Table Stakes vs Differentiators vs Anti-Features

### Table Stakes (Must-Have for v2.54)

| Feature | Description | Complexity | Existing Foundation |
|---------|-------------|------------|---------------------|
| **Crosshair guidelines** | Two projected lines (X + Z) through the probe point on the ground plane, rendered as overlay geometry | LOW | `SurfaceChartProjection.Project()` already maps 3D→2D; `SurfaceAxisOverlayPresenter` already draws projected grid lines |
| **Tooltip readout** | Enhanced probe bubble showing X, Z, Value with configurable format; appears on hover, persists on pin | LOW | `SurfaceProbeOverlayPresenter` already renders readout bubbles with `SurfaceChartProbeEvidenceFormatter` |
| **Mouse-wheel zoom** | Scroll wheel zooms data window centered on cursor position | DONE | `ApplyDolly()` in `SurfaceChartInteractionController` already does anchor-aware zoom |
| **Left-drag orbit** | Drag to rotate 3D camera around data | DONE | `ApplyOrbit()` already implemented |
| **Right-drag pan** | Drag to shift data window | DONE | `ApplyPan()` already implemented |
| **Pin probe on click** | Shift+click pins a probe for comparison | DONE | `TogglePinnedProbe()` already implemented |
| **Fit-to-data reset** | Button/key to reset view to full data bounds | DONE | `FitToData()` and `ResetCamera()` already public |

### Differentiators (High-Value, Medium Complexity)

| Feature | Description | Complexity | Existing Foundation |
|---------|-------------|------------|---------------------|
| **Snap-to-nearest-data** | Crosshair snaps to nearest sample vertex rather than free-floating | MEDIUM | `SurfaceProbeService.ResolveFromScreenPosition()` already finds nearest tile hit; needs "snap to grid vertex" mode |
| **Multi-series probe** | Tooltip shows values from ALL series at the same X/Z position | MEDIUM | `Plot3D.Series` is accessible; needs per-series probe resolution |
| **Axis-value readout on crosshair** | Crosshair lines show their axis values at the edges (like ScottPlot's Crosshair labels) | LOW | Overlay system already renders axis tick labels; just needs two more label positions |
| **Keyboard zoom/pan** | Arrow keys pan, +/- zoom, Home resets | LOW | `SurfaceChartRuntime` methods already exist; just needs key binding |
| **Zoom-to-rect** | Ctrl+drag draws a rectangle, releasing zooms to that region | DONE | `FocusSelection` gesture mode already implements this |
| **Probe delta comparison** | When pinned probes exist, show delta values in hover readout | DONE | `CreateDeltaReadout()` already implemented |

### Anti-Features (Avoid for v2.54)

| Feature | Why Avoid | Instead |
|---------|-----------|---------|
| **2D axis-line crosshair** | Videra charts are 3D; screen-space H/V lines don't correspond to data axes | Use projected ground-plane guidelines |
| **Animated zoom transitions** | Adds complexity, latency, and fighting with interaction quality modes | Instant zoom (already shipped) |
| **Tooltip with HTML/rich text** | Avalonia overlay uses `DrawingContext` + `FormattedText`; rich text needs a different rendering path | Keep plain-text readouts with good formatting |
| **Minimap/overview panel** | Significant UI scope; chart already has FitToData | Defer to future milestone |
| **Gesture-based pinch zoom** | Touch input is a separate platform concern | Desktop mouse/keyboard only |
| **Axis-specific zoom** (zoom only X or only Y) | Adds interaction mode complexity; Ctrl+drag rect already covers selective zoom | Use focus-selection |

## Architecture Analysis

### Current Interaction Pipeline

```
VideraChartView.Input.cs
  └─ OnPointerMoved → UpdateProbeScreenPosition → _overlayCoordinator.UpdateProbeScreenPosition
  └─ OnPointerPressed → _interactionController.HandlePointerPressed (orbit/pan/pin/selection)
  └─ OnPointerReleased → _interactionController.HandlePointerReleased
  └─ OnPointerWheelChanged → ApplyDolly (anchor-aware zoom)

SurfaceChartOverlayCoordinator
  ├─ ProbeState (hover + pinned probes)
  ├─ AxisState (projected axes + grid)
  └─ LegendState (series legend)

SurfaceProbeOverlayPresenter
  ├─ ResolveFromScreenPosition → SurfaceProbeService → SurfacePickHit → SurfaceProbeInfo
  └─ Render → DrawReadoutBubble (hover + pinned)
```

### What Needs Adding for Crosshair

The crosshair is **not** a separate interaction mode — it's a **visual enhancement** to the existing probe system. When the mouse moves, two projected guidelines should appear on the ground plane (X=probe.AxisX and Z=probe.AxisY), extending from the probe point to the axis edges.

**Implementation path:**
1. In `SurfaceChartOverlayCoordinator.Refresh()`, when `ProbeState.HoveredProbe` is resolved, compute two `SurfaceAxisLineGeometry` segments for the crosshair
2. In `SurfaceChartOverlayCoordinator.Render()`, draw the crosshair lines before the probe bubble
3. Style: thin dashed lines, semi-transparent, distinct from grid lines

**Dependencies:**
- `SurfaceChartProjection.Project()` — already available
- `SurfaceAxisOverlayState` — extend with optional crosshair lines
- `SurfaceProbeOverlayPresenter` — add crosshair rendering

### What Needs Adding for Enhanced Tooltips

The existing probe readout is already a tooltip. Enhancements:
1. **Multi-series awareness**: When multiple series exist, resolve probe for each series at the same X/Z and show all values
2. **Configurable content**: Allow `SurfaceChartOverlayOptions` to control what fields appear (X, Z, Value, series name, delta)
3. **Snapping indicator**: Show whether the probe is exact or approximate (already has `IsApproximate`)

**Implementation path:**
1. Add `SurfaceProbeInfo[]` resolution across all series in `SurfaceProbeOverlayPresenter.CreateState()`
2. Extend readout formatting in `SurfaceChartProbeEvidenceFormatter` for multi-series
3. Add `TooltipContent` options to `SurfaceChartOverlayOptions`

### What Needs Adding for Zoom/Pan Polish

The core zoom/pan is done. Polish items:
1. **Keyboard bindings**: Arrow keys for pan, +/- for zoom, Home for reset
2. **Zoom limits**: Already has `MinimumWindowSpan`; add configurable max zoom level
3. **Cursor feedback**: Change cursor to crosshair when hovering, grab when dragging, grabbing when panning

**Implementation path:**
1. Override `OnKeyDown` in `VideraChartView` for keyboard shortcuts
2. Add `Cursor` property management based on gesture state
3. Add `SurfaceChartInteractionOptions` for configurable limits

## Dependencies on Existing Code

| Existing Component | Used By New Feature | Modification Needed |
|--------------------|--------------------|--------------------|
| `SurfaceChartProjection` | Crosshair guidelines | None — already projects 3D→2D |
| `SurfaceProbeOverlayPresenter` | Crosshair + enhanced tooltip | Extend `CreateState()` and `Render()` |
| `SurfaceChartOverlayCoordinator` | Crosshair state | Add crosshair line computation |
| `SurfaceChartInteractionController` | Keyboard zoom/pan | Add keyboard handler dispatch |
| `SurfaceChartOverlayOptions` | Tooltip content control | Add tooltip-specific options |
| `Plot3D.Series` | Multi-series probe | Iterate series for probe resolution |
| `SurfaceProbeService` | Snap-to-data | Already resolves nearest hit |

## Standard Patterns from Professional Chart Libraries

### ScottPlot 5 (Mature .NET 2D Charting)
- **Crosshair**: `Plot.Add.Crosshair(x, y)` — two axis lines + labels at edges. Static placement, not mouse-following by default.
- **Tooltip**: `Plot.Add.Tooltip(tip, text, label)` — annotation with tail pointing to data point. Customizable fill, border, font, tail width, angle.
- **Interactive plottables**: Draggable markers, lines, spans, rectangles — respond to hover without manual mouse wiring.
- **Zoom**: Mouse wheel + left-drag rect zoom. Right-drag pan. Middle-click autoscale.

### LiveCharts2 (.NET Cross-Platform)
- **Crosshair**: Built-in `CrosshairBehavior` — lines follow pointer, show axis values at edges.
- **Tooltip**: Built-in `DefaultTooltip` — appears near data points, shows all series values at hovered X.
- **Zoom**: `ZoomAndPanBehavior` — wheel zoom, drag pan, double-click reset. Supports axis-specific zooming.

### OxyPlot (.NET Scientific Charting)
- **Crosshair**: `TrackerHitResult` with crosshair lines. Snap-to-nearest data point.
- **Tooltip**: Tracker overlay showing series name, X/Y values, custom format string.
- **Zoom**: Mouse wheel, rubber-band selection, axis-specific zoom. Keyboard shortcuts.

### SciChart (Commercial .NET/WPF)
- **Crosshair**: `CursorModifier` — vertical line + horizontal line + axis labels. Rollover mode shows all series at X.
- **Tooltip**: `RolloverModifier` — vertical line + per-series markers + tooltip panel.
- **Zoom**: Extmodifiers for zoom, pan, rubber-band. Animated zoom transitions.

## Common Pitfalls

### Pitfall 1: 3D Crosshair Confusion
**What goes wrong:** Implementing 2D-style screen-space crosshair lines in a 3D chart
**Why it happens:** Copying 2D chart patterns directly
**How to avoid:** Crosshair lines should be projected on the ground plane (XZ plane at Y=valueRange.Minimum), not screen-space H/V lines
**Warning signs:** Crosshair lines don't align with axis grid when camera rotates

### Pitfall 2: Probe Performance on Multi-Series
**What goes wrong:** Resolving probe for every series on every mouse move causes frame drops
**Why it happens:** Naive per-series probe resolution
**How to avoid:** Cache probe results per-series, only re-resolve when series data or camera changes
**Warning signs:** Interaction quality drops to `Refine` on simple hover

### Pitfall 3: Tooltip Clipping
**What goes wrong:** Tooltip bubble extends outside chart bounds when probe is near edges
**Why it happens:** Not clamping bubble position to view size
**How to avoid:** Already handled by `ClampBubbleOrigin()` — extend for new tooltip sizes
**Warning signs:** Tooltip text cut off at chart edges

### Pitfall 4: Crosshair Overdraw with Grid
**What goes wrong:** Crosshair lines are indistinguishable from grid lines
**Why it happens:** Same color/width as grid
**How to avoid:** Use distinct style (dashed, brighter color, slightly thicker) and draw on top of grid
**Warning signs:** Users can't see crosshair in dense grid areas

## Code Examples

### Crosshair Line Computation (Projected Ground Plane)
```csharp
// In SurfaceChartOverlayCoordinator or new presenter
internal static SurfaceAxisLineGeometry[] CreateCrosshairLines(
    SurfaceProbeInfo probe,
    SurfaceMetadata metadata,
    SurfaceValueRange valueRange,
    SurfaceChartProjection projection)
{
    var y = (float)valueRange.Minimum;
    
    // Horizontal guideline (constant Z, varying X)
    var hStart = projection.Project(new Vector3((float)metadata.HorizontalAxis.Minimum, y, (float)probe.AxisY));
    var hEnd = projection.Project(new Vector3((float)metadata.HorizontalAxis.Maximum, y, (float)probe.AxisY));
    
    // Vertical guideline (constant X, varying Z)
    var vStart = projection.Project(new Vector3((float)probe.AxisX, y, (float)metadata.VerticalAxis.Minimum));
    var vEnd = projection.Project(new Vector3((float)probe.AxisX, y, (float)metadata.VerticalAxis.Maximum));
    
    return [new SurfaceAxisLineGeometry(hStart, hEnd), new SurfaceAxisLineGeometry(vStart, vEnd)];
}
```

### Multi-Series Probe Resolution
```csharp
// In SurfaceProbeOverlayPresenter
internal static SurfaceProbeInfo?[] ResolveMultiSeriesProbe(
    IReadOnlyList<Plot3DSeries> series,
    SurfaceMetadata metadata,
    SurfaceCameraFrame cameraFrame,
    IReadOnlyList<SurfaceTile> loadedTiles,
    Point screenPosition)
{
    var results = new SurfaceProbeInfo?[series.Count];
    for (int i = 0; i < series.Count; i++)
    {
        if (series[i].SurfaceSource is ISurfaceTileSource source)
        {
            results[i] = SurfaceProbeService.ResolveFromScreenPosition(
                source.Metadata, cameraFrame, loadedTiles, screenPosition);
        }
        // Scatter/Bar/Contour: resolve by nearest data point
    }
    return results;
}
```

### Keyboard Handler
```csharp
// In VideraChartView (new partial file)
protected override void OnKeyDown(KeyEventArgs e)
{
    base.OnKeyDown(e);
    if (e.Handled) return;
    
    var panStep = _runtime.ViewState.DataWindow.Width * 0.1;
    var zoomFactor = 0.85;
    
    switch (e.Key)
    {
        case Key.Left: Pan(-panStep, 0); e.Handled = true; break;
        case Key.Right: Pan(panStep, 0); e.Handled = true; break;
        case Key.Up: Pan(0, -panStep); e.Handled = true; break;
        case Key.Down: Pan(0, panStep); e.Handled = true; break;
        case Key.Add: Zoom(zoomFactor); e.Handled = true; break;
        case Key.Subtract: Zoom(1.0 / zoomFactor); e.Handled = true; break;
        case Key.Home: FitToData(); e.Handled = true; break;
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual mouse wiring | Built-in gesture controller | v2.47+ | Interaction is control-internal, not host-managed |
| Per-chart-view probe | Plot-owned probe evidence | v2.51+ | Probe evidence is chart-local, not viewer-scoped |
| Separate chart controls | Single `VideraChartView` | v2.47 | All chart types share one interaction pipeline |

**What's already done (don't re-implement):**
- Orbit, pan, dolly, focus-selection gestures
- Hover probe with readout bubble
- Pinned probe with delta comparison
- FitToData and ResetCamera
- Selection rectangle for zoom-to-rect

## Open Questions

1. **Crosshair visibility toggle**: Should crosshair be always-on when probe is active, or configurable via `SurfaceChartOverlayOptions`?
   - Recommendation: Configurable, default-on. Add `ShowCrosshair` bool.

2. **Multi-series probe priority**: When multiple series overlap, which series' value takes precedence in the tooltip?
   - Recommendation: Show all series values, ordered by draw order (last = front). Add series name label.

3. **Keyboard shortcuts scope**: Should keyboard shortcuts work only when chart has focus, or always when hovered?
   - Recommendation: Only when focused. Add `Focusable = true` and handle `GotFocus`/`LostFocus`.

4. **Cursor feedback**: Should the cursor change to indicate available gestures?
   - Recommendation: Yes — crosshair on hover, grab on orbit start, move on pan start. Low effort, high UX value.

## Sources

### Primary (HIGH confidence)
- Videra source code: `VideraChartView.Input.cs`, `SurfaceChartInteractionController.cs`, `SurfaceProbeOverlayPresenter.cs`, `SurfaceChartOverlayCoordinator.cs`, `SurfaceChartProjection.cs`
- ScottPlot 5 Cookbook: https://scottplot.net/cookbook/5.0/ — Crosshair, Tooltip, InteractivePlottables sections

### Secondary (MEDIUM confidence)
- LiveCharts2 documentation (training data) — CrosshairBehavior, DefaultTooltip, ZoomAndPanBehavior
- OxyPlot documentation (training data) — TrackerHitResult, ZoomRectangleBinding
- SciChart documentation (training data) — CursorModifier, RolloverModifier, ZoomExtentsModifier

### Tertiary (LOW confidence)
- Training data for LiveCharts2/OxyPlot/SciChart specifics — versions and API details may have changed

## Metadata

**Confidence breakdown:**
- Architecture analysis: HIGH — from source code
- ScottPlot patterns: HIGH — from official docs
- LiveCharts2/OxyPlot/SciChart: MEDIUM — from training data, not verified against current docs
- Complexity estimates: MEDIUM — based on existing codebase patterns

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (stable — chart interactivity patterns are well-established)
