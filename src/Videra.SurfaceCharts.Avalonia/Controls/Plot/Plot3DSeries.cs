using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes one immutable series attached to a <see cref="Plot3D"/>.
/// </summary>
public sealed class Plot3DSeries
{
    internal Plot3DSeries(
        Plot3DSeriesKind kind,
        string? name,
        ISurfaceTileSource? surfaceSource,
        ScatterChartData? scatterData,
        ContourChartData? contourData)
    {
        Kind = kind;
        Name = string.IsNullOrWhiteSpace(name) ? null : name;
        SurfaceSource = surfaceSource;
        ScatterData = scatterData;
        ContourData = contourData;
    }

    /// <summary>
    /// Gets the chart family for this series.
    /// </summary>
    public Plot3DSeriesKind Kind { get; }

    /// <summary>
    /// Gets the optional host-facing series name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the tiled surface source for surface and waterfall series.
    /// </summary>
    public ISurfaceTileSource? SurfaceSource { get; }

    /// <summary>
    /// Gets the scatter dataset for scatter series.
    /// </summary>
    public ScatterChartData? ScatterData { get; }

    /// <summary>
    /// Gets the contour dataset for contour series.
    /// </summary>
    public ContourChartData? ContourData { get; }
}
