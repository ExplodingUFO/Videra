# Phase 432: Chart Type Inventory and API Design - Research

**Researched:** 2026-04-30
**Domain:** 3D chart type architecture, API surface mapping, rendering pipeline, probe/overlay infrastructure
**Confidence:** HIGH

## Summary

This is an inventory and API design phase for v2.65's chart expansion. The current codebase has five chart kinds (Surface, Waterfall, Scatter, Bar, Contour) with a well-established pattern for adding new chart families. Each chart kind follows a consistent architecture: data model class in Core, renderer that builds a render scene, Plot3DSeries subclass for the host-facing handle, probe strategy for interaction, and rendering status for diagnostics. The design phase must document these seams precisely so that subsequent implementation phases (434-437) can follow the pattern without architectural drift.

The five new chart families (Line, Ribbon, Vector Field, Heatmap Slice, Box Plot) each require the same set of integration points: a Plot3DSeriesKind enum value, a data model, a renderer, a Plot3DSeries subclass, a Plot3DAddApi method, an ISeriesProbeStrategy, legend/tooltips support, and a rendering status record. The research maps every existing integration point and designs the API contracts for each new family.

**Primary recommendation:** Follow the exact pattern established by Bar and Contour (the most recent additions) -- data model in Core, renderer in Core/Rendering, series subclass in Avalonia/Controls/Plot, probe strategy in Core/Picking, rendering status in Avalonia/Controls.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Data model (ChartData) | Core | -- | Immutable data structures, no UI dependency |
| Renderer (build scene) | Core/Rendering | -- | Geometry computation, no UI dependency |
| Render scene | Core | -- | Render-ready snapshot, consumed by painters |
| Plot3DSeries subclass | Avalonia/Controls/Plot | -- | Host-facing handle, binds to Plot3D |
| Plot3DAddApi method | Avalonia/Controls/Plot | -- | Public authoring API entry point |
| Probe strategy | Core/Picking | -- | Hit-testing logic, no UI dependency |
| Probe strategy dispatch | Avalonia/Controls/Overlay | -- | Maps Plot3DSeriesKind to strategy |
| Legend/tooltip overlay | Avalonia/Controls/Overlay | -- | Visual presentation of series metadata |
| Rendering status | Avalonia/Controls | -- | Diagnostics record for chart kind |
| Scene painter | Avalonia/Controls | -- | Avalonia DrawingContext rendering |
| Composition (multi-series) | Avalonia/Controls/Plot | -- | Merges same-kind series for rendering |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| System.Numerics.Vector3 | .NET 8 BCL | 3D point/vector math | BCL, zero allocation |
| Avalonia.Media.DrawingContext | 11.x | 2D projection rendering | Platform UI layer |
| Avalonia.Media.StreamGeometry | 11.x | Line/contour path rendering | Platform UI layer |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Collections.ObjectModel | .NET 8 BCL | ReadOnlyCollection for immutable data | All data model classes |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Static renderer classes | Instance-based renderers with caching | Static is simpler; caching handled at scene cache level |

## Architecture Patterns

### System Architecture Diagram

```
User Code
    |
    v
Plot3DAddApi.Line/Ribbon/VectorField/HeatmapSlice/BoxPlot
    |
    v
Plot3DSeries subclass (LinePlot3DSeries, etc.)
    |
    v
Plot3D.AddSeries() --> Plot3D._series list
    |
    +---> Plot3D.ActiveSeries --> determines active chart kind
    |
    +---> Plot3DSeriesComposition.Create*Data() --> merges same-kind series
    |
    +---> *Renderer.BuildScene() --> *RenderScene
    |         |
    |         v
    |    SurfaceScenePainter.Draw*() --> Avalonia DrawingContext
    |
    +---> ISeriesProbeStrategy.TryResolve() --> SurfaceProbeInfo
    |         |
    |         v
    |    SeriesProbeStrategyDispatcher --> maps Plot3DSeriesKind to strategy
    |
    +---> SurfaceLegendOverlayPresenter.CreateLegendEntry() --> legend indicator
    |
    +---> *ChartRenderingStatus --> diagnostics
```

### Recommended Project Structure

