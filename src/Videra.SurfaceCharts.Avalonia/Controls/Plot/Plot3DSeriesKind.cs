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

    /// <summary>
    /// A contour plot with iso-lines extracted from a 2D scalar field.
    /// </summary>
    Contour,

    /// <summary>
    /// A 3D polyline series with per-segment color and width.
    /// </summary>
    Line,

    /// <summary>
    /// A 3D tube/ribbon series with configurable radius and per-segment color.
    /// </summary>
    Ribbon,

    /// <summary>
    /// A 3D vector field with arrows showing direction and magnitude.
    /// </summary>
    VectorField,

    /// <summary>
    /// A heatmap slice from a 3D scalar field at a fixed axis position.
    /// </summary>
    HeatmapSlice,

    /// <summary>
    /// A 3D box plot for statistical distribution visualization.
    /// </summary>
    BoxPlot,

    /// <summary>
    /// A histogram with configurable bins and mode.
    /// </summary>
    Histogram,

    /// <summary>
    /// A function plot evaluating y = f(x) over a domain.
    /// </summary>
    FunctionPlot,

    /// <summary>
    /// A pie or donut chart with configurable slices.
    /// </summary>
    Pie,
}
