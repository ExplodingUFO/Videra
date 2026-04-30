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
/// Defines the shape of markers rendered at each point of a line series.
/// </summary>
public enum MarkerShape
{
    /// <summary>No markers are rendered.</summary>
    None,
    /// <summary>Circular markers.</summary>
    Circle,
    /// <summary>Square markers.</summary>
    Square,
    /// <summary>Diamond-shaped markers.</summary>
    Diamond
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
    /// <param name="markerShape">The marker shape. Defaults to <see cref="MarkerShape.None"/>.</param>
    /// <param name="markerSize">The marker size in pixels. Must be positive. Defaults to 4.</param>
    /// <param name="markerColor">Optional marker ARGB color. When null, uses the series <paramref name="color"/>.</param>
    public LineSeries(
        IReadOnlyList<ScatterPoint> points,
        uint color,
        float width = 1.5f,
        string? label = null,
        MarkerShape markerShape = MarkerShape.None,
        float markerSize = 4f,
        uint? markerColor = null)
    {
        ArgumentNullException.ThrowIfNull(points);
        if (markerSize <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(markerSize), "Marker size must be positive.");
        }

        _pointsView = Array.AsReadOnly(points.ToArray());
        Color = color;
        Width = width;
        Label = label;
        MarkerShape = markerShape;
        MarkerSize = markerSize;
        MarkerColor = markerColor;
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

    /// <summary>Gets the marker shape. Defaults to <see cref="MarkerShape.None"/>.</summary>
    public MarkerShape MarkerShape { get; }

    /// <summary>Gets the marker size in pixels. Defaults to 4.</summary>
    public float MarkerSize { get; }

    /// <summary>
    /// Gets the optional marker ARGB color. When null, uses the series <see cref="Color"/>.
    /// </summary>
    public uint? MarkerColor { get; }
}