```
src/Videra.SurfaceCharts.Core/
    LineChartData.cs              # Data model
    LineRenderer.cs               # Rendering/LineRenderer.cs
    LineRenderScene.cs            # Rendering/LineRenderScene.cs
    LineRenderSegment.cs          # Rendering/LineRenderSegment.cs
    LineProbeStrategy.cs          # Picking/LineProbeStrategy.cs

src/Videra.SurfaceCharts.Avalonia/Controls/
    Plot/LinePlot3DSeries.cs      # Host-facing handle
    Plot/RibbonPlot3DSeries.cs
    LineChartRenderingStatus.cs   # Diagnostics
    RibbonChartRenderingStatus.cs
```

### Pattern 1: Chart Kind Addition Pattern (from Bar/Contour)

**What:** Every chart kind follows a 7-step integration pattern.
**When to use:** Adding any new chart family to Videra.
**Example (from Bar chart, verified in codebase):**

Step 1 -- Plot3DSeriesKind enum value:
```csharp
// Source: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs
public enum Plot3DSeriesKind
{
    Surface, Waterfall, Scatter, Bar, Contour,
    // New values go here:
    Line, Ribbon, VectorField, HeatmapSlice, BoxPlot,
}
```

Step 2 -- Data model in Core:
```csharp
// Source: src/Videra.SurfaceCharts.Core/BarChartData.cs
public sealed class BarChartData
{
    public BarChartLayout Layout { get; }
    public IReadOnlyList<BarSeries> Series => _seriesView;
    public int SeriesCount => _seriesView.Count;
    public int CategoryCount => _seriesView.Count > 0 ? _seriesView[0].CategoryCount : 0;
}
```

Step 3 -- Renderer in Core/Rendering:
```csharp
// Source: src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs
public static class BarRenderer
{
    public static BarRenderScene BuildScene(BarChartData data) { ... }
}
```

Step 4 -- Render scene in Core/Rendering:
```csharp
// Source: src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs
public sealed class BarRenderScene
{
    public int CategoryCount { get; }
    public int SeriesCount { get; }
    public IReadOnlyList<BarRenderBar> Bars => _barsView;
}
```

Step 5 -- Plot3DSeries subclass:
```csharp
// Source: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs
public sealed class BarPlot3DSeries : Plot3DSeries
{
    internal BarPlot3DSeries(string? name, BarChartData data)
        : base(Plot3DSeriesKind.Bar, name, surfaceSource: null, scatterData: null, data, contourData: null)
    { }
}
```

Step 6 -- Plot3DAddApi method:
```csharp
// Source: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs
public BarPlot3DSeries Bar(double[] values, string? name = null) { ... }
public BarPlot3DSeries Bar(BarChartData data, string? name = null) { ... }
```

Step 7 -- Probe strategy:
```csharp
// Source: src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs
public sealed class BarProbeStrategy : ISeriesProbeStrategy
{
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata) { ... }
}
```

### Pattern 2: Plot3DSeries Constructor Pattern

**What:** Plot3DSeries has a discriminated union constructor -- each chart kind passes its data to exactly one slot and null to the rest.
**When to use:** Creating any new Plot3DSeries subclass.
**Example:**
```csharp
// Source: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs
internal Plot3DSeries(
    Plot3DSeriesKind kind,
    string? name,
    ISurfaceTileSource? surfaceSource,   // Surface/Waterfall only
    ScatterChartData? scatterData,       // Scatter only
    BarChartData? barData,               // Bar only
    ContourChartData? contourData)       // Contour only
```

**New chart kinds will need:** A new data property on Plot3DSeries (e.g., `LineChartData? LineData`) and a new constructor slot, OR a generic data slot. The existing pattern uses per-kind slots -- new kinds should follow this for consistency.

### Anti-Patterns to Avoid

- **Shared data slot:** Do not use a single `object? Data` property -- the per-kind slots provide type safety and compile-time verification.
- **Rendering in Core:** Do not add Avalonia dependencies to Core -- renderers produce scene objects, painters consume them in the Avalonia layer.
- **Probe in Avalonia:** Do not put probe hit-testing logic in the Avalonia layer -- it belongs in Core/Picking so it can be tested without UI.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| 3D arrow geometry | Custom arrow mesh builder | Existing Vector3 math + cylinder/cone primitives | Arrow rendering is well-understood; focus on data model |
| Bilinear interpolation | Custom interpolation | Already in SurfaceProbeService.TryResolveInterpolatedValue | For heatmap slice probing |
| Point-to-segment distance | Custom distance calc | Already in ContourProbeStrategy.PointToSegmentDistance | For line/ribbon probing |
| Scene caching | Custom cache invalidation | ContourSceneCache pattern (revision-keyed) | For expensive render scenes |

