# v2.54 Chart Interactivity — Pitfalls Research

**Researched:** 2026-04-29
**Domain:** Crosshair, tooltips, mouse-driven probe, zoom/pan controls for Avalonia 3D charts
**Confidence:** HIGH (codebase-driven analysis)

---

## Executive Summary

Videra's existing interaction architecture is well-separated: `SurfaceChartInteractionController` owns gesture state, `SurfaceProbeService` does ray-casting, `SurfaceChartProjection` handles world↔screen math, and the overlay layer renders via Avalonia `DrawingContext`. Adding crosshair, enhanced tooltips, and zoom/pan refinements on top of this foundation is viable, but there are **8 high-risk pitfalls** specific to this combination of technologies and the existing architecture.

---

## Pitfall 1: Crosshair Lines Fighting the Overlay Render Loop

**What goes wrong:** Crosshair lines (horizontal + vertical guide lines following the cursor) are rendered every frame the pointer moves. If the crosshair is drawn in the same overlay pass as tooltips and axis labels, every pointer move triggers a full overlay rebuild — recomputing axis state, legend state, and probe state — even though only the crosshair position changed.

**Why it happens:** The current `InvalidateOverlay()` path in `VideraChartView.Overlay.cs` (line 58) rebuilds ALL overlay state via `_overlayCoordinator.Refresh(...)`. A crosshair that moves on every `PointerMoved` event will call `InvalidateOverlay()` → `Refresh()` → full axis/legend/probe recomputation → `_overlayLayer.InvalidateVisual()`.

**How to avoid:**
- **Separate crosshair from overlay coordinator.** Render crosshair lines directly in the `SurfaceChartOverlayLayer.Render()` method using just the raw screen position — no state rebuild needed.
- Or introduce a `DirtyReason` enum to `InvalidateOverlay()` so it can skip axis/legend recomputation when only the crosshair position changed.
- Cache the last overlay state and only recompute when source/tiles/camera actually change.

**Warning signs:** Pointer-move latency increases noticeably. Overlay CPU time spikes during hover.

**Existing code reference:** `VideraChartView.Overlay.cs` lines 58-98 — the `InvalidateOverlay()` method rebuilds everything unconditionally.

---

## Pitfall 2: Tooltip Positioning Drift During Orbit/Pan

**What goes wrong:** Tooltips are positioned in screen space but their content comes from world-space probe data. During orbit/pan gestures, the camera moves but the tooltip stays anchored to the old screen position, creating visual disconnect — the tooltip "slides off" the surface point it describes.

**Why it happens:** `SurfaceProbeOverlayPresenter` (line 113-123) positions the hovered tooltip at the raw `probeScreenPosition` passed from the pointer event. During orbit, the pointer position is correct in screen space, but the world point under the cursor changes because the camera is moving. The tooltip shows data for the world point that was under the cursor when the gesture started, not where it is now.

**How to avoid:**
- **During active gestures, suppress the hovered tooltip entirely** (the existing `InteractionQuality.Interactive` state already signals this). Only show crosshair + coordinates during orbit/pan.
- If you must show live probe data during gesture, re-resolve the probe on every frame using the current camera frame — but this is expensive and conflicts with the tile LOD downgrade during interaction.
- For pinned probes, keep them rendered but update their screen positions via `SurfaceChartProjection.Project()` using the current camera.

**Warning signs:** Tooltip text says "X: 5.2, Y: 3.1" but the crosshair is visibly over a different surface region.

**Existing code reference:** `SurfaceProbeOverlayPresenter.CreateState()` (line 43-44) resolves probe from `probeScreenPosition` against the current camera frame — but during orbit the camera is changing between overlay refreshes.

---

## Pitfall 3: Mouse-Driven Probe Ray-Cast Performance on Large Datasets

**What goes wrong:** `SurfaceHeightfieldPicker.Pick()` iterates ALL loaded tiles and for each tile tests ALL triangle pairs (O(rows × columns) per tile). On a 1024×1024 surface with multiple LOD tiles loaded, every pointer move triggers ~2M triangle intersection tests.

**Why it happens:** The picker (`SurfaceHeightfieldPicker.cs` lines 135-148) does a brute-force scan of every row×column cell in the tile, testing two triangles per cell. There is no spatial acceleration structure (BVH, grid, or hierarchical test).

