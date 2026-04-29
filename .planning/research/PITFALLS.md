# v2.53 Chart Type Expansion — Pitfalls Research

**Researched:** 2026-04-29
**Domain:** .NET chart library extension — Bar charts, Contour plots, DateTime axes, log scale, custom tick formatters, chart legends
**Confidence:** HIGH (codebase analysis) / MEDIUM (external patterns)

---

## Executive Summary

Videra's SurfaceCharts module currently supports three chart families (Surface, Waterfall, Scatter) with a linear-only axis model. The `SurfaceAxisScaleKind` enum already declares `Log` and `DateTime` variants, but `Log` is explicitly blocked at construction time with a throw. The tick generator, overlay presenters, and rendering pipeline are all linear-only. Adding Bar charts, Contour plots, DateTime axes, log scale, custom tick formatters, and chart legends requires changes across all three layers (Core, Avalonia overlay, Rendering) and introduces significant integration risk.

**Primary risk:** Each feature touches the axis/overlay/rendering pipeline at different points. Implementing them in isolation creates integration debt that compounds when features combine (e.g., Bar chart + DateTime axis + custom tick formatter).

---

## Pitfall 1: Log Scale — Raw vs. Display Space Confusion

**What goes wrong:** The axis stores raw values (e.g., 1, 10, 100, 1000) but the rendering pipeline needs display-space coordinates (e.g., 0, 1, 2, 3 in log10 space). Mixing these spaces causes: incorrect tick placement, broken probe readouts, wrong grid lines, and data-to-screen mapping errors.

**Why it happens:** `SurfaceAxisDescriptor` currently stores `Minimum`/`Maximum` as raw values and uses them directly in `SurfaceAxisTickGenerator.CreateMajorTickValues()`. The tick generator does linear interpolation between min/max. For log scale, ticks must be placed at powers of 10 (or the log base), not linearly spaced.

**Current code evidence:**
```csharp
// SurfaceAxisDescriptor.cs line 46-51 — Log is explicitly blocked
if (scaleKind == SurfaceAxisScaleKind.Log)
{
    throw new ArgumentException(
        "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated.",
        nameof(scaleKind));
}
```

**How to avoid:**
1. Introduce a `SurfaceAxisTransform` interface that maps raw ↔ display space
2. Linear: identity transform
3. Log: `display = Math.Log(raw, base)` / `raw = Math.Pow(base, display)`
4. DateTime: `display = ticks` / `raw = DateTime.FromBinary(ticks)`
5. Store display-space min/max internally, expose raw-space via public API
6. Make `SurfaceAxisTickGenerator` work in display space, format labels in raw space

**Warning signs:**
- Tick values that look "wrong" on a log axis (linearly spaced instead of exponentially)
- Probe readouts showing display-space values instead of raw values
- Grid lines that don't align with data features

**Suggested phase:** Axis transform infrastructure (early phase — everything else depends on this)

---

## Pitfall 2: DateTime Axis — Tick Generation and Formatting

**What goes wrong:** DateTime axes need "nice" tick boundaries (start of hour, day, month, year) rather than arbitrary numeric steps. The current `ComputeNiceStep()` uses factors [1, 2, 2.5, 5, 10] which don't map to time units. DateTime formatting needs locale-aware, variable-precision output (e.g., "2026-04-29" vs "14:30" vs "2026-04-29 14:30:05").

**Why it happens:** `SurfaceAxisTickGenerator.CreateMajorTickValues()` assumes numeric axis space. It computes `step = ComputeNiceStep(axisSpan / targetTickCount)` which produces steps like 0.5, 1, 2, 5 — not 1 hour, 1 day, 1 month.

**Current code evidence:**
```csharp
// SurfaceAxisTickGenerator.cs — linear-only tick generation
var step = ComputeNiceStep(axisSpan / targetTickCount);
var firstTick = Math.Ceiling(axisMinimum / step) * step;
```

**How to avoid:**
1. Create `DateTimeTickGenerator` that snaps to calendar boundaries
2. Use `System.DateTime` / `System.DateTimeOffset` as the raw value type
3. Store as `long` (ticks) internally for uniform handling
4. Implement adaptive precision: show date when zoomed out, time when zoomed in
5. Support timezone-aware formatting via `IFormatProvider`

