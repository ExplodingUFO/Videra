# Phase 432: API Contracts for New Chart Families

**Designed:** 2026-04-30
**Source:** Patterns from 432-RESEARCH.md, 432-PATTERNS.md, and codebase inspection

## 1. Line Chart API Contract

### Plot3DAddApi Signatures

```csharp
/// <summary>
/// Adds a 3D line series from coordinate arrays.
/// </summary>
public LinePlot3DSeries Line(double[] xs, double[] ys, double[] zs, string? name = null);

/// <summary>
/// Adds a 3D line series from a full line dataset.
/// </summary>
public LinePlot3DSeries Line(LineChartData data, string? name = null);
```

### Data Model

```csharp
// File: src/Videra.SurfaceCharts.Core/LineChartData.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable line-chart dataset.
/// </summary>
public sealed class LineChartData
{
    private readonly ReadOnlyCollection<LineSeries> _seriesView;

    public LineChartData(IReadOnlyList<LineSeries> series, ScatterChartMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(metadata);
        if (series.Count == 0)
        {
            throw new ArgumentException("Line chart data must contain at least one series.", nameof(series));
        }
        _seriesView = Array.AsReadOnly(series.ToArray());
        Metadata = metadata;
    }

    public ScatterChartMetadata Metadata { get; }
    public IReadOnlyList<LineSeries> Series => _seriesView;
    public int SeriesCount => _seriesView.Count;
    public int PointCount => _seriesView.Sum(s => s.Points.Count);
}

/// <summary>
/// Represents one immutable line series.
/// </summary>
public sealed class LineSeries
{
    private readonly ReadOnlyCollection<ScatterPoint> _pointsView;

    public LineSeries(IReadOnlyList<ScatterPoint> points, uint color, float width = 1.5f, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        _pointsView = Array.AsReadOnly(points.ToArray());
        Color = color;
        Width = width;
        Label = label;
    }

    public IReadOnlyList<ScatterPoint> Points => _pointsView;
    public uint Color { get; }
    public float Width { get; }
    public string? Label { get; }
}
```

### Plot3DSeries Subclass

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/LinePlot3DSeries.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a line plottable.
/// </summary>
public sealed class LinePlot3DSeries : Plot3DSeries
{
    internal LinePlot3DSeries(string? name, LineChartData data)
        : base(Plot3DSeriesKind.Line, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: data)
    {
    }

    /// <summary>
    /// Updates the ARGB color for the line series.
    /// </summary>
    public void SetColor(uint color)
    {
        var data = LineData ?? throw new InvalidOperationException("Line series requires line data.");
        // Rebuild series with new color
        // ...
    }

    /// <summary>
    /// Updates the line width.
    /// </summary>
    public void SetWidth(float width)
    {
        var data = LineData ?? throw new InvalidOperationException("Line series requires line data.");
        // Rebuild series with new width
        // ...
    }
}
```

### Renderer

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/LineRenderer.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds render-ready line segments from immutable line chart data.
/// </summary>
public static class LineRenderer
{
    /// <summary>
    /// Builds one render-ready line scene from immutable line chart data.
    /// </summary>
    public static LineRenderScene BuildScene(LineChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var segments = new List<LineRenderSegment>();

        foreach (var series in data.Series)
        {
            for (var i = 0; i < series.Points.Count - 1; i++)
            {
                var start = series.Points[i];
                var end = series.Points[i + 1];
                segments.Add(new LineRenderSegment(
                    new Vector3((float)start.Horizontal, (float)start.Value, (float)start.Depth),
                    new Vector3((float)end.Horizontal, (float)end.Value, (float)end.Depth),
                    series.Color));
            }
        }

        return new LineRenderScene(data.SeriesCount, segments);
    }
}
```

### Render Scene

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/LineRenderScene.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a render-ready line-chart snapshot.
/// </summary>
public sealed class LineRenderScene
{
    private readonly ReadOnlyCollection<LineRenderSegment> _segmentsView;

    public LineRenderScene(int seriesCount, IReadOnlyList<LineRenderSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);
        SeriesCount = seriesCount;
        _segmentsView = Array.AsReadOnly(segments.ToArray());
    }

    public int SeriesCount { get; }
    public IReadOnlyList<LineRenderSegment> Segments => _segmentsView;
}

