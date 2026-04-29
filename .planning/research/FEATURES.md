# Feature Research: v2.53 Chart Type Expansion and Axis Semantics

**Researched:** 2026-04-29
**Domain:** 3D chart type expansion, axis semantics, and chart legend for Videra SurfaceCharts
**Confidence:** HIGH (architecture patterns), MEDIUM (implementation complexity estimates)

## Summary

This research covers six features for the v2.53 milestone: Bar charts, Contour plots, DateTime axis, Log scale, Custom tick formatters, and Chart legend. The existing SurfaceCharts architecture provides a strong foundation — the `Plot3D`/`Plot3DSeries`/`Plot3DAddApi` pattern is extensible, `SurfaceAxisScaleKind` already has `Log` and `DateTime` enum values, and the overlay system already has legend and axis infrastructure. The key architectural insight is that most features can be added by extending existing contracts rather than introducing new rendering paths.

**Primary recommendation:** Extend the existing `Plot3DSeriesKind` and `SurfaceAxisScaleKind` contracts, add new Core data models for bar and contour data, and evolve the overlay system to support multi-series legends and axis-specific formatters. The rendering layer needs the most new work for bar charts (cuboid mesh generation) and contour plots (iso-line extraction + 3D line rendering).

---

## User Constraints

> No CONTEXT.md exists for this research. This is project-level feature research, not a phase-specific research task.

---

## Phase Requirements

> No phase requirement IDs provided — this is exploratory feature research for milestone planning.

---

## Feature Analysis

### 1. Bar Chart (`Plot3DSeriesKind.Bar`)

**Category:** TABLE STAKES — Bar charts are a fundamental chart type expected in any professional charting library.

**Expected behavior:**
- 3D bars rising from a base plane at grid positions
- Each bar's height represents the value at that grid cell
- Color can be per-bar (from colormap) or uniform
- Configurable bar width, gap ratio, and base plane height
- Bars should be pickable (hover probe shows value)
- Works with existing axis overlay, legend, and snapshot export

**Data model options:**

| Option | Description | Complexity | Recommendation |
|--------|-------------|------------|----------------|
| Reuse `SurfaceMatrix` | Same grid data, different rendering | Low | ✅ Best fit — bars are just discrete surface |
| New `BarChartData` type | Separate data model | Medium | ❌ Unnecessary — same grid semantics |
| Columnar bar data | Like `ScatterColumnarSeries` | Medium | ❌ Over-engineering for v2.53 |

**Rendering approach:**
- Generate cuboid mesh geometry per bar (6 faces, 12 triangles each)
- Reuse existing color map infrastructure for value-to-color mapping
- Can share the same GPU pipeline as surface rendering (same vertex format)
- Software fallback: draw filled rectangles in 2D projection

**Dependencies on existing code:**
- `Plot3DAddApi` — add `Bar()` overloads
- `Plot3DSeries` — extend with bar-specific config (bar width, gap, base height)
- `SurfaceRenderer` — extend to handle bar mesh generation
- `SurfaceColorMap` — reuse for value-to-color mapping
- `SurfaceAxisOverlayPresenter` — works as-is (same axis semantics)
- `SurfaceLegendOverlayPresenter` — works as-is (same color range)

**Complexity:** MEDIUM — Mesh generation is straightforward; the main work is integrating bar rendering into the existing GPU/software pipeline without duplicating too much.

**Key design decisions:**
- Bar width as fraction of grid cell (default 0.8)
- Gap ratio between bars (default 0.2)
- Base plane Y coordinate (default = value range minimum)
- Whether bars can be stacked or grouped (defer to later milestone)

---

### 2. Contour Plot (`Plot3DSeriesKind.Contour`)

**Category:** MEANINGFUL DIFFERENTIATOR — Contour plots are a professional analysis feature that differentiates Videra from basic chart libraries.

**Expected behavior:**
- Iso-lines rendered at specific value levels on the surface
- Lines can be rendered at the surface height or projected onto a base plane
- Configurable contour levels (auto-generated or user-specified)
- Optional filled contours (between iso-lines)
- Works with existing colormap for line coloring

**Data model options:**

