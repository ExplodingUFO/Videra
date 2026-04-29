# Stack Research — v2.53 Chart Type Expansion and Axis Semantics

**Domain:** Expanding Videra SurfaceCharts with bar charts, contour plots, DateTime axes, log scale, custom tick formatters, and chart legend
**Researched:** 2026-04-29
**Confidence:** HIGH (codebase analysis) / MEDIUM (external library evaluation)

## Summary

Videra's SurfaceCharts module currently supports three chart families (Surface, Waterfall, Scatter) rendered through Avalonia's `DrawingContext` with Silk.NET GPU backends. The v2.53 milestone adds bar charts, contour plots, DateTime axes, log scale, custom tick formatters, and enhanced chart legends.

**Critical finding:** The codebase already has significant scaffolding for these features:
- `SurfaceAxisScaleKind` enum already defines `Log`, `DateTime`, and `ExplicitCoordinates` values
- `Log` is explicitly **blocked** in `SurfaceAxisDescriptor` constructor with `throw new ArgumentException("Logarithmic axis scaling is reserved...")`
- `DateTime` is defined but **not handled** in `SurfaceGeometryGrid.MapNormalizedCoordinate()` (falls through to default case)
- `SurfaceChartOverlayOptions.LabelFormatter` (a `Func<string, double, string>?`) already exists for custom tick formatting
- `SurfaceLegendOverlayPresenter` already renders a color-mapped swatch legend
- Rendering is through Avalonia's `DrawingContext` (internally SkiaSharp), NOT direct SkiaSharp

**Primary recommendation:** Build all new features as repo-local C# code within the existing `Core` → `Rendering` → `Avalonia` layer split. No new external dependencies are required. The contour algorithm (marching squares) and bar geometry are straightforward to implement in `SurfaceCharts.Core`.

## Standard Stack

### Core — No Changes Required

| Technology | Version | Purpose | Status |
|------------|---------|---------|--------|
| `.NET` | `8.0.x` | Runtime | **Keep as-is.** .NET 10.0.203 SDK already installed. |
| `Avalonia` | `11.3.9` | Control shell, overlay rendering | **Keep as-is.** Avalonia 12.0.x available but upgrade should be a separate milestone. |
| `Silk.NET` | repo line | GPU backends | **Keep as-is.** Not involved in new chart types. |

### New Dependencies — NONE Required

All new features can be implemented with built-in .NET 8 APIs:

| Feature | Implementation Approach | External Library Needed? |
|---------|------------------------|-------------------------|
| Bar chart | Custom geometry in `SurfaceCharts.Core` | No — `System.Numerics.Vector3` already used |
| Contour plot | Marching squares algorithm in `SurfaceCharts.Core` | No — pure math on `SurfaceScalarField` |
| DateTime axis | `System.DateTimeOffset.Ticks` + `System.Globalization.DateTimeFormatInfo` | No — built into .NET |
| Log axis | `Math.Log10` / `Math.Pow` — already in `SurfaceGeometryGrid.MapNormalizedCoordinate` | No — built into .NET |
| Custom tick formatters | `Func<string, double, string>?` already exists on `SurfaceChartOverlayOptions` | No — already implemented |
| Chart legend enhancement | Extend existing `SurfaceLegendOverlayPresenter` | No — already uses Avalonia `DrawingContext` |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Custom marching squares | `MathNet.Numerics` | Adds 3MB dependency for one algorithm; marching squares is ~100 lines of C# |
| Custom bar geometry | `HelixToolkit` | Duplicates Videra's own 3D engine; wrong dependency direction |
| `ScottPlot` for 2D contour | Internal implementation | ScottPlot is 2D-only; Videra needs 3D-projected contour lines |
| `LiveCharts2` | Internal implementation | Not aligned with Videra's 3D rendering model |

## Architecture Patterns

### Current Layer Split (Keep)

```
src/
  Videra.SurfaceCharts.Core/         ← Data contracts, algorithms, rendering primitives
  Videra.SurfaceCharts.Rendering/    ← GPU/software render backends
  Videra.SurfaceCharts.Avalonia/     ← Avalonia control shell, overlay rendering
  Videra.SurfaceCharts.Processing/   ← Cache/pyramid I/O
```

