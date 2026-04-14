namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a rectangular viewport in normalized unit space.
/// </summary>
public readonly record struct SurfaceNormalizedViewport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceNormalizedViewport"/> struct.
    /// </summary>
    /// <param name="startX">The normalized starting horizontal coordinate.</param>
    /// <param name="startY">The normalized starting vertical coordinate.</param>
    /// <param name="width">The normalized viewport width.</param>
    /// <param name="height">The normalized viewport height.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is not finite, outside unit space, or when the rectangle exceeds unit bounds.</exception>
    public SurfaceNormalizedViewport(double startX, double startY, double width, double height)
    {
        ValidateFinite(nameof(startX), startX);
        ValidateFinite(nameof(startY), startY);
        ValidateFinite(nameof(width), width);
        ValidateFinite(nameof(height), height);

        if (startX < 0.0 || startX > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(startX), "Normalized coordinates must stay within unit space.");
        }

        if (startY < 0.0 || startY > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(startY), "Normalized coordinates must stay within unit space.");
        }

        if (width <= 0.0 || width > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Normalized width must be greater than zero and at most one.");
        }

        if (height <= 0.0 || height > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Normalized height must be greater than zero and at most one.");
        }

        if (startX + width > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The normalized viewport must stay within unit space.");
        }

        if (startY + height > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "The normalized viewport must stay within unit space.");
        }

        StartX = startX;
        StartY = startY;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the normalized starting horizontal coordinate.
    /// </summary>
    public double StartX { get; }

    /// <summary>
    /// Gets the normalized starting vertical coordinate.
    /// </summary>
    public double StartY { get; }

    /// <summary>
    /// Gets the normalized viewport width.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the normalized viewport height.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Gets the exclusive normalized horizontal end coordinate.
    /// </summary>
    public double EndXExclusive => StartX + Width;

    /// <summary>
    /// Gets the exclusive normalized vertical end coordinate.
    /// </summary>
    public double EndYExclusive => StartY + Height;

    private static void ValidateFinite(string paramName, double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Normalized viewport values must be finite.");
        }
    }
}