| Option | Description | Complexity | Recommendation |
|--------|-------------|------------|----------------|
| Derive from `SurfaceMatrix` | Extract contours from existing surface data | Medium | ✅ Best fit — contours are derived data |
| New `ContourData` type | Pre-computed contour lines | Medium | ❌ Unnecessary — contouring is deterministic |
| Real-time contour extraction | Compute contours during rendering | High | ❌ Too expensive for interactive use |

**Rendering approach:**
- **Marching squares algorithm** to extract iso-lines from the surface grid
- Render as 3D line segments at the surface height
- Optional: project contours onto base plane (2D contour map)
- Line thickness and color per contour level
- Can reuse existing line rendering infrastructure (scatter `ConnectPoints`)

**Dependencies on existing code:**
- `Plot3DAddApi` — add `Contour()` overloads
- `SurfaceMatrix` — source data for contour extraction
- `SurfaceRenderer` — extend to handle line rendering
- `SurfaceColorMap` — reuse for contour level coloring
- New: `ContourExtractor` in Core — marching squares implementation

**Complexity:** MEDIUM-HIGH — The marching squares algorithm is well-documented but needs careful implementation for edge cases (saddle points, ambiguous cases). The 3D line rendering is simpler than surface rendering.

**Key design decisions:**
- Auto-generated contour levels (nice intervals) vs user-specified
- Number of contour levels (default 10-15)
- Line rendering style (solid, dashed, colored)
- Whether to support filled contours (defer to later milestone)

---

### 3. DateTime Axis (`SurfaceAxisScaleKind.DateTime`)

**Category:** TABLE STAKES — DateTime axes are essential for time-series data visualization.

**Expected behavior:**
- Axis labels show formatted DateTime values (e.g., "2026-01-15", "14:30:00", "Jan 2026")
- Tick generation respects time intervals (seconds, minutes, hours, days, months, years)
- Tick labels adapt to the visible range (show hours when zoomed in, months when zoomed out)
- Supports DateTime stored as `double` (OADate, Unix timestamp, or ticks)

**Current state:**
- `SurfaceAxisScaleKind.DateTime` exists in the enum
- `SurfaceGeometryGrid.MapNormalizedCoordinate()` maps it linearly (same as `Linear`)
- No DateTime-specific tick generation or formatting

**Implementation approach:**

The axis stores DateTime values as `double` (OADate format recommended — `DateTime.ToOADate()` / `DateTime.FromOADate()`). The tick generator needs to:
1. Determine appropriate time interval based on visible range
2. Generate ticks at nice time boundaries (start of hour, day, month, etc.)
3. Format labels according to the interval

**Time interval selection:**

| Visible Range | Interval | Label Format |
|---------------|----------|--------------|
| < 2 seconds | 100ms, 500ms, 1s | "HH:mm:ss.fff" |
| < 2 minutes | 1s, 5s, 10s, 30s | "HH:mm:ss" |
| < 2 hours | 1m, 5m, 10m, 30m | "HH:mm" |
| < 2 days | 1h, 3h, 6h, 12h | "HH:mm" or "MM-dd HH:mm" |
| < 2 months | 1d, 7d, 14d | "MM-dd" |
| < 2 years | 1M, 3M, 6M | "yyyy-MM" |
| > 2 years | 1Y, 2Y, 5Y | "yyyy" |

**Dependencies on existing code:**
- `SurfaceAxisDescriptor` — already has `ScaleKind` property
- `SurfaceAxisTickGenerator` — needs new `CreateDateTimeTickValues()` method
- `SurfaceChartOverlayOptions` — needs DateTime-specific formatter
- `SurfaceGeometryGrid.MapNormalizedCoordinate()` — already handles `DateTime` case (linear mapping)
- Overlay presenters — need to use DateTime formatter for DateTime axes

**Complexity:** MEDIUM — The main work is the tick generator. The DateTime-to-double mapping is straightforward (OADate). The formatter can use standard .NET DateTime format strings.

**Key design decisions:**
- Storage format: OADate (recommended) vs Unix timestamp vs ticks
- Whether to support timezone conversion (defer to later milestone)
- Whether to support custom DateTime format strings (yes, via `LabelFormatter`)

---

### 4. Log Scale (`SurfaceAxisScaleKind.Log`)

**Category:** TABLE STAKES — Logarithmic scales are essential for scientific and engineering data.