### Extension Points for v2.53

**Core layer additions:**

```
Videra.SurfaceCharts.Core/
  BarChart/                          ← NEW
    BarChartData.cs                  ← Bar dataset contract (like ScatterChartData)
    BarSeries.cs                     ← Single bar series
    BarRenderScene.cs                ← Render-ready bar geometry
    BarRenderer.cs                   ← Builds render scene from data
  Contour/                           ← NEW
    ContourLine.cs                   ← Single isoline (list of Vector3 segments)
    ContourSet.cs                    ← Collection of contour lines at specific values
    ContourExtractor.cs              ← Marching squares on SurfaceScalarField
  SurfaceAxisScaleKind.cs            ← MODIFY: unblock Log, handle DateTime
  SurfaceAxisDescriptor.cs           ← MODIFY: remove Log throw, add DateTime validation
  SurfaceGeometryGrid.cs             ← MODIFY: handle DateTime in MapNormalizedCoordinate
```

**Avalonia layer additions:**

```
Videra.SurfaceCharts.Avalonia/Controls/
  Plot/
    Plot3DSeriesKind.cs              ← MODIFY: add Bar, Contour values
    Plot3DAddApi.cs                  ← MODIFY: add Bar(), Contour() methods
    Plot3DSeries.cs                  ← MODIFY: add BarData, ContourData properties
  Overlay/
    SurfaceAxisTickGenerator.cs      ← MODIFY: add log-scale tick generation
    SurfaceLegendOverlayPresenter.cs ← MODIFY: add multi-series legend with names
```

### Pattern: Adding a New Chart Family

The existing pattern for adding chart families (established in v2.1 Scatter, v2.1.27 Waterfall):

1. **Core:** Define data contract (`*ChartData`), render primitives (`*RenderScene`), and renderer (`*Renderer`)
2. **Avalonia/Plot:** Add to `Plot3DSeriesKind` enum, add `Plot3DAddApi.*()` method
3. **Avalonia/Rendering:** Route through `VideraChartView` rendering pipeline
4. **Avalonia/Overlay:** Update overlay presenters for new series type

```csharp
// Pattern: Plot3DSeriesKind enum extension
public enum Plot3DSeriesKind
{
    Surface,
    Waterfall,
    Scatter,
    Bar,       // NEW
    Contour,   // NEW
}

// Pattern: Plot3DAddApi method
public Plot3DSeries Bar(BarChartData data, string? name = null)
{
    ArgumentNullException.ThrowIfNull(data);
    return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Bar, name, surfaceSource: null, barData: data));
}
```

### Pattern: Axis Scale Kind Extension

The existing `SurfaceAxisScaleKind` enum and `SurfaceGeometryGrid.MapNormalizedCoordinate` already handle the dispatch pattern:

```csharp
// Current code in SurfaceGeometryGrid.MapNormalizedCoordinate (line 131-143)
protected static double MapNormalizedCoordinate(SurfaceAxisDescriptor axis, double normalized)
{
    return axis.ScaleKind switch
    {
        SurfaceAxisScaleKind.Linear or SurfaceAxisScaleKind.DateTime =>
            axis.Minimum + (axis.Span * normalized),
        SurfaceAxisScaleKind.Log =>
            Math.Pow(10d, Math.Log10(axis.Minimum) + ((Math.Log10(axis.Maximum) - Math.Log10(axis.Minimum)) * normalized)),
        // ...
    };
}
```

**The Log mapping math is already correct.** The only blocker is the `throw` in `SurfaceAxisDescriptor` constructor (line 46-51).

### Anti-Patterns to Avoid

- **Don't add a generic chart engine dependency.** Videra's chart family is intentionally bounded.
- **Don't merge chart semantics into VideraView.** Keep SurfaceCharts independent.
- **Don't add SkiaSharp as a direct dependency.** Avalonia's `DrawingContext` already wraps SkiaSharp.
- **Don't create a 2D contour renderer.** Contour lines must project into the existing 3D camera space.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| DateTime tick spacing | Custom calendar math | `System.Globalization.DateTimeFormatInfo` + `Calendar` | Handles month boundaries, DST, leap years correctly |
| Log-axis validation | Custom domain checks | `Math.Log10` range validation | Already in .NET; just need to check `minimum > 0` |
| Contour line smoothing | Catmull-Rom or Bezier | Simple linear segments first | Marching squares produces clean segments; smoothing adds complexity without evidence of need |
| Bar chart hit testing | Custom ray-box intersection | Reuse existing `SurfaceHeightfieldPicker` pattern | The picker infrastructure already exists for surfaces |

