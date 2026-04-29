using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Represents the visual state of the multi-series legend overlay.
/// </summary>
internal sealed class SurfaceLegendOverlayState
{
    /// <summary>
    /// Gets the empty legend state (no entries).
    /// </summary>
    public static SurfaceLegendOverlayState Empty { get; } = new(
        entries: [],
        position: SurfaceChartLegendPosition.TopRight,
        bounds: default,
        isTruncated: false);

    /// <summary>
    /// Initializes a new instance of <see cref="SurfaceLegendOverlayState"/>.
    /// </summary>
    public SurfaceLegendOverlayState(
        IReadOnlyList<SurfaceLegendEntry> entries,
        SurfaceChartLegendPosition position,
        Rect bounds,
        bool isTruncated)
    {
        ArgumentNullException.ThrowIfNull(entries);

        Entries = entries;
        Position = position;
        Bounds = bounds;
        IsTruncated = isTruncated;
    }

    /// <summary>
    /// Gets the legend entries for visible series.
    /// </summary>
    public IReadOnlyList<SurfaceLegendEntry> Entries { get; }

    /// <summary>
    /// Gets the corner position of the legend.
    /// </summary>
    public SurfaceChartLegendPosition Position { get; }

    /// <summary>
    /// Gets the bounding rectangle of the entire legend.
    /// </summary>
    public Rect Bounds { get; }

    /// <summary>
    /// Gets a value indicating whether the legend was truncated due to space constraints.
    /// </summary>
    public bool IsTruncated { get; }
}

/// <summary>
/// Represents a single entry in the legend overlay.
/// </summary>
internal sealed record SurfaceLegendEntry
{
    /// <summary>
    /// Initializes a new instance of <see cref="SurfaceLegendEntry"/>.
    /// </summary>
    public SurfaceLegendEntry(
        string seriesName,
        Plot3DSeriesKind seriesKind,
        bool isVisible,
        uint color,
        LegendIndicatorKind indicatorKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesName);

        SeriesName = seriesName;
        SeriesKind = seriesKind;
        IsVisible = isVisible;
        Color = color;
        IndicatorKind = indicatorKind;
    }

    /// <summary>
    /// Gets the display name of the series.
    /// </summary>
    public string SeriesName { get; }

    /// <summary>
    /// Gets the kind of the series (Surface, Waterfall, Scatter, etc.).
    /// </summary>
    public Plot3DSeriesKind SeriesKind { get; }

    /// <summary>
    /// Gets a value indicating whether the series is visible.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the ARGB color of the series.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the visual indicator kind for this entry.
    /// </summary>
    public LegendIndicatorKind IndicatorKind { get; }
}

/// <summary>
/// Selects the visual indicator shape for a legend entry.
/// </summary>
internal enum LegendIndicatorKind
{
    /// <summary>
    /// Small colored rectangle (for surface and waterfall series).
    /// </summary>
    Swatch,

    /// <summary>
    /// Small colored dot/circle (for scatter series).
    /// </summary>
    Dot,

    /// <summary>
    /// Small colored line segment (for contour series).
    /// </summary>
    Line,
}