/// <summary>
/// A render-ready line segment.
/// </summary>
public readonly record struct LineRenderSegment(Vector3 Start, Vector3 End, uint Color);
```

### Probe Strategy

```csharp
// File: src/Videra.SurfaceCharts.Core/Picking/LineProbeStrategy.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for line series by finding the nearest line segment.
/// </summary>
public sealed class LineProbeStrategy : ISeriesProbeStrategy
{
    private readonly LineRenderScene _scene;
    private readonly double _snapRadius;

    public LineProbeStrategy(LineRenderScene scene, double snapRadius = 0.05)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        // Point-to-segment distance probing (reuse ContourProbeStrategy.PointToSegmentDistance pattern)
        // Find nearest segment within snap radius
        // Project cursor onto segment
        // Interpolate Y value at projection point
        // Return SurfaceProbeInfo
    }
}
```

### Rendering Status

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/LineChartRenderingStatus.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the current readiness state for line chart rendering.
/// </summary>
public sealed record LineChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int SeriesCount { get; init; }
    public int SegmentCount { get; init; }
    public Size ViewSize { get; init; }
}
```

---

## 2. Ribbon Chart API Contract

### Plot3DAddApi Signatures

```csharp
/// <summary>
/// Adds a 3D ribbon series from coordinate arrays with a tube radius.
/// </summary>
public RibbonPlot3DSeries Ribbon(double[] xs, double[] ys, double[] zs, float radius, string? name = null);

/// <summary>
/// Adds a 3D ribbon series from a full ribbon dataset.
/// </summary>
public RibbonPlot3DSeries Ribbon(RibbonChartData data, string? name = null);
```

### Data Model

```csharp
// File: src/Videra.SurfaceCharts.Core/RibbonChartData.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable ribbon-chart dataset.
/// </summary>
public sealed class RibbonChartData
{
    private readonly ReadOnlyCollection<RibbonSeries> _seriesView;

    public RibbonChartData(IReadOnlyList<RibbonSeries> series, ScatterChartMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(metadata);
        if (series.Count == 0)
        {
            throw new ArgumentException("Ribbon chart data must contain at least one series.", nameof(series));
        }
        _seriesView = Array.AsReadOnly(series.ToArray());
        Metadata = metadata;
    }

    public ScatterChartMetadata Metadata { get; }
    public IReadOnlyList<RibbonSeries> Series => _seriesView;
    public int SeriesCount => _seriesView.Count;
}

/// <summary>
/// Represents one immutable ribbon series.
/// </summary>
public sealed class RibbonSeries
{
    private readonly ReadOnlyCollection<ScatterPoint> _pointsView;

    public RibbonSeries(IReadOnlyList<ScatterPoint> points, float radius, uint color, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (radius <= 0f) throw new ArgumentOutOfRangeException(nameof(radius), "Ribbon radius must be positive.");
        _pointsView = Array.AsReadOnly(points.ToArray());
        Radius = radius;
        Color = color;
        Label = label;
    }

    public IReadOnlyList<ScatterPoint> Points => _pointsView;
    public float Radius { get; }
    public uint Color { get; }
    public string? Label { get; }
}
```

### Plot3DSeries Subclass

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/RibbonPlot3DSeries.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed class RibbonPlot3DSeries : Plot3DSeries
{
    internal RibbonPlot3DSeries(string? name, RibbonChartData data)
        : base(Plot3DSeriesKind.Ribbon, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: data)
    {
    }

    public void SetRadius(float radius) { /* rebuild with new radius */ }
    public void SetColor(uint color) { /* rebuild with new color */ }
}
```

### Renderer

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderer.cs
namespace Videra.SurfaceCharts.Core;

public static class RibbonRenderer
{
    public static RibbonRenderScene BuildScene(RibbonChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var segments = new List<RibbonRenderSegment>();

        foreach (var series in data.Series)
        {
            for (var i = 0; i < series.Points.Count - 1; i++)
            {
                var start = series.Points[i];
                var end = series.Points[i + 1];
                segments.Add(new RibbonRenderSegment(
                    new Vector3((float)start.Horizontal, (float)start.Value, (float)start.Depth),
                    new Vector3((float)end.Horizontal, (float)end.Value, (float)end.Depth),
                    series.Radius,
                    series.Color));
            }
        }

        return new RibbonRenderScene(data.SeriesCount, segments);
    }
}
```

