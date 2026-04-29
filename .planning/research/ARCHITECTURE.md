# v2.54 Chart Interactivity — Architecture Research

**Researched:** 2026-04-29
**Domain:** SurfaceCharts interactivity — crosshair, tooltips, probe, zoom/pan controls
**Confidence:** HIGH

---

## Executive Summary

The existing SurfaceCharts architecture already has a robust interaction foundation. The `VideraChartView` Avalonia control handles pointer events through `SurfaceChartInteractionController`, which implements orbit (left drag), pan (right drag), focus selection (Ctrl+left drag), pin toggle (Shift+left click), and dolly/zoom (mouse wheel). Probe resolution happens via `SurfaceProbeService` with both heightfield-ray-pick and viewport-normalized-coordinate paths. The overlay system (`SurfaceChartOverlayCoordinator`) coordinates axis, legend, and probe readout presentation through composable presenter classes.

For v2.54, the primary gaps are: (1) visual crosshair lines at the probe position, (2) enhanced tooltip content beyond the current readout bubble, (3) potential probe enhancements for scatter/bar/contour series, and (4) optional UI zoom/pan control buttons. The architecture supports all of these through the existing overlay presenter pattern — new features are additive overlays, not structural changes.

---

## Existing Architecture Map

### Layer Stack (bottom → top)

```
┌─────────────────────────────────────────────────────────────────┐
│  VideraChartView (Avalonia Decorator)                           │
│  ├── .Core.cs      — lifecycle, Plot3D, runtime init            │
│  ├── .Input.cs     — pointer events → InteractionController     │
│  ├── .Overlay.cs   — overlay coordinator, projection caching    │
│  ├── .Rendering.cs — software/native render pipeline            │
│  └── .Properties.cs — ViewState styled property, quality        │
├─────────────────────────────────────────────────────────────────┤
│  Interaction Layer                                              │
│  ├── SurfaceChartInteractionController — gesture recognition    │
│  ├── SurfaceChartRuntime — runtime state, tile cache            │
│  ├── SurfaceCameraController — camera state management          │
│  └── SurfaceTileScheduler / SurfaceTileCache                    │
├─────────────────────────────────────────────────────────────────┤
│  Overlay Layer                                                  │
│  ├── SurfaceChartOverlayCoordinator — orchestrates presenters   │
│  ├── SurfaceAxisOverlayPresenter — axis ticks, labels, grid     │
│  ├── SurfaceProbeOverlayPresenter — probe readout bubbles       │
│  ├── SurfaceLegendOverlayPresenter — multi-series legend        │
│  ├── SurfaceProbeService — screen→sample probe resolution       │
│  └── SurfaceChartProjection — 3D→screen for overlays            │
├─────────────────────────────────────────────────────────────────┤
│  Plot Layer                                                     │
│  ├── Plot3D — owns series model, color map, overlay options     │
│  ├── Plot3DSeries — immutable series descriptor                 │
│  └── Plot3DAddApi — series authoring                            │
├─────────────────────────────────────────────────────────────────┤
│  Core Layer (SurfaceCharts.Core)                                │
│  ├── SurfaceViewState — DataWindow + Camera + DisplaySpace      │
│  ├── SurfaceCameraPose — Target, Yaw, Pitch, Distance, FOV     │
│  ├── SurfaceDataWindow — StartX, StartY, Width, Height          │
│  ├── SurfaceMetadata — axes, value range                        │
│  ├── SurfaceCameraFrame — computed camera with matrices         │
│  └── Picking: SurfaceProbeInfo, SurfaceProbeRequest, Picker     │
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow: Pointer → Probe → Overlay

```
PointerMoved event
  │
  ▼
VideraChartView.OnPointerMoved()
  ├── UpdateProbeScreenPosition(point)
  │     └── SurfaceChartOverlayCoordinator.UpdateProbeScreenPosition()
  │           └── stores Point? _probeScreenPosition
  └── InteractionController.HandlePointerMoved()
        └── ApplyOrbit / ApplyPan / UpdateSelection (if gesture active)

InvalidateOverlay()
  │
  ▼
