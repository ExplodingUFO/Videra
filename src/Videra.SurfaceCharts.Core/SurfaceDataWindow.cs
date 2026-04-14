namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a rectangular data window in surface-chart sample space.
/// </summary>
public sealed record SurfaceDataWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceDataWindow"/> class.
    /// </summary>
    /// <param name="xMin">The inclusive horizontal start coordinate.</param>
    /// <param name="xMax">The exclusive horizontal end coordinate.</param>
    /// <param name="yMin">The inclusive vertical start coordinate.</param>
    /// <param name="yMax">The exclusive vertical end coordinate.</param>
    public SurfaceDataWindow(double xMin, double xMax, double yMin, double yMax)
    {
        ValidateFinite(xMin, nameof(xMin));
        ValidateFinite(xMax, nameof(xMax));
        ValidateFinite(yMin, nameof(yMin));
        ValidateFinite(yMax, nameof(yMax));

        if (xMax <= xMin)
        {
            throw new ArgumentOutOfRangeException(nameof(xMax), "Data-window horizontal span must be positive.");
        }

        if (yMax <= yMin)
        {
            throw new ArgumentOutOfRangeException(nameof(yMax), "Data-window vertical span must be positive.");
        }

        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }

    /// <summary>
    /// Gets the inclusive horizontal start coordinate.
    /// </summary>
    public double XMin { get; }

    /// <summary>
    /// Gets the exclusive horizontal end coordinate.
    /// </summary>
    public double XMax { get; }

    /// <summary>
    /// Gets the inclusive vertical start coordinate.
    /// </summary>
    public double YMin { get; }

    /// <summary>
    /// Gets the exclusive vertical end coordinate.
    /// </summary>
    public double YMax { get; }

    /// <summary>
    /// Gets the horizontal span of the data window.
    /// </summary>
    public double Width => XMax - XMin;

    /// <summary>
    /// Gets the vertical span of the data window.
    /// </summary>
    public double Height => YMax - YMin;

    /// <summary>
    /// Clamps the data window to the supplied dataset metadata.
    /// </summary>
    /// <param name="metadata">The dataset metadata to clamp against.</param>
    /// <returns>The clamped data window.</returns>
    public SurfaceDataWindow ClampTo(SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWidth = Math.Min(Width, metadata.Width);
        var clampedHeight = Math.Min(Height, metadata.Height);

        var clampedXMin = XMin;
        if (clampedXMin < 0.0)
        {
            clampedXMin = 0.0;
        }

        if (clampedXMin + clampedWidth > metadata.Width)
        {
            clampedXMin = metadata.Width - clampedWidth;
        }

        if (clampedXMin < 0.0)
        {
            clampedXMin = 0.0;
        }

        var clampedYMin = YMin;
        if (clampedYMin < 0.0)
        {
            clampedYMin = 0.0;
        }

        if (clampedYMin + clampedHeight > metadata.Height)
        {
            clampedYMin = metadata.Height - clampedHeight;
        }

        if (clampedYMin < 0.0)
        {
            clampedYMin = 0.0;
        }

        return new SurfaceDataWindow(
            clampedXMin,
            clampedXMin + clampedWidth,
            clampedYMin,
            clampedYMin + clampedHeight);
    }

    /// <summary>
    /// Converts this data window to the legacy sample-space viewport model.
    /// </summary>
    /// <returns>The equivalent viewport.</returns>
    public SurfaceViewport ToViewport()
    {
        return new SurfaceViewport(XMin, YMin, Width, Height);
    }

    private static void ValidateFinite(double value, string paramName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(paramName, "Data-window coordinates must be finite.");
        }
    }
}
