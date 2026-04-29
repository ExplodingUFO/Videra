namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Identifies the chart family represented by a plot series.
/// </summary>
public enum Plot3DSeriesKind
{
    /// <summary>
    /// A tiled heightfield surface.
    /// </summary>
    Surface,

    /// <summary>
    /// A waterfall heightfield presentation.
    /// </summary>
    Waterfall,

    /// <summary>
    /// A 3D scatter dataset.
    /// </summary>
    Scatter,

    /// <summary>
    /// A vertical bar chart series.
    /// </summary>
    Bar,
}
