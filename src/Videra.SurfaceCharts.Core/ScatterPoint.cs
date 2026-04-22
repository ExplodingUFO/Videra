namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents one discrete scatter point in chart space.
/// </summary>
public readonly record struct ScatterPoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterPoint"/> struct.
    /// </summary>
    /// <param name="horizontal">The horizontal coordinate in axis space.</param>
    /// <param name="value">The value-axis coordinate.</param>
    /// <param name="depth">The depth coordinate in axis space.</param>
    /// <param name="color">The optional ARGB point color override.</param>
    public ScatterPoint(double horizontal, double value, double depth, uint? color = null)
    {
        if (!double.IsFinite(horizontal))
        {
            throw new ArgumentOutOfRangeException(nameof(horizontal), "Point horizontal coordinate must be finite.");
        }

        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Point value must be finite.");
        }

        if (!double.IsFinite(depth))
        {
            throw new ArgumentOutOfRangeException(nameof(depth), "Point depth coordinate must be finite.");
        }

        Horizontal = horizontal;
        Value = value;
        Depth = depth;
        Color = color;
    }

    /// <summary>
    /// Gets the horizontal coordinate in axis space.
    /// </summary>
    public double Horizontal { get; }

    /// <summary>
    /// Gets the value-axis coordinate.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the depth coordinate in axis space.
    /// </summary>
    public double Depth { get; }

    /// <summary>
    /// Gets the optional ARGB point color override.
    /// </summary>
    public uint? Color { get; }
}
