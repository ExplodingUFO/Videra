namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Describes the chart's current interaction-quality mode.
/// </summary>
public enum SurfaceChartInteractionQuality
{
    /// <summary>
    /// The chart is settled and can refine requests for the current view.
    /// </summary>
    Refine,

    /// <summary>
    /// The chart is actively moving and can prefer lighter requests until input settles.
    /// </summary>
    Interactive,
}
