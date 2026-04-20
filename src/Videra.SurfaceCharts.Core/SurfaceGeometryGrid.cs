namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the surface geometry grid that maps sample-space coordinates into axis-space coordinates.
/// </summary>
public abstract class SurfaceGeometryGrid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceGeometryGrid"/> class.
    /// </summary>
    protected SurfaceGeometryGrid(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the horizontal sample count.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the vertical sample count.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Maps one horizontal sample-space coordinate into axis-space.
    /// </summary>
    public abstract double MapHorizontalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis);

    /// <summary>
    /// Maps one vertical sample-space coordinate into axis-space.
    /// </summary>
    public abstract double MapVerticalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis);

    /// <summary>
    /// Gets the horizontal window center in axis-space.
    /// </summary>
    public virtual double GetHorizontalWindowCenter(double start, double span, SurfaceAxisDescriptor axis)
    {
        var end = span <= 1d ? start : start + span - 1d;
        return (MapHorizontalCoordinate(start, axis) + MapHorizontalCoordinate(end, axis)) * 0.5d;
    }

    /// <summary>
    /// Gets the vertical window center in axis-space.
    /// </summary>
    public virtual double GetVerticalWindowCenter(double start, double span, SurfaceAxisDescriptor axis)
    {
        var end = span <= 1d ? start : start + span - 1d;
        return (MapVerticalCoordinate(start, axis) + MapVerticalCoordinate(end, axis)) * 0.5d;
    }

    /// <summary>
    /// Gets the horizontal window span in axis-space.
    /// </summary>
    public virtual double GetHorizontalWindowSpan(double start, double span, SurfaceAxisDescriptor axis)
    {
        var end = span <= 1d ? start : start + span - 1d;
        return Math.Abs(MapHorizontalCoordinate(end, axis) - MapHorizontalCoordinate(start, axis));
    }

    /// <summary>
    /// Gets the vertical window span in axis-space.
    /// </summary>
    public virtual double GetVerticalWindowSpan(double start, double span, SurfaceAxisDescriptor axis)
    {
        var end = span <= 1d ? start : start + span - 1d;
        return Math.Abs(MapVerticalCoordinate(end, axis) - MapVerticalCoordinate(start, axis));
    }

    /// <summary>
    /// Gets the mapped horizontal-axis minimum for this grid.
    /// </summary>
    public double GetHorizontalMinimum(SurfaceAxisDescriptor axis)
    {
        return MapHorizontalCoordinate(0d, axis);
    }

    /// <summary>
    /// Gets the mapped horizontal-axis maximum for this grid.
    /// </summary>
    public double GetHorizontalMaximum(SurfaceAxisDescriptor axis)
    {
        return MapHorizontalCoordinate(Width - 1d, axis);
    }

    /// <summary>
    /// Gets the mapped vertical-axis minimum for this grid.
    /// </summary>
    public double GetVerticalMinimum(SurfaceAxisDescriptor axis)
    {
        return MapVerticalCoordinate(0d, axis);
    }

    /// <summary>
    /// Gets the mapped vertical-axis maximum for this grid.
    /// </summary>
    public double GetVerticalMaximum(SurfaceAxisDescriptor axis)
    {
        return MapVerticalCoordinate(Height - 1d, axis);
    }

    /// <summary>
    /// Maps one sample-space coordinate along a regular grid.
    /// </summary>
    protected static double MapRegularCoordinate(SurfaceAxisDescriptor axis, double sampleIndex, int sampleCount)
    {
        ArgumentNullException.ThrowIfNull(axis);

        if (sampleCount <= 1 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = Math.Clamp(sampleIndex / (sampleCount - 1d), 0d, 1d);
        return MapNormalizedCoordinate(axis, normalized);
    }

    /// <summary>
    /// Maps one normalized axis coordinate into axis-space.
    /// </summary>
    protected static double MapNormalizedCoordinate(SurfaceAxisDescriptor axis, double normalized)
    {
        ArgumentNullException.ThrowIfNull(axis);

        return axis.ScaleKind switch
        {
            SurfaceAxisScaleKind.Linear or SurfaceAxisScaleKind.DateTime =>
                axis.Minimum + (axis.Span * normalized),
            SurfaceAxisScaleKind.Log =>
                Math.Pow(
                    10d,
                    Math.Log10(axis.Minimum) + ((Math.Log10(axis.Maximum) - Math.Log10(axis.Minimum)) * normalized)),
            SurfaceAxisScaleKind.ExplicitCoordinates =>
                throw new InvalidOperationException("Explicit-coordinate axes require explicit grid geometry."),
            _ => throw new ArgumentOutOfRangeException(nameof(axis), "Unsupported axis scale kind.")
        };
    }
}

