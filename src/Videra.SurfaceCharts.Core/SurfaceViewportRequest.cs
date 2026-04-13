namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a viewport request against a surface-chart dataset.
/// </summary>
public readonly record struct SurfaceViewportRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceViewportRequest"/> struct.
    /// </summary>
    /// <param name="metadata">The dataset metadata.</param>
    /// <param name="viewport">The requested viewport.</param>
    /// <param name="outputWidth">The output width in pixels.</param>
    /// <param name="outputHeight">The output height in pixels.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metadata"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="outputWidth"/> or <paramref name="outputHeight"/> is not positive.</exception>
    public SurfaceViewportRequest(SurfaceMetadata metadata, SurfaceViewport viewport, int outputWidth, int outputHeight)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(outputHeight);

        Metadata = metadata;
        Viewport = viewport;
        OutputWidth = outputWidth;
        OutputHeight = outputHeight;
    }

    /// <summary>
    /// Gets the dataset metadata.
    /// </summary>
    public SurfaceMetadata Metadata { get; }

    /// <summary>
    /// Gets the requested viewport.
    /// </summary>
    public SurfaceViewport Viewport { get; }

    /// <summary>
    /// Gets the output width in pixels.
    /// </summary>
    public int OutputWidth { get; }

    /// <summary>
    /// Gets the output height in pixels.
    /// </summary>
    public int OutputHeight { get; }

    /// <summary>
    /// Gets the viewport clamped to the dataset bounds.
    /// </summary>
    public SurfaceViewport ClampedViewport => Viewport.ClampTo(Metadata);

    /// <summary>
    /// Gets the clamped viewport normalized to unit space.
    /// </summary>
    public SurfaceViewport NormalizedViewport => ClampedViewport.Normalize(Metadata);

    /// <summary>
    /// Gets the zoom density measured in samples per output pixel.
    /// </summary>
    public double ZoomDensity
        => Math.Max(ClampedViewport.Width / OutputWidth, ClampedViewport.Height / OutputHeight);
}