### Render Scene

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/RibbonRenderScene.cs
namespace Videra.SurfaceCharts.Core;

public sealed class RibbonRenderScene
{
    private readonly ReadOnlyCollection<RibbonRenderSegment> _segmentsView;

    public RibbonRenderScene(int seriesCount, IReadOnlyList<RibbonRenderSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);
        SeriesCount = seriesCount;
        _segmentsView = Array.AsReadOnly(segments.ToArray());
    }

    public int SeriesCount { get; }
    public IReadOnlyList<RibbonRenderSegment> Segments => _segmentsView;
}

public readonly record struct RibbonRenderSegment(Vector3 Start, Vector3 End, float Radius, uint Color);
```

### Probe Strategy

```csharp
// File: src/Videra.SurfaceCharts.Core/Picking/RibbonProbeStrategy.cs
namespace Videra.SurfaceCharts.Core;

public sealed class RibbonProbeStrategy : ISeriesProbeStrategy
{
    private readonly RibbonRenderScene _scene;
    private readonly double _snapRadius;

    public RibbonProbeStrategy(RibbonRenderScene scene, double snapRadius = 0.1)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        // Point-to-segment distance probing with radius check
        // Distance threshold includes segment radius
        // Return SurfaceProbeInfo with interpolated Y value
    }
}
```

### Rendering Status

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/RibbonChartRenderingStatus.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed record RibbonChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int SeriesCount { get; init; }
    public int SegmentCount { get; init; }
    public Size ViewSize { get; init; }
}
```

---

## 3. Vector Field Chart API Contract

### Plot3DAddApi Signatures

```csharp
/// <summary>
/// Adds a 3D vector field from position and direction arrays.
/// </summary>
public VectorFieldPlot3DSeries VectorField(
    double[] xs, double[] ys, double[] zs,
    double[] dxs, double[] dys, double[] dzs,
    string? name = null);

/// <summary>
/// Adds a 3D vector field from a full vector field dataset.
/// </summary>
public VectorFieldPlot3DSeries VectorField(VectorFieldChartData data, string? name = null);
```

### Data Model

```csharp
// File: src/Videra.SurfaceCharts.Core/VectorFieldChartData.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable vector-field dataset.
/// </summary>
public sealed class VectorFieldChartData
{
    private readonly ReadOnlyCollection<VectorFieldPoint> _pointsView;

    public VectorFieldChartData(
        IReadOnlyList<VectorFieldPoint> points,
        SurfaceAxisDescriptor horizontalAxis,
        SurfaceAxisDescriptor depthAxis,
        SurfaceValueRange magnitudeRange)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentNullException.ThrowIfNull(horizontalAxis);
        ArgumentNullException.ThrowIfNull(depthAxis);
        _pointsView = Array.AsReadOnly(points.ToArray());
        HorizontalAxis = horizontalAxis;
        DepthAxis = depthAxis;
        MagnitudeRange = magnitudeRange;
    }

    public IReadOnlyList<VectorFieldPoint> Points => _pointsView;
    public SurfaceAxisDescriptor HorizontalAxis { get; }
    public SurfaceAxisDescriptor DepthAxis { get; }
    public SurfaceValueRange MagnitudeRange { get; }
    public int PointCount => _pointsView.Count;
}

/// <summary>
/// A single vector field sample point.
/// </summary>
public readonly record struct VectorFieldPoint(Vector3 Position, Vector3 Direction, double Magnitude);
```

### Plot3DSeries Subclass

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/VectorFieldPlot3DSeries.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed class VectorFieldPlot3DSeries : Plot3DSeries
{
    internal VectorFieldPlot3DSeries(string? name, VectorFieldChartData data)
        : base(Plot3DSeriesKind.VectorField, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null, vectorFieldData: data)
    {
    }

    public void SetScale(float scale) { /* rebuild with new arrow scale */ }
}
```

### Renderer

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/VectorFieldRenderer.cs
namespace Videra.SurfaceCharts.Core;

public static class VectorFieldRenderer
{
    public static VectorFieldRenderScene BuildScene(VectorFieldChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var arrows = new List<VectorFieldRenderArrow>();

        foreach (var point in data.Points)
        {
            var color = MapMagnitudeToColor(point.Magnitude, data.MagnitudeRange);
            arrows.Add(new VectorFieldRenderArrow(
                point.Position,
                point.Direction,
                point.Magnitude,
                color));
        }

        return new VectorFieldRenderScene(data.PointCount, arrows);
    }

    private static uint MapMagnitudeToColor(double magnitude, SurfaceValueRange range)
    {
        // Map magnitude to color using range normalization
        // Default: blue (low) to red (high) gradient
    }
}
```