## Common Pitfalls

### Pitfall 1: Log Axis with Zero or Negative Values
**What goes wrong:** `Math.Log10(0)` = `-Infinity`, `Math.Log10(-1)` = `NaN`
**Why it happens:** User passes `minimum <= 0` for a log-scale axis
**How to avoid:** Validate in `SurfaceAxisDescriptor` constructor: if `ScaleKind == Log`, require `minimum > 0 && maximum > 0`
**Warning signs:** `NaN` propagation in vertex positions, invisible geometry

### Pitfall 2: DateTime Axis Precision Loss
**What goes wrong:** `double` has 15-16 significant digits; `DateTimeOffset.Ticks` (100ns resolution) over `DateTime.MinValue` to `DateTime.MaxValue` spans ~3.15×10¹⁸ — losing precision at the nanosecond level
**Why it happens:** Storing raw ticks as `double` axis bounds
**How to avoid:** Store `DateTimeOffset` ticks as `long` in the axis descriptor, convert to `double` only for normalized mapping. Use `DateTimeOffset.FromUnixTimeSeconds()` for reasonable ranges.
**Warning signs:** Tick labels showing wrong times, rounding errors in probe readouts

### Pitfall 3: Contour Lines on Masked/NaN Regions
**What goes wrong:** Marching squares produces contour segments crossing NaN/masked cells
**Why it happens:** Algorithm treats NaN as a valid interpolation endpoint
**How to avoid:** Check `SurfaceMask` before interpolation; skip cells with any masked vertex
**Warning signs:** Contour lines appearing in regions that should be empty

### Pitfall 4: Bar Chart Z-Fighting
**What goes wrong:** Adjacent bars with identical heights render with flickering/z-fighting artifacts
**Why it happens:** Bar tops are coplanar; GPU depth buffer has limited precision
**How to avoid:** Add a small epsilon offset to bar top Y based on bar index, or use a consistent winding order
**Warning signs:** Flickering bar tops when rotating the view

### Pitfall 5: Legend Overflow with Many Series
**What goes wrong:** Legend overlay extends beyond viewport when many series are present
**Why it happens:** Current legend is fixed-position, single-swatch
**How to avoid:** Implement scrollable or collapsible legend, or limit to N entries with "+M more"
**Warning signs:** Legend text clipped at viewport edge

## Code Examples

### Unblocking Log Axis Scale

```csharp
// File: src/Videra.SurfaceCharts.Core/SurfaceAxisDescriptor.cs
// REMOVE lines 46-51:
//   if (scaleKind == SurfaceAxisScaleKind.Log)
//   {
//       throw new ArgumentException(
//           "Logarithmic axis scaling is reserved until raw axis values and display-space coordinates are separated.",
//           nameof(scaleKind));
//   }

// ADD validation for log axis:
if (scaleKind == SurfaceAxisScaleKind.Log)
{
    if (minimum <= 0d || maximum <= 0d)
    {
        throw new ArgumentOutOfRangeException(
            nameof(minimum), "Logarithmic axis requires positive minimum and maximum values.");
    }
}
```

### DateTime Axis Descriptor

```csharp
// New constructor overload for SurfaceAxisDescriptor
public SurfaceAxisDescriptor(
    string label,
    string? unit,
    DateTimeOffset minimum,
    DateTimeOffset maximum)
    : this(label, unit, minimum.UtcTicks, maximum.UtcTicks, SurfaceAxisScaleKind.DateTime)
{
}

// DateTime tick formatter in SurfaceChartOverlayOptions
public static Func<string, double, string> CreateDateTimeFormatter(
    string format = "yyyy-MM-dd HH:mm:ss",
    DateTimeKind kind = DateTimeKind.Utc)
{
    return (axisKey, value) =>
    {
        var ticks = (long)value;
        var dto = new DateTimeOffset(ticks, TimeSpan.Zero);
        return dto.ToString(format, CultureInfo.InvariantCulture);
    };
}
```

