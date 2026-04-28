namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a rectangular viewport in surface-chart sample space.
/// </summary>
public readonly record struct SurfaceViewport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceViewport"/> struct.
    /// </summary>
    /// <param name="startX">The starting horizontal coordinate.</param>
    /// <param name="startY">The starting vertical coordinate.</param>
    /// <param name="width">The viewport width.</param>
    /// <param name="height">The viewport height.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is not finite or when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    public SurfaceViewport(double startX, double startY, double width, double height)
    {
        if (!double.IsFinite(startX))
        {
            throw new ArgumentOutOfRangeException(nameof(startX), "Viewport coordinates must be finite.");
        }

        if (!double.IsFinite(startY))
        {
            throw new ArgumentOutOfRangeException(nameof(startY), "Viewport coordinates must be finite.");
        }

        if (!double.IsFinite(width))
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Viewport dimensions must be finite.");
        }

        if (!double.IsFinite(height))
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Viewport dimensions must be finite.");
        }

        if (width <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Viewport width must be positive.");
        }

        if (height <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Viewport height must be positive.");
        }

        StartX = startX;
        StartY = startY;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the starting horizontal coordinate.
    /// </summary>
    public double StartX { get; }

    /// <summary>
    /// Gets the starting vertical coordinate.
    /// </summary>
    public double StartY { get; }

    /// <summary>
    /// Gets the viewport width.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the viewport height.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Gets the exclusive horizontal end coordinate.
    /// </summary>
    public double EndXExclusive => StartX + Width;

    /// <summary>
    /// Gets the exclusive vertical end coordinate.
    /// </summary>
    public double EndYExclusive => StartY + Height;

    /// <summary>
    /// Converts the viewport to the authoritative data-window contract.
    /// </summary>
    /// <returns>The equivalent data window.</returns>
    public SurfaceDataWindow ToDataWindow()
    {
        return SurfaceDataWindow.FromViewport(this);
    }

    /// <summary>
    /// Converts the authoritative data-window contract into a sample-space viewport.
    /// </summary>
    /// <param name="dataWindow">The data window to convert.</param>
    /// <returns>The equivalent viewport.</returns>
    public static SurfaceViewport FromDataWindow(SurfaceDataWindow dataWindow)
    {
        return dataWindow.ToViewport();
    }

    /// <summary>
    /// Clamps the viewport to the supplied dataset metadata.
    /// </summary>
    /// <param name="metadata">The dataset metadata to clamp against.</param>
    /// <returns>The clamped viewport.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    public SurfaceViewport ClampTo(SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clampedWidth = Math.Min(Width, metadata.Width);
        var clampedHeight = Math.Min(Height, metadata.Height);

        var clampedStartX = StartX;
        if (clampedStartX < 0.0)
        {
            clampedStartX = 0.0;
        }

        if (clampedStartX + clampedWidth > metadata.Width)
        {
            clampedStartX = metadata.Width - clampedWidth;
        }

        if (clampedStartX < 0.0)
        {
            clampedStartX = 0.0;
        }

        var clampedStartY = StartY;
        if (clampedStartY < 0.0)
        {
            clampedStartY = 0.0;
        }

        if (clampedStartY + clampedHeight > metadata.Height)
        {
            clampedStartY = metadata.Height - clampedHeight;
        }

        if (clampedStartY < 0.0)
        {
            clampedStartY = 0.0;
        }

        return new SurfaceViewport(clampedStartX, clampedStartY, clampedWidth, clampedHeight);
    }

    /// <summary>
    /// Normalizes the viewport to unit-space coordinates relative to the supplied dataset metadata.
    /// </summary>
    /// <param name="metadata">The dataset metadata to normalize against.</param>
    /// <returns>The normalized viewport.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    public SurfaceNormalizedViewport Normalize(SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var clamped = ClampTo(metadata);
        return new SurfaceNormalizedViewport(
            clamped.StartX / metadata.Width,
            clamped.StartY / metadata.Height,
            clamped.Width / metadata.Width,
            clamped.Height / metadata.Height);
    }
}
