# v2.54 Chart Interactivity — Research Summary

**Project:** Videra SurfaceCharts
**Domain:** 3D chart interactivity — crosshair, tooltips, probe, zoom/pan controls
**Researched:** 2026-04-29
**Confidence:** HIGH

## Executive Summary

Videra's SurfaceCharts module already has a robust interaction foundation: `VideraChartView` handles pointer events, `SurfaceChartInteractionController` manages orbit/pan/dolly/focus gestures, `SurfaceProbeService` resolves probes via ray picking, and `SurfaceProbeOverlayPresenter` renders readout bubbles. The v2.54 milestone enhances this foundation with crosshair lines, improved tooltips, better mouse-driven probe UX for non-surface series, and zoom/pan UI controls. This is a **presentation-layer milestone** — no new NuGet packages, no backend changes, no Core contract modifications. All features build on the existing Avalonia 11.3.9 + SkiaSharp overlay system.

The key architectural insight: crosshair, tooltips, and toolbar controls should all be **chart-local overlay extensions** following the existing `SurfaceChartOverlayCoordinator` / presenter pattern. The main risk is performance — crosshair on every pointer move can trigger full overlay rebuilds, and probe ray-casting is O(rows × columns) per tile. Mitigations are well-understood: separate crosshair from the coordinator refresh path, debounce probe resolution, and cache results. The second major risk is DPI scaling mismatch between the native GPU surface and Avalonia's logical-pixel overlay — this must be verified at 100%, 150%, and 200% scaling.

All 4 research files converge on the same conclusion: this milestone is low-risk, high-value. The existing architecture is well-prepared for these additions. The primary work is extending existing presenters and adding new overlay state records, not building new infrastructure.

## Key Findings

### Recommended Stack

No new dependencies. Everything needed exists in the current Avalonia 11.3.9 + SkiaSharp stack. Crosshair, tooltip, and toolbar are rendered via the existing `SurfaceChartOverlayLayer` using Avalonia's `DrawingContext`. Keyboard shortcuts use Avalonia's built-in `OnKeyDown` handler. Zoom/pan buttons are lightweight overlay-rendered controls or inline Avalonia elements in the chart's `_hostContainer` Grid.

**Core technologies (unchanged):**
- **Avalonia 11.3.9**: UI framework, input events, overlay rendering — already installed
- **SkiaSharp** (bundled with Avalonia): 2D drawing primitives for crosshair/tooltips
- **System.Numerics** (BCL): Vector2/Vector3 for projection math — already used throughout

### Expected Features

**Must-have (table stakes) — NEW for v2.54:**
- **Crosshair guidelines**: Two projected lines (X + Z) through the probe point on the ground plane, rendered as overlay geometry. NOT screen-space H/V lines — Videra is 3D, so crosshair must project onto the ground plane.
- **Enhanced tooltip readout**: Richer probe bubble showing X, Z, Value with configurable format; appears on hover, persists on pin. Multi-series awareness.
- **Keyboard zoom/pan**: Arrow keys pan, +/- zoom, Home resets. Focus-gated to avoid stealing host input.