## Common Pitfalls

### Pitfall 1: Forgetting Plot3DSeriesComposition
**What goes wrong:** New chart kind only works with single series, fails when multiple series of same kind are added.
**Why it happens:** Plot3DSeriesComposition.Create*Data() must handle merging for the new kind.
**How to avoid:** Add a CreateLineData/CreateRibbonData/etc. method to Plot3DSeriesComposition.
**Warning signs:** Multiple series of same kind only show the last one.

### Pitfall 2: Missing Active*Series/Active*Data Properties on Plot3D
**What goes wrong:** New chart kind is invisible or probe doesn't work.
**Why it happens:** Plot3D has per-kind properties (ActiveBarSeries, ActiveBarData, etc.) that the overlay coordinator uses.
**How to avoid:** Add ActiveLineSeries, ActiveLineData, etc. to Plot3D.
**Warning signs:** Chart renders but probe returns null or legend shows wrong kind.

### Pitfall 3: Missing RenderingStatus Projection
**What goes wrong:** VideraChartView.Create*RenderingStatus() returns stale data for new kind.
**Why it happens:** Each chart kind has a Create*RenderingStatus() method called from Refresh().
**How to avoid:** Add CreateLineRenderingStatus() etc. and call from Refresh()/ArrangeOverride().
**Warning signs:** RenderingStatus event never fires for new chart kind.

### Pitfall 4: Legend Indicator Kind Mismatch
**What goes wrong:** New chart kind shows wrong legend indicator (e.g., dot instead of line).
**Why it happens:** SurfaceLegendOverlayPresenter.CreateLegendEntry() has a switch on Plot3DSeriesKind.
**How to avoid:** Add case for new kinds with appropriate LegendIndicatorKind.
**Warning signs:** Legend looks wrong in demo.

### Pitfall 5: Probe Strategy Not Registered
**What goes wrong:** Probing new chart kind returns null.
**Why it happens:** SeriesProbeStrategyDispatcher is constructed with a dictionary mapping kind to strategy.
**How to avoid:** Register new strategy in the dispatcher construction site.
**Warning signs:** Probe tooltip shows "No data" for new chart kind.

## Code Examples

### Adding a New Chart Kind (Complete Checklist)

Based on the Bar chart pattern (verified in codebase):

```csharp
// 1. Plot3DSeriesKind enum -- add value
public enum Plot3DSeriesKind { ..., Line }

// 2. Data model -- LineChartData.cs in Core
public sealed class LineChartData { ... }

// 3. Renderer -- LineRenderer.cs in Core/Rendering
public static class LineRenderer { public static LineRenderScene BuildScene(LineChartData data) { ... } }

// 4. Render scene -- LineRenderScene.cs in Core/Rendering
public sealed class LineRenderScene { ... }

// 5. Plot3DSeries subclass -- LinePlot3DSeries.cs in Avalonia/Controls/Plot
public sealed class LinePlot3DSeries : Plot3DSeries { ... }

// 6. Plot3DAddApi method -- add to Plot3DAddApi.cs
public LinePlot3DSeries Line(double[] xs, double[] ys, double[] zs, string? name = null) { ... }

// 7. Plot3DSeries -- add data property and constructor slot
public LineChartData? LineData { get; }

// 8. Plot3D -- add ActiveLineSeries, ActiveLineData, GetVisibleSeries support
internal Plot3DSeries? ActiveLineSeries { get; }
internal LineChartData? ActiveLineData { get; }

// 9. Plot3DSeriesComposition -- add CreateLineData()
public static LineChartData? CreateLineData(IReadOnlyList<Plot3DSeries> series) { ... }

// 10. Probe strategy -- LineProbeStrategy.cs in Core/Picking
public sealed class LineProbeStrategy : ISeriesProbeStrategy { ... }

// 11. Register probe strategy in dispatcher construction

// 12. Legend -- add case in SurfaceLegendOverlayPresenter.CreateLegendEntry()

// 13. Rendering status -- LineChartRenderingStatus.cs
public sealed record LineChartRenderingStatus { ... }

// 14. VideraChartView -- add CreateLineRenderingStatus(), call from Refresh()

// 15. Plot3DOutputEvidence -- handle new kind in CreateRenderingEvidence()

// 16. Plot3DDatasetEvidence -- handle new kind in series identity creation
```