**Expected behavior:**
- Axis values are mapped logarithmically (base 10)
- Ticks appear at powers of 10 (1, 10, 100, 1000, ...)
- Minor ticks at 2×, 3×, ..., 9× of each power
- Labels show values in scientific notation or plain numbers
- Handles edge cases: zero, negative values, very small/large ranges

**Current state:**
- `SurfaceAxisScaleKind.Log` exists in the enum
- `SurfaceAxisDescriptor` constructor throws: "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated."
- `SurfaceGeometryGrid.MapNormalizedCoordinate()` has log mapping: `Math.Pow(10, log10(min) + (log10(max) - log10(min)) * normalized)`

**Implementation approach:**

The log mapping is already implemented in `SurfaceGeometryGrid`. The main work is:
1. Remove the "reserved" exception from `SurfaceAxisDescriptor`
2. Add validation: minimum must be > 0 for log scale
3. Implement log-aware tick generation
4. Implement log-aware tick formatting

**Log tick generation:**

```
For range [1, 1000]:
  Major ticks: 1, 10, 100, 1000
  Minor ticks: 2, 3, 4, 5, 6, 7, 8, 9, 20, 30, ..., 90, 200, 300, ..., 900
```

**Dependencies on existing code:**
- `SurfaceAxisDescriptor` — remove exception, add validation
- `SurfaceAxisTickGenerator` — add `CreateLogTickValues()` method
- `SurfaceChartOverlayOptions` — add log-specific formatter (scientific notation)
- `SurfaceGeometryGrid.MapNormalizedCoordinate()` — already handles `Log` case
- Overlay presenters — need to use log formatter for log axes

**Complexity:** MEDIUM — The tick generation is straightforward (powers of 10). The main challenge is edge case handling (zero, negative, very small ranges).

**Key design decisions:**
- Base-10 only vs configurable base (base-10 only for v2.53)
- How to handle zero/negative values (throw exception or clamp to minimum positive)
- Whether to support log-log plots (yes, if both axes are log scale)

---

### 5. Custom Tick Formatters

**Category:** TABLE STAKES — Custom formatters are essential for professional chart presentation.

**Expected behavior:**
- User can provide a custom formatter function per axis
- Formatter receives axis key and value, returns formatted string
- Can be used for custom numeric formats, DateTime formats, unit suffixes, etc.
- Works with all axis types (linear, log, DateTime)

**Current state:**
- `SurfaceChartOverlayOptions.LabelFormatter` already exists: `Func<string, double, string>?`
- Used by `FormatLabel()` method
- Already supports custom formatting for all axes

**Implementation approach:**

The existing `LabelFormatter` property is already sufficient for custom formatting. The main work is:
1. Add per-axis formatter support (separate formatters for X, Y, Z, Legend)
2. Add built-in DateTime formatter
3. Add built-in log formatter
4. Document the formatter API

**Dependencies on existing code:**
- `SurfaceChartOverlayOptions` — extend with per-axis formatters
- `SurfaceAxisOverlayPresenter` — already uses `FormatLabel()`
- `SurfaceLegendOverlayPresenter` — already uses `FormatLabel()`

**Complexity:** LOW — The infrastructure already exists. The main work is adding per-axis formatter properties and built-in formatters.

**Key design decisions:**
- Single `LabelFormatter` vs per-axis formatters (both — single for simple cases, per-axis for complex)
- Whether to expose formatter as a class/interface vs delegate (delegate for simplicity)
- Whether to support formatter composition (defer to later milestone)

---

### 6. Chart Legend

**Category:** TABLE STAKES — A chart legend is essential for multi-series charts.

**Expected behavior:**
- Shows series name, color, and visibility for each series in the plot
- Supports toggling series visibility by clicking legend items
- Adapts to the number of series (auto-layout)
- Works with all chart types (surface, waterfall, scatter, bar, contour)

**Current state:**
- `SurfaceLegendOverlayState` exists — shows color range for active series only
- `SurfaceLegendOverlayPresenter` exists — renders color swatch with min/max labels
- No multi-series support

**Implementation approach:**

The existing legend shows a color range for the active surface series. For multi-series support:
1. Extend `SurfaceLegendOverlayState` with series entries (name, color, visibility)
2. Add legend item rendering (color swatch + name)
3. Add click handling for visibility toggling
4. Position legend appropriately (auto-layout)

