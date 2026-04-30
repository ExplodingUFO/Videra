# Phase 432: Chart Type Inventory and API Design - Pattern Map

**Mapped:** 2026-04-30
**Files analyzed:** 30 new files + 9 modified files
**Analogs found:** 8 / 8

## File Classification

### New Files (per chart family: Line, Ribbon, VectorField, HeatmapSlice, BoxPlot)

| New File | Role | Data Flow | Closest Analog | Match Quality |
|----------|------|-----------|----------------|---------------|
| `src/Videra.SurfaceCharts.Core/{Kind}ChartData.cs` | model | transform | `src/Videra.SurfaceCharts.Core/BarChartData.cs` | exact |
| `src/Videra.SurfaceCharts.Core/Rendering/{Kind}Renderer.cs` | service | transform | `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs` | exact |
| `src/Videra.SurfaceCharts.Core/Rendering/{Kind}RenderScene.cs` | model | transform | `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs` | exact |
| `src/Videra.SurfaceCharts.Core/Picking/{Kind}ProbeStrategy.cs` | service | request-response | `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs` | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/{Kind}Plot3DSeries.cs` | component | CRUD | `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs` | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/{Kind}ChartRenderingStatus.cs` | model | transform | `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs` | exact |

### Modified Files

| Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---------------|------|-----------|----------------|---------------|
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesKind.cs` | config | N/A | self (add enum values) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeries.cs` | component | CRUD | self (add data properties + constructor slots) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DAddApi.cs` | controller | CRUD | self (add API methods) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` | component | CRUD | self (add Active*Series/Active*Data) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesComposition.cs` | service | transform | self (add Create*Data methods) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SeriesProbeStrategyDispatcher.cs` | service | event-driven | self (register new strategies) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayPresenter.cs` | component | transform | self (add legend cases) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` | controller | CRUD | self (add Create*RenderingStatus) | exact |
| `src/Videra.SurfaceCharts.Avalonia/Controls/Overlay/SurfaceLegendOverlayState.cs` | config | N/A | self (add LegendIndicatorKind values) | exact |

## Pattern Assignments

### `{Kind}ChartData.cs` (model, transform)

**Analog:** `src/Videra.SurfaceCharts.Core/BarChartData.cs`

**Imports pattern** (lines 1-3):
```csharp
using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;
```

**Core data model pattern** (lines 24-110):
```csharp
public sealed class BarChartData
{
    private readonly ReadOnlyCollection<BarSeries> _seriesView;

    public BarChartData(IReadOnlyList<BarSeries> series, BarChartLayout layout = BarChartLayout.Grouped)
    {
        Layout = layout;
        _seriesView = CreateSeriesView(series, out _);
    }

    public BarChartLayout Layout { get; }
    public IReadOnlyList<BarSeries> Series => _seriesView;
    public int SeriesCount => _seriesView.Count;
    public int CategoryCount => _seriesView.Count > 0 ? _seriesView[0].CategoryCount : 0;

    private static ReadOnlyCollection<BarSeries> CreateSeriesView(IReadOnlyList<BarSeries> series, out int categoryCount)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.Count == 0)
        {
            throw new ArgumentException("Bar chart data must contain at least one series.", nameof(series));
        }
        categoryCount = series[0].CategoryCount;
        return Array.AsReadOnly(series.ToArray());
    }
}
```

**Validation pattern** (lines 48-61):
```csharp
ArgumentNullException.ThrowIfNull(categoryLabels);
if (categoryLabels.Count != categoryCount)
{
    throw new ArgumentException(
        $"Category label count must match the category count. Expected {categoryCount} but found {categoryLabels.Count}.",
        nameof(categoryLabels));
}
```

**Key conventions:**
- `sealed class` for immutable data models
- `ReadOnlyCollection<T>` for all exposed collections
- `Array.AsReadOnly(series.ToArray())` to create immutable views
- Constructor validation with `ArgumentNullException.ThrowIfNull`
- XML doc comments on all public members
- Namespace: `Videra.SurfaceCharts.Core`

---

### `{Kind}Renderer.cs` (service, transform)

**Analog:** `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs`

