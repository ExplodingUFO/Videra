using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Immutable state for the chart toolbar overlay (zoom/pan/reset/fit buttons).
/// </summary>
internal sealed class SurfaceChartToolbarOverlayState
{
    /// <summary>
    /// Gets the empty toolbar state (no buttons visible).
    /// </summary>
    public static readonly SurfaceChartToolbarOverlayState Empty = new(
        isVisible: false,
        buttons: []);

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartToolbarOverlayState"/> class.
    /// </summary>
    public SurfaceChartToolbarOverlayState(
        bool isVisible,
        IReadOnlyList<SurfaceChartToolbarButton> buttons)
    {
        IsVisible = isVisible;
        Buttons = buttons;
    }

    /// <summary>
    /// Gets a value indicating whether the toolbar is visible.
    /// </summary>
    public bool IsVisible { get; }

    /// <summary>
    /// Gets the toolbar buttons.
    /// </summary>
    public IReadOnlyList<SurfaceChartToolbarButton> Buttons { get; }
}

/// <summary>
/// Describes a single toolbar button.
/// </summary>
internal sealed class SurfaceChartToolbarButton
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartToolbarButton"/> class.
    /// </summary>
    public SurfaceChartToolbarButton(
        string icon,
        string tooltip,
        SurfaceChartToolbarAction action,
        Rect screenBounds)
    {
        Icon = icon;
        Tooltip = tooltip;
        Action = action;
        ScreenBounds = screenBounds;
    }

    /// <summary>
    /// Gets the button icon text (unicode symbol).
    /// </summary>
    public string Icon { get; }

    /// <summary>
    /// Gets the button tooltip text.
    /// </summary>
    public string Tooltip { get; }

    /// <summary>
    /// Gets the action this button triggers.
    /// </summary>
    public SurfaceChartToolbarAction Action { get; }

    /// <summary>
    /// Gets the screen-space bounds for hit-testing.
    /// </summary>
    public Rect ScreenBounds { get; }
}

/// <summary>
/// Identifies the action a toolbar button triggers.
/// </summary>
internal enum SurfaceChartToolbarAction
{
    /// <summary>Zoom in (dolly in).</summary>
    ZoomIn,

    /// <summary>Zoom out (dolly out).</summary>
    ZoomOut,

    /// <summary>Reset camera to default pose.</summary>
    ResetCamera,

    /// <summary>Fit to data (reset data window to full bounds).</summary>
    FitToData,
}
