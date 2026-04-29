# v2.53 Chart Type Expansion and Axis Semantics — Architecture Research

**Researched:** 2026-04-29
**Domain:** SurfaceCharts chart type expansion, axis semantics, legend system
**Confidence:** HIGH

## Summary

The existing Videra SurfaceCharts architecture is well-structured for extension. The `Plot3D` → `Plot3DSeries` → `Plot3DAddApi` model already supports three series kinds (Surface, Waterfall, Scatter) through a clean enum-based dispatch. The overlay system (`SurfaceChartOverlayCoordinator` → axis/legend/probe presenters) is decoupled from the render pipeline. The axis model (`SurfaceAxisDescriptor` + `SurfaceAxisScaleKind`) already defines `Log`, `DateTime`, and `ExplicitCoordinates` enum values, and the geometry grid (`SurfaceGeometryGrid.MapNormalizedCoordinate`) already contains log-scale math — but `SurfaceAxisDescriptor`'s constructor **throws** on `SurfaceAxisScaleKind.Log` with a message saying it's reserved until raw/display-space separation is implemented.

The key architectural insight: **most infrastructure already exists in skeleton form**. The work is not "build from scratch" but "unblock, implement, and wire" the reserved/placeholder paths, plus add two genuinely new chart types (Bar, Contour) that follow the established pattern.

**Primary recommendation:** Follow the existing `Plot3DSeriesKind` → `Plot3DAddApi` → renderer → overlay pattern for Bar and Contour. Unblock Log by implementing the raw→display separation. DateTime uses the existing `DateTime` scale kind with a new tick formatter. Custom tick formatters extend the existing `LabelFormatter` delegate. The legend system extends the existing `SurfaceLegendOverlayPresenter`.

## Standard Stack

### Core

| Component | Location | Purpose | Why Standard |
|-----------|----------|---------|--------------|
| `Plot3D` | `Controls/Plot/Plot3D.cs` | Series model owner, revision tracking | Single source of truth for chart state |
| `Plot3DSeries` | `Controls/Plot/Plot3DSeries.cs` | Immutable series descriptor | Clean data/behavior separation |
| `Plot3DSeriesKind` | `Controls/Plot/Plot3DSeriesKind.cs` | Chart family enum | Extension point for new types |
| `Plot3DAddApi` | `Controls/Plot/Plot3DAddApi.cs` | `Plot.Add.*` authoring API | Consumer-facing entry point |
| `SurfaceChartOverlayOptions` | `Controls/SurfaceChartOverlayOptions.cs` | Axis/grid/legend/label config | Already has `LabelFormatter` delegate |
| `SurfaceAxisDescriptor` | `Core/SurfaceAxisDescriptor.cs` | Axis label/unit/min/max/scale | Already has `SurfaceAxisScaleKind` enum |
| `SurfaceGeometryGrid` | `Core/SurfaceGeometryGrid.cs` | Sample→axis coordinate mapping | Already has Log math in `MapNormalizedCoordinate` |
| `SurfaceChartOverlayCoordinator` | `Controls/Overlay/SurfaceChartOverlayCoordinator.cs` | Overlay lifecycle | Already manages axis + legend + probe |

### Rendering Pipeline

| Component | Location | Purpose |
|-----------|----------|---------|
| `SurfaceRenderer` | `Core/Rendering/SurfaceRenderer.cs` | Tiles → `SurfaceRenderScene` |
| `ScatterRenderer` | `Core/Rendering/ScatterRenderer.cs` | `ScatterChartData` → `ScatterRenderScene` |
| `SurfaceScenePainter` | `Controls/SurfaceScenePainter.cs` | `SurfaceRenderScene` → Avalonia triangles |
| `SurfaceChartRenderHost` | `Rendering/SurfaceChartRenderHost.cs` | Backend-agnostic render orchestration |

### Overlay Presenters

| Component | Location | Purpose |
|-----------|----------|---------|
| `SurfaceAxisOverlayPresenter` | `Controls/Overlay/SurfaceAxisOverlayPresenter.cs` | 3D-projected axis lines, ticks, labels, grid |
| `SurfaceLegendOverlayPresenter` | `Controls/Overlay/SurfaceLegendOverlayPresenter.cs` | Color swatch legend with min/max labels |
| `SurfaceAxisTickGenerator` | `Controls/Overlay/SurfaceAxisTickGenerator.cs` | Nice-number tick value computation |

