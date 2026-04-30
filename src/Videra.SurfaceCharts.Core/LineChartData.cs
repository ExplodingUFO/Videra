using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one immutable line-chart dataset containing one or more polyline series.
/// </summary>
public sealed class LineChartData
{
    private readonly ReadOnlyCollection<LineSeries> _seriesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineChartData"/> class.
    /// </summary>
    /// <param name="series">The line series collection. Must not be empty.</param>
    /// <param name="metadata">The axis metadata describing the chart coordinate space.</param>
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

    /// <summary>
    /// Gets the axis metadata describing the chart coordinate space.
    /// </summary>
    public ScatterChartMetadata Metadata { get; }

    /// <summary>
    /// Gets the immutable line series collection.
    /// </summary>
    public IReadOnlyList<LineSeries> Series => _seriesView;

    /// <summary>
    /// Gets the number of series in the dataset.
    /// </summary>
    public int SeriesCount => _seriesView.Count;

    /// <summary>
    /// Gets the total number of points across all series.
    /// </summary>
    public int PointCount
    {
        get
        {
            var count = 0;
            for (var i = 0; i < _seriesView.Count; i++)
            {
                count += _seriesView[i].Points.Count;
            }

            return count;
        }
    }
}

/// <summary>
/// Represents one immutable line series defined by an array of polyline vertices.
/// </summary>
public sealed class LineSeries
{
    private readonly ReadOnlyCollection<ScatterPoint> _pointsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineSeries"/> class.
    /// </summary>
    /// <param name="points">The polyline vertices. Must not be null.</param>
    /// <param name="color">The ARGB series color.</param>
    /// <param name="width">The line width in pixels. Defaults to 1.5.</param>
    /// <param name="label">The optional series label.</param>
    public LineSeries(IReadOnlyList<ScatterPoint> points, uint color, float width = 1.5f, string? label = null)
    {
        ArgumentNullException.ThrowIfNull(points);

        _pointsView = Array.AsReadOnly(points.ToArray());
        Color = color;
        Width = width;
        Label = label;
    }

    /// <summary>
    /// Gets the immutable polyline vertices.
    /// </summary>
    public IReadOnlyList<ScatterPoint> Points => _pointsView;

    /// <summary>
    /// Gets the ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the line width in pixels.
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }
}