**How to avoid:**
- **Debounce probe resolution.** Only resolve the probe after the pointer has been stationary for ~16ms (one frame). The existing `UpdateProbeScreenPosition` in the overlay coordinator already has a dirty check (line 36), but it only checks exact position equality — not time-gating.
- **Add a simple grid-based early-out.** Before triangle testing, check if the pick ray's XY footprint falls within the tile's XY bounds. The existing `IntersectsTileBounds` (line 334-349) does this, but only at the tile level. Add a sub-tile AABB test.
- **Cache the last pick result.** If the screen position hasn't moved more than a few pixels, return the cached probe. The existing `VertexSnapDistanceEpsilon = 0.05f` already handles minor drift, but caching avoids re-entering the picker entirely.
- During `InteractionQuality.Interactive`, skip probe resolution entirely and only show the crosshair.

**Warning signs:** Pointer movement becomes jerky. CPU time in `SurfaceHeightfieldPicker.Pick()` exceeds 4ms per frame.

**Existing code reference:** `SurfaceHeightfieldPicker.TryPickTile()` (line 120-172) — the inner loop is `O(tile.Width * tile.Height)`.

---

## Pitfall 4: Zoom/Pan Gesture Conflicting with Probe Pin Toggle

**What goes wrong:** The existing gesture model uses `Shift+LeftClick` for pin toggle and `LeftClick` for orbit. If zoom is added as a new gesture (e.g., `Ctrl+Scroll` for semantic zoom vs. dolly), and pan is `RightDrag`, users will accidentally pin probes when they meant to orbit, or accidentally orbit when they meant to zoom.

**Why it happens:** `SurfaceChartInteractionController.ResolveLeftButtonGestureMode()` (line 137-150) dispatches gesture mode based on `KeyModifiers` at press time. The gesture mode is locked at press and doesn't change during the drag. Adding new gesture modes (e.g., `Ctrl+LeftClick` for selection zoom) requires careful modifier key precedence.

**How to avoid:**
- **Document the gesture map explicitly** before implementing. Create a gesture matrix:
  | Input | Modifier | Action |
  |-------|----------|--------|
  | Left drag | None | Orbit |
  | Left drag | Shift | Pin toggle (press only) |
  | Left drag | Ctrl | Focus selection (existing) |
  | Right drag | None | Pan |
  | Scroll | None | Dolly (existing) |
  | Scroll | Ctrl | Semantic zoom (NEW) |
  | Middle drag | None | Pan (NEW alternative) |
- **Don't add middle-click as a gesture button** without testing — Avalonia's `PointerUpdateKind` doesn't always report middle button consistently across platforms.
- **Add a gesture mode indicator** in the overlay (e.g., small "Orbit" / "Pan" / "Zoom" label) during active gesture so users can see what mode they're in.

**Warning signs:** Users report "the chart does something different from what I expected." Bug reports mention accidental probe pinning.

**Existing code reference:** `SurfaceChartInteractionController.cs` lines 26-47, 137-150 — the gesture resolution logic.

---

## Pitfall 5: Crosshair Lines Occluding Axis Labels and Tick Marks

**What goes wrong:** Crosshair lines drawn across the full chart area will overlap with axis labels, tick marks, and legend overlays. The crosshair lines may be drawn on top of axis text, making both unreadable.

**Why it happens:** The overlay layer renders in a fixed order: axis grid → axis lines → axis ticks → axis labels → legend → probe bubbles → (NEW: crosshair). If crosshair is drawn last, it covers everything. If drawn first, it's covered by probe bubbles but visible through axis labels.

**How to avoid:**
- **Clip crosshair lines to the chart interior.** Don't extend them to the chart edges — stop at the axis boundary. Use the `SurfaceAxisOverlayState` front-corner positions to determine the clip region.
- **Draw crosshair with low opacity** (alpha ~80-120) so axis text remains readable underneath.
- **Use dashed lines** for crosshair to visually distinguish from axis grid lines (which are solid with low alpha).
- Consider drawing crosshair ONLY on the surface mesh area, not extending into the axis/label margins.

**Warning signs:** Axis labels become unreadable when the cursor is near chart edges. Users complain about visual clutter.

**Existing code reference:** `SurfaceAxisOverlayPresenter.Render()` (line 87-114) — axis rendering order. `SurfaceChartOverlayCoordinator.Render()` (line 89-96) — overlay render order.

