namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents an authoritative rectangular data window in surface-chart sample space.
/// </summary>
public readonly record struct SurfaceDataWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceDataWindow"/> struct.
    /// </summary>
    /// <param name="startX">The starting horizontal coordinate.</param>
    /// <param name="startY">The starting vertical coordinate.</param>
    /// <param name="width">The data-window width.</param>
    /// <param name="height">The data-window height.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any coordinate is not finite or when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    public SurfaceDataWindow(double startX, double startY, double width, double height)
    {
        if (!double.IsFinite(startX))
        {
            throw new ArgumentOutOfRangeException(nameof(startX), "Data-window coordinates must be finite.");
        }

        if (!double.IsFinite(startY))
        {
            throw new ArgumentOutOfRangeException(nameof(startY), "Data-window coordinates must be finite.");
        }

        if (!double.IsFinite(width))
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Data-window dimensions must be finite.");
        }

        if (!double.IsFinite(height))
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Data-window dimensions must be finite.");
        }

        if (width <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Data-window width must be positive.");
        }

        if (height <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Data-window height must be positive.");
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
    /// Gets the data-window width.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the data-window height.
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
    /// Clamps the data window to the supplied dataset metadata.
    /// </summary>
    /// <param name="metadata">The dataset metadata to clamp against.</param>
    /// <returns>The clamped data window.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    public SurfaceDataWindow ClampTo(SurfaceMetadata metadata)
    {
        return ToViewport().ClampTo(metadata).ToDataWindow();
    }

    /// <summary>
    /// Normalizes the data window to unit-space coordinates relative to the supplied dataset metadata.
    /// </summary>
    /// <param name="metadata">The dataset metadata to normalize against.</param>
    /// <returns>The normalized data window.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    public SurfaceNormalizedViewport Normalize(SurfaceMetadata metadata)
    {
        return ToViewport().Normalize(metadata);
    }

    /// <summary>
    /// Converts the data window to a sample-space viewport.
    /// </summary>
    /// <returns>The equivalent viewport.</returns>
    public SurfaceViewport ToViewport()
    {
        return new SurfaceViewport(StartX, StartY, Width, Height);
    }

    /// <summary>
    /// Converts a sample-space viewport into the authoritative data-window contract.
    /// </summary>
    /// <param name="viewport">The viewport to convert.</param>
    /// <returns>The equivalent data window.</returns>
    public static SurfaceDataWindow FromViewport(SurfaceViewport viewport)
    {
        return new SurfaceDataWindow(viewport.StartX, viewport.StartY, viewport.Width, viewport.Height);
    }
}