**Imports pattern** (lines 1-3):
```csharp
using System.Numerics;

namespace Videra.SurfaceCharts.Core;
```

**Core renderer pattern** (lines 8-40):
```csharp
public static class BarRenderer
{
    private const float BarWidthFraction = 0.8f;
    private const float BarDepthFraction = 0.6f;
    private const float ZFightEpsilon = 0.001f;

    public static BarRenderScene BuildScene(BarChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.Layout switch
        {
            BarChartLayout.Grouped => BuildGroupedScene(data),
            BarChartLayout.Stacked => BuildStackedScene(data),
            _ => throw new ArgumentOutOfRangeException(nameof(data), "Unsupported bar chart layout."),
        };
    }

    private static BarRenderScene BuildGroupedScene(BarChartData data)
    {
        var categoryCount = data.CategoryCount;
        var seriesCount = data.SeriesCount;
        var bars = new List<BarRenderBar>(categoryCount * seriesCount);
        // ... geometry computation ...
        return new BarRenderScene(categoryCount, seriesCount, BarChartLayout.Grouped, bars);
    }
}
```

**Key conventions:**
- `public static class` for stateless renderers
- Single public `BuildScene(ChartData data)` entry point
- Private helper methods for each variant
- `System.Numerics.Vector3` for all 3D geometry
- Returns immutable render scene object
- Namespace: `Videra.SurfaceCharts.Core`

---

### `{Kind}RenderScene.cs` (model, transform)

**Analog:** `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs`

**Imports pattern** (lines 1-3):
```csharp
using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;
```

**Core render scene pattern** (lines 8-54):
```csharp
public sealed class BarRenderScene
{
    private readonly ReadOnlyCollection<BarRenderBar> _barsView;

    public BarRenderScene(
        int categoryCount,
        int seriesCount,
        BarChartLayout layout,
        IReadOnlyList<BarRenderBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);

        CategoryCount = categoryCount;
        SeriesCount = seriesCount;
        Layout = layout;
        _barsView = Array.AsReadOnly(bars.ToArray());
    }

    public int CategoryCount { get; }
    public int SeriesCount { get; }
    public BarChartLayout Layout { get; }
    public IReadOnlyList<BarRenderBar> Bars => _barsView;
}
```

**Render element pattern** (from `BarRenderBar.cs`):
```csharp
public readonly record struct BarRenderBar(Vector3 Position, Vector3 Size, uint Color);
```

**Key conventions:**
- `sealed class` for render scenes
- `readonly record struct` for render elements (points, bars, segments)
- `ReadOnlyCollection<T>` for all exposed collections
- Constructor validation with `ArgumentNullException.ThrowIfNull` and `ArgumentOutOfRangeException`
- Namespace: `Videra.SurfaceCharts.Core`

---

### `{Kind}ProbeStrategy.cs` (service, request-response)

**Analog:** `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs`

**Imports pattern** (lines 1-3):
```csharp
using System.Numerics;

namespace Videra.SurfaceCharts.Core;
```

**Core probe strategy pattern** (lines 8-67):
```csharp
public sealed class BarProbeStrategy : ISeriesProbeStrategy
{
    private readonly BarRenderScene _scene;

    public BarProbeStrategy(BarRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        BarRenderBar? hitBar = null;
        var minDistSq = double.MaxValue;

        foreach (var bar in _scene.Bars)
        {
            var halfWidth = bar.Size.X / 2f;
            var halfDepth = bar.Size.Z / 2f;
            var dx = chartX - bar.Position.X;
            var dz = chartZ - bar.Position.Z;

            if (Math.Abs(dx) <= halfWidth && Math.Abs(dz) <= halfDepth)
            {
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    hitBar = bar;
                }
            }
        }

        if (hitBar is not BarRenderBar hit)
        {
            return null;
        }

        var value = hit.Position.Y + hit.Size.Y;
        return new SurfaceProbeInfo(
            sampleX: hit.Position.X,
            sampleY: hit.Position.Z,
            axisX: hit.Position.X,
            axisY: hit.Position.Z,
            value: value,
            isApproximate: false);
    }
}
```

