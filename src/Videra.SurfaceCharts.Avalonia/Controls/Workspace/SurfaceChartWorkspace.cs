namespace Videra.SurfaceCharts.Avalonia.Controls.Workspace;

/// <summary>
/// Host-owned workspace that tracks registered <see cref="VideraChartView"/> instances and
/// their <see cref="SurfaceChartPanelInfo"/> metadata.
/// </summary>
/// <remarks>
/// The workspace does NOT own chart lifecycle — charts are created and destroyed by the host.
/// Disposing the workspace clears references but does not dispose the chart views themselves.
/// </remarks>
public sealed class SurfaceChartWorkspace : IDisposable
{
    private readonly Dictionary<string, (VideraChartView Chart, SurfaceChartPanelInfo Info)> _panels = new();
    private string? _activeChartId;
    private bool _disposed;

    /// <summary>
    /// Registers a chart view with its panel info. The first registered chart becomes active.
    /// </summary>
    /// <param name="chart">The chart view to register.</param>
    /// <param name="info">Panel metadata for the chart.</param>
    /// <exception cref="ObjectDisposedException">The workspace has been disposed.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> or <paramref name="info"/> is null.</exception>
    /// <exception cref="InvalidOperationException">A chart with the same <see cref="SurfaceChartPanelInfo.ChartId"/> is already registered.</exception>
    public void Register(VideraChartView chart, SurfaceChartPanelInfo info)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(info);

        if (_panels.ContainsKey(info.ChartId))
        {
            throw new InvalidOperationException($"Chart '{info.ChartId}' is already registered.");
        }

        _panels[info.ChartId] = (chart, info);
        _activeChartId ??= info.ChartId;
    }

    /// <summary>
    /// Removes a chart from the workspace. If the removed chart was active, the first
    /// remaining chart becomes active (or null if the workspace is empty).
    /// </summary>
    /// <param name="chartId">The chart id to unregister.</param>
    public void Unregister(string chartId)
    {
        _panels.Remove(chartId);

        if (_activeChartId == chartId)
        {
            _activeChartId = _panels.Keys.FirstOrDefault();
        }
    }

    /// <summary>
    /// Sets which chart is currently active (focused).
    /// </summary>
    /// <param name="chartId">The chart id to make active.</param>
    /// <exception cref="InvalidOperationException">The chart id is not registered.</exception>
    public void SetActiveChart(string chartId)
    {
        if (!_panels.ContainsKey(chartId))
        {
            throw new InvalidOperationException($"Chart '{chartId}' is not registered.");
        }

        _activeChartId = chartId;
    }

    /// <summary>
    /// Returns the active chart id, or null if the workspace is empty.
    /// </summary>
    public string? GetActiveChartId() => _activeChartId;

    /// <summary>
    /// Returns all registered chart-panel pairs.
    /// </summary>
    public IReadOnlyList<(VideraChartView Chart, SurfaceChartPanelInfo Info)> GetRegisteredCharts()
    {
        return _panels.Values.ToList();
    }

    /// <summary>
    /// Returns the panel info for a specific chart view.
    /// </summary>
    /// <param name="chart">The chart view to look up.</param>
    /// <returns>The panel info associated with the chart.</returns>
    /// <exception cref="InvalidOperationException">The chart is not registered.</exception>
    public SurfaceChartPanelInfo GetPanelInfo(VideraChartView chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        foreach (var pair in _panels.Values)
        {
            if (ReferenceEquals(pair.Chart, chart))
            {
                return pair.Info;
            }
        }

        throw new InvalidOperationException("The specified chart is not registered in this workspace.");
    }

    /// <summary>
    /// Returns a snapshot of the workspace status. Deferred to Plan 02.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown — status layer is deferred.</exception>
    public SurfaceChartWorkspaceStatus CaptureWorkspaceStatus()
    {
        throw new NotImplementedException("Deferred to Plan 02: workspace status and evidence layer.");
    }

    /// <summary>
    /// Returns a bounded text block describing workspace state. Deferred to Plan 02.
    /// </summary>
    /// <exception cref="NotImplementedException">Always thrown — evidence formatter is deferred.</exception>
    public string CreateWorkspaceEvidence()
    {
        throw new NotImplementedException("Deferred to Plan 02: workspace evidence formatter.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _panels.Clear();
        _activeChartId = null;
        _disposed = true;
    }
}
