using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Immutable state for the crosshair overlay, representing two projected ground-plane guidelines
/// through the probe point with axis-value pill labels at the guideline endpoints.
/// </summary>
internal sealed class SurfaceCrosshairOverlayState
{
    /// <summary>
    /// Gets the empty (invisible) crosshair state.
    /// </summary>
    public static SurfaceCrosshairOverlayState Empty { get; } = new(
        isVisible: false,
        xGuidelineStart: default,
        xGuidelineEnd: default,
        zGuidelineStart: default,
        zGuidelineEnd: default,
        xValueText: null,
        zValueText: null,
        xPillPosition: default,
        zPillPosition: default);

    public SurfaceCrosshairOverlayState(
        bool isVisible,
        Point xGuidelineStart,
        Point xGuidelineEnd,
        Point zGuidelineStart,
        Point zGuidelineEnd,
        string? xValueText,
        string? zValueText,
        Point xPillPosition,
        Point zPillPosition)
    {
        IsVisible = isVisible;
        XGuidelineStart = xGuidelineStart;
        XGuidelineEnd = xGuidelineEnd;
        ZGuidelineStart = zGuidelineStart;
        ZGuidelineEnd = zGuidelineEnd;
        XValueText = xValueText;
        ZValueText = zValueText;
        XPillPosition = xPillPosition;
        ZPillPosition = zPillPosition;
    }

    /// <summary>
    /// Gets a value indicating whether the crosshair should be rendered.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the screen-space start point of the X guideline (projected ground-plane line at probe Z).
    /// </summary>
    public Point XGuidelineStart { get; }

    /// <summary>
    /// Gets the screen-space end point of the X guideline.
    /// </summary>
    public Point XGuidelineEnd { get; }

    /// <summary>
    /// Gets the screen-space start point of the Z guideline (projected ground-plane line at probe X).
    /// </summary>
    public Point ZGuidelineStart { get; }

    /// <summary>
    /// Gets the screen-space end point of the Z guideline.
    /// </summary>
    public Point ZGuidelineEnd { get; }

    /// <summary>
    /// Gets the formatted X axis value text for the pill label, or null if not visible.
    /// </summary>
    public string? XValueText { get; }

    /// <summary>
    /// Gets the formatted Z axis value text for the pill label, or null if not visible.
    /// </summary>
    public string? ZValueText { get; }

    /// <summary>
    /// Gets the screen-space position for the X axis value pill (at the outer endpoint of the X guideline).
    /// </summary>
    public Point XPillPosition { get; }

    /// <summary>
    /// Gets the screen-space position for the Z axis value pill (at the outer endpoint of the Z guideline).
    /// </summary>
    public Point ZPillPosition { get; }
}