**Dependencies on existing code:**
- `SurfaceLegendOverlayState` — extend with series entries
- `SurfaceLegendOverlayPresenter` — extend with series item rendering
- `Plot3D` — expose series list for legend
- `Plot3DSeries` — add visibility property
- `VideraChartView` — add legend click handling

**Complexity:** LOW-MEDIUM — The infrastructure exists. The main work is extending the state/presenter with multi-series support and adding click handling.

**Key design decisions:**
- Legend position (right side, bottom, auto)
- Whether to show color range alongside series list (yes, for surface/waterfall)
- Whether to support legend item click for visibility toggling (yes)
- Whether to support legend item selection for active series (defer to later milestone)

---

## Table Stakes vs Differentiators vs Anti-Features

| Feature | Category | Rationale |
|---------|----------|-----------|
| Bar Chart | **Table Stakes** | Fundamental chart type; users expect it |
| Contour Plot | **Differentiator** | Professional analysis feature; not in basic chart libraries |
| DateTime Axis | **Table Stakes** | Essential for time-series data |
| Log Scale | **Table Stakes** | Essential for scientific/engineering data |
| Custom Formatters | **Table Stakes** | Essential for professional presentation |
| Chart Legend | **Table Stakes** | Essential for multi-series charts |

**Anti-Features (deferred):**

| Feature | Why Deferred |
|---------|--------------|
| Stacked/grouped bars | Adds complexity; single-series bars are sufficient for v2.53 |
| Filled contours | Rendering complexity; line contours are sufficient |
| Log-log plots | Edge case; single log axis is sufficient |
| Animated transitions | Not aligned with Videra's static-scene model |
| Interactive contour editing | Over-scoping; static contours are sufficient |
| 3D surface from contours | Different feature; contour is visualization, not data source |

---

## Complexity Summary

| Feature | Complexity | Main Challenge |
|---------|------------|----------------|
| Bar Chart | MEDIUM | Cuboid mesh generation + GPU pipeline integration |
| Contour Plot | MEDIUM-HIGH | Marching squares algorithm + 3D line rendering |
| DateTime Axis | MEDIUM | Time-interval-aware tick generation |
| Log Scale | MEDIUM | Edge case handling (zero, negative, small ranges) |
| Custom Formatters | LOW | API design (per-axis vs single formatter) |
| Chart Legend | LOW-MEDIUM | Multi-series state + click handling |

---

## Dependencies on Existing Code

### Strong Dependencies (must modify)

| Component | Features Affected | Change Required |
|-----------|-------------------|-----------------|
| `Plot3DSeriesKind` | Bar, Contour | Add `Bar`, `Contour` enum values |
| `Plot3DAddApi` | Bar, Contour | Add `Bar()`, `Contour()` methods |
| `Plot3DSeries` | Bar, Contour | Add bar/contour-specific config |
| `SurfaceAxisScaleKind` | DateTime, Log | Already exists; remove Log exception |
| `SurfaceAxisDescriptor` | Log | Remove "reserved" exception |
| `SurfaceAxisTickGenerator` | DateTime, Log | Add `CreateDateTimeTickValues()`, `CreateLogTickValues()` |
| `SurfaceChartOverlayOptions` | DateTime, Log, Formatters | Add per-axis formatters, DateTime/log presets |
| `SurfaceLegendOverlayState` | Legend | Add multi-series entries |
| `SurfaceLegendOverlayPresenter` | Legend | Add series item rendering |

### Weak Dependencies (extend or reuse)

| Component | Features Affected | Change Required |
|-----------|-------------------|-----------------|
| `SurfaceRenderer` | Bar, Contour | Add bar mesh generation, contour line rendering |
| `SurfaceColorMap` | Bar, Contour | Reuse for value-to-color mapping |
| `SurfaceGeometryGrid` | DateTime, Log | Already handles both (no change) |
| `VideraChartView` | Legend | Add legend click handling |
| `Plot3D` | Legend | Expose series list |

### No Change Required

| Component | Features Affected | Reason |
|-----------|-------------------|--------|
| `SurfaceMetadata` | All | Axis descriptors already support scale kinds |
| `SurfaceMatrix` | Bar, Contour | Reuse as data source |
| `ScatterChartData` | None | Not affected by new features |
| `ISurfaceChartRenderBackend` | Bar, Contour | Extend via new render methods |
| `SurfaceChartProjection` | All | Works with any axis scale |

---

## Recommended Implementation Order