---

## Pitfall 6: DPI Scaling Mismatch Between Overlay and Native Surface

**What goes wrong:** The native GPU surface (D3D11/Vulkan/Metal) renders at the display's actual DPI scale, but the Avalonia overlay layer renders at Avalonia's logical DPI. On HiDPI displays (150%, 200% scaling), the crosshair and tooltip positions don't align with the rendered surface geometry.

**Why it happens:** `SyncRenderHost()` in `VideraChartView.Rendering.cs` (line 70) passes `renderScale = (float)(VisualRoot?.RenderScaling ?? 1.0)` to the native host. But the overlay layer uses Avalonia's `DrawingContext` which operates in logical pixels. The `SurfaceChartProjection` uses `cameraFrame.ViewportPixels` (physical pixels) for projection math, while the overlay draws in logical pixels.

**How to avoid:**
- **Always convert between logical and physical pixels** when projecting 3D points for overlay rendering. The projection returns physical pixel coordinates; divide by `renderScale` to get logical pixels for the Avalonia `DrawingContext`.
- The existing code in `VideraChartView.Overlay.cs` line 64 creates the camera frame with `_runtime.CreateCameraFrame(_overlayViewSize, 1f)` — note the `1f` render scale. This means the overlay projection already works in logical pixels. Verify this is consistent.
- **Test at 100%, 150%, and 200% DPI scaling.** The most common bug is crosshair lines that are offset by the DPI scale factor.

**Warning signs:** Crosshair lines are offset from the cursor by a consistent pixel amount that changes with display scaling. Probe readouts show values for a point slightly off from where the crosshair appears.

**Existing code reference:** `VideraChartView.Rendering.cs` line 70 — `renderScale` for native host. `VideraChartView.Overlay.cs` line 64 — `1f` render scale for overlay projection.

---

## Pitfall 7: Probe Resolution Returning Stale Data After Zoom

**What goes wrong:** After a zoom operation changes the `SurfaceDataWindow`, the loaded tiles may not yet reflect the new viewport (tiles are loaded asynchronously). The probe resolution uses whatever tiles are currently loaded, which may be low-LOD tiles from before the zoom. The tooltip shows interpolated low-resolution values instead of the high-resolution data the user expects.

**Why it happens:** `SurfaceProbeService.Resolve()` (line 65-116) searches through `loadedTiles` for the best tile covering the probe sample coordinate. After zoom, the `SurfaceTileScheduler` has requested new high-LOD tiles, but they haven't arrived yet. The probe falls back to the overview tile or a lower-LOD tile.

**How to avoid:**
- **Show a "Loading..." indicator in the tooltip** when the best available tile's LOD is significantly lower than the expected LOD for the current viewport. Check `tile.Key.LevelX + tile.Key.LevelY` against the LOD selection from `_lodPolicy.Select()`.
- **Don't suppress the probe during tile loading** — show the best available data with a visual indicator (e.g., dimmed text, "approx" suffix).
- The existing `SurfacePickHit.IsApproximate` flag (line 169 in `SurfaceHeightfieldPicker.cs`) already signals when the hit is from a partial tile. Expose this in the tooltip.

**Warning signs:** After zooming, tooltip values "jump" when new tiles finish loading. Users see different values for the same surface point depending on zoom level.

**Existing code reference:** `SurfaceProbeService.cs` lines 65-116, `SurfacePickHit.IsApproximate` (line 169).

---

## Pitfall 8: Overlay Layer Consuming Pointer Events from Native Surface

**What goes wrong:** The `SurfaceChartOverlayLayer` is a transparent Avalonia control layered on top of the native GPU surface. If the overlay layer has `IsHitTestVisible = true` (the default), it intercepts pointer events before they reach the `VideraChartView` input handlers. This can break orbit/pan/dolly gestures entirely.

**Why it happens:** In `VideraChartView.Core.cs` line 23, the overlay layer is added as `_overlayLayer` to the visual tree. Avalonia's hit testing walks the visual tree top-down. The overlay layer fills its bounds with `context.FillRectangle(Brushes.Transparent, ...)` (line 256 in `VideraChartView.Rendering.cs`) — but `Transparent` brush still participates in hit testing by default.