**Alternative pattern (line-based probing from ContourProbeStrategy):**
```csharp
public sealed class ContourProbeStrategy : ISeriesProbeStrategy
{
    private readonly ContourRenderScene _scene;
    private readonly double _snapRadius;

    public ContourProbeStrategy(ContourRenderScene scene, double snapRadius = 0.05)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        // Find nearest contour line segment within snap radius
        // Project cursor onto nearest segment
        // Return SurfaceProbeInfo with iso-value
    }

    private static double PointToSegmentDistance(double px, double pz, double ax, double az, double bx, double bz)
    {
        // Reusable point-to-segment distance calculation
    }
}
```

**Key conventions:**
- `sealed class` implementing `ISeriesProbeStrategy`
- Constructor takes render scene (not data model)
- Single `TryResolve` method returning `SurfaceProbeInfo?`
- Returns `null` when no hit detected
- Uses named parameters in `SurfaceProbeInfo` constructor
- Namespace: `Videra.SurfaceCharts.Core`

---

### `{Kind}Plot3DSeries.cs` (component, CRUD)

**Analog:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs`

**Imports pattern** (lines 1-3):
```csharp
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;
```

**Core series subclass pattern** (lines 8-38):
```csharp
public sealed class BarPlot3DSeries : Plot3DSeries
{
    internal BarPlot3DSeries(string? name, BarChartData data)
        : base(Plot3DSeriesKind.Bar, name, surfaceSource: null, scatterData: null, data, contourData: null)
    {
    }

    public void SetSeriesColor(int seriesIndex, uint color)
    {
        var data = BarData ?? throw new InvalidOperationException("Bar series requires bar data.");
        ArgumentOutOfRangeException.ThrowIfNegative(seriesIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(seriesIndex, data.Series.Count);

        var current = data.Series[seriesIndex];
        if (current.Color == color)
        {
            return;
        }

        var series = data.Series.ToArray();
        series[seriesIndex] = new BarSeries(current.Values, color, current.Label);
        ReplaceBarData(data.CategoryLabels.Count > 0
            ? new BarChartData(series, data.CategoryLabels, data.Layout)
            : new BarChartData(series, data.Layout));
    }
}
```

**Simpler variant (from ContourPlot3DSeries):**
```csharp
public sealed class ContourPlot3DSeries : Plot3DSeries
{
    internal ContourPlot3DSeries(string? name, ContourChartData data)
        : base(Plot3DSeriesKind.Contour, name, surfaceSource: null, scatterData: null, barData: null, data)
    {
    }
}
```

**Key conventions:**
- `sealed class` extending `Plot3DSeries`
- `internal` constructor (only Plot3DAddApi creates instances)
- Pass `null` to all unused data slots in base constructor
- Pass `Plot3DSeriesKind.{Kind}` as first argument
- Use `Replace*Data()` private protected method for mutable updates
- Namespace: `Videra.SurfaceCharts.Avalonia.Controls`

---

### `{Kind}ChartRenderingStatus.cs` (model, transform)

**Analog:** `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs`

**Imports pattern** (lines 1-5):
```csharp
using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;
```

**Core rendering status pattern** (lines 10-56):
```csharp
public sealed record BarChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int SeriesCount { get; init; }
    public int CategoryCount { get; init; }
    public int BarCount { get; init; }
    public BarChartLayout Layout { get; init; }
    public Size ViewSize { get; init; }
}
```

**Key conventions:**
- `sealed record` for immutable diagnostics
- `{ init; }` auto-properties with sensible defaults
- `HasSource` and `IsReady` as standard boolean fields
- Kind-specific diagnostic fields (SeriesCount, CategoryCount, etc.)
- Namespace: `Videra.SurfaceCharts.Avalonia.Controls`

---

### Modified Files: Integration Points

#### `Plot3DSeriesKind.cs` - Add Enum Values

**Current state** (lines 1-32):
```csharp
public enum Plot3DSeriesKind
{
    Surface,
    Waterfall,
    Scatter,
    Bar,
    Contour,
}
```

**Add:** `Line, Ribbon, VectorField, HeatmapSlice, BoxPlot`

---

#### `Plot3DSeries.cs` - Add Data Properties and Constructor Slots

**Current constructor** (lines 14-28):
```csharp
internal Plot3DSeries(
    Plot3DSeriesKind kind,
    string? name,
    ISurfaceTileSource? surfaceSource,
    ScatterChartData? scatterData,
    BarChartData? barData,
    ContourChartData? contourData)
