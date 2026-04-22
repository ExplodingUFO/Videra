namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes the axis semantics and value range for a scatter-chart dataset.
/// </summary>
public sealed class ScatterChartMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterChartMetadata"/> class.
    /// </summary>
    /// <param name="horizontalAxis">The horizontal axis descriptor.</param>
    /// <param name="depthAxis">The depth axis descriptor.</param>
    /// <param name="valueRange">The inclusive value-axis range.</param>
    public ScatterChartMetadata(
        SurfaceAxisDescriptor horizontalAxis,
        SurfaceAxisDescriptor depthAxis,
        SurfaceValueRange valueRange)
    {
        ArgumentNullException.ThrowIfNull(horizontalAxis);
        ArgumentNullException.ThrowIfNull(depthAxis);

        HorizontalAxis = horizontalAxis;
        DepthAxis = depthAxis;
        ValueRange = valueRange;
    }

    /// <summary>
    /// Gets the horizontal axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor HorizontalAxis { get; }

    /// <summary>
    /// Gets the depth axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor DepthAxis { get; }

    /// <summary>
    /// Gets the inclusive value-axis range.
    /// </summary>
    public SurfaceValueRange ValueRange { get; }

    /// <summary>
    /// Determines whether a point falls inside the declared metadata bounds.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns><see langword="true"/> when the point is within bounds.</returns>
    public bool Contains(ScatterPoint point)
    {
        return point.Horizontal >= HorizontalAxis.Minimum
            && point.Horizontal <= HorizontalAxis.Maximum
            && point.Depth >= DepthAxis.Minimum
            && point.Depth <= DepthAxis.Maximum
            && point.Value >= ValueRange.Minimum
            && point.Value <= ValueRange.Maximum;
    }
}
