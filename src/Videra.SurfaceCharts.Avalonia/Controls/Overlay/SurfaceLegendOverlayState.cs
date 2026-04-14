using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceLegendOverlayState
{
    public static SurfaceLegendOverlayState Empty { get; } = new(
        titleText: null,
        titlePosition: default,
        swatchBounds: default,
        swatches: [],
        minimumText: null,
        minimumTextPosition: default,
        maximumText: null,
        maximumTextPosition: default);

    public SurfaceLegendOverlayState(
        string? titleText,
        Point titlePosition,
        Rect swatchBounds,
        IReadOnlyList<SurfaceLegendSwatchState> swatches,
        string? minimumText,
        Point minimumTextPosition,
        string? maximumText,
        Point maximumTextPosition)
    {
        ArgumentNullException.ThrowIfNull(swatches);

        TitleText = titleText;
        TitlePosition = titlePosition;
        SwatchBounds = swatchBounds;
        Swatches = swatches;
        MinimumText = minimumText;
        MinimumTextPosition = minimumTextPosition;
        MaximumText = maximumText;
        MaximumTextPosition = maximumTextPosition;
    }

    public string? TitleText { get; }

    public Point TitlePosition { get; }

    public Rect SwatchBounds { get; }

    public IReadOnlyList<SurfaceLegendSwatchState> Swatches { get; }

    public string? MinimumText { get; }

    public Point MinimumTextPosition { get; }

    public string? MaximumText { get; }

    public Point MaximumTextPosition { get; }
}

internal readonly record struct SurfaceLegendSwatchState(Rect Bounds, uint Color);