```

**Add:** New data properties and constructor parameters for each chart kind:
- `LineChartData? LineData { get; }`
- `RibbonChartData? RibbonData { get; }`
- `VectorFieldChartData? VectorFieldData { get; }`
- `HeatmapSliceData? HeatmapSliceData { get; }`
- `BoxPlotData? BoxPlotData { get; }`

**Add:** `Replace*Data()` methods following the `ReplaceBarData` pattern (lines 103-118):
```csharp
private protected void ReplaceBarData(BarChartData data)
{
    ArgumentNullException.ThrowIfNull(data);
    if (Kind != Plot3DSeriesKind.Bar)
    {
        throw new InvalidOperationException("Only bar series can update bar data.");
    }
    if (ReferenceEquals(BarData, data)) { return; }
    BarData = data;
    NotifyChanged();
}
```

---

#### `Plot3DAddApi.cs` - Add API Methods

**Current pattern** (lines 90-122):
```csharp
public BarPlot3DSeries Bar(double[] values, string? name = null)
{
    ArgumentNullException.ThrowIfNull(values);
    var series = new BarSeries(values, color: 0xFF4488CCu, label: name);
    return Bar(new BarChartData([series]), name);
}

public BarPlot3DSeries Bar(BarChartData data, string? name = null)
{
    ArgumentNullException.ThrowIfNull(data);
    return (BarPlot3DSeries)_plot.AddSeries(new BarPlot3DSeries(name, data));
}
```

**Add:** Overloaded methods for each chart kind:
- `Line(double[] xs, double[] ys, double[] zs, string? name = null)` and `Line(LineChartData data, string? name = null)`
- `Ribbon(double[] xs, double[] ys, double[] zs, float radius, string? name = null)` and `Ribbon(RibbonChartData data, string? name = null)`
- `VectorField(...)` overloads
- `HeatmapSlice(...)` overloads
- `BoxPlot(...)` overloads

---

#### `Plot3D.cs` - Add Active*Series/Active*Data Properties

**Current pattern** (lines 113-158):
```csharp
internal BarChartData? ActiveBarData =>
    ActiveSeries?.Kind == Plot3DSeriesKind.Bar
        ? Plot3DSeriesComposition.CreateBarData(GetVisibleSeries(Plot3DSeriesKind.Bar))
        : null;

internal Plot3DSeries? ActiveBarSeries
{
    get
    {
        var activeSeries = ActiveSeries;
        return activeSeries?.Kind == Plot3DSeriesKind.Bar ? activeSeries : null;
    }
}
```

**Add:** Properties for each new kind:
- `ActiveLineData`, `ActiveLineSeries`
- `ActiveRibbonData`, `ActiveRibbonSeries`
- `ActiveVectorFieldData`, `ActiveVectorFieldSeries`
- `ActiveHeatmapSliceData`, `ActiveHeatmapSliceSeries`
- `ActiveBoxPlotData`, `ActiveBoxPlotSeries`

---

#### `Plot3DSeriesComposition.cs` - Add Create*Data Methods

**Current pattern** (lines 46-73):
```csharp
public static BarChartData? CreateBarData(IReadOnlyList<Plot3DSeries> series)
{
    var datasets = series
        .Select(static item => item.BarData)
        .OfType<BarChartData>()
        .ToArray();

    if (datasets.Length == 0) { return null; }
    if (datasets.Length == 1) { return datasets[0]; }

    var first = datasets[0];
    var compatible = datasets.All(data =>
        data.Layout == first.Layout &&
        data.CategoryCount == first.CategoryCount);
    if (!compatible) { return datasets[^1]; }

    return new BarChartData(datasets.SelectMany(static data => data.Series).ToArray(), first.Layout);
}
```

**Add:** `CreateLineData`, `CreateRibbonData`, `CreateVectorFieldData`, `CreateHeatmapSliceData`, `CreateBoxPlotData`

---

#### `SeriesProbeStrategyDispatcher.cs` - Register New Strategies

**Current pattern** (lines 8-20):
```csharp
internal sealed class SeriesProbeStrategyDispatcher
{
    private readonly Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy> _strategies;