### Log-Scale Tick Generation

```csharp
// In SurfaceAxisTickGenerator — add log-aware overload
public static IReadOnlyList<double> CreateLogTickValues(
    double axisMinimum, double axisMaximum, double axisLength)
{
    if (axisMinimum <= 0d || axisMaximum <= 0d)
    {
        return [axisMinimum, axisMaximum];
    }

    var logMin = Math.Floor(Math.Log10(axisMinimum));
    var logMax = Math.Ceiling(Math.Log10(axisMaximum));
    var ticks = new List<double>();

    for (var logValue = logMin; logValue <= logMax; logValue++)
    {
        var value = Math.Pow(10d, logValue);
        if (value >= axisMinimum && value <= axisMaximum)
        {
            ticks.Add(value);
        }

        // Add intermediate ticks (2, 3, 5) at each decade
        foreach (var factor in new[] { 2d, 3d, 5d })
        {
            var intermediate = value * factor;
            if (intermediate >= axisMinimum && intermediate <= axisMaximum)
            {
                ticks.Add(intermediate);
            }
        }
    }

    return ticks;
}
```

### Contour Extraction (Marching Squares)

```csharp
// Core algorithm outline for ContourExtractor
public static ContourSet ExtractContours(
    SurfaceScalarField field,
    SurfaceMask? mask,
    IReadOnlyList<double> isoValues)
{
    var lines = new List<ContourLine>();
    foreach (var isoValue in isoValues)
    {
        var segments = MarchingSquares(field, mask, isoValue);
        lines.Add(new ContourLine(isoValue, segments));
    }
    return new ContourSet(lines);
}

// Each segment is a pair of Vector3 points on the surface mesh
// Marching squares processes each 2×2 cell and emits 0-2 line segments
```

### Bar Chart Data Contract

```csharp
// BarChartData mirrors ScatterChartData pattern
public sealed class BarChartData
{
    public BarChartData(
        ScatterChartMetadata metadata,
        IReadOnlyList<BarSeries> series)
    {
        // validation follows ScatterChartData pattern
    }

    public ScatterChartMetadata Metadata { get; }
    public IReadOnlyList<BarSeries> Series { get; }
}

public sealed class BarSeries
{
    public BarSeries(
        IReadOnlyList<BarEntry> entries,
        uint color,
        string? label = null)
    {
        Entries = entries;
        Color = color;
        Label = label;
    }

    public IReadOnlyList<BarEntry> Entries { get; }
    public uint Color { get; }
    public string? Label { get; }
}

public readonly record struct BarEntry(
    double X,        // horizontal position
    double Z,        // depth position
    double Baseline, // bottom of bar (usually 0)
    double Value);   // top of bar
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `SurfaceAxisScaleKind.Log` blocked | Scaffolded but reserved | v2.3 (enum defined, throw added) | Unblocking is a one-line removal + validation |
| `SurfaceAxisScaleKind.DateTime` no-op | Mapped same as Linear | v2.3 | Needs actual tick formatting + validation |
| `LabelFormatter` null default | `Func<string, double, string>?` property | v2.50 | Already usable for DateTime/custom formatters |
| Single-series legend swatch | Color-mapped value legend | v2.42 | Needs multi-series extension |
| `SurfaceAxisTickGenerator` linear-only | Nice-step linear ticks | v2.42 | Needs log-aware overload |

**Deprecated/outdated:**
- `SurfaceAxisScaleKind.Log` throw message says "reserved until raw axis values and display-space coordinates are separated" — the separation now exists via `SurfaceGeometryGrid` hierarchy

## Open Questions

1. **Bar chart rendering approach: axis-aligned rectangles or true 3D boxes?**
   - What we know: Current rendering uses `SurfacePatchGeometryBuilder` for triangle meshes and `ScatterRenderer` for point primitives
   - What's unclear: Whether bars should be 3D boxes (6 faces, 12 triangles each) or axis-aligned quads projected into 3D
   - Recommendation: Start with axis-aligned quads (simpler, fewer triangles); add 3D box option later if needed

2. **Contour lines: render as overlay or as 3D geometry?**
   - What we know: Current overlay is 2D Avalonia `DrawingContext`; 3D geometry goes through `SurfaceRenderTile`
   - What's unclear: Whether contour lines should be projected 2D lines on the overlay or 3D line geometry in the scene
   - Recommendation: 3D line geometry in the scene (consistent with how surfaces render); overlay only for labels

3. **Multi-series legend positioning:**
   - What we know: Current legend is fixed-position right-side swatch
   - What's unclear: How to handle 5+ series without overflowing
   - Recommendation: Stack vertically with scroll or limit to N visible + overflow indicator

4. **Bar + Surface mixed rendering:**
   - What we know: Current `Plot3D` supports multiple series but only one "active" surface series drives the tile pipeline
   - What's unclear: Whether bars and surfaces should share the same 3D space or use separate viewports
   - Recommendation: Share the same 3D space (consistent axis semantics); bars render as additional geometry

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK | All features | ✓ | 10.0.203 | — |
| Avalonia | Overlay rendering | ✓ | 11.3.9 | — |
| xUnit | Tests | ✓ | 2.9.3 | — |
| FluentAssertions | Tests | ✓ | 8.9.0 | — |

**No missing dependencies.** All features are implementable with the existing stack.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 + FluentAssertions 8.9.0 |
| Config file | `tests/Videra.SurfaceCharts.Core.Tests/Videra.SurfaceCharts.Core.Tests.csproj` |
| Quick run command | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --no-restore` |
| Full suite command | `dotnet test --no-restore` |