1. **Log Scale** — Remove exception, add tick generator, add formatter. Low risk, high value.
2. **Custom Formatters** — Extend existing `LabelFormatter` API. Low risk, enables DateTime.
3. **DateTime Axis** — Add tick generator + formatter. Medium risk, depends on formatters.
4. **Chart Legend** — Extend existing legend infrastructure. Low-medium risk.
5. **Bar Chart** — New chart type. Medium risk, needs rendering work.
6. **Contour Plot** — New chart type. Medium-high risk, needs algorithm + rendering.

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `SurfaceChartView` per chart type | Single `VideraChartView` + `Plot.Add.*` | v2.47-v2.48 | All new chart types go through `Plot3DAddApi` |
| Direct `Source` API | `Plot.Add.Surface/Waterfall/Scatter` | v2.48 | Data loading through Plot model |
| Per-chart overlay logic | Unified overlay system (axis, legend, probe) | v2.43-v2.50 | New features extend existing overlay |
| `SurfaceAxisScaleKind.Log` reserved | Log mapping exists in `SurfaceGeometryGrid` | Pre-v2.53 | Mapping ready, tick generation needed |
| `SurfaceAxisScaleKind.DateTime` linear | DateTime mapping exists in `SurfaceGeometryGrid` | Pre-v2.53 | Mapping ready, tick generation needed |

**Deprecated/outdated:**
- `SurfaceChartView`, `WaterfallChartView`, `ScatterChartView` — replaced by `VideraChartView`
- Direct `VideraChartView.Source` API — replaced by `Plot.Add.*`

---

## Open Questions

1. **Bar chart rendering approach**
   - What we know: Cuboid mesh generation is straightforward; can reuse surface pipeline
   - What's unclear: Whether to use instanced rendering (one mesh per bar) or merged geometry
   - Recommendation: Start with merged geometry; optimize with instancing if performance is poor

2. **Contour extraction performance**
   - What we know: Marching squares is O(n) where n = grid cells
   - What's unclear: Performance on large grids (1000×1000+)
   - Recommendation: Extract contours once when data changes; cache the result

3. **DateTime storage format**
   - What we know: OADate is the .NET standard for DateTime-to-double conversion
   - What's unclear: Whether users will want Unix timestamps or custom epochs
   - Recommendation: Use OADate internally; support custom epochs via `LabelFormatter`

4. **Log scale edge cases**
   - What we know: Zero and negative values cannot be represented on a log scale
   - What's unclear: Whether to throw, clamp, or skip invalid values
   - Recommendation: Throw on construction if minimum ≤ 0; document the constraint

5. **Legend interaction model**
   - What we know: Click-to-toggle visibility is standard in chart libraries
   - What's unclear: Whether to support click-to-select active series
   - Recommendation: Visibility toggling only for v2.53; active series selection deferred

---

## Sources

### Primary (HIGH confidence)
- Existing codebase: `src/Videra.SurfaceCharts.Core/`, `src/Videra.SurfaceCharts.Avalonia/`, `src/Videra.SurfaceCharts.Rendering/`
- `SurfaceAxisScaleKind.cs` — enum values for Linear, Log, DateTime, ExplicitCoordinates
- `SurfaceGeometryGrid.cs` — `MapNormalizedCoordinate()` already handles Log and DateTime
- `SurfaceChartOverlayOptions.cs` — `LabelFormatter` property already exists
- `SurfaceLegendOverlayState.cs` / `SurfaceLegendOverlayPresenter.cs` — legend infrastructure exists
- `SurfaceAxisTickGenerator.cs` — linear tick generation exists; needs DateTime/log variants

### Secondary (MEDIUM confidence)
- Marching squares algorithm — well-documented contour extraction algorithm
- OADate format — .NET standard for DateTime-to-double conversion
- Log scale tick generation — standard approach (powers of 10)

### Tertiary (LOW confidence)
- Complexity estimates — based on experience with similar chart libraries; actual complexity may vary

---

## Metadata

**Confidence breakdown:**
- Architecture patterns: HIGH — existing codebase is well-structured and extensible
- Feature behavior: HIGH — features are well-defined in the charting literature
- Implementation complexity: MEDIUM — estimates based on similar projects; actual may vary
- Integration points: HIGH — existing contracts are clear and well-documented

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (30 days — stable architecture, feature scope may evolve)
