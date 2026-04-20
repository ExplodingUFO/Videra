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
        : this(new SurfaceRegularGrid(width, height), horizontalAxis, verticalAxis, valueRange)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceMetadata"/> class.
    /// </summary>
    /// <param name="geometry">The surface geometry grid.</param>
    /// <param name="horizontalAxis">The horizontal axis descriptor.</param>
    /// <param name="verticalAxis">The vertical axis descriptor.</param>
    /// <param name="valueRange">The inclusive data value range for the dataset.</param>
    /// <exception cref="ArgumentNullException">Thrown when the geometry or one of the axis descriptors is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the axis descriptors do not match the geometry mapping semantics.</exception>
    public SurfaceMetadata(
        SurfaceGeometryGrid geometry,
        SurfaceAxisDescriptor horizontalAxis,
        SurfaceAxisDescriptor verticalAxis,
        SurfaceValueRange valueRange)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(horizontalAxis);
        ArgumentNullException.ThrowIfNull(verticalAxis);

        ValidateAxisCompatibility(geometry, horizontalAxis, nameof(horizontalAxis), isHorizontal: true);
        ValidateAxisCompatibility(geometry, verticalAxis, nameof(verticalAxis), isHorizontal: false);

        Geometry = geometry;
        Width = geometry.Width;
        Height = geometry.Height;
        HorizontalAxis = horizontalAxis;
        VerticalAxis = verticalAxis;
        ValueRange = valueRange;
    }

    /// <summary>
    /// Gets the surface geometry grid.
    /// </summary>
    public SurfaceGeometryGrid Geometry { get; }

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

    /// <summary>
    /// Maps one horizontal sample-space coordinate into axis-space.
    /// </summary>
    public double MapHorizontalCoordinate(double sampleX)
    {
        return Geometry.MapHorizontalCoordinate(sampleX, HorizontalAxis);
    }

    /// <summary>
    /// Maps one vertical sample-space coordinate into axis-space.
    /// </summary>
    public double MapVerticalCoordinate(double sampleY)
    {
        return Geometry.MapVerticalCoordinate(sampleY, VerticalAxis);
    }

    private static void ValidateAxisCompatibility(
        SurfaceGeometryGrid geometry,
        SurfaceAxisDescriptor axis,
        string paramName,
        bool isHorizontal)
    {
        if (geometry is SurfaceExplicitGrid)
        {
            if (axis.ScaleKind != SurfaceAxisScaleKind.ExplicitCoordinates)
            {
                throw new ArgumentException("Explicit grid geometry requires explicit-coordinate axis semantics.", paramName);
            }

            var expectedMinimum = isHorizontal
                ? geometry.GetHorizontalMinimum(axis)
                : geometry.GetVerticalMinimum(axis);
            var expectedMaximum = isHorizontal
                ? geometry.GetHorizontalMaximum(axis)
                : geometry.GetVerticalMaximum(axis);

            if (!NearlyEqual(axis.Minimum, expectedMinimum) || !NearlyEqual(axis.Maximum, expectedMaximum))
            {
                throw new ArgumentException("Explicit-coordinate axis bounds must match the supplied grid coordinates.", paramName);
            }
        }
        else if (axis.ScaleKind == SurfaceAxisScaleKind.ExplicitCoordinates)
        {
            throw new ArgumentException("Explicit-coordinate axis semantics require explicit grid geometry.", paramName);
        }
    }

    private static bool NearlyEqual(double left, double right)
    {
        var scale = Math.Max(1d, Math.Max(Math.Abs(left), Math.Abs(right)));
        return Math.Abs(left - right) <= (1e-9d * scale);
    }
}

