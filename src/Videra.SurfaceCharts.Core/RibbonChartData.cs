using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable ribbon-chart dataset containing one or more tube series.
/// </summary>
public sealed class RibbonChartData
{
    private readonly ReadOnlyCollection<RibbonSeries> _seriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonChartData"/> class.
    /// </summary>
    /// <param name="series">The ribbon series collection. Must not be empty.</param>
    /// <param name="metadata">The axis metadata describing the chart coordinate space.</param>
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

    /// <summary>
    /// Gets the axis metadata describing the chart coordinate space.
    /// </summary>
    public ScatterChartMetadata Metadata { get; }

    /// <summary>
    /// Gets the immutable ribbon series collection.
    /// </summary>
    public IReadOnlyList<RibbonSeries> Series => _seriesView;

    /// <summary>
    /// Gets the number of series in the dataset.
    /// </summary>
    public int SeriesCount => _seriesView.Count;
}

/// <summary>
/// Represents one immutable ribbon series defined by an array of polyline vertices and a tube radius.
/// </summary>
public sealed class RibbonSeries
{
    private readonly ReadOnlyCollection<ScatterPoint> _pointsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonSeries"/> class.
    /// </summary>
    /// <param name="points">The polyline vertices. Must not be null.</param>
    /// <param name="radius">The tube radius. Must be positive.</param>
    /// <param name="color">The ARGB series color.</param>
    /// <param name="label">The optional series label.</param>
    public RibbonSeries(IReadOnlyList<ScatterPoint> points, float radius, uint color, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (radius <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Ribbon radius must be positive.");
        }

        _pointsView = Array.AsReadOnly(points.ToArray());
        Radius = radius;
        Color = color;
        Label = label;
    }

    /// <summary>
    /// Gets the immutable polyline vertices.
    /// </summary>
    public IReadOnlyList<ScatterPoint> Points => _pointsView;

    /// <summary>
    /// Gets the tube radius.
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// Gets the ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }
}
