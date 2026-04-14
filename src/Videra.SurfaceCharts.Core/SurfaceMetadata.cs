namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes the dimensions and axis semantics of a surface-chart dataset.
/// </summary>
public sealed class SurfaceMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceMetadata"/> class.
    /// </summary>
    /// <param name="width">The dataset width in samples.</param>
    /// <param name="height">The dataset height in samples.</param>
    /// <param name="horizontalAxis">The horizontal axis descriptor.</param>
    /// <param name="verticalAxis">The vertical axis descriptor.</param>
    /// <param name="valueRange">The inclusive value range for the dataset.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentNullException">Thrown when an axis descriptor is <c>null</c>.</exception>
    public SurfaceMetadata(
        int width,
        int height,
        SurfaceAxisDescriptor horizontalAxis,
        SurfaceAxisDescriptor verticalAxis,
        SurfaceValueRange valueRange)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(horizontalAxis);
        ArgumentNullException.ThrowIfNull(verticalAxis);

        Width = width;
        Height = height;
        HorizontalAxis = horizontalAxis;
        VerticalAxis = verticalAxis;
        ValueRange = valueRange;
    }

    /// <summary>
    /// Gets the dataset width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the dataset height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the horizontal axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor HorizontalAxis { get; }

    /// <summary>
    /// Gets the vertical axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor VerticalAxis { get; }

    /// <summary>
    /// Gets the inclusive data value range.
    /// </summary>
    public SurfaceValueRange ValueRange { get; }

    /// <summary>
    /// Gets the total sample count.
    /// </summary>
    public long SampleCount => (long)Width * Height;
}