**Already DONE (don't re-implement):**
- Mouse-wheel zoom (dolly), left-drag orbit, right-drag pan, pin probe on Shift+click, fit-to-data reset, zoom-to-rect (focus selection), probe delta comparison

**Should-have (differentiators):**
- **Snap-to-nearest-data**: Crosshair snaps to nearest sample vertex rather than free-floating
- **Multi-series probe**: Tooltip shows values from ALL series at the same X/Z position
- **Axis-value readout on crosshair**: Crosshair lines show their axis values at the edges (ScottPlot-style)
- **Cursor feedback**: Crosshair on hover, grab on orbit, move on pan
- **Zoom/pan toolbar buttons**: Overlay-rendered buttons for zoom in/out, fit-to-data, reset camera

**Defer (anti-features for v2.54):**
- 2D axis-line crosshair (Videra is 3D — use projected ground-plane guidelines)
- Animated zoom transitions (adds latency, conflicts with interaction quality)
- HTML/rich text tooltips (DrawingContext uses plain text)
- Minimap/overview panel (FitToData already covers this)
- Gesture-based pinch zoom (desktop mouse/keyboard only)
- Axis-specific zoom (Ctrl+drag rect already covers selective zoom)

### Architecture Approach

The existing SurfaceCharts architecture has a clean layered design: `VideraChartView` (Avalonia control) → Interaction Layer (gesture controller, runtime, camera) → Overlay Layer (coordinator, axis/legend/probe presenters, projection) → Plot Layer (Plot3D, series model) → Core Layer (view state, camera pose, data window, picking). All v2.54 features are additive overlays to this stack.

**New components:**
1. `SurfaceCrosshairOverlayState` + `SurfaceCrosshairOverlayPresenter` — crosshair line geometry and rendering
2. `SurfaceTooltipContent` — rich tooltip data model (series name, deltas, world coords)
3. `ISeriesProbeStrategy` interface with implementations for scatter, bar, contour series
4. `SurfaceChartZoomPanControl` — overlay-rendered zoom/pan/reset buttons

**Modified components:**
1. `SurfaceChartOverlayCoordinator` — add crosshair + zoom/pan control orchestration
2. `SurfaceProbeOverlayPresenter` — enhanced tooltip rendering, crosshair integration
3. `SurfaceChartOverlayOptions` — add crosshair/tooltip/zoom-control options
4. `SurfaceChartProbeEvidenceFormatter` — richer tooltip content formatting
5. `VideraChartView.Input.cs` — keyboard handler, cursor feedback
6. `Plot3D` — expose series-kind-aware probe strategy

**Key pattern to follow:** The overlay presenter pattern — immutable state record → static `CreateState()` + `Render()` methods → coordinator integration. Every new overlay feature follows this exact pattern.

### Critical Pitfalls

1. **Crosshair lines fighting the overlay render loop** — Every pointer move triggers `InvalidateOverlay()` → full axis/legend/probe rebuild. FIX: Separate crosshair render path; render directly in overlay layer using just the raw screen position without state rebuild. Or introduce a `DirtyReason` enum.

2. **Tooltip positioning drift during orbit/pan** — Tooltip stays anchored to old screen position while camera moves. FIX: Suppress hovered tooltip during active gestures; only show crosshair + coordinates during orbit/pan. Pinned probes update via projection.

3. **Probe ray-cast performance on large datasets** — `SurfaceHeightfieldPicker.Pick()` is O(rows × columns) per tile. FIX: Debounce probe resolution (~16ms), cache last result, skip probe during `InteractionQuality.Interactive`.

4. **DPI scaling mismatch** — Native GPU surface renders at physical DPI; overlay renders at logical DPI. Crosshair lines offset on HiDPI displays. FIX: Verify overlay uses logical pixels consistently; test at 100%, 150%, 200%.

5. **Overlay layer consuming pointer events** — Transparent overlay intercepts events before chart view. FIX: Set `IsHitTestVisible = false` on overlay layer or route events properly. Test on all platforms.

6. **Snapshot export includes interaction chrome** — Crosshair/tooltip captured in offscreen renders. FIX: Add `IsSnapshotMode` flag to overlay coordinator; suppress interaction overlays during snapshot.

## Implications for Roadmap

### Phase 1: Crosshair Overlay
**Rationale:** Foundation for all other interactivity enhancements. No dependencies on other phases. The crosshair is a visual enhancement to the existing probe system — it shows projected guidelines at the probe position.
**Delivers:** Crosshair lines (projected ground-plane guidelines), axis-value readout pills at crosshair edges, crosshair visibility toggle in `SurfaceChartOverlayOptions`
**Addresses:** Crosshair guidelines (table stakes), axis-value readout on crosshair (differentiator)
**Avoids:** Pitfall 1 (separate crosshair render path from overlay coordinator), Pitfall 5 (clip to chart interior, low opacity, dashed lines), Pitfall 11 (suppress during active gestures)

### Phase 2: Enhanced Tooltips
**Rationale:** Builds on existing `SurfaceProbeOverlayPresenter`. Depends on probe system (already done). Can be developed in parallel with Phase 1.
**Delivers:** Richer tooltip content (series name, world coords, delta from pinned), configurable tooltip fields via `SurfaceChartOverlayOptions`, multi-series probe resolution showing all series values at hovered X/Z
**Addresses:** Enhanced tooltip readout (table stakes), multi-series probe (differentiator), snap-to-nearest-data (differentiator)
**Avoids:** Pitfall 2 (suppress hovered tooltip during active gestures), Pitfall 7 (show LOD-aware "approx" indicator), Pitfall 12 (text truncation for long readouts)

### Phase 3: Series Probe Strategies
**Rationale:** Extends probe resolution to scatter/bar/contour series. Independent of Phases 1-2. Can be developed in parallel.
**Delivers:** `ISeriesProbeStrategy` interface, `ScatterProbeStrategy` (nearest-point), `BarProbeStrategy` (bar hit-test), `ContourProbeStrategy` (contour line hit-test), dispatch logic in `SurfaceProbeOverlayPresenter`
**Addresses:** Mouse-driven probe for all series types
**Avoids:** Pitfall 3 (debounce + cache probe resolution)

### Phase 4: Keyboard & Toolbar Controls
**Rationale:** UI polish layer. Independent of Phases 1-3. Can be developed in parallel.
**Delivers:** Keyboard shortcuts (arrows pan, +/- zoom, Home resets), zoom/pan toolbar buttons (overlay-rendered), cursor feedback (crosshair on hover, grab on orbit, move on pan)
**Addresses:** Keyboard zoom/pan (differentiator), zoom/pan toolbar buttons (differentiator), cursor feedback (differentiator)
**Avoids:** Pitfall 4 (document gesture matrix, test modifier precedence), Pitfall 8 (overlay hit testing)

### Phase 5: Snapshot & Doctor Integration
**Rationale:** Integration phase. Depends on Phases 1-4 being complete. Ensures new features don't break existing contracts.
**Delivers:** `IsSnapshotMode` flag suppressing interaction chrome in exports, extended `SurfaceChartProbeEvidence` with crosshair/interaction fields, updated `surfacecharts-support-summary.txt` format
**Addresses:** Pitfall 13 (Doctor/support evidence gap), Pitfall 14 (snapshot includes interaction chrome)

### Phase Ordering Rationale

- **Phases 1-4 are largely independent** — they can be developed in parallel worktrees. The only ordering constraint is within each phase (state → presenter → coordinator wiring).
- **Phase 5 must come last** — it integrates all other phases with snapshot export and Doctor evidence.
- **Phase 1 first** because crosshair is the most visible user-facing feature and establishes the lightweight overlay pattern that Phases 2-4 follow.
- **No backend/Core changes needed** — all phases are overlay-layer additions, keeping risk low and enabling parallel development.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3:** Scatter/bar/contour probe strategies need algorithm design — nearest-point search for scatter, hit-test geometry for bars, contour line identification. May need spatial indexing for large datasets.

Phases with standard patterns (skip research-phase):
- **Phase 1:** Crosshair is a well-documented pattern (ScottPlot, LiveCharts2). Direct overlay extension.
- **Phase 2:** Tooltip enhancement follows existing readout bubble pattern. Extend formatter.
- **Phase 4:** Keyboard handler and toolbar buttons are standard Avalonia patterns.
- **Phase 5:** Integration wiring — no new algorithms or patterns.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | No new dependencies; all libraries already installed and verified |
| Features | HIGH | Table stakes/differentiators clear from ScottPlot, LiveCharts2, OxyPlot, SciChart comparison |
| Architecture | HIGH | Integration points traced through all layers via direct codebase reading |
| Pitfalls | HIGH | 8 high-risk pitfalls derived directly from codebase analysis with line-level references |

**Overall confidence:** HIGH

### Gaps to Address

- **Scatter probe algorithm**: Brute-force nearest-point is O(n). For large scatter datasets (>10k points), may need spatial index (KD-tree). Start with brute-force, optimize if profiling shows issues.
- **Contour probe semantics**: What does "probe a contour line" mean? Recommend showing contour level value + nearest point coordinates. Validate with users.
- **Crosshair default visibility**: Should crosshair default on or off? Recommend on for surface/waterfall, off for scatter/bar/contour. Validate with users.
- **Toolbar rendering approach**: Overlay-rendered (simpler, consistent) vs. real Avalonia controls (accessibility). Start with overlay; add accessibility in follow-up milestone.
- **Platform hit-testing**: Avalonia pointer behavior differs across Windows/Linux/macOS. Test pointer capture during drag, scroll wheel delta normalization, modifier key detection on all platforms.

## Sources

### Primary (HIGH confidence)
- Videra source code: `VideraChartView.Input.cs`, `VideraChartView.Overlay.cs`, `VideraChartView.Rendering.cs`, `SurfaceChartInteractionController.cs`, `SurfaceProbeOverlayPresenter.cs`, `SurfaceChartOverlayCoordinator.cs`, `SurfaceChartProjection.cs`, `SurfaceProbeService.cs`, `SurfaceHeightfieldPicker.cs`
- Avalonia 11.3.9 — verified via `dotnet list package`
- ScottPlot 5 Cookbook — Crosshair, Tooltip, InteractivePlottables patterns

### Secondary (MEDIUM confidence)
- LiveCharts2 documentation (training data) — CrosshairBehavior, DefaultTooltip, ZoomAndPanBehavior
- OxyPlot documentation (training data) — TrackerHitResult, ZoomRectangleBinding
- SciChart documentation (training data) — CursorModifier, RolloverModifier, ZoomExtentsModifier
- Avalonia pointer event model and DPI scaling behavior (platform-dependent)

### Tertiary (LOW confidence)
- None — all core findings verified against existing codebase

---
*Research completed: 2026-04-29*
*Ready for roadmap: yes*