### API Contract Design: Line Chart

```csharp
// Plot3DAddApi signatures:
public LinePlot3DSeries Line(double[] xs, double[] ys, double[] zs, string? name = null);
public LinePlot3DSeries Line(LineChartData data, string? name = null);

// Data model:
public sealed class LineChartData
{
    public IReadOnlyList<LineSeries> Series { get; }
    public ScatterChartMetadata Metadata { get; } // Reuse existing metadata type
}

public sealed class LineSeries
{
    public IReadOnlyList<ScatterPoint> Points { get; } // Reuse ScatterPoint
    public uint Color { get; }
    public float Width { get; }
    public string? Label { get; }
}

// LinePlot3DSeries host-facing handle:
public sealed class LinePlot3DSeries : Plot3DSeries
{
    public void SetColor(uint color);
    public void SetWidth(float width);
}
```

### API Contract Design: Ribbon Chart

```csharp
// Plot3DAddApi signatures:
public RibbonPlot3DSeries Ribbon(double[] xs, double[] ys, double[] zs, float radius, string? name = null);
public RibbonPlot3DSeries Ribbon(RibbonChartData data, string? name = null);

// Data model:
public sealed class RibbonChartData
{
    public IReadOnlyList<RibbonSeries> Series { get; }
    public ScatterChartMetadata Metadata { get; }
}

public sealed class RibbonSeries
{
    public IReadOnlyList<ScatterPoint> Points { get; }
    public float Radius { get; }
    public uint Color { get; }
    public string? Label { get; }
}
```

### API Contract Design: Vector Field

```csharp
// Plot3DAddApi signatures:
public VectorFieldPlot3DSeries VectorField(
    double[] xs, double[] ys, double[] zs,
    double[] dxs, double[] dys, double[] dzs,
    string? name = null);
public VectorFieldPlot3DSeries VectorField(VectorFieldChartData data, string? name = null);

// Data model:
public sealed class VectorFieldChartData
{
    public IReadOnlyList<VectorFieldPoint> Points { get; }
    public SurfaceAxisDescriptor HorizontalAxis { get; }
    public SurfaceAxisDescriptor DepthAxis { get; }
    public SurfaceValueRange MagnitudeRange { get; }
}

public readonly record struct VectorFieldPoint(
    Vector3 Position,
    Vector3 Direction, // dx, dy, dz
    double Magnitude);
```

### API Contract Design: Heatmap Slice

```csharp
// Plot3DAddApi signatures:
public HeatmapSlicePlot3DSeries HeatmapSlice(
    double[,] values,
    HeatmapSliceAxis axis,
    double position,
    string? name = null);
public HeatmapSlicePlot3DSeries HeatmapSlice(HeatmapSliceData data, string? name = null);

// Data model:
public enum HeatmapSliceAxis { X, Y, Z }

public sealed class HeatmapSliceData
{
    public SurfaceScalarField Field { get; }
    public HeatmapSliceAxis Axis { get; }
    public double Position { get; } // 0..1 normalized
    public SurfaceColorMap? ColorMap { get; }
}
```

### API Contract Design: Box Plot

