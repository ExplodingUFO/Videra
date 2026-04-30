using System.Numerics;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Adds chart series to a <see cref="Plot3D"/>.
/// </summary>
public sealed class Plot3DAddApi
{
    private readonly Plot3D _plot;
    private readonly SurfacePyramidBuilder _surfacePyramidBuilder = new(maxTileWidth: 64, maxTileHeight: 64);

    internal Plot3DAddApi(Plot3D plot)
    {
        _plot = plot ?? throw new ArgumentNullException(nameof(plot));
    }

    /// <summary>
    /// Adds a tiled heightfield surface from an existing tile source.
    /// </summary>
    public SurfacePlot3DSeries Surface(ISurfaceTileSource source, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (SurfacePlot3DSeries)_plot.AddSeries(new SurfacePlot3DSeries(name, source));
    }

    /// <summary>
    /// Adds a tiled heightfield surface from an in-memory matrix.
    /// </summary>
    public SurfacePlot3DSeries Surface(SurfaceMatrix matrix, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        return Surface(_surfacePyramidBuilder.Build(matrix), name);
    }

    /// <summary>
    /// Adds a tiled heightfield surface from a 2D numeric array.
    /// </summary>
    public SurfacePlot3DSeries Surface(double[,] values, string? name = null)
    {
        return Surface(CreateSurfaceMatrix(values), name);
    }

    /// <summary>
    /// Adds a waterfall presentation from an existing tile source.
    /// </summary>
    public WaterfallPlot3DSeries Waterfall(ISurfaceTileSource source, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return (WaterfallPlot3DSeries)_plot.AddSeries(new WaterfallPlot3DSeries(name, source));
    }

    /// <summary>
    /// Adds a waterfall presentation from an in-memory matrix.
    /// </summary>
    public WaterfallPlot3DSeries Waterfall(SurfaceMatrix matrix, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        return Waterfall(_surfacePyramidBuilder.Build(matrix), name);
    }

    /// <summary>
    /// Adds a waterfall presentation from a 2D numeric array.
    /// </summary>
    public WaterfallPlot3DSeries Waterfall(double[,] values, string? name = null)
    {
        return Waterfall(CreateSurfaceMatrix(values), name);
    }