    public SeriesProbeStrategyDispatcher(IReadOnlyDictionary<Plot3DSeriesKind, ISeriesProbeStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        _strategies = new Dictionary<Plot3DSeriesKind, ISeriesProbeStrategy>(strategies);
    }
}
```

**Action:** Construction site must add entries for new kinds to the dictionary passed to the constructor.

---

#### `SurfaceLegendOverlayPresenter.cs` - Add Legend Cases

**Current pattern** (lines 120-147):
```csharp
private static SurfaceLegendEntry CreateLegendEntry(Plot3DSeries series)
{
    var indicatorKind = series.Kind switch
    {
        Plot3DSeriesKind.Surface => LegendIndicatorKind.Swatch,
        Plot3DSeriesKind.Waterfall => LegendIndicatorKind.Swatch,
        Plot3DSeriesKind.Scatter => LegendIndicatorKind.Dot,
        _ => LegendIndicatorKind.Swatch,
    };

    var color = series.Kind switch
    {
        Plot3DSeriesKind.Surface => 0xFF4DA3FF,
        Plot3DSeriesKind.Waterfall => 0xFF4DA3FF,
        Plot3DSeriesKind.Scatter => 0xFFFF6B6B,
        _ => 0xFFCCCCCC,
    };
    // ...
}
```

**Add cases:**
- `Plot3DSeriesKind.Line => LegendIndicatorKind.Line` (already exists in enum)
- `Plot3DSeriesKind.Ribbon => LegendIndicatorKind.Swatch`
- `Plot3DSeriesKind.VectorField => LegendIndicatorKind.Swatch` or new arrow indicator
- `Plot3DSeriesKind.HeatmapSlice => LegendIndicatorKind.Swatch`
- `Plot3DSeriesKind.BoxPlot => LegendIndicatorKind.Swatch`

---

#### `VideraChartView.Core.cs` - Add Create*RenderingStatus Methods

**Current pattern** (lines 279-295):
```csharp
private BarChartRenderingStatus CreateBarRenderingStatus()
{
    var barData = Plot.ActiveBarData;
    var hasBarData = barData is not null;
    return new BarChartRenderingStatus
    {
        HasSource = hasBarData,
        IsReady = hasBarData,
        BackendKind = SurfaceChartRenderBackendKind.Software,
        IsInteracting = _interactionController.HasActiveGesture,
        SeriesCount = barData?.SeriesCount ?? 0,
        CategoryCount = barData?.CategoryCount ?? 0,
        BarCount = barData is not null ? barData.SeriesCount * barData.CategoryCount : 0,
        Layout = barData?.Layout ?? BarChartLayout.Grouped,
        ViewSize = _runtime.ViewSize,
    };
}
```

**Add:** `CreateLineRenderingStatus()`, `CreateRibbonRenderingStatus()`, `CreateVectorFieldRenderingStatus()`, `CreateHeatmapSliceRenderingStatus()`, `CreateBoxPlotRenderingStatus()`

**Also add:** Properties and update calls in `Refresh()` and `ArrangeOverride()`:
```csharp
public LineChartRenderingStatus LineRenderingStatus { get; private set; }
// In constructor:
LineRenderingStatus = CreateLineRenderingStatus();
// In Refresh():
UpdateLineRenderingStatus();
```

---

## Shared Patterns

### Immutable Data Model Pattern
**Source:** `src/Videra.SurfaceCharts.Core/BarChartData.cs`
**Apply to:** All new `{Kind}ChartData.cs` files
```csharp
using System.Collections.ObjectModel;
// sealed class, ReadOnlyCollection<T>, Array.AsReadOnly(), constructor validation
```

### Static Renderer Pattern
**Source:** `src/Videra.SurfaceCharts.Core/Rendering/BarRenderer.cs`
**Apply to:** All new `{Kind}Renderer.cs` files
```csharp
public static class {Kind}Renderer
{
    public static {Kind}RenderScene BuildScene({Kind}ChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        // geometry computation
        return new {Kind}RenderScene(...);
    }
}
```

### Render Scene + Element Pattern
**Source:** `src/Videra.SurfaceCharts.Core/Rendering/BarRenderScene.cs` + `BarRenderBar.cs`
**Apply to:** All new `{Kind}RenderScene.cs` files
```csharp
// Scene: sealed class with ReadOnlyCollection<Element>
// Element: readonly record struct with Vector3 Position, uint Color
```

### Probe Strategy Pattern
**Source:** `src/Videra.SurfaceCharts.Core/Picking/BarProbeStrategy.cs`
**Apply to:** All new `{Kind}ProbeStrategy.cs` files
```csharp
public sealed class {Kind}ProbeStrategy : ISeriesProbeStrategy
{
    private readonly {Kind}RenderScene _scene;
    public {Kind}ProbeStrategy({Kind}RenderScene scene) { ... }
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata) { ... }
}
```

### Plot3DSeries Subclass Pattern
**Source:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BarPlot3DSeries.cs`
**Apply to:** All new `{Kind}Plot3DSeries.cs` files
```csharp
public sealed class {Kind}Plot3DSeries : Plot3DSeries
{
    internal {Kind}Plot3DSeries(string? name, {Kind}ChartData data)
        : base(Plot3DSeriesKind.{Kind}, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ...)
    { }
}
```

