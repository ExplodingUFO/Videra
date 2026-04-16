using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes the projected screen-space footprint for one surface patch or tile.
/// </summary>
public readonly record struct SurfaceTileProjectedFootprint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTileProjectedFootprint"/> struct.
    /// </summary>
    /// <param name="screenMinimum">The minimum projected screen-space corner.</param>
    /// <param name="screenMaximum">The maximum projected screen-space corner.</param>
    /// <param name="viewDepth">The nearest normalized depth covered by the footprint.</param>
    /// <param name="isVisible">Whether the footprint intersects the current viewport.</param>
    /// <param name="sampleWidth">The covered horizontal sample span.</param>
    /// <param name="sampleHeight">The covered vertical sample span.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a component is not finite or when a span is not positive.</exception>
    public SurfaceTileProjectedFootprint(
        Vector2 screenMinimum,
        Vector2 screenMaximum,
        float viewDepth,
        bool isVisible,
        double sampleWidth,
        double sampleHeight)
    {
        if (!float.IsFinite(screenMinimum.X) || !float.IsFinite(screenMinimum.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(screenMinimum), "Screen-space footprint coordinates must be finite.");
        }

        if (!float.IsFinite(screenMaximum.X) || !float.IsFinite(screenMaximum.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(screenMaximum), "Screen-space footprint coordinates must be finite.");
        }

        if (screenMaximum.X < screenMinimum.X || screenMaximum.Y < screenMinimum.Y)
        {
            throw new ArgumentOutOfRangeException(nameof(screenMaximum), "Screen-space maximum must not precede the minimum.");
        }

        if (!float.IsFinite(viewDepth))
        {
            throw new ArgumentOutOfRangeException(nameof(viewDepth), "View depth must be finite.");
        }

        if (!double.IsFinite(sampleWidth) || sampleWidth <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleWidth), "Sample width must be finite and positive.");
        }

        if (!double.IsFinite(sampleHeight) || sampleHeight <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleHeight), "Sample height must be finite and positive.");
        }

        ScreenMinimum = screenMinimum;
        ScreenMaximum = screenMaximum;
        ViewDepth = viewDepth;
        IsVisible = isVisible;
        SampleWidth = sampleWidth;
        SampleHeight = sampleHeight;
    }

    /// <summary>
    /// Gets the minimum projected screen-space corner.
    /// </summary>
    public Vector2 ScreenMinimum { get; }

    /// <summary>
    /// Gets the maximum projected screen-space corner.
    /// </summary>
    public Vector2 ScreenMaximum { get; }

    /// <summary>
    /// Gets the projected screen-space center.
    /// </summary>
    public Vector2 ScreenCenter => (ScreenMinimum + ScreenMaximum) * 0.5f;

    /// <summary>
    /// Gets the projected width in pixels.
    /// </summary>
    public float ProjectedWidthPixels => ScreenMaximum.X - ScreenMinimum.X;

    /// <summary>
    /// Gets the projected height in pixels.
    /// </summary>
    public float ProjectedHeightPixels => ScreenMaximum.Y - ScreenMinimum.Y;

    /// <summary>
    /// Gets the projected screen area in pixels.
    /// </summary>
    public float ScreenAreaPixels => ProjectedWidthPixels * ProjectedHeightPixels;

    /// <summary>
    /// Gets the nearest normalized depth covered by the footprint.
    /// </summary>
    public float ViewDepth { get; }

    /// <summary>
    /// Gets a value indicating whether the footprint intersects the current viewport.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the covered horizontal sample span.
    /// </summary>
    public double SampleWidth { get; }

    /// <summary>
    /// Gets the covered vertical sample span.
    /// </summary>
    public double SampleHeight { get; }

    /// <summary>
    /// Gets the horizontal sampling density in samples per screen pixel.
    /// </summary>
    public double HorizontalSamplesPerPixel => SampleWidth / Math.Max(ProjectedWidthPixels, 1f);

    /// <summary>
    /// Gets the vertical sampling density in samples per screen pixel.
    /// </summary>
    public double VerticalSamplesPerPixel => SampleHeight / Math.Max(ProjectedHeightPixels, 1f);

    /// <summary>
    /// Gets the larger sampling density in samples per screen pixel.
    /// </summary>
    public double MaxSamplesPerPixel => Math.Max(HorizontalSamplesPerPixel, VerticalSamplesPerPixel);
}