SurfaceChartOverlayCoordinator.Refresh()
  ├── SurfaceAxisOverlayPresenter.CreateState()    → AxisState
  ├── SurfaceLegendOverlayPresenter.CreateState()  → LegendState
  └── SurfaceProbeOverlayPresenter.CreateState()
        └── SurfaceProbeService.ResolveFromScreenPosition()
              ├── SurfaceHeightfieldPicker.CreatePickRay()
              ├── SurfaceHeightfieldPicker.Pick()
              └── SurfaceProbeInfo.FromPickHit()

RenderOverlay() → SurfaceChartOverlayCoordinator.Render()
  ├── SurfaceAxisOverlayPresenter.Render()
  ├── SurfaceLegendOverlayPresenter.Render()
  └── SurfaceProbeOverlayPresenter.Render()
        ├── DrawReadoutBubble() for hovered probe
        └── DrawPinnedReadouts() for pinned probes
```

---

## Integration Points for v2.54

### 1. Crosshair Lines

**What exists:** The probe system resolves a `SurfaceProbeInfo` with `AxisX`, `AxisY`, `Value`, `SampleX`, `SampleY`, and `WorldPosition`. The `SurfaceChartProjection` can project any `Vector3` world position to a screen `Point`. The overlay already renders at the probe screen position.

**Integration point:** `SurfaceProbeOverlayPresenter.Render()` already receives the `hoveredProbeScreenPosition` and the `projection`. Crosshair lines are horizontal + vertical lines through this position, clipped to the chart bounds (`SurfaceChartProjection.ScreenBounds`).

**New component needed:** `SurfaceCrosshairOverlayState` + rendering logic inside `SurfaceProbeOverlayPresenter` (or a new `SurfaceCrosshairOverlayPresenter`).

**Modification needed:** `SurfaceChartOverlayCoordinator` — add crosshair state to `Refresh()` and `Render()`.

### 2. Enhanced Tooltips

**What exists:** `SurfaceProbeOverlayPresenter.DrawReadoutBubble()` renders a text bubble with `SurfaceChartProbeEvidenceFormatter.CreateHoveredOverlayReadout()`. The readout format is controlled by `SurfaceChartOverlayOptions` (precision, format, label formatters).

**Integration point:** The readout text is already formatted per-probe. Enhanced tooltips could add: series name, tile key, approximate flag, world coordinates, delta from pinned probe. All data is already in `SurfaceProbeInfo`.

**New component needed:** Possibly a `SurfaceTooltipOverlayPresenter` if tooltip rendering diverges significantly from the current bubble pattern. Otherwise, extend `SurfaceProbeOverlayPresenter` with richer formatting.

**Modification needed:** `SurfaceChartProbeEvidenceFormatter` — add richer tooltip content methods. `SurfaceChartOverlayOptions` — add tooltip-specific options (show world coords, show deltas, etc.).

### 3. Mouse-Driven Probe (Enhancement)

**What exists:** The probe system already works for surface/waterfall series via `SurfaceHeightfieldPicker` (ray-based picking) and `SurfaceProbeService` (viewport-normalized coordinate mapping). The probe resolves `SurfaceProbeInfo` with full sample/axis/world coordinates.

**Integration point:** For scatter series, the probe would need to find the nearest scatter point. For bar series, the probe would need to identify the hovered bar. For contour series, the probe would need to identify the hovered contour line/level.

**New component needed:** `ISeriesProbeStrategy` interface with implementations for each series kind:
- `SurfaceProbeStrategy` (existing — `SurfaceProbeService`)
- `ScatterProbeStrategy` (new — nearest-point search)
- `BarProbeStrategy` (new — bar hit test)
- `ContourProbeStrategy` (new — contour line hit test)

**Modification needed:** `SurfaceProbeOverlayPresenter.CreateState()` — dispatch to appropriate strategy based on active series kind. `Plot3D` or `VideraChartView` — expose series-kind-aware probe resolution.

### 4. Zoom/Pan Controls

**What exists:** `SurfaceChartInteractionController` implements:
- Orbit: left drag (0.5°/pixel)
- Pan: right drag (data window translation)
- Dolly: mouse wheel (0.85 factor, centered on hovered probe)
- Focus Selection: Ctrl+left drag (zoom to rect)
- `VideraChartView.FitToData()` — resets to full data bounds
- `VideraChartView.ResetCamera()` — resets camera to default pose
- `VideraChartView.ZoomTo(dataWindow)` — programmatic zoom

**Integration point:** These are already public APIs on `VideraChartView`. UI buttons would call these methods. The overlay system could render button controls, or they could be separate Avalonia controls positioned alongside the chart.

**New component needed:** `SurfaceChartZoomPanControl` — an Avalonia control (or overlay) with zoom-in, zoom-out, fit-to-data, reset-camera buttons. Could be rendered as an overlay within `SurfaceChartOverlayCoordinator` or as a separate sibling control in the host layout.

**Modification needed:** `SurfaceChartOverlayCoordinator` — add zoom/pan control state. `SurfaceChartOverlayOptions` — add zoom/pan control visibility/position options.

---

## New vs Modified Components

### New Components

| Component | Layer | Purpose |
|-----------|-------|---------|
| `SurfaceCrosshairOverlayState` | Overlay | Crosshair line geometry + visibility |
| `SurfaceCrosshairOverlayPresenter` | Overlay | Renders crosshair lines at probe position |
| `SurfaceTooltipContent` | Core/Overlay | Rich tooltip data (series name, deltas, world coords) |
| `ISeriesProbeStrategy` | Core | Interface for series-kind-specific probe resolution |
| `ScatterProbeStrategy` | Core | Nearest scatter point probe |
| `BarProbeStrategy` | Core | Bar hit-test probe |
| `ContourProbeStrategy` | Core | Contour line hit-test probe |
| `SurfaceChartZoomPanControl` | Avalonia | UI buttons for zoom/pan/reset |

### Modified Components

| Component | Layer | Change |
|-----------|-------|--------|
| `SurfaceChartOverlayCoordinator` | Overlay | Add crosshair + zoom/pan control orchestration |
| `SurfaceProbeOverlayPresenter` | Overlay | Enhanced tooltip rendering, crosshair integration |
| `SurfaceChartOverlayOptions` | Avalonia | Add crosshair/tooltip/zoom-control options |
| `SurfaceChartProbeEvidenceFormatter` | Avalonia | Richer tooltip content formatting |
| `VideraChartView.Overlay.cs` | Avalonia | Wire crosshair + zoom/pan control state |
| `VideraChartView.Input.cs` | Avalonia | Possibly extend gesture handling for new controls |
| `Plot3D` | Plot | Expose series-kind-aware probe strategy |

### Unchanged Components

| Component | Reason |
|-----------|--------|
| `SurfaceChartInteractionController` | Existing gesture model is complete; zoom/pan buttons call existing APIs |
| `SurfaceChartRuntime` | Runtime state management unchanged |
| `SurfaceCameraController` | Camera state unchanged |
| `SurfaceProbeService` | Surface probe resolution unchanged; new strategies are additive |
| `SurfaceChartProjection` | Projection math unchanged |
| `SurfaceAxisOverlayPresenter` | Axis overlay unchanged |
| `SurfaceLegendOverlayPresenter` | Legend overlay unchanged |

---

## Build Order (Dependency-Aware)

```
Phase 1: Crosshair (no external deps)
  ├── SurfaceCrosshairOverlayState (state record)
  ├── SurfaceCrosshairOverlayPresenter (render logic)
  ├── SurfaceChartOverlayCoordinator modification (wire crosshair)
  └── SurfaceChartOverlayOptions addition (crosshair visibility)

