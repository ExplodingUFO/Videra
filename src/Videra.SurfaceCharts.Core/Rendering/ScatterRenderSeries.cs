using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one render-ready scatter series.
/// </summary>
public sealed class ScatterRenderSeries
{
    private readonly ReadOnlyCollection<ScatterRenderPoint> _pointsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterRenderSeries"/> class.
    /// </summary>
    /// <param name="points">The render-ready points.</param>
    /// <param name="color">The default series color.</param>
    /// <param name="label">The optional series label.</param>
    /// <param name="connectPoints">Whether the render series should draw line segments between declared points.</param>
    public ScatterRenderSeries(
        IReadOnlyList<ScatterRenderPoint> points,
        uint color,
        string? label = null,
        bool connectPoints = false)
    {
        ArgumentNullException.ThrowIfNull(points);

        Label = label;
        Color = color;
        ConnectPoints = connectPoints;
        _pointsView = Array.AsReadOnly(points.ToArray());
    }

    /// <summary>
    /// Gets the optional series label.
    /// </summary>
    public string? Label { get; }

    /// <summary>
    /// Gets the default ARGB series color.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets whether this render series should connect consecutive points with line segments.
    /// </summary>
    public bool ConnectPoints { get; }

    /// <summary>
    /// Gets the immutable render-ready points.
    /// </summary>
    public IReadOnlyList<ScatterRenderPoint> Points => _pointsView;
}
