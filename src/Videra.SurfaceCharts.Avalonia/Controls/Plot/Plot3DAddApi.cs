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
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Surface, name, source, scatterData: null));
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
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Waterfall, name, source, scatterData: null));
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
        return _plot.AddSeries(new Plot3DSeries(Plot3DSeriesKind.Scatter, name, surfaceSource: null, data));
    }
}