### Render Scene

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/VectorFieldRenderScene.cs
namespace Videra.SurfaceCharts.Core;

public sealed class VectorFieldRenderScene
{
    private readonly ReadOnlyCollection<VectorFieldRenderArrow> _arrowsView;

    public VectorFieldRenderScene(int arrowCount, IReadOnlyList<VectorFieldRenderArrow> arrows)
    {
        ArgumentNullException.ThrowIfNull(arrows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(arrowCount);
        ArrowCount = arrowCount;
        _arrowsView = Array.AsReadOnly(arrows.ToArray());
    }

    public int ArrowCount { get; }
    public IReadOnlyList<VectorFieldRenderArrow> Arrows => _arrowsView;
}

public readonly record struct VectorFieldRenderArrow(
    Vector3 Position,
    Vector3 Direction,
    double Magnitude,
    uint Color);
```

### Probe Strategy

```csharp
// File: src/Videra.SurfaceCharts.Core/Picking/VectorFieldProbeStrategy.cs
namespace Videra.SurfaceCharts.Core;

public sealed class VectorFieldProbeStrategy : ISeriesProbeStrategy
{
    private readonly VectorFieldRenderScene _scene;

    public VectorFieldProbeStrategy(VectorFieldRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        // Nearest-point probing returning magnitude as value
        // Find arrow with smallest XZ distance to chartX, chartZ
        // Return SurfaceProbeInfo with magnitude value
    }
}
```

### Rendering Status

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/VectorFieldChartRenderingStatus.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed record VectorFieldChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int ArrowCount { get; init; }
    public SurfaceValueRange MagnitudeRange { get; init; }
    public Size ViewSize { get; init; }
}
```

---

## 4. Heatmap Slice Chart API Contract

### Plot3DAddApi Signatures

```csharp
/// <summary>
/// Adds a heatmap slice from a 2D scalar field at the specified axis position.
/// </summary>
public HeatmapSlicePlot3DSeries HeatmapSlice(
    double[,] values,
    HeatmapSliceAxis axis,
    double position,
    string? name = null);

/// <summary>
/// Adds a heatmap slice from a full heatmap slice dataset.
/// </summary>
public HeatmapSlicePlot3DSeries HeatmapSlice(HeatmapSliceData data, string? name = null);
```

### Data Model

```csharp
// File: src/Videra.SurfaceCharts.Core/HeatmapSliceData.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Selects the axis along which a heatmap slice is taken.
/// </summary>
public enum HeatmapSliceAxis
{
    /// <summary>
    /// Slice perpendicular to the X axis.
    /// </summary>
    X,

    /// <summary>
    /// Slice perpendicular to the Y (value) axis.
    /// </summary>
    Y,

    /// <summary>
    /// Slice perpendicular to the Z (depth) axis.
    /// </summary>
    Z,
}

/// <summary>
/// Represents one immutable heatmap-slice dataset.
/// </summary>
public sealed class HeatmapSliceData
{
    public HeatmapSliceData(
        SurfaceScalarField field,
        HeatmapSliceAxis axis,
        double position,
        SurfaceColorMap? colorMap = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (position < 0d || position > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be normalized between 0 and 1.");
        }
        Field = field;
        Axis = axis;
        Position = position;
        ColorMap = colorMap;
    }

    public SurfaceScalarField Field { get; }
    public HeatmapSliceAxis Axis { get; }
    public double Position { get; }  // 0..1 normalized
    public SurfaceColorMap? ColorMap { get; }
}
```

### Plot3DSeries Subclass

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/HeatmapSlicePlot3DSeries.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed class HeatmapSlicePlot3DSeries : Plot3DSeries
{
    internal HeatmapSlicePlot3DSeries(string? name, HeatmapSliceData data)
        : base(Plot3DSeriesKind.HeatmapSlice, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: data)
    {
    }

    public void SetPosition(double position) { /* rebuild with new slice position */ }
    public void SetColorMap(SurfaceColorMap colorMap) { /* rebuild with new color map */ }
}
```

### Renderer

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/HeatmapSliceRenderer.cs
namespace Videra.SurfaceCharts.Core;

public static class HeatmapSliceRenderer
{
    public static HeatmapSliceRenderScene BuildScene(HeatmapSliceData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var cells = new List<HeatmapSliceRenderCell>();
        var field = data.Field;
        var colorMap = data.ColorMap;

        // Extract 2D slice from scalar field at the given axis and position
        // For each cell in the slice grid:
        //   - Compute world-space position based on axis and position
        //   - Map scalar value to color (using color map or default)
        //   - Create HeatmapSliceRenderCell

        return new HeatmapSliceRenderScene(cells.Count, data.Axis, data.Position, cells);
    }
}
```

### Render Scene

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/HeatmapSliceRenderScene.cs
namespace Videra.SurfaceCharts.Core;

public sealed class HeatmapSliceRenderScene
{
    private readonly ReadOnlyCollection<HeatmapSliceRenderCell> _cellsView;

