namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a single contour level with its extracted line segments.
/// </summary>
public sealed class ContourLine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContourLine"/> class.
    /// </summary>
    /// <param name="isoValue">The iso-value this contour represents.</param>
    /// <param name="segments">The line segments for this contour level.</param>
    public ContourLine(float isoValue, IReadOnlyList<ContourSegment> segments)
    {
        IsoValue = isoValue;
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    /// <summary>
    /// Gets the iso-value this contour represents.
    /// </summary>
    public float IsoValue { get; }

    /// <summary>
    /// Gets the line segments for this contour level.
    /// </summary>
    public IReadOnlyList<ContourSegment> Segments { get; }
}
