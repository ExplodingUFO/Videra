# Project Research Summary

**Project:** Videra v2.53 — Chart Type Expansion and Axis Semantics
**Domain:** 3D scientific/analytical charting library (.NET + Avalonia + SkiaSharp)
**Researched:** 2026-04-29
**Confidence:** HIGH

## Executive Summary

Videra SurfaceCharts is a 3D charting module built on .NET 8, Avalonia, and SkiaSharp (via Avalonia's DrawingContext). It currently supports three chart families — Surface, Waterfall, Scatter — with a linear-only axis model. The v2.53 milestone adds bar charts, contour plots, DateTime axes, log scale, custom tick formatters, and an enhanced chart legend. The research concludes that **no new dependencies are required** — all features are implementable within the existing stack using built-in .NET 8 APIs.

The most important architectural finding is that **most infrastructure already exists in skeleton form**. The `SurfaceAxisScaleKind` enum already defines `Log` and `DateTime` values. The `SurfaceGeometryGrid.MapNormalizedCoordinate()` already contains correct log-scale math. The `SurfaceChartOverlayOptions.LabelFormatter` delegate already exists. The `SurfaceLegendOverlayPresenter` already renders a color-mapped swatch. The work is not "build from scratch" but "unblock, implement, and wire" the reserved/placeholder paths — plus add two genuinely new chart types (Bar, Contour) following the established `Plot3DSeriesKind` → `Plot3DAddApi` → renderer → overlay pattern.

The primary risk is **feature interaction complexity**: each feature touches the axis/overlay/rendering pipeline at different points, and combining them (e.g., Bar chart + DateTime axis + custom formatter) exposes conflicting assumptions. The mitigation is a dependency-aware build order: axis foundations first, then legend, then new chart types, then integration testing. The `SurfaceAxisDescriptor` constructor's `throw` on `SurfaceAxisScaleKind.Log` is the single highest-priority blocker — removing it (with proper validation) unblocks the entire axis enhancement path.

## Key Findings

### Recommended Stack

**No stack changes needed.** All six features build entirely on the existing .NET 8 + Avalonia + SkiaSharp stack. The contour algorithm (marching squares) is ~100 lines of C#. DateTime handling uses built-in `System.DateTimeOffset` and `System.Globalization`. Log scale uses `Math.Log10`/`Math.Pow`. Custom tick formatters use the existing `Func<string, double, string>?` delegate. Bar chart geometry uses `System.Numerics.Vector3` already in use.

**Core technologies (unchanged):**
- `.NET 8.0`: Runtime — all features use built-in APIs (Math, DateTime, Numerics)
- `Avalonia 11.3.9`: Control shell and overlay rendering — DrawingContext wraps SkiaSharp
- `Silk.NET`: GPU backends — not involved in new chart types

**Alternatives rejected:**
- `MathNet.Numerics` for marching squares — adds 3MB dependency for one algorithm
- `HelixToolkit` for bar geometry — duplicates Videra's own 3D engine
- `ScottPlot`/`LiveCharts2` for contour — 2D-only; Videra needs 3D-projected contour lines

### Expected Features

**Must have (table stakes):**
- **Bar Chart** — fundamental chart type; 3D bars rising from base plane at grid positions
- **DateTime Axis** — essential for time-series data; tick generation at calendar boundaries
- **Log Scale** — essential for scientific/engineering data; powers-of-10 ticks
- **Custom Tick Formatters** — essential for professional presentation; per-axis formatter support
- **Chart Legend** — essential for multi-series charts; series name + color + visibility

**Should have (differentiator):**
- **Contour Plot** — professional analysis feature; iso-lines rendered at specific value levels on the surface

**Defer to later milestones:**
- Stacked/grouped bars — single-series bars sufficient for v2.53
- Filled contours — line contours sufficient; filled requires triangulation
- Log-log plots — single log axis sufficient
- Animated transitions — not aligned with Videra's static-scene model
- Legend interactivity (click-to-toggle) — read-only legend for v2.53
- Categorical axis extension — bar charts can use numeric indices initially

### Architecture Approach

The existing SurfaceCharts architecture follows a clean `Core` → `Rendering` → `Avalonia` layer split with a well-established chart family addition pattern (established by Scatter v2.1, Waterfall v1.27). New chart types follow a 4-step pattern: (1) add enum value to `Plot3DSeriesKind`, (2) add `Plot3DAddApi.*()` method, (3) create Core data model + renderer, (4) wire rendering path in `VideraChartView`. The overlay system uses a coordinator → presenter → state pattern that is cleanly extensible.

**Major components to modify:**
1. `SurfaceAxisDescriptor` — remove Log throw, add validation (min > 0)
2. `SurfaceAxisTickGenerator` — add log-aware and DateTime-aware tick generation
3. `SurfaceChartOverlayOptions` — add per-axis formatters, DateTime format, legend position
4. `SurfaceLegendOverlayState`/`Presenter` — redesign for multi-series entries
5. `Plot3DSeriesKind`/`Plot3DAddApi`/`Plot3DSeries` — add Bar, Contour enum values and methods

**Major components to create:**
1. `BarChartData`/`BarRenderer`/`BarRenderScene` — bar chart data model and rendering
2. `ContourGenerator`/`ContourLine`/`ContourRenderer` — marching squares + 3D line rendering
3. `DateTimeAxisHelper` — DateTime↔double conversion utilities

### Critical Pitfalls

1. **Log Scale with Zero/Negative Values** — `Math.Log10(0)` = `-∞`, `Math.Log10(-1)` = `NaN`. Validate `axis.Minimum > 0` when `ScaleKind == Log` in `SurfaceAxisDescriptor` constructor.

2. **DateTime Precision Loss** — `double` has ~15 significant digits; storing raw ticks loses precision. Use seconds-since-epoch (not ticks) for DateTime axis values. Millisecond precision via fractional seconds.

3. **Raw vs. Display Space Confusion** — The axis stores raw values but rendering needs display-space coordinates. For log scale, ticks must be placed at powers of 10, not linearly spaced. The `SurfaceGeometryGrid.MapNormalizedCoordinate` already handles this correctly — the blocker is only the `throw` in `SurfaceAxisDescriptor`.

4. **Contour Line Discontinuity** — Marching squares produces disconnected segments at saddle points. Use asymptotic decider for disambiguation, or accept segment-based rendering (simpler).

5. **Bar Chart Z-Fighting** — Adjacent bar faces overlap and flicker. Add small gaps between bars (1-2% of bar width) or use consistent triangle winding order.

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Axis Foundation — Log Scale + DateTime Axis + Custom Formatters
**Rationale:** Axis enhancements are prerequisites for Bar and Contour with non-linear axes. The `SurfaceAxisDescriptor` throw-on-Log is the single highest-priority blocker. Custom formatters are needed by DateTime axis. These three features share the same files (`SurfaceAxisDescriptor`, `SurfaceAxisTickGenerator`, `SurfaceAxisOverlayPresenter`, `SurfaceChartOverlayOptions`) and should be done together to avoid merge conflicts.
**Delivers:** Unblocked log-scale axes, DateTime-aware tick generation, per-axis formatter support
**Addresses:** Log Scale (table stakes), DateTime Axis (table stakes), Custom Formatters (table stakes)
**Avoids:** Pitfall 1 (raw vs display space), Pitfall 2 (DateTime tick generation), Pitfall 5 (formatter breaking pipeline), Pitfall 7 (axis transform foundation)
**Key files:** `SurfaceAxisDescriptor.cs`, `SurfaceAxisTickGenerator.cs`, `SurfaceAxisOverlayPresenter.cs`, `SurfaceChartOverlayOptions.cs`

### Phase 2: Enhanced Chart Legend
**Rationale:** Legend should be generalized before new chart types need it. The existing `SurfaceLegendOverlayPresenter` renders a single color swatch — it needs to support multi-series entries with per-kind indicators (swatch for surface, dot for scatter, rectangle for bar, line for contour). Doing this before Bar/Contour means those phases can immediately use the new legend.
**Delivers:** Multi-series legend with per-kind indicators, position options
**Addresses:** Chart Legend (table stakes)
**Avoids:** Pitfall 6 (multi-series legend complexity), Pitfall 11 (keep legend 2D)
**Key files:** `SurfaceLegendOverlayState.cs`, `SurfaceLegendOverlayPresenter.cs`, `SurfaceChartOverlayCoordinator.cs`, `SurfaceChartOverlayOptions.cs`

### Phase 3: Bar Chart Series
**Rationale:** Bar chart is simpler than Contour (no algorithm needed), should be built first. It follows the established `Plot3DSeriesKind` → `Plot3DAddApi` → renderer → overlay pattern. Can reuse `SurfaceColorMap` for value-to-color mapping and `SurfaceMatrix` as data source.
**Delivers:** 3D bar chart rendering, `Plot3DSeriesKind.Bar`, `Plot3DAddApi.Bar()` method
**Addresses:** Bar Chart (table stakes)
**Avoids:** Pitfall 3 (3D geometry and depth sorting), Pitfall 4 (z-fighting)
**Key files:** `BarChartData.cs` (NEW), `BarRenderer.cs` (NEW), `BarRenderScene.cs` (NEW), `Plot3DSeriesKind.cs`, `Plot3DAddApi.cs`, `Plot3DSeries.cs`, `SurfaceScenePainter.cs`

### Phase 4: Contour Plot Series
**Rationale:** Contour requires marching squares algorithm — the most complex new component. Should be built after Bar to avoid blocking on algorithm complexity. The marching squares algorithm is well-documented (~100 lines of C#) but needs careful handling of edge cases (saddle points, NaN regions).
**Delivers:** Contour plot rendering, `Plot3DSeriesKind.Contour`, `Plot3DAddApi.Contour()` method
**Addresses:** Contour Plot (differentiator)
**Avoids:** Pitfall 4 (contour line discontinuity), Pitfall 10 (contour performance — cache results)
**Key files:** `ContourGenerator.cs` (NEW), `ContourLine.cs` (NEW), `ContourRenderer.cs` (NEW), `ContourRenderScene.cs` (NEW), `Plot3DSeriesKind.cs`, `Plot3DAddApi.cs`, `SurfaceScenePainter.cs`

### Phase 5: Integration and Evidence
**Rationale:** Wire everything into diagnostics, demo, and guardrails. Test all feature combinations (Bar + DateTime, Contour + Log, etc.) to catch interaction bugs.
**Delivers:** Updated evidence/diagnostics, demo scenarios, scope guardrails
**Avoids:** Pitfall 8 (feature combination interaction surface)
**Key files:** `Plot3DOutputEvidence.cs`, `Plot3DDatasetEvidence.cs`, demo code, guardrail scripts

### Phase Ordering Rationale

- **Axis first:** Everything depends on axis support. The Log throw is a one-line removal but requires validation logic. DateTime needs tick generation. Formatters are needed by both.
- **Legend before chart types:** New chart types need legend entries. Building legend first means Bar/Contour can immediately integrate.
- **Bar before Contour:** Bar follows the established pattern with no algorithm complexity. Contour needs marching squares, which is the riskiest new component.
- **Integration last:** Feature combinations are the highest-risk area. Testing all axis×series×formatter combinations catches interaction bugs.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 4 (Contour):** Marching squares edge cases (saddle points, NaN handling) need algorithm-level detail. The 16-case lookup table and asymptotic decider need careful implementation.

Phases with standard patterns (skip research-phase):
- **Phase 1 (Axis Foundation):** Well-documented patterns. Log tick generation is standard (powers of 10). DateTime tick generation uses .NET's built-in DateTime/DateTimeOffset APIs. The `SurfaceGeometryGrid.MapNormalizedCoordinate` already has correct math.
- **Phase 2 (Legend):** Extending existing overlay presenter. Clear state/presenter pattern.
- **Phase 3 (Bar Chart):** Follows established `Plot3DSeriesKind` pattern exactly. Cuboid mesh generation is straightforward geometry.
- **Phase 5 (Integration):** Standard wiring and evidence update pattern.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | No new dependencies needed; all features build on existing .NET 8 + Avalonia + SkiaSharp stack |
| Features | HIGH | Features are well-defined in charting literature; table stakes vs differentiator classification is clear |
| Architecture | HIGH | Existing chart family addition pattern is well-established (Scatter v2.1, Waterfall v1.27); codebase analysis is direct |
| Pitfalls | MEDIUM | Log axis edge cases and DateTime precision are well-known but need testing; contour algorithm edge cases are standard but need implementation validation |

**Overall confidence:** HIGH — The research is grounded in direct source code analysis. All integration points are identified. The main uncertainty is in implementation complexity estimates, which are based on experience with similar chart libraries.

### Gaps to Address

- **Bar chart data model:** Research suggests reusing `SurfaceMatrix` (simplest) vs. new `BarChartData` type (more flexible). Decision needed during Phase 3 planning. Recommendation: start with `SurfaceMatrix` reuse; add `BarChartData` only if categorical axes are needed.
- **Contour rendering path:** Research suggests 3D line geometry in the scene (consistent with surfaces) vs. 2D overlay projection. Decision needed during Phase 4 planning. Recommendation: 3D line geometry for consistency.
- **DateTime storage format:** Research suggests OADate (FEATURES.md) vs. Unix seconds (ARCHITECTURE.md). Decision needed during Phase 1 planning. Recommendation: Unix seconds since epoch — simpler, standard, sufficient precision.
- **Legend overflow strategy:** Research suggests scrollable legend vs. truncation ("+N more"). Decision needed during Phase 2 planning. Recommendation: truncation for v2.53; scrolling is over-engineering.
- **Categorical axes for bar charts:** Research identifies this as a pitfall (Pitfall 9) but deferring categorical axis extension means bar chart axes show numeric indices. Decision needed during Phase 3 planning. Recommendation: defer categorical axes; use numeric indices with `LabelFormatter` for custom labels.

## Sources

### Primary (HIGH confidence)
- Source code: `src/Videra.SurfaceCharts.Core/SurfaceAxisScaleKind.cs` — enum values defined
- Source code: `src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs` — Log throw at line 46-51
- Source code: `src/Videra.SurfaceCharts.Core/SurfaceGeometryGrid.cs` — `MapNormalizedCoordinate` at line 127-143
- Source code: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs` — current series kinds
- Source code: `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs` — current Add API
- Source code: `src/Videra.SurfaceCharts.Avalonia/Controls/SurfaceChartOverlayOptions.cs` — `LabelFormatter` at line 59
- Source code: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs` — existing legend
- Source code: `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceAxisTickGenerator.cs` — linear tick generation

### Secondary (MEDIUM confidence)
- Marching squares algorithm — standard computational geometry, well-documented
- OADate format — .NET standard for DateTime-to-double conversion
- Log scale tick generation — standard approach (powers of 10)

### Tertiary (LOW confidence)
- Complexity estimates — based on experience with similar chart libraries; actual complexity may vary

---
*Research completed: 2026-04-29*
*Ready for roadmap: yes*