    public HeatmapSliceRenderScene(
        int cellCount,
        HeatmapSliceAxis axis,
        double position,
        IReadOnlyList<HeatmapSliceRenderCell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cellCount);
        CellCount = cellCount;
        Axis = axis;
        Position = position;
        _cellsView = Array.AsReadOnly(cells.ToArray());
    }

    public int CellCount { get; }
    public HeatmapSliceAxis Axis { get; }
    public double Position { get; }
    public IReadOnlyList<HeatmapSliceRenderCell> Cells => _cellsView;
}

public readonly record struct HeatmapSliceRenderCell(Vector3 Position, Vector2 Size, uint Color);
```

### Probe Strategy

```csharp
// File: src/Videra.SurfaceCharts.Core/Picking/HeatmapSliceProbeStrategy.cs
namespace Videra.SurfaceCharts.Core;

public sealed class HeatmapSliceProbeStrategy : ISeriesProbeStrategy
{
    private readonly HeatmapSliceRenderScene _scene;

    public HeatmapSliceProbeStrategy(HeatmapSliceRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        // Find cell containing chartX, chartZ
        // Use bilinear interpolation for smooth value extraction
        // Return SurfaceProbeInfo with interpolated value
    }
}
```

### Rendering Status

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/HeatmapSliceChartRenderingStatus.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed record HeatmapSliceChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int CellCount { get; init; }
    public HeatmapSliceAxis Axis { get; init; }
    public double Position { get; init; }
    public Size ViewSize { get; init; }
}
```

---

## 5. Box Plot Chart API Contract

### Plot3DAddApi Signatures

```csharp
/// <summary>
/// Adds a box plot from a full box plot dataset.
/// </summary>
public BoxPlotPlot3DSeries BoxPlot(BoxPlotData data, string? name = null);
```

### Data Model

```csharp
// File: src/Videra.SurfaceCharts.Core/BoxPlotData.cs
namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable box-plot dataset.
/// </summary>
public sealed class BoxPlotData
{
    private readonly ReadOnlyCollection<BoxPlotCategory> _categoriesView;

    public BoxPlotData(IReadOnlyList<BoxPlotCategory> categories)
    {
        ArgumentNullException.ThrowIfNull(categories);
        if (categories.Count == 0)
        {
            throw new ArgumentException("Box plot data must contain at least one category.", nameof(categories));
        }
        _categoriesView = Array.AsReadOnly(categories.ToArray());
    }

    public IReadOnlyList<BoxPlotCategory> Categories => _categoriesView;
    public int CategoryCount => _categoriesView.Count;
}

/// <summary>
/// Describes one box-plot category with statistical summary.
/// </summary>
public sealed class BoxPlotCategory
{
    private readonly ReadOnlyCollection<double> _outliersView;

    public BoxPlotCategory(
        string label,
        double min,
        double q1,
        double median,
        double q3,
        double max,
        IReadOnlyList<double>? outliers = null,
        uint color = 0xFF4488CCu)
    {
        if (q1 > q3) throw new ArgumentException("Q1 must not exceed Q3.", nameof(q1));
        if (min > q1) throw new ArgumentException("Min must not exceed Q1.", nameof(min));
        if (max < q3) throw new ArgumentException("Max must not be less than Q3.", nameof(max));
        if (median < q1 || median > q3) throw new ArgumentException("Median must be between Q1 and Q3.", nameof(median));

        Label = label;
        Min = min;
        Q1 = q1;
        Median = median;
        Q3 = q3;
        Max = max;
        _outliersView = Array.AsReadOnly((outliers ?? Array.Empty<double>()).ToArray());
        Color = color;
    }

    public string Label { get; }
    public double Min { get; }
    public double Q1 { get; }
    public double Median { get; }
    public double Q3 { get; }
    public double Max { get; }
    public IReadOnlyList<double> Outliers => _outliersView;
    public uint Color { get; }
}
```

