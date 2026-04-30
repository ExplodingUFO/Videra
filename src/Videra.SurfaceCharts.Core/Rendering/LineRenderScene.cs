using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A render-ready line segment connecting two 3D points with a width and color.
/// </summary>
/// <param name="Start">The segment start position in chart-space coordinates.</param>
/// <param name="End">The segment end position in chart-space coordinates.</param>
/// <param name="Width">The line width in pixels.</param>
/// <param name="Color">The ARGB segment color.</param>
public readonly record struct LineRenderSegment(Vector3 Start, Vector3 End, float Width, uint Color);

/// <summary>
/// A render-ready marker at a line series point.
/// </summary>
/// <param name="Position">The marker center position in chart-space coordinates.</param>
/// <param name="Color">The ARGB marker color.</param>
/// <param name="Size">The marker size in pixels.</param>
/// <param name="Shape">The marker shape.</param>
public readonly record struct MarkerRenderSegment(Vector3 Position, uint Color, float Size, MarkerShape Shape);

/// <summary>
/// Represents a render-ready line-chart snapshot containing line segments and markers for all series.
/// </summary>
public sealed class LineRenderScene
{
    private readonly ReadOnlyCollection<LineRenderSegment> _segmentsView;
    private readonly ReadOnlyCollection<MarkerRenderSegment> _markersView;

    /// <summary>
    /// Initializes a new instance of the <see cref="LineRenderScene"/> class.
    /// </summary>
    /// <param name="seriesCount">The number of series. Must be positive.</param>
    /// <param name="segments">The render-ready line segments.</param>
    /// <param name="markers">Optional render-ready marker segments. Defaults to empty.</param>
    public LineRenderScene(
        int seriesCount,
        IReadOnlyList<LineRenderSegment> segments,
        IReadOnlyList<MarkerRenderSegment>? markers = null)
    {
        ArgumentNullException.ThrowIfNull(segments);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seriesCount);

        SeriesCount = seriesCount;
        _segmentsView = Array.AsReadOnly(segments.ToArray());
        _markersView = markers is not null
            ? Array.AsReadOnly(markers.ToArray())
            : Array.AsReadOnly(Array.Empty<MarkerRenderSegment>());
    }

    /// <summary>
    /// Gets the number of series in the scene.
    /// </summary>
    public int SeriesCount { get; }

    /// <summary>
    /// Gets the immutable render-ready line segments.
    /// </summary>
    public IReadOnlyList<LineRenderSegment> Segments => _segmentsView;

    /// <summary>
    /// Gets the immutable render-ready marker segments.
    /// </summary>
    public IReadOnlyList<MarkerRenderSegment> Markers => _markersView;
}