    /// <summary>
    /// Adds a 3D scatter dataset.
    /// </summary>
    public ScatterPlot3DSeries Scatter(ScatterChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (ScatterPlot3DSeries)_plot.AddSeries(new ScatterPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a 3D scatter dataset from coordinate arrays.
    /// </summary>
    public ScatterPlot3DSeries Scatter(double[] x, double[] y, double[] z, string? name = null, uint color = 0xFF2F80EDu)
    {
        return Scatter(CreateScatterData(x, y, z, name, color), name);
    }

    /// <summary>
    /// Adds a vertical bar chart series from an array of values.
    /// </summary>
    /// <param name="values">The bar values (one per category). Must not be empty and must not contain NaN.</param>
    /// <param name="name">Optional series name.</param>
    public BarPlot3DSeries Bar(double[] values, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        var series = new BarSeries(values, color: 0xFF4488CCu, label: name);
        return Bar(new BarChartData([series]), name);
    }

    /// <summary>
    /// Adds a vertical bar chart series from an array of values with category labels.
    /// </summary>
    /// <param name="values">The bar values (one per category). Must not be empty and must not contain NaN.</param>
    /// <param name="categoryLabels">The category labels. Count must match the value count.</param>
    /// <param name="name">Optional series name.</param>
    public BarPlot3DSeries Bar(double[] values, IReadOnlyList<string> categoryLabels, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(categoryLabels);
        var series = new BarSeries(values, color: 0xFF4488CCu, label: name);
        return Bar(new BarChartData([series], categoryLabels), name);
    }

    /// <summary>
    /// Adds a bar chart series from a full bar dataset.
    /// </summary>
    /// <param name="data">The bar dataset.</param>
    /// <param name="name">Optional series name.</param>
    public BarPlot3DSeries Bar(BarChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (BarPlot3DSeries)_plot.AddSeries(new BarPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a contour plot from a 2D scalar field with default iso-level count (10).
    /// </summary>
    /// <param name="values">The 2D scalar field values in row-major order.</param>
    /// <param name="name">Optional series name.</param>
    public ContourPlot3DSeries Contour(double[,] values, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);

        return Contour(CreateContourField(values), name);
    }

    /// <summary>
    /// Adds a contour plot from a 2D scalar field with explicit iso-levels.
    /// </summary>
    /// <param name="values">The 2D scalar field values in row-major order.</param>
    /// <param name="explicitLevels">The finite contour levels to extract, in deterministic extraction order.</param>
    /// <param name="name">Optional series name.</param>
    public ContourPlot3DSeries Contour(double[,] values, IReadOnlyList<float> explicitLevels, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Contour(CreateContourField(values), explicitLevels, name);
    }

    /// <summary>
    /// Adds a contour plot from a scalar field with default iso-level count (10).
    /// </summary>
    /// <param name="field">The scalar field to extract contours from.</param>
    /// <param name="name">Optional series name.</param>
    public ContourPlot3DSeries Contour(SurfaceScalarField field, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        return Contour(new ContourChartData(field), name);
    }

    /// <summary>
    /// Adds a contour plot from a scalar field with explicit iso-levels.
    /// </summary>
    /// <param name="field">The scalar field to extract contours from.</param>
    /// <param name="explicitLevels">The finite contour levels to extract, in deterministic extraction order.</param>
    /// <param name="name">Optional series name.</param>
    public ContourPlot3DSeries Contour(SurfaceScalarField field, IReadOnlyList<float> explicitLevels, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        return Contour(new ContourChartData(field, explicitLevels), name);
    }

    /// <summary>
    /// Adds a contour plot from a full contour dataset.
    /// </summary>
    /// <param name="data">The contour dataset.</param>
    /// <param name="name">Optional series name.</param>
    public ContourPlot3DSeries Contour(ContourChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (ContourPlot3DSeries)_plot.AddSeries(new ContourPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a 3D line series from coordinate arrays.
    /// </summary>
    /// <param name="xs">The X coordinates. Must not be empty.</param>
    /// <param name="ys">The Y (value) coordinates. Length must match xs.</param>
    /// <param name="zs">The Z (depth) coordinates. Length must match xs.</param>
    /// <param name="name">Optional series name.</param>
    public LinePlot3DSeries Line(double[] xs, double[] ys, double[] zs, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(xs);
        ArgumentNullException.ThrowIfNull(ys);
        ArgumentNullException.ThrowIfNull(zs);
        if (xs.Length == 0)
        {
            throw new ArgumentException("Line coordinate arrays must include at least one point.", nameof(xs));
        }

        if (xs.Length != ys.Length || xs.Length != zs.Length)
        {
            throw new ArgumentException("Line coordinate arrays must have matching lengths.", nameof(xs));
        }

        var points = new ScatterPoint[xs.Length];
        var xMin = double.MaxValue;
        var xMax = double.MinValue;
        var yMin = double.MaxValue;
        var yMax = double.MinValue;
        var zMin = double.MaxValue;
        var zMax = double.MinValue;
        for (var i = 0; i < xs.Length; i++)
        {
            var pt = new ScatterPoint(xs[i], ys[i], zs[i]);
            points[i] = pt;
            xMin = Math.Min(xMin, pt.Horizontal);
            xMax = Math.Max(xMax, pt.Horizontal);
            yMin = Math.Min(yMin, pt.Value);
            yMax = Math.Max(yMax, pt.Value);
            zMin = Math.Min(zMin, pt.Depth);
            zMax = Math.Max(zMax, pt.Depth);
        }

        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, xMin, xMax),
            new SurfaceAxisDescriptor("Z", null, zMin, zMax),
            new SurfaceValueRange(yMin, yMax));
        var series = new LineSeries(points, color: 0xFF4DA3FFu, label: name);
        return Line(new LineChartData([series], metadata), name);
    }

    /// <summary>
    /// Adds a 3D line series from a full line dataset.
    /// </summary>
    /// <param name="data">The line dataset.</param>
    /// <param name="name">Optional series name.</param>
    public LinePlot3DSeries Line(LineChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (LinePlot3DSeries)_plot.AddSeries(new LinePlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a 3D ribbon series from coordinate arrays with a tube radius.
    /// </summary>
    /// <param name="xs">The X coordinates. Must not be empty.</param>
    /// <param name="ys">The Y (value) coordinates. Length must match xs.</param>
    /// <param name="zs">The Z (depth) coordinates. Length must match xs.</param>
    /// <param name="radius">The tube radius. Must be positive.</param>
    /// <param name="name">Optional series name.</param>
    public RibbonPlot3DSeries Ribbon(double[] xs, double[] ys, double[] zs, float radius, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(xs);
        ArgumentNullException.ThrowIfNull(ys);
        ArgumentNullException.ThrowIfNull(zs);
        if (xs.Length == 0)
        {
            throw new ArgumentException("Ribbon coordinate arrays must include at least one point.", nameof(xs));
        }

        if (xs.Length != ys.Length || xs.Length != zs.Length)
        {
            throw new ArgumentException("Ribbon coordinate arrays must have matching lengths.", nameof(xs));
        }

        if (radius <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Ribbon radius must be positive.");
        }

        var points = new ScatterPoint[xs.Length];
        var xMin = double.MaxValue;
        var xMax = double.MinValue;
        var yMin = double.MaxValue;
        var yMax = double.MinValue;
        var zMin = double.MaxValue;
        var zMax = double.MinValue;
        for (var i = 0; i < xs.Length; i++)
        {
            var pt = new ScatterPoint(xs[i], ys[i], zs[i]);
            points[i] = pt;
            xMin = Math.Min(xMin, pt.Horizontal);
            xMax = Math.Max(xMax, pt.Horizontal);
            yMin = Math.Min(yMin, pt.Value);
            yMax = Math.Max(yMax, pt.Value);
            zMin = Math.Min(zMin, pt.Depth);
            zMax = Math.Max(zMax, pt.Depth);
        }

        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, xMin, xMax),
            new SurfaceAxisDescriptor("Z", null, zMin, zMax),
            new SurfaceValueRange(yMin, yMax));
        var series = new RibbonSeries(points, radius, color: 0xFF9B59B6u, label: name);
        return Ribbon(new RibbonChartData([series], metadata), name);
    }

    /// <summary>
    /// Adds a 3D ribbon series from a full ribbon dataset.
    /// </summary>
    /// <param name="data">The ribbon dataset.</param>
    /// <param name="name">Optional series name.</param>
    public RibbonPlot3DSeries Ribbon(RibbonChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (RibbonPlot3DSeries)_plot.AddSeries(new RibbonPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a 3D vector field from position and direction arrays.
    /// </summary>
    public VectorFieldPlot3DSeries VectorField(
        double[] xs, double[] ys, double[] zs,
        double[] dxs, double[] dys, double[] dzs,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(xs);
        ArgumentNullException.ThrowIfNull(ys);
        ArgumentNullException.ThrowIfNull(zs);
        ArgumentNullException.ThrowIfNull(dxs);
        ArgumentNullException.ThrowIfNull(dys);
        ArgumentNullException.ThrowIfNull(dzs);

        if (xs.Length == 0)
        {
            throw new ArgumentException("Vector field arrays must include at least one point.", nameof(xs));
        }

        if (xs.Length != ys.Length || xs.Length != zs.Length || xs.Length != dxs.Length || xs.Length != dys.Length || xs.Length != dzs.Length)
        {
            throw new ArgumentException("Vector field coordinate and direction arrays must have matching lengths.", nameof(xs));
        }

        var points = new VectorFieldPoint[xs.Length];
        var xMin = double.MaxValue;
        var xMax = double.MinValue;
        var zMin = double.MaxValue;
        var zMax = double.MinValue;
        var magMin = double.MaxValue;
        var magMax = double.MinValue;

        for (var i = 0; i < xs.Length; i++)
        {
            var pos = new Vector3((float)xs[i], (float)ys[i], (float)zs[i]);
            var dir = new Vector3((float)dxs[i], (float)dys[i], (float)dzs[i]);
            var mag = dir.Length();
            points[i] = new VectorFieldPoint(pos, dir, mag);
            xMin = Math.Min(xMin, xs[i]);
            xMax = Math.Max(xMax, xs[i]);
            zMin = Math.Min(zMin, zs[i]);
            zMax = Math.Max(zMax, zs[i]);
            magMin = Math.Min(magMin, mag);
            magMax = Math.Max(magMax, mag);
        }

        var data = new VectorFieldChartData(
            points,
            new SurfaceAxisDescriptor("X", null, xMin, xMax),
            new SurfaceAxisDescriptor("Z", null, zMin, zMax),
            new SurfaceValueRange(magMin, magMax));
        return VectorField(data, name);
    }

    /// <summary>
    /// Adds a 3D vector field from a full vector field dataset.
    /// </summary>
    public VectorFieldPlot3DSeries VectorField(VectorFieldChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (VectorFieldPlot3DSeries)_plot.AddSeries(new VectorFieldPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a heatmap slice from a 2D scalar field at the specified axis position.
    /// </summary>
    public HeatmapSlicePlot3DSeries HeatmapSlice(
        double[,] values,
        HeatmapSliceAxis axis,
        double position,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        var field = CreateContourField(values);
        var data = new HeatmapSliceData(field, axis, position);
        return HeatmapSlice(data, name);
    }

    /// <summary>
    /// Adds a heatmap slice from a full heatmap slice dataset.
    /// </summary>
    public HeatmapSlicePlot3DSeries HeatmapSlice(HeatmapSliceData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (HeatmapSlicePlot3DSeries)_plot.AddSeries(new HeatmapSlicePlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a box plot from a full box plot dataset.
    /// </summary>
    public BoxPlotPlot3DSeries BoxPlot(BoxPlotData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (BoxPlotPlot3DSeries)_plot.AddSeries(new BoxPlotPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a histogram from raw values with configurable bin count and mode.
    /// </summary>
    public HistogramPlot3DSeries Histogram(IReadOnlyList<double> values, int binCount = 20, HistogramMode mode = HistogramMode.Count, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Histogram(new HistogramData(values, binCount, mode), name);
    }

    /// <summary>
    /// Adds a histogram from a full histogram dataset.
    /// </summary>
    public HistogramPlot3DSeries Histogram(HistogramData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (HistogramPlot3DSeries)_plot.AddSeries(new HistogramPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a function plot that evaluates y = f(x) over the specified domain.
    /// </summary>
    public FunctionPlot3DSeries Function(Func<double, double> function, double xMin, double xMax, int sampleCount = 200, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(function);
        return Function(new FunctionPlotData(function, xMin, xMax, sampleCount), name);
    }

    /// <summary>
    /// Adds a function plot from a full function plot dataset.
    /// </summary>
    public FunctionPlot3DSeries Function(FunctionPlotData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (FunctionPlot3DSeries)_plot.AddSeries(new FunctionPlot3DSeries(name, data));
    }

    /// <summary>
    /// Adds a pie chart from slices with optional donut hole ratio.
    /// </summary>
    public PiePlot3DSeries Pie(IReadOnlyList<PieSlice> slices, double holeRatio = 0d, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(slices);
        return Pie(new PieChartData(slices, holeRatio), name);
    }

    /// <summary>
    /// Adds a pie chart from a full pie chart dataset.
    /// </summary>
    public PiePlot3DSeries Pie(PieChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (PiePlot3DSeries)_plot.AddSeries(new PiePlot3DSeries(name, data));
    }

    /// <summary>
    /// Attaches error bar data to the most recently added scatter series.
    /// </summary>
    public ScatterPlot3DSeries ErrorBar(ErrorBarData errorBarData)
    {
        ArgumentNullException.ThrowIfNull(errorBarData);

        var scatterSeries = _plot.ActiveSeries as ScatterPlot3DSeries
            ?? throw new InvalidOperationException("ErrorBar requires an active scatter series. Add a scatter series first.");

        scatterSeries.ErrorBarData = errorBarData;
        return scatterSeries;
    }

    /// <summary>
    /// Adds an OHLC/Candlestick chart from bars.
    /// </summary>
    public OHLCPlot3DSeries OHLC(IReadOnlyList<OHLCBar> bars, OHLCStyle style = OHLCStyle.Candlestick, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(bars);
        return OHLC(new OHLCData(bars, style), name);
    }

    /// <summary>
    /// Adds an OHLC/Candlestick chart from a full OHLC dataset.
    /// </summary>
    public OHLCPlot3DSeries OHLC(OHLCData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return (OHLCPlot3DSeries)_plot.AddSeries(new OHLCPlot3DSeries(name, data));
    }

    private static SurfaceMatrix CreateSurfaceMatrix(double[,] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var width = values.GetLength(0);
        var height = values.GetLength(1);
        var flatValues = new float[width * height];
        var range = CreateSurfaceValues(values, flatValues, width, height);
        var metadata = new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", null, 0d, width - 1),
            new SurfaceAxisDescriptor("Y", null, 0d, height - 1),
            range);

        return new SurfaceMatrix(metadata, flatValues);
    }

    private static SurfaceScalarField CreateContourField(double[,] values)
    {
        var width = values.GetLength(0);
        var height = values.GetLength(1);
        var flatValues = new float[width * height];
        var min = double.MaxValue;
        var max = double.MinValue;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var val = values[x, y];
                if (double.IsFinite(val))
                {
                    if (val < min) min = val;
                    if (val > max) max = val;
                }
                flatValues[y * width + x] = (float)val;
            }
        }

        if (min == double.MaxValue)
        {
            min = 0;
            max = 0;
        }

        var range = new SurfaceValueRange(min, max);
        return new SurfaceScalarField(width, height, flatValues, range);
    }

    private static SurfaceValueRange CreateSurfaceValues(double[,] values, float[] flatValues, int width, int height)
    {
        var min = double.MaxValue;
        var max = double.MinValue;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var value = values[x, y];
                if (!double.IsFinite(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(values), "Surface values must be finite.");
                }

                if (value < min)
                {
                    min = value;
                }

                if (value > max)
                {
                    max = value;
                }

                flatValues[y * width + x] = (float)value;
            }
        }

        return new SurfaceValueRange(min, max);
    }

    private static ScatterChartData CreateScatterData(double[] x, double[] y, double[] z, string? name, uint color)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        ArgumentNullException.ThrowIfNull(z);

        if (x.Length == 0)
        {
            throw new ArgumentException("Scatter coordinate arrays must include at least one point.", nameof(x));
        }

        if (x.Length != y.Length || x.Length != z.Length)
        {
            throw new ArgumentException("Scatter coordinate arrays must have matching lengths.", nameof(x));
        }

        var points = new ScatterPoint[x.Length];
        var xMin = double.MaxValue;
        var xMax = double.MinValue;
        var yMin = double.MaxValue;
        var yMax = double.MinValue;
        var zMin = double.MaxValue;
        var zMax = double.MinValue;

        for (var index = 0; index < x.Length; index++)
        {
            var point = new ScatterPoint(x[index], y[index], z[index]);
            points[index] = point;
            xMin = Math.Min(xMin, point.Horizontal);
            xMax = Math.Max(xMax, point.Horizontal);
            yMin = Math.Min(yMin, point.Value);
            yMax = Math.Max(yMax, point.Value);
            zMin = Math.Min(zMin, point.Depth);
            zMax = Math.Max(zMax, point.Depth);
        }

        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, xMin, xMax),
            new SurfaceAxisDescriptor("Z", null, zMin, zMax),
            new SurfaceValueRange(yMin, yMax));
        var series = new ScatterSeries(points, color, name);
        return new ScatterChartData(metadata, [series]);
    }
}