**How to avoid:**
- **Set `IsHitTestVisible = false` on the overlay layer** if pointer events should pass through to the chart view. The existing code already renders the overlay as a separate layer, but verify hit testing behavior.
- Alternatively, keep the overlay hit-testable but route all input through the overlay layer back to the interaction controller. This is more complex but allows the overlay to handle its own tooltip hover regions.
- **Test on all three platforms** (Windows/Linux/macOS) — hit test behavior for transparent overlays can differ.

**Warning signs:** Orbit/pan gestures stop working after the overlay layer is added. Pointer events are swallowed. The chart appears frozen.

**Existing code reference:** `VideraChartView.Rendering.cs` line 256 — `FillRectangle(Brushes.Transparent, ...)`. `VideraChartView.Core.cs` line 23 — overlay layer creation.

---

## Additional Pitfalls (Medium Confidence)

### Pitfall 9: DoubleTap for Zoom-to-Fit Interfering with Probe Pin

**What goes wrong:** If `DoubleTapped` is used for zoom-to-fit and `Shift+Click` for probe pin, rapid Shift+Click sequences may trigger both.

**How to avoid:** Use `e.Handled = true` aggressively in the gesture handler. Check `e.KeyModifiers` in the `DoubleTapped` handler and skip zoom-to-fit if Shift is held.

### Pitfall 10: Scroll Wheel Zoom Anchor Point Drift

**What goes wrong:** The existing `ApplyDolly()` (line 208-241) uses the hovered probe position as the zoom anchor. If no probe is resolved (cursor is off the surface), it falls back to the window center. This creates inconsistent zoom behavior — sometimes zooming toward the cursor, sometimes toward the center.

**How to avoid:** Always use the cursor position projected onto the data plane as the zoom anchor, even when no surface probe is resolved. Project the cursor ray onto the Y=0 plane (or the data range midpoint) to get a consistent anchor point.

### Pitfall 11: Crosshair Rendered During Gesture Creates Visual Noise

**What goes wrong:** During orbit/pan gestures, the crosshair continues to follow the cursor, creating a distracting visual trail while the 3D scene is rotating.

**How to avoid:** Suppress crosshair rendering during active gestures (`HasActiveGesture == true`). Show only the gesture mode indicator.

### Pitfall 12: Tooltip Text Overflow on Small Chart Sizes

**What goes wrong:** Probe readout text (X, Y, Z, delta) can be 80+ characters. On small chart controls (e.g., 400×300), the tooltip bubble extends beyond the chart bounds.

**How to avoid:** The existing `ClampBubbleOrigin()` (line 224-231) already clamps bubble position to view bounds. But it doesn't handle text that's wider than the view. Add text truncation or multi-line wrapping for long readouts.

---

## Integration Pitfalls

### Pitfall 13: Adding Interaction Without Updating Doctor/Support Evidence

**What goes wrong:** New interaction features (crosshair, enhanced tooltips, zoom modes) change the chart's observable behavior but the Doctor and support evidence contracts don't reflect them.

**How to avoid:** Extend `SurfaceChartProbeEvidence` with crosshair presence, zoom level, and interaction mode fields. Update `surfacecharts-support-summary.txt` format.

### Pitfall 14: Interaction Features Breaking the Snapshot Export

**What goes wrong:** `Plot.CaptureSnapshotAsync()` renders offscreen to a `RenderTargetBitmap`. If crosshair/tooltip overlays are included in the snapshot, the exported PNG will contain interaction chrome that shouldn't be in a static export.

**How to avoid:** The snapshot path (`RenderOffscreenAsync` in `VideraChartView.Rendering.cs` line 104) calls `SyncRenderHost()` → `bitmap.Render(this)`. Ensure the overlay layer suppresses crosshair/tooltip rendering when in snapshot mode. Add a `IsSnapshotMode` flag to the overlay coordinator.

### Pitfall 15: Interaction Quality State Machine Growing Too Complex

**What goes wrong:** The existing `InteractionQuality` enum has two values: `Interactive` and `Refine`. Adding crosshair, tooltip, and zoom behaviors creates pressure to add more states (e.g., `Hovering`, `Zooming`, `Panning`). The state machine becomes unmanageable.

**How to avoid:** Keep `InteractionQuality` as-is. Use separate boolean flags for crosshair visibility, tooltip visibility, and gesture mode. Don't fold visual concerns into the LOD quality state machine.

---

## Prevention Strategy Summary