Phase 2: Enhanced Tooltips (depends on existing probe)
  ├── SurfaceTooltipContent (rich data model)
  ├── SurfaceChartProbeEvidenceFormatter extension (rich formatting)
  ├── SurfaceProbeOverlayPresenter modification (enhanced rendering)
  └── SurfaceChartOverlayOptions addition (tooltip options)

Phase 3: Series Probe Strategies (independent per series kind)
  ├── ISeriesProbeStrategy interface
  ├── ScatterProbeStrategy (nearest-point math)
  ├── BarProbeStrategy (bar hit-test)
  ├── ContourProbeStrategy (contour line hit-test)
  └── SurfaceProbeOverlayPresenter/Plot3D modification (dispatch)

Phase 4: Zoom/Pan Controls (independent UI)
  ├── SurfaceChartZoomPanControl (Avalonia control or overlay)
  ├── SurfaceChartOverlayCoordinator modification (wire control)
  ├── SurfaceChartOverlayOptions addition (control visibility/position)
  └── VideraChartView.Overlay.cs modification (wire control state)
```

**Parallelization:** Phases 1, 2, 3, and 4 are largely independent and can be developed in parallel worktrees. Phase 3 has no dependency on 1 or 2. Phase 4 has no dependency on 1, 2, or 3. The only ordering constraint is within each phase (state before presenter before coordinator wiring).

---

## Key Patterns to Follow

### Overlay Presenter Pattern (from existing code)

```csharp
// 1. State record (immutable data for rendering)
internal sealed class SurfaceCrosshairOverlayState
{
    public static SurfaceCrosshairOverlayState Empty { get; } = new(...);
    // geometry, visibility, bounds
}