```csharp
// Plot3DAddApi signatures:
public BoxPlotPlot3DSeries BoxPlot(BoxPlotData data, string? name = null);

// Data model:
public sealed class BoxPlotData
{
    public IReadOnlyList<BoxPlotCategory> Categories { get; }
}

public sealed class BoxPlotCategory
{
    public string Label { get; }
    public double Min { get; }
    public double Q1 { get; }
    public double Median { get; }
    public double Q3 { get; }
    public double Max { get; }
    public IReadOnlyList<double> Outliers { get; }
    public uint Color { get; }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Per-kind source property on VideraChartView | Plot3D owns series, VideraChartView reads Plot.ActiveSeries | v2.64 | Single source of truth for chart kind |
| Direct rendering in VideraChartView | Renderer builds scene, painter draws scene | v2.64 | Separation of Core and Avalonia layers |
| Probe in Avalonia layer | ISeriesProbeStrategy in Core/Picking | v2.64 | Testable without UI |
| Per-kind legend code | SurfaceLegendOverlayPresenter with switch on kind | v2.64 | Centralized legend logic |

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Plot3DSeries will need a new data property slot for each new chart kind (LineChartData, RibbonChartData, etc.) | Pattern 2 | Medium -- could use generic slot but breaks consistency |
| A2 | SurfaceProbeInfo (sampleX, sampleY, axisX, axisY, value) is sufficient for all new chart kinds | Probe strategy | Low -- may need extension for vector field (dx, dy, dz) |
| A3 | LegendIndicatorKind enum needs new values for line, ribbon, vector, heatmap, box | Legend overlay | Low -- easy to add |
| A4 | Rendering status pattern (sealed record with HasSource, IsReady, kind-specific fields) applies to all new kinds | Rendering status | Low -- proven pattern |

## Open Questions

1. **Should Plot3DSeries use a generic data slot or per-kind slots?**
   - What we know: Current pattern uses per-kind slots (SurfaceSource, ScatterData, BarData, ContourData)
   - What's unclear: Whether 10+ slots is sustainable
   - Recommendation: Keep per-kind slots for type safety; the constructor already has 6 parameters

2. **Should SurfaceProbeInfo be extended for vector field (dx, dy, dz) display?**
   - What we know: Current SurfaceProbeInfo has single Value field
   - What's unclear: Whether vector field probe needs multiple values
   - Recommendation: Use Value for magnitude, add optional VectorComponents field or use a separate probe info type

3. **Should heatmap slice reuse the existing surface rendering pipeline or have its own?**
   - What we know: Surface rendering uses tile-based pipeline with GPU backend
   - What's unclear: Whether a plane at arbitrary axis position fits the tile model
   - Recommendation: Use a simpler approach -- render as a textured quad in the scene painter, not through the tile pipeline

## Environment Availability

> Skip (code/config-only phase, no external dependencies needed).

## Validation Architecture

> Skipped -- `workflow.nyquist_validation` is explicitly `false` in config.

## Security Domain

> Skipped -- `security_enforcement` is not set (absent = enabled), but this phase is an inventory/design phase with no security-relevant changes. New chart types are data visualization only with no auth, session, or input validation concerns.

## Sources

### Primary (HIGH confidence)
- Codebase inspection: Plot3DSeriesKind.cs, IPlottable3D.cs, Plot3D.cs, Plot3DAddApi.cs
- Codebase inspection: Plot3DSeries.cs, BarPlot3DSeries.cs, ContourPlot3DSeries.cs
- Codebase inspection: BarRenderer.cs, ScatterRenderer.cs, ContourRenderScene.cs
- Codebase inspection: ISeriesProbeStrategy.cs, BarProbeStrategy.cs, ContourProbeStrategy.cs, ScatterProbeStrategy.cs
- Codebase inspection: SeriesProbeStrategyDispatcher.cs, SurfaceProbeService.cs
- Codebase inspection: SurfaceLegendOverlayPresenter.cs, SurfaceProbeOverlayPresenter.cs
- Codebase inspection: VideraChartView.Core.cs, VideraChartView.Rendering.cs, VideraChartView.Overlay.cs
- Codebase inspection: BarChartData.cs, ContourChartData.cs, ScatterChartData.cs
- Codebase inspection: Plot3DSeriesComposition.cs, SurfaceScenePainter.cs
- Codebase inspection: DataLogger3D.cs, SurfaceChartLinkGroup.cs, SurfaceChartInteractionPropagator.cs

### Secondary (MEDIUM confidence)
- REQUIREMENTS.md, STATE.md, ROADMAP.md for v2.65 scope and decisions

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all items verified in codebase
- Architecture: HIGH -- patterns verified through Bar/Contour implementation
- Pitfalls: HIGH -- derived from codebase structure analysis
- API contracts: MEDIUM -- designed from existing patterns, not yet validated with implementation

**Research date:** 2026-04-30
**Valid until:** 2026-05-30 (stable -- architecture patterns are well-established)