### Rendering Status Pattern
**Source:** `src/Videra.SurfaceCharts.Avalonia/Controls/BarChartRenderingStatus.cs`
**Apply to:** All new `{Kind}ChartRenderingStatus.cs` files
```csharp
public sealed record {Kind}ChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    // Kind-specific fields...
}
```

### Active*Series/Active*Data Property Pattern
**Source:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3D.cs` lines 113-158
**Apply to:** Plot3D.cs for each new kind
```csharp
internal {Kind}ChartData? Active{Kind}Data =>
    ActiveSeries?.Kind == Plot3DSeriesKind.{Kind}
        ? Plot3DSeriesComposition.Create{Kind}Data(GetVisibleSeries(Plot3DSeriesKind.{Kind}))
        : null;

internal Plot3DSeries? Active{Kind}Series
{
    get
    {
        var activeSeries = ActiveSeries;
        return activeSeries?.Kind == Plot3DSeriesKind.{Kind} ? activeSeries : null;
    }
}
```

### Composition Merge Pattern
**Source:** `src/Videra.SurfaceCharts.Avalonia/Controls/Plot/Plot3DSeriesComposition.cs` lines 46-73
**Apply to:** Plot3DSeriesComposition.cs for each new kind
```csharp
public static {Kind}ChartData? Create{Kind}Data(IReadOnlyList<Plot3DSeries> series)
{
    var datasets = series.Select(static item => item.{Kind}Data).OfType<{Kind}ChartData>().ToArray();
    if (datasets.Length == 0) return null;
    if (datasets.Length == 1) return datasets[0];
    // Merge logic...
}
```

### Rendering Status Update Pattern
**Source:** `src/Videra.SurfaceCharts.Avalonia/Controls/VideraChartView.Core.cs` lines 268-295
**Apply to:** VideraChartView.Core.cs for each new kind
```csharp
private void Update{Kind}RenderingStatus()
{
    var nextStatus = Create{Kind}RenderingStatus();
    if ({Kind}RenderingStatus == nextStatus) return;
    {Kind}RenderingStatus = nextStatus;
}
```

## No Analog Found

Files with no close match in the codebase (planner should use RESEARCH.md patterns instead):

| File | Role | Data Flow | Reason |
|------|------|-----------|--------|
| None | -- | -- | All new chart types follow the exact Bar/Contour pattern |

## Metadata

**Analog search scope:** `src/Videra.SurfaceCharts.Core/`, `src/Videra.SurfaceCharts.Avalonia/Controls/`
**Files scanned:** 25
**Pattern extraction date:** 2026-04-30