// 2. Presenter (static CreateState + Render methods)
internal static class SurfaceCrosshairOverlayPresenter
{
    public static SurfaceCrosshairOverlayState CreateState(
        Point? probeScreenPosition,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions options)
    {
        // compute crosshair geometry from probe position + projection bounds
    }

    public static void Render(DrawingContext context, SurfaceCrosshairOverlayState state)
    {
        // draw horizontal + vertical lines
    }
}

// 3. Coordinator integration
// In SurfaceChartOverlayCoordinator.Refresh():
//   CrosshairState = SurfaceCrosshairOverlayPresenter.CreateState(...)
// In SurfaceChartOverlayCoordinator.Render():
//   SurfaceCrosshairOverlayPresenter.Render(context, CrosshairState)
```

### Probe Strategy Pattern (new)

```csharp
internal interface ISeriesProbeStrategy
{
    SurfaceProbeInfo? ResolveFromScreenPosition(
        SeriesProbeContext context,
        Point screenPosition);
}

internal readonly record struct SeriesProbeContext(
    SurfaceMetadata Metadata,
    SurfaceCameraFrame CameraFrame,
    IReadOnlyList<SurfaceTile> LoadedTiles,
    Plot3DSeries Series);
```

---

## Common Pitfalls

### 1. Crosshair Clipping
**What goes wrong:** Crosshair lines extend beyond chart bounds, overlapping axis labels or legend.
**How to avoid:** Clip crosshair lines to `SurfaceChartProjection.ScreenBounds` (already computed from projected chart corners).

### 2. Probe Resolution for Non-Surface Series
**What goes wrong:** `SurfaceProbeService` only resolves probes for surface/waterfall tiles. Scatter/bar/contour series have no probe resolution.
**How to avoid:** Implement `ISeriesProbeStrategy` per series kind. Dispatch from `SurfaceProbeOverlayPresenter.CreateState()` based on `Plot3D.ActiveSeries.Kind`.

### 3. Tooltip Positioning at Edges
**What goes wrong:** Tooltip bubble extends outside viewport when probe is near chart edge.
**How to avoid:** Already handled by `SurfaceProbeOverlayPresenter.ClampBubbleOrigin()` — reuse this pattern for enhanced tooltips.

### 4. Zoom/Pan Control Hit Testing
**What goes wrong:** Zoom/pan buttons intercept pointer events meant for chart interaction (orbit/pan).
**How to avoid:** Render zoom/pan controls in the overlay layer with `IsHitTestVisible = true` only on the button rects. The overlay layer already handles hit testing (`_overlayLayer.IsHitTestVisible = true`).

### 5. Interaction Quality During Crosshair
**What goes wrong:** Crosshair rendering during `Interactive` quality mode causes frame drops.
**How to avoid:** Crosshair is a simple overlay (2 lines) — negligible rendering cost. No special quality handling needed.

---

## Code Examples

### Crosshair Rendering (reference pattern)

```csharp
// In SurfaceCrosshairOverlayPresenter.Render():
public static void Render(DrawingContext context, SurfaceCrosshairOverlayState state)
{
    if (!state.IsVisible || state.ScreenPosition is null)
        return;

    var pos = state.ScreenPosition.Value;
    var bounds = state.ChartBounds;

    // Horizontal line (full width of chart)
    context.DrawLine(CrosshairPen,
        new Point(bounds.Left, pos.Y),
        new Point(bounds.Right, pos.Y));

    // Vertical line (full height of chart)
    context.DrawLine(CrosshairPen,
        new Point(pos.X, bounds.Top),
        new Point(pos.X, bounds.Bottom));

    // Center marker
    context.DrawEllipse(CrosshairMarkerFill, CrosshairMarkerPen,
        pos, 3d, 3d);
}
```

### Series Probe Strategy Dispatch

```csharp
// In SurfaceProbeOverlayPresenter.CreateState():
var activeSeries = plot?.ActiveSeries;
var probe = activeSeries?.Kind switch
{
    Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall =>
        SurfaceProbeService.ResolveFromScreenPosition(metadata, cameraFrame, loadedTiles, screenPos),
    Plot3DSeriesKind.Scatter =>
        ScatterProbeStrategy.ResolveFromScreenPosition(context, screenPos),
    Plot3DSeriesKind.Bar =>
        BarProbeStrategy.ResolveFromScreenPosition(context, screenPos),
    Plot3DSeriesKind.Contour =>
        ContourProbeStrategy.ResolveFromScreenPosition(context, screenPos),
    _ => null,
};
```

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 8 SDK | All components | ✓ | 8.x | — |
| Avalonia | UI layer | ✓ | (in project) | — |
| System.Numerics | Vector3/Matrix4x4 | ✓ | (in .NET 8) | — |

**No external dependencies needed.** All features are implemented within the existing SurfaceCharts.Core and SurfaceCharts.Avalonia projects.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + FluentAssertions |
| Config file | (in existing test projects) |
| Quick run command | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/` |
| Full suite command | `dotnet test` |

