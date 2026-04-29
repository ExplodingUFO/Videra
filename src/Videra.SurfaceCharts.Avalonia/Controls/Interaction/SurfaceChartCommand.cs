namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

/// <summary>
/// Built-in chart-local commands that hosts may wire to their own buttons or context menus.
/// </summary>
public enum SurfaceChartCommand
{
    /// <summary>
    /// Zooms the active data window in around its center.
    /// </summary>
    ZoomIn,

    /// <summary>
    /// Zooms the active data window out around its center.
    /// </summary>
    ZoomOut,

    /// <summary>
    /// Pans the active data window left.
    /// </summary>
    PanLeft,

    /// <summary>
    /// Pans the active data window right.
    /// </summary>
    PanRight,

    /// <summary>
    /// Pans the active data window up.
    /// </summary>
    PanUp,

    /// <summary>
    /// Pans the active data window down.
    /// </summary>
    PanDown,

    /// <summary>
    /// Restores the default camera pose for the current data window.
    /// </summary>
    ResetCamera,

    /// <summary>
    /// Fits the active data window to the current source bounds.
    /// </summary>
    FitToData,
}