**Warning signs:**
- Ticks at "1.5 hours" instead of "1:00, 2:00, 3:00"
- Labels showing raw tick numbers instead of formatted dates
- Tick density that doesn't adapt to zoom level

**Suggested phase:** DateTime axis support (after axis transform infrastructure)

---

## Pitfall 3: Bar Chart — 3D Geometry and Depth Sorting

**What goes wrong:** Bar charts in 3D require: (1) extruded rectangle geometry, (2) proper depth sorting for transparent/overlapping bars, (3) different data model (discrete categories vs. continuous surface), and (4) interaction model (click on bar vs. hover on surface).

**Why it happens:** The current rendering pipeline is built for heightfield tiles (`SurfaceChartGpuRenderBackend`) and scatter points (`ScatterRenderScene`). Neither handles extruded bar geometry. The depth sorting in `SurfaceChartGpuRenderBackend` uses tile-based rendering, not per-primitive sorting.

**Current code evidence:**
```csharp
// Plot3DSeriesKind.cs — only Surface, Waterfall, Scatter
public enum Plot3DSeriesKind
{
    Surface,
    Waterfall,
    Scatter,
}
```

**How to avoid:**
1. Add `Bar` to `Plot3DSeriesKind`
2. Create `BarChartData` model with categorical X/Z axes and numeric Y values
3. Reuse the existing GPU pipeline for bar geometry (instanced quads with extrusion)
4. For depth sorting: use the existing `SurfaceChartRenderChangeSet` pattern
5. Keep bar rendering chart-local (don't merge into VideraView)

**Warning signs:**
- Bars rendering behind each other incorrectly
- Performance degradation with many bars (no instancing)
- Bar interaction conflicting with surface probe system

**Suggested phase:** Bar chart series (after axis transform infrastructure)

---

## Pitfall 4: Contour Plot — Isoline Extraction and Rendering

**What goes wrong:** Contour plots require: (1) marching squares algorithm for isoline extraction, (2) line rendering in 3D space, (3) optional filled contours, and (4) label placement on contour lines. The current rendering pipeline doesn't have line-based rendering for chart data.

**Why it happens:** Surface charts render heightfield tiles via GPU shaders. Contour plots need vector line extraction from the same heightfield data, which is a fundamentally different rendering path.

**How to avoid:**
1. Add `Contour` to `Plot3DSeriesKind`
2. Implement marching squares in `SurfaceCharts.Core` (CPU-side, chart-local)
3. Reuse existing `SurfaceMatrix` as input data
4. Render contour lines via the existing overlay system (2D projection) or add a 3D line renderer
5. Start with line contours only; defer filled contours

**Warning signs:**
- Contour lines that don't close properly
- Performance issues with high-resolution grids
- Contour labels overlapping or placed at jagged points

**Suggested phase:** Contour plot series (after axis transform infrastructure)

---

## Pitfall 5: Custom Tick Formatters — Breaking the Formatting Pipeline

**What goes wrong:** Custom formatters can: (1) throw exceptions that crash the overlay, (2) return empty/null strings, (3) produce text that's too long for the layout, (4) not respect the axis key convention ("X", "Y", "Z", "Legend").

**Why it happens:** `SurfaceChartOverlayOptions.LabelFormatter` is a `Func<string, double, string>?` delegate. It's called from `FormatLabel()` which is used by tick rendering, probe readouts, and legend labels. A bad formatter breaks all of them.

**Current code evidence:**
```csharp
// SurfaceChartOverlayOptions.cs — formatter is a raw delegate
public Func<string, double, string>? LabelFormatter { get; init; }

public string FormatLabel(string axisKey, double value)
{
    if (LabelFormatter is not null)
    {
        return LabelFormatter(axisKey, value);  // No try-catch!
    }
    // ...
}
```

**How to avoid:**
1. Wrap formatter calls in try-catch, fall back to default formatting on exception
2. Validate formatter output (non-null, non-empty, reasonable length)
3. Document the axis key convention clearly
4. Consider a richer formatter interface: `IChartLabelFormatter` with `CanFormat` + `Format`

**Warning signs:**
- Overlay crashes when custom formatter is set
- Tick labels showing empty strings
- Labels overflowing their layout bounds

**Suggested phase:** Custom tick formatter support (can be done in parallel with axis transforms)

---

## Pitfall 6: Chart Legend — Multi-Series and Mixed-Mode Complexity

**What goes wrong:** The current legend is a single color-mapped swatch for the active surface/waterfall series. Expanding to: (1) multiple series with different legend entries, (2) scatter series (point/marker legend), (3) bar series (filled rectangle legend), (4) contour series (line legend) — requires a complete legend model redesign.

**Why it happens:** `SurfaceLegendOverlayPresenter` assumes a single `SurfaceColorMap` and renders a continuous swatch. It doesn't support discrete series entries.

**Current code evidence:**
```csharp
// SurfaceLegendOverlayPresenter.cs — single swatch model
public static SurfaceLegendOverlayState CreateState(
    SurfaceMetadata? metadata,
    SurfaceColorMap? colorMap,
    bool useColorMapRange,
    SurfaceChartProjection? projection,
    SurfaceChartOverlayOptions? overlayOptions)
```

**How to avoid:**
1. Create `LegendEntry` model with: series name, visual indicator (swatch/marker/line), visibility
2. `LegendEntry` visual indicator types: `ColorSwatch` (surface/waterfall), `MarkerShape` (scatter), `FilledRectangle` (bar), `LineStyle` (contour)
3. Layout engine that stacks entries vertically with overflow handling
4. Keep legend chart-local (don't create a generic legend framework)

**Warning signs:**
- Legend entries overlapping
- Legend not updating when series are added/removed
- Performance issues with many legend entries

**Suggested phase:** Chart legend expansion (after all series types are defined)

---

## Pitfall 7: Axis Transform Infrastructure — Foundation for Everything

**What goes wrong:** If axis transforms aren't implemented first, every subsequent feature (log scale, DateTime, bar chart categories) re-implements its own ad-hoc coordinate mapping, creating inconsistent behavior.

**Why it happens:** The current codebase has `SurfaceAxisScaleKind` enum but the actual transform logic is missing. `SurfaceAxisDescriptor` stores raw min/max and the tick generator works in raw space.

**How to avoid:**
1. **Phase 1:** Create `IAxisTransform` with `RawToDisplay(double)` and `DisplayToRaw(double)`
2. **Phase 2:** Implement `LinearAxisTransform`, `LogAxisTransform`, `DateTimeAxisTransform`
3. **Phase 3:** Update `SurfaceAxisTickGenerator` to work in display space
4. **Phase 4:** Update overlay presenters to format raw values via transform

**This is the highest-risk pitfall because it's the foundation for everything else.**

---

## Pitfall 8: Combining Features — Exponential Interaction Surface

**What goes wrong:** Features that work individually break when combined: Bar chart + DateTime axis + custom formatter, or Contour plot + log scale + multi-series legend.

**Why it happens:** Each feature adds assumptions about axis type, data model, and rendering path. Combining them exposes conflicting assumptions.

**How to avoid:**
1. Design the axis transform layer to be composable
2. Test every series type with every axis type
3. Test every formatter with every axis type
4. Keep the rendering pipeline generic enough to handle all combinations

**Warning signs:**
- Feature X works alone but breaks when Feature Y is also enabled
- Axis labels showing wrong values in combined mode
- Performance degradation in combined mode

**Suggested phase:** Integration testing phase (after all individual features)

---

## Pitfall 9: Bar Chart Data Model — Categorical vs. Continuous

**What goes wrong:** Bar charts need categorical X/Z axes (discrete labels) while surface/scatter charts need continuous axes. Mixing these in the same axis model breaks assumptions.

**Why it happens:** `SurfaceAxisDescriptor` assumes continuous numeric range (Minimum/Maximum). Bar charts need `string[]` categories.

**How to avoid:**
1. Extend `SurfaceAxisScaleKind` with `Categorical = 4`
2. `SurfaceAxisDescriptor` gains optional `string[] Categories` property
3. When `ScaleKind == Categorical`, `Minimum`/`Maximum` are indices into `Categories`
4. Tick generator formats category labels instead of numeric values

**Warning signs:**
- Bar chart axes showing numeric indices instead of category names
- Probe readouts showing "2.5" instead of "Category B"
- Grid lines placed between categories instead of at boundaries

**Suggested phase:** Bar chart series (with categorical axis support)

---

## Pitfall 10: Contour Plot Performance — CPU vs. GPU

**What goes wrong:** Marching squares on large grids (>1000x1000) is CPU-intensive. If done every frame, it kills performance.

**Why it happens:** Contour extraction is inherently serial (each cell depends on neighbors). It can't be easily parallelized on GPU.

**How to avoid:**
1. Extract contour lines once when data changes, cache the result
2. Invalidate cache only when: data changes, contour levels change, or zoom changes (for adaptive detail)
3. Use the existing `SurfaceChartRenderChangeSet` pattern for dirty tracking
4. For filled contours: use GPU shaders (color by value threshold)

**Warning signs:**
- Frame rate drops when contour plot is active
- Re-extraction happening every frame
- Memory usage growing unbounded with cached contours

**Suggested phase:** Contour plot series (with caching strategy)

---

## Pitfall 11: Legend Layout — 2D Overlay vs. 3D Scene

**What goes wrong:** Legend is currently a 2D overlay (screen-space). If legend needs to be in 3D scene space (e.g., attached to a corner of the 3D box), it requires different projection logic.

**Why it happens:** `SurfaceLegendOverlayPresenter` uses screen-space coordinates (`Rect`, `Point`). 3D legend would need projection from world-space.

**How to avoid:**
1. Keep legend as 2D overlay (current approach) — simpler, always readable
2. If 3D legend is needed later, create a separate `SurfaceLegend3DPresenter`
3. Don't mix 2D and 3D legend logic in the same presenter

**Warning signs:**
- Legend text becoming unreadable when camera rotates
- Legend clipping through 3D geometry
- Legend position not updating with camera movement

**Suggested phase:** Keep legend 2D for v2.53; defer 3D legend to future milestone

---

## Pitfall 12: Custom Formatter — Axis Key Convention Fragility

**What goes wrong:** The formatter receives axis keys ("X", "Y", "Z", "Legend") as strings. If a user doesn't know the convention, their formatter won't work for all axes.

**Why it happens:** `SurfaceChartOverlayOptions.FormatLabel()` uses string comparison to route to the right formatter. The axis keys are hardcoded in `SurfaceAxisOverlayPresenter`.

**How to avoid:**
1. Document the axis key convention in XML docs
2. Consider using an enum instead of string: `AxisKey.X`, `AxisKey.Y`, `AxisKey.Z`, `AxisKey.Legend`
3. Provide a `FormatAllAxes(Func<double, string>)` convenience method

**Warning signs:**
- User's formatter only works for X axis but not Y/Z
- Formatter receives unexpected axis keys
- Documentation doesn't mention the convention

**Suggested phase:** Custom tick formatter support (with enum-based axis keys)

---

## Summary: Recommended Phase Ordering

| Phase | Feature | Why This Order |
|-------|---------|----------------|
| 1 | Axis Transform Infrastructure | Foundation for everything else |
| 2 | DateTime Axis Support | Uses axis transform; needed by bar chart |
| 3 | Log Scale Support | Uses axis transform; independent of DateTime |
| 4 | Custom Tick Formatters | Uses axis transform; can parallel with 2/3 |
| 5 | Bar Chart Series | Uses categorical axis + DateTime |
| 6 | Contour Plot Series | Independent of bar chart |
| 7 | Chart Legend Expansion | Needs all series types defined |
| 8 | Integration Testing | Tests all combinations |

---

## Integration Pitfalls Matrix

| Feature Combination | Risk | Pitfall |
|---------------------|------|---------|
| Bar + DateTime | HIGH | Categorical X vs. DateTime X conflict |
| Bar + Log | MEDIUM | Bar height on log scale — zero problem |
| Contour + Log | MEDIUM | Isoline extraction on log-transformed data |
| DateTime + Custom Formatter | LOW | Formatter must handle DateTime values |
| Log + Custom Formatter | LOW | Formatter must handle log-space values |
| Multi-series Legend + Bar | MEDIUM | Legend entry types must be extensible |
| Bar + Contour (same plot) | HIGH | Different data models, same axis space |

---

## Project Constraints (from AGENTS.md)

- **No god code:** Each feature must be bounded and chart-local
- **No compat layers:** Don't create wrappers for removed APIs
- **No hidden fallback/downshift:** Unsupported axis/series combinations = explicit diagnostics
- **Single chart control:** `VideraChartView` is the only shipped control
- **Plot.Add.* is the data path:** No direct public `Source` API
- **Chart-local rendering:** Don't merge chart semantics into VideraView