### Phase Requirements → Test Map

| Feature | Test Type | Automated Command |
|---------|-----------|-------------------|
| Crosshair rendering | Integration | `dotnet test --filter "Crosshair"` |
| Crosshair clipping | Unit | `dotnet test --filter "CrosshairClipping"` |
| Enhanced tooltip content | Unit | `dotnet test --filter "TooltipContent"` |
| Scatter probe | Integration | `dotnet test --filter "ScatterProbe"` |
| Bar probe | Integration | `dotnet test --filter "BarProbe"` |
| Contour probe | Integration | `dotnet test --filter "ContourProbe"` |
| Zoom/pan controls | Integration | `dotnet test --filter "ZoomPanControl"` |

---

## Open Questions

1. **Crosshair style options:** Should crosshair support dashed lines, different colors per axis, or snap-to-grid? Recommendation: start with solid lines, add style options in `SurfaceChartOverlayOptions`.

2. **Tooltip vs readout bubble:** Should enhanced tooltips replace the current readout bubble or augment it? Recommendation: augment — keep the bubble for hover, add a richer tooltip panel for pinned probes or on-demand.

3. **Scatter probe nearest-point algorithm:** For large scatter datasets (>10k points), brute-force nearest-point is O(n). Should we use a spatial index (KD-tree)? Recommendation: start with brute-force (scatter datasets are typically <100k points), optimize if profiling shows issues.

4. **Zoom/pan control placement:** Should controls be rendered as an overlay (inside the chart) or as a separate Avalonia control (outside the chart)? Recommendation: overlay — consistent with the existing pattern, no host layout changes needed.

5. **Contour probe semantics:** What does "probe a contour line" mean? Show the contour level value? The nearest point on the line? Recommendation: show the contour level value + nearest point coordinates.

---

## Sources

### Primary (HIGH confidence)
- Direct codebase reading: all files listed in `files_to_read` and discovered via glob/grep
- Architecture patterns derived from existing implementation

### Secondary (MEDIUM confidence)
- Avalonia pointer event model (standard Avalonia documentation)
- 3D projection math patterns (standard graphics programming)

---

## Metadata

**Confidence breakdown:**
- Architecture understanding: HIGH — direct codebase reading
- Integration points: HIGH — traced data flow through all layers
- New component design: HIGH — follows existing patterns exactly
- Build order: HIGH — dependency analysis from code references
- Pitfalls: MEDIUM — based on patterns observed in existing code

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (stable — architecture is well-established)
