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
    public Plot3DSeries Surface(ISurfaceTileSource source, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Surface, name, source, scatterData: null, contourData: null));
    }

    /// <summary>
    /// Adds a tiled heightfield surface from an in-memory matrix.
    /// </summary>
    public Plot3DSeries Surface(SurfaceMatrix matrix, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        return Surface(_surfacePyramidBuilder.Build(matrix), name);
    }

    /// <summary>
    /// Adds a waterfall presentation from an existing tile source.
    /// </summary>
    public Plot3DSeries Waterfall(ISurfaceTileSource source, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Waterfall, name, source, scatterData: null, contourData: null));
    }

    /// <summary>
    /// Adds a waterfall presentation from an in-memory matrix.
    /// </summary>
    public Plot3DSeries Waterfall(SurfaceMatrix matrix, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(matrix);
        return Waterfall(_surfacePyramidBuilder.Build(matrix), name);
    }

    /// <summary>
    /// Adds a 3D scatter dataset.
    /// </summary>
    public Plot3DSeries Scatter(ScatterChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Scatter, name, surfaceSource: null, data, contourData: null));
    }

    /// <summary>
    /// Adds a contour plot from a 2D scalar field with default iso-level count (10).
    /// </summary>
    /// <param name="values">The 2D scalar field values in row-major order.</param>
    /// <param name="name">Optional series name.</param>
    public Plot3DSeries Contour(double[,] values, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(values);

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
        var field = new SurfaceScalarField(width, height, flatValues, range);
        return Contour(field, name);
    }

    /// <summary>
    /// Adds a contour plot from a scalar field with default iso-level count (10).
    /// </summary>
    /// <param name="field">The scalar field to extract contours from.</param>
    /// <param name="name">Optional series name.</param>
    public Plot3DSeries Contour(SurfaceScalarField field, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(field);
        return Contour(new ContourChartData(field), name);
    }

    /// <summary>
    /// Adds a contour plot from a full contour dataset.
    /// </summary>
    /// <param name="data">The contour dataset.</param>
    /// <param name="name">Optional series name.</param>
    public Plot3DSeries Contour(ContourChartData data, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Contour, name, surfaceSource: null, scatterData: null, data));
    }
}
