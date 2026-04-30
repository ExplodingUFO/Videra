namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Per-chart metadata stored alongside each registered <see cref="VideraChartView"/> in a
/// <see cref="SurfaceChartWorkspace"/>.
/// </summary>
/// <param name="ChartId">Stable identity for the panel.</param>
/// <param name="Label">User-visible name (e.g. "Surface A", "Contour B").</param>
/// <param name="ChartKind">Chart family represented by the registered view.</param>
/// <param name="RecipeContext">Optional cookbook recipe or scenario that produced this chart.</param>
public sealed record SurfaceChartPanelInfo(
    string ChartId,
    string Label,
    Plot3DSeriesKind ChartKind,
    string? RecipeContext = null);
