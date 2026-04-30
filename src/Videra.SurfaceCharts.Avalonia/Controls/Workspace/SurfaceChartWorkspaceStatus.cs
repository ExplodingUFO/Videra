namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Per-chart status within a workspace snapshot.
/// </summary>
/// <param name="ChartId">Stable identity for the panel.</param>
/// <param name="Label">User-visible name.</param>
/// <param name="ChartKind">Chart family represented by the registered view.</param>
/// <param name="IsReady">Whether the chart's rendering status reports ready.</param>
/// <param name="SeriesCount">Number of series in the chart's dataset evidence.</param>
/// <param name="PointCount">Total point count across all series in the chart's dataset evidence.</param>
public sealed record SurfaceChartPanelStatus(
    string ChartId,
    string Label,
    Plot3DSeriesKind ChartKind,
    bool IsReady,
    int SeriesCount,
    long PointCount);

/// <summary>
/// Aggregate snapshot of workspace state at a point in time.
/// </summary>
/// <param name="ChartCount">Number of registered charts.</param>
/// <param name="ActiveChartId">The currently active (focused) chart id, or null if empty.</param>
/// <param name="Panels">Per-chart status entries.</param>
/// <param name="LinkGroupCount">Number of active link groups (set by the host, not tracked by workspace directly).</param>
/// <param name="AllReady">True when all registered charts report ready.</param>
public sealed record SurfaceChartWorkspaceStatus(
    int ChartCount,
    string? ActiveChartId,
    IReadOnlyList<SurfaceChartPanelStatus> Panels,
    int LinkGroupCount,
    bool AllReady);