## Architecture Patterns

### Pattern 1: Series Kind Extension

**What:** Adding a new chart type follows a 4-step pattern already established by Surface/Waterfall/Scatter.

**When to use:** For Bar chart and Contour plot.

**Steps:**
1. Add enum value to `Plot3DSeriesKind`
2. Add `Plot3DAddApi.Bar(...)` / `Plot3DAddApi.Contour(...)` methods
3. Create data model in Core (e.g., `BarChartData`, `ContourChartData`)
4. Create renderer in Core (e.g., `BarRenderer`, `ContourRenderer`)
5. Wire rendering path in `VideraChartView`

**Example (existing Scatter pattern):**
```
Plot3DSeriesKind.Scatter           // enum value
Plot3DAddApi.Scatter(data, name)   // authoring API
ScatterChartData                   // Core data model
ScatterRenderer.BuildScene(data)   // Core renderer
ScatterRenderScene                 // Render-ready output
```

### Pattern 2: Axis Scale Extension

**What:** `SurfaceAxisScaleKind` already defines `Linear`, `Log`, `DateTime`, `ExplicitCoordinates`. The geometry grid's `MapNormalizedCoordinate` already handles `Log` math. The blocker is `SurfaceAxisDescriptor`'s constructor validation.

**When to use:** For Log scale and DateTime axis.

**Current state:**
```csharp
// SurfaceAxisDescriptor.cs line 46-51:
if (scaleKind == SurfaceAxisScaleKind.Log)
{
    throw new ArgumentException(
        "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated.",
        nameof(scaleKind));
}
```

**Resolution path:** Implement the raw↔display coordinate separation that the error message describes, then remove the throw.

### Pattern 3: Overlay Extension

**What:** The overlay system uses a coordinator → presenter → state pattern. Each overlay type (axis, legend, probe) has:
- A `*State` class (immutable snapshot)
- A `*Presenter` static class (creates state + renders)
- Coordinator integration in `SurfaceChartOverlayCoordinator.Refresh()`

**When to use:** For enhanced legend (multi-series), custom tick formatters.

## New Components Needed

### 1. Bar Chart

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `Plot3DSeriesKind.Bar` | Avalonia Controls | Enum value | **MODIFY** `Plot3DSeriesKind.cs` |
| `Plot3DAddApi.Bar(...)` | Avalonia Controls | Method | **MODIFY** `Plot3DAddApi.cs` |
| `BarChartData` | Core | Data model | **NEW** |
| `BarChartMetadata` | Core | Metadata | **NEW** |
| `BarRenderer` | Core/Rendering | Renderer | **NEW** |
| `BarRenderScene` | Core/Rendering | Render output | **NEW** |
| `BarRenderBar` | Core/Rendering | Single bar geometry | **NEW** |
| `Plot3DSeries` constructor | Avalonia Controls | Extend for bar data | **MODIFY** |
| `VideraChartView.Core.cs` | Avalonia Controls | Wire bar rendering path | **MODIFY** |
| `SurfaceScenePainter` | Avalonia Controls | Render bar geometry | **MODIFY** |

**Design notes:**
- Bar chart data is a 2D grid of heights (reuses `SurfaceMatrix` or new `BarChartMatrix`)
- Each bar is a rectangular prism — needs 6 faces (12 triangles) per bar
- Can reuse `SurfaceColorMap` for value→color mapping
- Metadata needs `SurfaceAxisDescriptor` for X/Z axes and `SurfaceValueRange` for Y
- `BarRenderer` converts data → `BarRenderScene` with positioned, colored bar geometry
- `SurfaceScenePainter` needs a new `DrawBars()` path alongside existing `DrawScene()`

### 2. Contour Plot

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `Plot3DSeriesKind.Contour` | Avalonia Controls | Enum value | **MODIFY** `Plot3DSeriesKind.cs` |
| `Plot3DAddApi.Contour(...)` | Avalonia Controls | Method | **MODIFY** `Plot3DAddApi.cs` |
| `ContourChartData` | Core | Data model | **NEW** |
| `ContourLine` | Core | Single isoline | **NEW** |
| `ContourGenerator` | Core | Marching squares algorithm | **NEW** |
| `ContourRenderer` | Core/Rendering | Renderer | **NEW** |
| `ContourRenderScene` | Core/Rendering | Render output | **NEW** |
| `Plot3DSeries` constructor | Avalonia Controls | Extend for contour data | **MODIFY** |
| `SurfaceScenePainter` | Avalonia Controls | Render contour lines | **MODIFY** |