| Pitfall | Prevention | Priority |
|---------|------------|----------|
| 1. Crosshair rebuilding all overlay state | Separate crosshair render path; dirty-reason enum | HIGH |
| 2. Tooltip drift during orbit | Suppress hovered tooltip during active gestures | HIGH |
| 3. Probe ray-cast performance | Debounce + cache + skip during Interactive quality | HIGH |
| 4. Gesture mode conflicts | Document gesture matrix; test modifier precedence | HIGH |
| 5. Crosshair occluding axis labels | Clip to chart interior; use low opacity + dashing | MEDIUM |
| 6. DPI scaling mismatch | Verify overlay uses logical pixels consistently | HIGH |
| 7. Stale probe data after zoom | Show LOD-aware "approx" indicator in tooltip | MEDIUM |
| 8. Overlay consuming pointer events | Set `IsHitTestVisible = false` or route events properly | HIGH |
| 9. DoubleTap vs probe pin | Check modifiers in DoubleTapped; use Handled | LOW |
| 10. Zoom anchor drift | Project cursor onto data plane as fallback anchor | MEDIUM |
| 11. Crosshair noise during gesture | Suppress crosshair during active gesture | LOW |
| 12. Tooltip overflow on small charts | Add text truncation for long readouts | LOW |
| 13. Doctor/support evidence gap | Extend probe evidence contract | MEDIUM |
| 14. Snapshot includes interaction chrome | Add IsSnapshotMode flag to overlay | HIGH |
| 15. State machine complexity | Keep InteractionQuality simple; use separate flags | MEDIUM |

---

## Architectural Recommendations

### 1. Crosshair Should Be a Lightweight Overlay Primitive

Don't route crosshair through the full `SurfaceChartOverlayCoordinator.Refresh()` path. Instead:
```csharp
// In SurfaceChartOverlayLayer.Render():
if (_crosshairPosition is Point pos && !_owner._interactionController.HasActiveGesture)
{
    context.DrawLine(CrosshairPen, new Point(pos.X, 0), new Point(pos.X, Bounds.Height));
    context.DrawLine(CrosshairPen, new Point(0, pos.Y), new Point(Bounds.Width, pos.Y));
}
```

### 2. Tooltip State Should Be Decoupled from Probe Resolution

The probe resolution (ray-cast) is expensive. The tooltip rendering is cheap. Decouple them:
- Probe resolution: debounced, cached, runs on pointer settle
- Tooltip rendering: instant, uses cached probe data, runs every overlay paint

### 3. Zoom/Pan Should Use the Existing `SurfaceDataWindow` Model

The existing `ApplyDolly()` and `ApplyPan()` already manipulate `SurfaceDataWindow`. Any new zoom controls (semantic zoom, zoom-to-selection, zoom-to-fit) should go through `SurfaceChartRuntime.ZoomTo(dataWindow)` — don't create a parallel zoom path.

### 4. Test Interaction on All Three Platforms

Avalonia pointer behavior differs subtly across Windows (WM_POINTER), Linux (X11/Wayland), and macOS (NSEvent). Test:
- Pointer capture during drag
- Scroll wheel delta normalization
- Modifier key detection during drag
- Touch vs mouse disambiguation

---

## Sources

### Primary (HIGH confidence)
- `Videra.SurfaceCharts.Avalonia/Controls/Interaction/SurfaceChartInteractionController.cs` — existing gesture model
- `Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceProbeService.cs` — probe resolution logic
- `Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceChartProjection.cs` — world↔screen projection
- `Videra.SurfaceCharts.Core/Picking/SurfaceHeightfieldPicker.cs` — ray-cast picker
- `Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Overlay.cs` — overlay invalidation path
- `Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Input.cs` — pointer event routing
- `Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Rendering.cs` — render + overlay layer
- Avalonia UI docs: Pointer devices (https://docs.avaloniaui.net/docs/input-interaction/pointer)

### Secondary (MEDIUM confidence)
- Avalonia hit testing behavior for transparent brushes (platform-dependent)
- DPI scaling behavior across Windows/Linux/macOS Avalonia hosts

---

## Metadata

**Confidence breakdown:**
- Pitfalls 1-8: HIGH — derived directly from codebase analysis
- Pitfalls 9-12: MEDIUM — derived from general 3D chart interaction experience + codebase
- Pitfalls 13-15: MEDIUM — derived from project integration patterns

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (stable — architecture is established)