### Phase Requirements → Test Map
| Feature | Test Type | Automated Command | File Exists? |
|---------|-----------|-------------------|-------------|
| Log axis unblocking | unit | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "Axis"` | ✅ existing axis tests |
| DateTime axis mapping | unit | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "DateTime"` | ❌ Wave 0 |
| Contour extraction | unit | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "Contour"` | ❌ Wave 0 |
| Bar chart data contract | unit | `dotnet test tests/Videra.SurfaceCharts.Core.Tests --filter "BarChart"` | ❌ Wave 0 |
| Legend multi-series | integration | `dotnet test tests/Videra.SurfaceCharts.Avalonia.IntegrationTests --filter "Legend"` | ❌ Wave 0 |

### Wave 0 Gaps
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/AxisScaleKindTests.cs` — covers Log unblocking, DateTime mapping
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/ContourExtractorTests.cs` — covers marching squares
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/BarChartDataTests.cs` — covers bar data validation
- [ ] `tests/Videra.SurfaceCharts.Core.Tests/BarRendererTests.cs` — covers bar geometry generation
- [ ] `tests/Videra.SurfaceCharts.Avalonia.IntegrationTests/LegendOverlayTests.cs` — covers multi-series legend

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
- Source code: `src/Videra.SurfaceCharts.Core/Rendering/SurfaceRenderer.cs` — tile rendering pipeline
- Source code: `src/Videra.SurfaceCharts.Core/Rendering/ScatterRenderer.cs` — scatter rendering pattern
- Source code: `src/Videra.SurfaceCharts.Core/ScatterChartData.cs` — data contract pattern for new chart types

### Secondary (MEDIUM confidence)
- NuGet: Avalonia 11.3.9 (current), 12.0.2 (available — do not upgrade in this milestone)
- NuGet: .NET 8.0 runtime features (System.Numerics, System.Globalization)

### Tertiary (LOW confidence)
- Marching squares implementation details — standard algorithm, no source needed

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — no new dependencies needed; all features build on existing stack
- Architecture: HIGH — existing chart family addition pattern is well-established (Scatter v2.1, Waterfall v1.27)
- Pitfalls: MEDIUM — log axis edge cases and DateTime precision are well-known but need testing
- Contour algorithm: MEDIUM — marching squares is standard but needs integration with SurfaceMask

**Research date:** 2026-04-29
**Valid until:** 2026-05-29 (30 days — stable stack, no external dependency changes)