**Design notes:**
- Contour data is a 2D scalar field (same as surface data) with iso-value levels
- `ContourGenerator` implements marching squares to extract isolines
- Each `ContourLine` is a polyline at a specific iso-value
- Can reuse `SurfaceColorMap` for level→color mapping
- Contour lines are projected to 3D space and rendered as line segments
- `SurfaceScenePainter` needs a new `DrawContours()` path

### 3. Logarithmic Axis

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `SurfaceAxisDescriptor` constructor | Core | Remove Log throw | **MODIFY** |
| `SurfaceAxisTickGenerator` | Avalonia Controls | Log-aware tick generation | **MODIFY** |
| `SurfaceAxisOverlayPresenter` | Avalonia Controls | Log-scale label formatting | **MODIFY** |

**Design notes:**
- `SurfaceGeometryGrid.MapNormalizedCoordinate` already handles Log:
  ```csharp
  SurfaceAxisScaleKind.Log =>
      Math.Pow(10d, Math.Log10(axis.Minimum) + ((Math.Log10(axis.Maximum) - Math.Log10(axis.Minimum)) * normalized)),
  ```
- The "raw↔display separation" means: tick values should be in **axis-space** (the user's domain), not in **normalized space** (0..1). The current `SurfaceAxisTickGenerator.CreateMajorTickValues` operates in axis-space already, so the separation may already be sufficient for linear axes. For log axes, tick generation needs to produce powers of 10 (or other log-distributed values).
- Minimum requirement: axis min must be > 0 for log scale.

### 4. DateTime Axis

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `DateTimeAxisHelper` | Core | Ticks↔DateTime conversion | **NEW** |
| `SurfaceAxisTickGenerator` | Avalonia Controls | DateTime-aware tick generation | **MODIFY** |
| `SurfaceChartOverlayOptions` | Avalonia Controls | DateTime format options | **MODIFY** |
| `SurfaceAxisOverlayPresenter` | Avalonia Controls | DateTime label rendering | **MODIFY** |

**Design notes:**
- DateTime values are stored as `double` (Unix timestamp seconds or Ticks)
- `SurfaceAxisScaleKind.DateTime` already exists in the enum
- `SurfaceGeometryGrid.MapNormalizedCoordinate` already handles `DateTime` (same as Linear):
  ```csharp
  SurfaceAxisScaleKind.Linear or SurfaceAxisScaleKind.DateTime =>
      axis.Minimum + (axis.Span * normalized),
  ```
- Tick generation needs to produce "nice" time intervals (1s, 5s, 1min, 1hr, 1day, etc.)
- Label formatting needs `DateTimeOffset.FromUnixTimeSeconds()` or similar
- `SurfaceChartOverlayOptions` needs a `DateTimeFormat` property

### 5. Custom Tick Formatters

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `SurfaceChartOverlayOptions.LabelFormatter` | Avalonia Controls | Already exists | **NO CHANGE** |
| `SurfaceAxisOverlayPresenter` | Avalonia Controls | Use formatter for tick labels | **MODIFY** |
| `SurfaceAxisTickGenerator` | Avalonia Controls | Pass formatter through | **MODIFY** |

**Design notes:**
- `SurfaceChartOverlayOptions.LabelFormatter` already exists as `Func<string, double, string>?`
- It receives axis key ("X", "Y", "Z", "Legend") and numeric value
- Currently used in `FormatLabel()` but the axis overlay presenter uses `FormatLabel()` for tick labels
- Enhancement: add per-axis formatter support (e.g., `XAxisFormatter`, `YAxisFormatter`, `ZAxisFormatter`)
- Or: the existing `LabelFormatter` with axis key dispatch already works — just needs documentation

### 6. Enhanced Chart Legend

| Component | Layer | Type | New/Modified |
|-----------|-------|------|--------------|
| `SurfaceLegendOverlayPresenter` | Avalonia Controls | Multi-series legend | **MODIFY** |
| `SurfaceLegendOverlayState` | Avalonia Controls | Multi-series state | **MODIFY** |
| `SurfaceChartOverlayOptions` | Avalonia Controls | Legend visibility/position | **MODIFY** |
| `SurfaceChartOverlayCoordinator` | Avalonia Controls | Multi-series legend refresh | **MODIFY** |

**Design notes:**
- Current legend is a single color swatch for the active surface/waterfall series
- Enhanced legend needs:
  - Multiple entries (one per series in `Plot3D.Series`)
  - Series name + color indicator (dot/swatch/line)
  - Optional visibility toggle per series
  - Position options (top-right, bottom-right, etc.)
- `SurfaceLegendOverlayState` needs to become multi-entry
- For scatter series: colored dot + label
- For bar series: colored rectangle + label
- For contour series: colored line segment + label

## Integration Points

### Modified Files Summary

| File | Changes | Impact |
|------|---------|--------|
| `Plot3DSeriesKind.cs` | Add `Bar`, `Contour` enum values | Low — enum extension |
| `Plot3DAddApi.cs` | Add `Bar()`, `Contour()` methods | Low — follows existing pattern |
| `Plot3DSeries.cs` | Add `BarChartData?`, `ContourChartData?` properties | Low — new nullable properties |
| `Plot3D.cs` | Add `ActiveBarSeries`, `ActiveContourSeries` helpers | Low — follows existing pattern |
| `SurfaceAxisDescriptor.cs` | Remove Log throw, add validation | Medium — must handle min > 0 |
| `SurfaceAxisTickGenerator.cs` | Add Log/DateTime tick generation | Medium — new algorithms |
| `SurfaceAxisOverlayPresenter.cs` | Log/DateTime label formatting | Medium — format changes |
| `SurfaceChartOverlayOptions.cs` | DateTime format, per-axis formatters, legend position | Low — new properties |
| `SurfaceLegendOverlayPresenter.cs` | Multi-series legend rendering | Medium — redesign state |
| `SurfaceLegendOverlayState.cs` | Multi-entry state model | Medium — structural change |
| `SurfaceChartOverlayCoordinator.cs` | Multi-series legend refresh | Low — pass series list |
| `SurfaceScenePainter.cs` | Add `DrawBars()`, `DrawContours()` | Medium — new render paths |
| `VideraChartView.Core.cs` | Wire bar/contour rendering | Low — switch on series kind |

### New Files Summary

| File | Layer | Purpose |
|------|-------|---------|
| `BarChartData.cs` | Core | Bar chart data model |
| `BarChartMetadata.cs` | Core | Bar chart axis/range metadata |
| `BarRenderer.cs` | Core/Rendering | Data → bar render scene |
| `BarRenderScene.cs` | Core/Rendering | Render-ready bar geometry |
| `BarRenderBar.cs` | Core/Rendering | Single bar geometry data |
| `ContourChartData.cs` | Core | Contour data model |
| `ContourLine.cs` | Core | Single isoline polyline |
| `ContourGenerator.cs` | Core | Marching squares algorithm |
| `ContourRenderer.cs` | Core/Rendering | Data → contour render scene |
| `ContourRenderScene.cs` | Core/Rendering | Render-ready contour lines |
| `DateTimeAxisHelper.cs` | Core | DateTime↔double conversion utilities |

## Build Order (Dependency-Aware)

### Wave 1: Axis Foundation (no new chart types)

**Rationale:** Axis enhancements are prerequisites for Bar and Contour with non-linear axes.

1. **Remove Log scale block** in `SurfaceAxisDescriptor` — unblock `SurfaceAxisScaleKind.Log`
2. **Implement log-aware tick generation** in `SurfaceAxisTickGenerator` — powers-of-10 ticks
3. **Add DateTime tick generation** in `SurfaceAxisTickGenerator` — nice time intervals
4. **Add per-axis formatter support** to `SurfaceChartOverlayOptions` — `XAxisFormatter`, `YAxisFormatter`, `ZAxisFormatter`
5. **Update axis overlay presenter** — use formatters, handle log/dateTime labels

**Dependencies:** None — pure axis work.

### Wave 2: Enhanced Legend

**Rationale:** Legend should be generalized before new chart types need it.

6. **Redesign `SurfaceLegendOverlayState`** for multi-entry — list of `{name, color, kind}` entries
7. **Redesign `SurfaceLegendOverlayPresenter`** for multi-series rendering — dots, lines, rectangles per kind
8. **Wire `SurfaceChartOverlayCoordinator`** to pass `Plot3D.Series` to legend refresh
9. **Add legend position options** to `SurfaceChartOverlayOptions` — `LegendPosition` enum

**Dependencies:** Wave 1 (formatter support for legend labels).

### Wave 3: Bar Chart

**Rationale:** Bar chart is simpler than Contour (no algorithm), should be built first.

10. **Create `BarChartData`** + `BarChartMetadata` in Core — 2D height grid + axis descriptors
11. **Create `BarRenderer`** in Core/Rendering — data → positioned bar geometry
12. **Create `BarRenderScene`** + `BarRenderBar` in Core/Rendering — render-ready output
13. **Add `Plot3DSeriesKind.Bar`** to enum
14. **Add `Plot3DAddApi.Bar()`** methods — from matrix and from tile source
15. **Extend `Plot3DSeries`** with `BarChartData?` property
16. **Add `DrawBars()`** to `SurfaceScenePainter` — project and render bar faces
17. **Wire bar rendering** in `VideraChartView.Core.cs` — dispatch on `Plot3DSeriesKind.Bar`

**Dependencies:** Wave 1 (axis support for bar axes).

### Wave 4: Contour Plot

**Rationale:** Contour requires marching squares algorithm — most complex new component.

18. **Create `ContourGenerator`** in Core — marching squares algorithm
19. **Create `ContourLine`** in Core — polyline data structure
20. **Create `ContourChartData`** in Core — scalar field + iso-level specification
21. **Create `ContourRenderer`** in Core/Rendering — data → contour render scene
22. **Create `ContourRenderScene`** in Core/Rendering — render-ready contour lines
23. **Add `Plot3DSeriesKind.Contour`** to enum
24. **Add `Plot3DAddApi.Contour()`** methods
25. **Extend `Plot3DSeries`** with `ContourChartData?` property
26. **Add `DrawContours()`** to `SurfaceScenePainter` — project and render contour lines
27. **Wire contour rendering** in `VideraChartView.Core.cs`

**Dependencies:** Wave 1 (axis support), Wave 2 (legend for contour levels).

### Wave 5: Integration and Evidence

**Rationale:** Wire everything into evidence/diagnostics/demo.

28. **Update `Plot3DOutputEvidence`** — include bar/contour series info
29. **Update `Plot3DDatasetEvidence`** — include bar/contour dataset info
30. **Update demo** — add bar chart and contour plot demo scenarios
31. **Update support summary** — include new chart types
32. **Update guardrails** — scope documentation for new chart types

**Dependencies:** Waves 1-4.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Marching squares | Custom iso-line extraction | `ContourGenerator` with standard marching squares table | Well-known algorithm, 16 cases, lookup table approach |
| Nice tick values | Custom step computation | Extend existing `SurfaceAxisTickGenerator.ComputeNiceStep()` | Already handles linear nicely-numbered steps |
| DateTime formatting | Custom date formatting | `DateTimeOffset` + standard format strings | .NET has excellent DateTime formatting |
| Log tick values | Custom log distribution | `Math.Pow(10, ...)` with existing nice-step | Standard log-axis approach |
| 3D bar geometry | Custom vertex/index buffers | Reuse `SurfacePatchGeometryBuilder` pattern | Existing geometry builder handles grid topology |
| Color mapping | Custom color interpolation | Reuse `SurfaceColorMap` | Already handles value→ARGB mapping |

## Common Pitfalls

### Pitfall 1: Log Scale with Zero/Negative Values
**What goes wrong:** `Math.Log10(0)` = `-∞`, `Math.Log10(-1)` = `NaN`
**Why it happens:** User passes data with zero or negative values to a log-scale axis
**How to avoid:** Validate `axis.Minimum > 0` when `ScaleKind == Log`. Add guard in `SurfaceAxisDescriptor` constructor.
**Warning signs:** NaN propagation through render pipeline, invisible bars/contour lines.

### Pitfall 2: DateTime Precision Loss
**What goes wrong:** Large Unix timestamps lose precision when stored as `double`
**Why it happens:** `double` has ~15 significant digits; Unix timestamps are ~10 digits (seconds since 1970)
**How to avoid:** Use seconds-since-epoch (not ticks) for DateTime axis values. Millisecond precision is sufficient for charts.
**Warning signs:** Tick labels show identical values, jittery axis labels.

### Pitfall 3: Contour Line Discontinuity
**What goes wrong:** Marching squares produces disconnected line segments that should be connected
**Why it happens:** Ambiguous saddle-point cases in the 16-case lookup table
**How to avoid:** Use the asymptotic decider for saddle-point disambiguation, or accept segment-based rendering (simpler).
**Warning signs:** Visual gaps in contour lines at saddle points.

### Pitfall 4: Bar Chart Z-Fighting
**What goes wrong:** Adjacent bar faces overlap and flicker
**Why it happens:** Bars share edges, causing depth-buffer fighting
**How to avoid:** Add small gaps between bars (1-2% of bar width), or use consistent triangle winding order.
**Warning signs:** Flickering bar faces when rotating the view.

### Pitfall 5: Legend Overflow
**What goes wrong:** Legend with many series extends beyond view bounds
**Why it happens:** No scrolling or truncation for multi-series legend
**How to avoid:** Add max-visible-entries with "... and N more" truncation. Or make legend scrollable.
**Warning signs:** Legend text rendered outside view bounds.

### Pitfall 6: Breaking Existing Series Contract
**What goes wrong:** Adding `BarChartData?` and `ContourChartData?` to `Plot3DSeries` breaks existing consumers
**Why it happens:** Constructor signature changes, or new required parameters
**How to avoid:** Add as nullable optional properties. Keep existing constructor for Surface/Waterfall/Scatter. Add new internal constructor overloads.
**Warning signs:** Compilation errors in existing test/demo code.

## Code Examples

### Adding a New Series Kind (Bar)

```csharp
// Plot3DSeriesKind.cs — add enum value
public enum Plot3DSeriesKind
{
    Surface,
    Waterfall,
    Scatter,
    Bar,      // NEW
    Contour,  // NEW
}

// Plot3DAddApi.cs — add authoring method
public Plot3DSeries Bar(BarChartData data, string? name = null)
{
    ArgumentNullException.ThrowIfNull(data);
    return _plot.AddSeries(new Plot3DSeries(
        Plot3DSeriesKind.Bar, name,
        surfaceSource: null, scatterData: null, barData: data));
}

// Plot3DSeries.cs — add nullable property
internal Plot3DSeries(
    Plot3DSeriesKind kind, string? name,
    ISurfaceTileSource? surfaceSource,
    ScatterChartData? scatterData,
    BarChartData? barData = null,      // NEW
    ContourChartData? contourData = null) // NEW
{
    // ... existing init ...
    BarData = barData;
    ContourData = contourData;
}

public BarChartData? BarData { get; }        // NEW
public ContourChartData? ContourData { get; } // NEW
```

### Unblocking Log Scale

```csharp
// SurfaceAxisDescriptor.cs — remove the throw, add min > 0 validation
if (scaleKind == SurfaceAxisScaleKind.Log)
{
    if (minimum <= 0d)
    {
        throw new ArgumentOutOfRangeException(
            nameof(minimum),
            "Logarithmic axis minimum must be positive.");
    }
    if (maximum <= 0d)
    {
        throw new ArgumentOutOfRangeException(
            nameof(maximum),
            "Logarithmic axis maximum must be positive.");
    }
}
```

### Log-Aware Tick Generation

```csharp
// SurfaceAxisTickGenerator.cs — add log tick generation
public static IReadOnlyList<double> CreateLogTickValues(
    double axisMinimum, double axisMaximum, double axisLength)
{
    if (axisMinimum <= 0d || axisMaximum <= axisMinimum)
    {
        return [axisMinimum];
    }

    var logMin = Math.Log10(axisMinimum);
    var logMax = Math.Log10(axisMaximum);
    var logSpan = logMax - logMin;
    var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 72d), 2, 7);
    var logStep = ComputeNiceStep(logSpan / targetTickCount);
    var firstLogTick = Math.Ceiling(logMin / logStep) * logStep;
    var ticks = new List<double>();

    for (var logTick = firstLogTick; logTick <= logMax + (logStep * 0.5d); logTick += logStep)
    {
        var value = Math.Pow(10d, logTick);
        if (value >= axisMinimum && value <= axisMaximum)
        {
            ticks.Add(value);
        }
    }

    return ticks.Count == 0 ? [axisMinimum, axisMaximum] : ticks;
}
```

### DateTime Tick Generation

```csharp
// SurfaceAxisTickGenerator.cs — add DateTime tick generation
private static readonly double[] NiceTimeSteps =
[
    1, 2, 5, 10, 15, 30,           // seconds
    60, 120, 300, 600, 1800, 3600, // minutes/hours
    7200, 14400, 43200, 86400,     // hours/day
    172800, 604800, 2592000,       // days/weeks/months
    7776000, 15552000, 31536000,   // months/year
];

public static IReadOnlyList<double> CreateDateTimeTickValues(
    double axisMinimum, double axisMaximum, double axisLength)
{
    var axisSpan = axisMaximum - axisMinimum;
    var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 100d), 2, 7);
    var roughStep = axisSpan / targetTickCount;

    // Find the smallest nice step >= roughStep
    var step = NiceTimeSteps.FirstOrDefault(s => s >= roughStep);
    if (step == 0d) step = NiceTimeSteps[^1];

    var firstTick = Math.Ceiling(axisMinimum / step) * step;
    var ticks = new List<double>();

    for (var tick = firstTick; tick <= axisMaximum + (step * 0.5d); tick += step)
    {
        ticks.Add(tick);
    }

    return ticks.Count == 0 ? [axisMinimum, axisMaximum] : ticks;
}
```

### Bar Renderer (Skeleton)

```csharp
// BarRenderer.cs
public sealed class BarRenderer
{
    public BarRenderScene BuildScene(BarChartData data, SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(colorMap);

        var bars = new List<BarRenderBar>(data.Width * data.Height);
        var values = data.Values.Span;
        var barWidth = 1.0d / data.Width;
        var barDepth = 1.0d / data.Height;
        var gap = 0.02d; // 2% gap between bars

        for (var row = 0; row < data.Height; row++)
        {
            for (var col = 0; col < data.Width; col++)
            {
                var value = values[row * data.Width + col];
                var x = data.Metadata.MapHorizontalCoordinate(col) + (gap * barWidth * 0.5d);
                var z = data.Metadata.MapVerticalCoordinate(row) + (gap * barDepth * 0.5d);
                var color = colorMap.Map(value);

                bars.Add(new BarRenderBar(
                    new Vector3((float)x, 0f, (float)z),
                    new Vector3(
                        (float)(barWidth * (1d - gap)),
                        (float)value,
                        (float)(barDepth * (1d - gap))),
                    color));
            }
        }

        return new BarRenderScene(data.Metadata, bars);
    }
}
```

## State of the Art

| Existing Capability | Status | What's Missing |
|---------------------|--------|----------------|
| `SurfaceAxisScaleKind.Log` | Enum defined, geometry math exists | Constructor throws — needs removal |
| `SurfaceAxisScaleKind.DateTime` | Enum defined, geometry math exists | No tick generator, no label formatter |
| `SurfaceAxisScaleKind.ExplicitCoordinates` | Fully implemented | Nothing — works with `SurfaceExplicitGrid` |
| `SurfaceChartOverlayOptions.LabelFormatter` | Exists as `Func<string, double, string>?` | No per-axis granularity |
| Legend overlay | Single-series color swatch | No multi-series, no per-kind indicators |
| `Plot3DSeriesKind` | Surface, Waterfall, Scatter | No Bar, no Contour |

**Deprecated/outdated:**
- The `SurfaceAxisDescriptor` throw-on-Log is a placeholder from before the geometry grid's log math was implemented. The geometry grid is ready; the descriptor is the blocker.

## Open Questions

1. **Bar chart in 3D space — flat or 3D?**
   - What we know: Surface/Waterfall are 3D heightfields, Scatter is 3D points
   - What's unclear: Should bars be 3D rectangular prisms (X, Y, Z) or 2D bars projected in 3D?
   - Recommendation: 3D rectangular prisms for consistency with the 3D chart model. The existing `SurfaceMetadata` pattern (horizontal + vertical axis + value range) maps naturally to bar X/Z positions + bar height.

2. **Contour rendering — lines or filled regions?**
   - What we know: Contour plots can show isolines (wireframe) or filled regions (color between levels)
   - What's unclear: Which is the first-version scope?
   - Recommendation: Isolines only for v2.53. Filled regions require triangulation between contour levels, which is significantly more complex. Can be added later.

3. **Bar chart — grouped or stacked?**
   - What we know: Multiple series can exist on one plot
   - What's unclear: Should multiple bar series stack or group side-by-side?
   - Recommendation: Side-by-side grouping for v2.53. Stacking requires cumulative height computation and different color semantics.

4. **DateTime axis — what epoch and resolution?**
   - What we know: Unix epoch (1970-01-01) is standard
   - What's unclear: Seconds or milliseconds? UTC or local?
   - Recommendation: UTC seconds since epoch. Millisecond precision via fractional seconds. `DateTimeOffset.FromUnixTimeSeconds()` for display.

5. **Legend interactivity — click to toggle series visibility?**
   - What we know: Current legend is read-only
   - What's unclear: Should legend entries be clickable?
   - Recommendation: Read-only for v2.53. Interactivity can be added as a follow-up.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 8 SDK | All components | ✓ | 8.x | — |
| Avalonia | Overlay rendering | ✓ | (existing) | — |
| xUnit | Tests | ✓ | (existing) | — |
| SkiaSharp | PNG encoding (snapshot) | ✓ | (existing) | — |

**Missing dependencies with no fallback:**
- None — all required infrastructure exists.

**Missing dependencies with fallback:**
- None.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xUnit (existing) |
| Config file | `tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj` |
| Quick run command | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --no-build` |
| Full suite command | `dotnet test tests/ --no-build` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| AXIS-01 | Log scale axis construction | unit | `dotnet test --filter "Log"` | ❌ Wave 0 |
| AXIS-02 | Log tick generation | unit | `dotnet test --filter "LogTick"` | ❌ Wave 0 |
| AXIS-03 | DateTime tick generation | unit | `dotnet test --filter "DateTimeTick"` | ❌ Wave 0 |
| AXIS-04 | Custom formatter dispatch | unit | `dotnet test --filter "Formatter"` | ❌ Wave 0 |
| BAR-01 | Bar chart data model | unit | `dotnet test --filter "BarChart"` | ❌ Wave 0 |
| BAR-02 | Bar renderer geometry | unit | `dotnet test --filter "BarRenderer"` | ❌ Wave 0 |
| BAR-03 | Bar series integration | integration | `dotnet test --filter "BarSeries"` | ❌ Wave 0 |
| CONT-01 | Marching squares algorithm | unit | `dotnet test --filter "ContourGenerator"` | ❌ Wave 0 |
| CONT-02 | Contour renderer | unit | `dotnet test --filter "ContourRenderer"` | ❌ Wave 0 |
| CONT-03 | Contour series integration | integration | `dotnet test --filter "ContourSeries"` | ❌ Wave 0 |
| LEG-01 | Multi-series legend state | unit | `dotnet test --filter "LegendMulti"` | ❌ Wave 0 |
| LEG-02 | Legend position options | unit | `dotnet test --filter "LegendPosition"` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test tests/Videra.SurfaceCharts.Core.Tests --no-build`
- **Per wave merge:** `dotnet test tests/ --no-build`
- **Phase gate:** Full suite green before `/gsd-verify-work`

### Wave 0 Gaps
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/Axis/LogScaleAxisTests.cs` — covers AXIS-01, AXIS-02
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/Axis/DateTimeAxisTests.cs` — covers AXIS-03
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/Chart/BarChartTests.cs` — covers BAR-01, BAR-02
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/Chart/ContourGeneratorTests.cs` — covers CONT-01
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/Chart/ContourRendererTests.cs` — covers CONT-02

## Sources

### Primary (HIGH confidence)
- Source code: `Plot3D.cs`, `Plot3DSeries.cs`, `Plot3DAddApi.cs`, `Plot3DSeriesKind.cs` — direct reading
- Source code: `SurfaceAxisDescriptor.cs`, `SurfaceAxisScaleKind.cs`, `SurfaceGeometryGrid.cs` — direct reading
- Source code: `SurfaceChartOverlayOptions.cs`, `SurfaceAxisOverlayPresenter.cs`, `SurfaceLegendOverlayPresenter.cs` — direct reading
- Source code: `SurfaceRenderer.cs`, `ScatterRenderer.cs`, `SurfaceScenePainter.cs` — direct reading
- Source code: `SurfaceChartOverlayCoordinator.cs`, `SurfaceAxisTickGenerator.cs` — direct reading

### Secondary (MEDIUM confidence)
- Marching squares algorithm — standard computational geometry, well-documented
- Nice-number tick algorithm — Heckbert 1990, standard in charting libraries

### Tertiary (LOW confidence)
- None — all findings based on direct source code analysis.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — based on direct source code reading
- Architecture: HIGH — patterns clearly visible in existing code
- Pitfalls: MEDIUM — based on general charting library experience + source analysis

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (30 days — stable architecture)