### Plot3DSeries Subclass

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/Plot/BoxPlotPlot3DSeries.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed class BoxPlotPlot3DSeries : Plot3DSeries
{
    internal BoxPlotPlot3DSeries(string? name, BoxPlotData data)
        : base(Plot3DSeriesKind.BoxPlot, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null, vectorFieldData: null,
            heatmapSliceData: null, boxPlotData: data)
    {
    }

    public void SetCategoryColor(int index, uint color) { /* rebuild category with new color */ }
}
```

### Renderer

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/BoxPlotRenderer.cs
namespace Videra.SurfaceCharts.Core;

public static class BoxPlotRenderer
{
    public static BoxPlotRenderScene BuildScene(BoxPlotData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var boxes = new List<BoxPlotRenderBox>();
        var whiskers = new List<BoxPlotRenderWhisker>();
        var outliers = new List<BoxPlotRenderOutlier>();

        for (var i = 0; i < data.CategoryCount; i++)
        {
            var cat = data.Categories[i];
            var x = (float)i;

            // Box: IQR rectangle from Q1 to Q3
            boxes.Add(new BoxPlotRenderBox(
                new Vector3(x, (float)cat.Q1, 0f),
                new Vector3(0.6f, (float)(cat.Q3 - cat.Q1), 0.4f),
                cat.Color));

            // Lower whisker: Min to Q1
            whiskers.Add(new BoxPlotRenderWhisker(
                new Vector3(x, (float)cat.Min, 0f),
                new Vector3(x, (float)cat.Q1, 0f),
                cat.Color));

            // Upper whisker: Q3 to Max
            whiskers.Add(new BoxPlotRenderWhisker(
                new Vector3(x, (float)cat.Q3, 0f),
                new Vector3(x, (float)cat.Max, 0f),
                cat.Color));

            // Median line
            // (rendered as a special whisker or separate element)

            // Outliers
            foreach (var outlier in cat.Outliers)
            {
                outliers.Add(new BoxPlotRenderOutlier(
                    new Vector3(x, (float)outlier, 0f),
                    cat.Color));
            }
        }

        return new BoxPlotRenderScene(data.CategoryCount, boxes, whiskers, outliers);
    }
}
```

### Render Scene

```csharp
// File: src/Videra.SurfaceCharts.Core/Rendering/BoxPlotRenderScene.cs
namespace Videra.SurfaceCharts.Core;

public sealed class BoxPlotRenderScene
{
    private readonly ReadOnlyCollection<BoxPlotRenderBox> _boxesView;
    private readonly ReadOnlyCollection<BoxPlotRenderWhisker> _whiskersView;
    private readonly ReadOnlyCollection<BoxPlotRenderOutlier> _outliersView;

    public BoxPlotRenderScene(
        int categoryCount,
        IReadOnlyList<BoxPlotRenderBox> boxes,
        IReadOnlyList<BoxPlotRenderWhisker> whiskers,
        IReadOnlyList<BoxPlotRenderOutlier> outliers)
    {
        ArgumentNullException.ThrowIfNull(boxes);
        ArgumentNullException.ThrowIfNull(whiskers);
        ArgumentNullException.ThrowIfNull(outliers);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryCount);
        CategoryCount = categoryCount;
        _boxesView = Array.AsReadOnly(boxes.ToArray());
        _whiskersView = Array.AsReadOnly(whiskers.ToArray());
        _outliersView = Array.AsReadOnly(outliers.ToArray());
    }

    public int CategoryCount { get; }
    public IReadOnlyList<BoxPlotRenderBox> Boxes => _boxesView;
    public IReadOnlyList<BoxPlotRenderWhisker> Whiskers => _whiskersView;
    public IReadOnlyList<BoxPlotRenderOutlier> Outliers => _outliersView;
}

public readonly record struct BoxPlotRenderBox(Vector3 Position, Vector3 Size, uint Color);
public readonly record struct BoxPlotRenderWhisker(Vector3 Start, Vector3 End, uint Color);
public readonly record struct BoxPlotRenderOutlier(Vector3 Position, uint Color);
```