/// <summary>
/// Represents the existing regular-grid surface geometry.
/// </summary>
public sealed class SurfaceRegularGrid : SurfaceGeometryGrid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceRegularGrid"/> class.
    /// </summary>
    public SurfaceRegularGrid(int width, int height)
        : base(width, height)
    {
    }

    /// <inheritdoc />
    public override double MapHorizontalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis)
    {
        return MapRegularCoordinate(axis, sampleIndex, Width);
    }

    /// <inheritdoc />
    public override double MapVerticalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis)
    {
        return MapRegularCoordinate(axis, sampleIndex, Height);
    }

    /// <inheritdoc />
    public override double GetHorizontalWindowCenter(double start, double span, SurfaceAxisDescriptor axis)
    {
        var minimum = MapRegularBoundary(axis, start, Width);
        var maximum = MapRegularBoundary(axis, start + span, Width);
        return (minimum + maximum) * 0.5d;
    }

    /// <inheritdoc />
    public override double GetVerticalWindowCenter(double start, double span, SurfaceAxisDescriptor axis)
    {
        var minimum = MapRegularBoundary(axis, start, Height);
        var maximum = MapRegularBoundary(axis, start + span, Height);
        return (minimum + maximum) * 0.5d;
    }

    /// <inheritdoc />
    public override double GetHorizontalWindowSpan(double start, double span, SurfaceAxisDescriptor axis)
    {
        return Math.Abs(MapRegularBoundary(axis, start + span, Width) - MapRegularBoundary(axis, start, Width));
    }

    /// <inheritdoc />
    public override double GetVerticalWindowSpan(double start, double span, SurfaceAxisDescriptor axis)
    {
        return Math.Abs(MapRegularBoundary(axis, start + span, Height) - MapRegularBoundary(axis, start, Height));
    }

    private static double MapRegularBoundary(SurfaceAxisDescriptor axis, double boundaryIndex, int sampleCount)
    {
        if (sampleCount <= 0 || axis.Maximum <= axis.Minimum)
        {
            return axis.Minimum;
        }

        var normalized = Math.Clamp(boundaryIndex / sampleCount, 0d, 1d);
        return MapNormalizedCoordinate(axis, normalized);
    }
}

/// <summary>
/// Represents a surface geometry grid backed by explicit per-sample coordinates.
/// </summary>
public sealed class SurfaceExplicitGrid : SurfaceGeometryGrid
{
    private readonly ReadOnlyMemory<double> horizontalCoordinates;
    private readonly ReadOnlyMemory<double> verticalCoordinates;

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceExplicitGrid"/> class.
    /// </summary>
    public SurfaceExplicitGrid(ReadOnlyMemory<double> horizontalCoordinates, ReadOnlyMemory<double> verticalCoordinates)
        : base(horizontalCoordinates.Length, verticalCoordinates.Length)
    {
        ValidateCoordinates(horizontalCoordinates.Span, nameof(horizontalCoordinates));
        ValidateCoordinates(verticalCoordinates.Span, nameof(verticalCoordinates));

        this.horizontalCoordinates = horizontalCoordinates.ToArray();
        this.verticalCoordinates = verticalCoordinates.ToArray();
    }

    /// <summary>
    /// Gets the explicit horizontal coordinates.
    /// </summary>
    public ReadOnlyMemory<double> HorizontalCoordinates => horizontalCoordinates;

    /// <summary>
    /// Gets the explicit vertical coordinates.
    /// </summary>
    public ReadOnlyMemory<double> VerticalCoordinates => verticalCoordinates;

    /// <inheritdoc />
    public override double MapHorizontalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis)
    {
        ArgumentNullException.ThrowIfNull(axis);
        return MapExplicitCoordinate(horizontalCoordinates.Span, sampleIndex);
    }

    /// <inheritdoc />
    public override double MapVerticalCoordinate(double sampleIndex, SurfaceAxisDescriptor axis)
    {
        ArgumentNullException.ThrowIfNull(axis);
        return MapExplicitCoordinate(verticalCoordinates.Span, sampleIndex);
    }

    private static void ValidateCoordinates(ReadOnlySpan<double> coordinates, string paramName)
    {
        if (coordinates.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(paramName, "Explicit coordinates must not be empty.");
        }

        var previous = coordinates[0];
        if (!double.IsFinite(previous))
        {
            throw new ArgumentOutOfRangeException(paramName, "Explicit coordinates must be finite.");
        }

        for (var index = 1; index < coordinates.Length; index++)
        {
            var current = coordinates[index];
            if (!double.IsFinite(current))
            {
                throw new ArgumentOutOfRangeException(paramName, "Explicit coordinates must be finite.");
            }

            if (current <= previous)
            {
                throw new ArgumentException("Explicit coordinates must be strictly increasing.", paramName);
            }

            previous = current;
        }
    }

    private static double MapExplicitCoordinate(ReadOnlySpan<double> coordinates, double sampleIndex)
    {
        if (coordinates.Length == 1)
        {
            return coordinates[0];
        }

        var clampedSample = Math.Clamp(sampleIndex, 0d, coordinates.Length - 1d);
        var lowerIndex = (int)Math.Floor(clampedSample);
        var upperIndex = (int)Math.Ceiling(clampedSample);
        if (lowerIndex == upperIndex)
        {
            return coordinates[lowerIndex];
        }

        var weight = clampedSample - lowerIndex;
        return coordinates[lowerIndex] + ((coordinates[upperIndex] - coordinates[lowerIndex]) * weight);
    }
}
