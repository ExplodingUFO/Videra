namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a data-window request against a surface-chart dataset.
/// </summary>
public readonly record struct SurfaceViewportRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceViewportRequest"/> struct.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="dataWindow">The authoritative requested data window.</param>
    /// <param name="outputWidth">The output width in pixels.</param>
    /// <param name="outputHeight">The output height in pixels.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="outputWidth"/> or <paramref name="outputHeight"/> is not positive.</exception>
    public SurfaceViewportRequest(SurfaceMetadata metadata, SurfaceDataWindow dataWindow, int outputWidth, int outputHeight)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputHeight);

        Metadata = metadata;
        DataWindow = dataWindow;
        OutputWidth = outputWidth;
        OutputHeight = outputHeight;
    }

    /// <summary>
    /// Gets the dataset metadata.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the authoritative requested data window.
    /// </summary>
    public SurfaceDataWindow DataWindow { get; }

    /// <summary>
    /// Gets the output width in pixels.
    /// </summary>
    public int OutputWidth { get; }

    /// <summary>
    /// Gets the output height in pixels.
    /// </summary>
    public int OutputHeight { get; }

    /// <summary>
    /// Gets the data window clamped to the dataset bounds.
    /// </summary>
    public SurfaceDataWindow ClampedDataWindow => DataWindow.ClampTo(Metadata);

    /// <summary>
    /// Gets the clamped data window normalized to unit space.
    /// </summary>
    public SurfaceNormalizedViewport NormalizedViewport => ClampedDataWindow.Normalize(Metadata);

    /// <summary>
    /// Gets the horizontal zoom density measured in samples per output pixel.
    /// </summary>
    public double HorizontalZoomDensity => ClampedDataWindow.Width / OutputWidth;

    /// <summary>
    /// Gets the vertical zoom density measured in samples per output pixel.
    /// </summary>
    public double VerticalZoomDensity => ClampedDataWindow.Height / OutputHeight;

    /// <summary>
    /// Gets the zoom density measured in samples per output pixel.
    /// </summary>
    public double ZoomDensity => Math.Max(HorizontalZoomDensity, VerticalZoomDensity);
}