### Probe Strategy

```csharp
// File: src/Videra.SurfaceCharts.Core/Picking/BoxPlotProbeStrategy.cs
namespace Videra.SurfaceCharts.Core;

public sealed class BoxPlotProbeStrategy : ISeriesProbeStrategy
{
    private readonly BoxPlotRenderScene _scene;

    public BoxPlotProbeStrategy(BoxPlotRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        // Category-based probing: find nearest category by X position
        // Return SurfaceProbeInfo with median value
        // Could also return Q1/Q3/Min/Max via extended probe info
    }
}
```

### Rendering Status

```csharp
// File: src/Videra.SurfaceCharts.Avalonia/Controls/BoxPlotChartRenderingStatus.cs
namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed record BoxPlotChartRenderingStatus
{
    public bool HasSource { get; init; }
    public bool IsReady { get; init; }
    public SurfaceChartRenderBackendKind BackendKind { get; init; } = SurfaceChartRenderBackendKind.Software;
    public bool IsInteracting { get; init; }
    public int CategoryCount { get; init; }
    public int BoxCount { get; init; }
    public Size ViewSize { get; init; }
}
```

---

## 6. Common Patterns

All new chart families follow these shared patterns:

### Data Models

- `sealed class` for immutable data
- `ReadOnlyCollection<T>` for all exposed collections
- `Array.AsReadOnly(series.ToArray())` to create immutable views
- Constructor validation: `ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException`
- XML doc comments on all public members
- Namespace: `Videra.SurfaceCharts.Core`

### Renderers

- `public static class` for stateless renderers (except Surface which is instance-based)
- Single public `BuildScene({Kind}ChartData data)` entry point
- `ArgumentNullException.ThrowIfNull(data)` at entry
- Private helper methods for geometry computation
- Returns immutable render scene object
- Namespace: `Videra.SurfaceCharts.Core`

### Render Scenes

- `sealed class` with `ReadOnlyCollection<{Kind}RenderElement>`
- Elements are `readonly record struct` with `Vector3 Position`, `uint Color`
- Constructor validation with `ArgumentNullException.ThrowIfNull` and `ArgumentOutOfRangeException`
- Namespace: `Videra.SurfaceCharts.Core`

### Probe Strategies

- `sealed class` implementing `ISeriesProbeStrategy`
- Constructor takes render scene (not data model) for Bar/Line/Ribbon/BoxPlot, data model for Scatter/VectorField
- Single `TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)` returning `SurfaceProbeInfo?`
- Returns `null` when no hit detected
- Namespace: `Videra.SurfaceCharts.Core`

### Plot3DSeries Subclasses

- `sealed class` extending `Plot3DSeries`
- `internal` constructor (only `Plot3DAddApi` creates instances)
- Pass `Plot3DSeriesKind.{Kind}` as first argument
- Pass data to appropriate slot, null to all other slots
- Optional mutable update methods using `Replace*Data()` private protected pattern
- Namespace: `Videra.SurfaceCharts.Avalonia.Controls`

### Rendering Status Records

- `sealed record` for immutable diagnostics
- `{ init; }` auto-properties with sensible defaults
- `HasSource` and `IsReady` as standard boolean fields
- `SurfaceChartRenderBackendKind BackendKind` defaulting to `Software`
- `bool IsInteracting` for gesture state
- Kind-specific diagnostic fields (count, range, etc.)
- `Size ViewSize` for viewport dimensions
- Namespace: `Videra.SurfaceCharts.Avalonia.Controls`

### Legend Integration

- Line: `LegendIndicatorKind.Line` with default color `0xFF4DA3FF` (blue)
- Ribbon: `LegendIndicatorKind.Swatch` with default color `0xFF9B59B6` (purple)
- VectorField: `LegendIndicatorKind.Swatch` with default color `0xFF2ECC71` (green)
- HeatmapSlice: `LegendIndicatorKind.Swatch` with default color `0xFFF39C12` (orange)
- BoxPlot: `LegendIndicatorKind.Swatch` with default color `0xFF4488CC` (blue-gray)
