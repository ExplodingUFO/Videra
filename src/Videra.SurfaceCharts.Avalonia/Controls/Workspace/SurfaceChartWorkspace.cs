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
    private readonly List<(SurfaceChartLinkGroup Group, SurfaceChartInteractionPropagator? Propagator)> _linkGroups = [];
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
    /// Registers a link group and optional interaction propagator with the workspace.
    /// </summary>
    /// <param name="group">The link group to register.</param>
    /// <param name="propagator">Optional propagator associated with this link group.</param>
    public void RegisterLinkGroup(SurfaceChartLinkGroup group, SurfaceChartInteractionPropagator? propagator = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(group);
        _linkGroups.Add((group, propagator));
    }

    /// <summary>
    /// Removes a link group from the workspace.
    /// </summary>
    /// <param name="group">The link group to unregister.</param>
    public void UnregisterLinkGroup(SurfaceChartLinkGroup group)
    {
        var index = _linkGroups.FindIndex(entry => ReferenceEquals(entry.Group, group));
        if (index >= 0)
        {
            _linkGroups.RemoveAt(index);
        }
    }

    /// <summary>
    /// Captures the linked interaction state for all registered propagators.
    /// </summary>
    /// <returns>A list of linked interaction states.</returns>
    public IReadOnlyList<SurfaceChartLinkedInteractionState> CaptureLinkedInteractionStates()
    {
        var states = new List<SurfaceChartLinkedInteractionState>(_linkGroups.Count);
        foreach (var (group, propagator) in _linkGroups)
        {
            if (propagator is not null)
            {
                states.Add(propagator.CaptureInteractionState());
            }
            else
            {
                states.Add(new SurfaceChartLinkedInteractionState
                {
                    Policy = group.Policy,
                    MemberCount = group.Members.Count,
                    PropagateSelection = false,
                    PropagateProbe = false,
                    PropagateMeasurement = false,
                });
            }
        }

        return states;
    }

    /// <summary>
    /// Returns a snapshot of the current workspace status, including per-chart rendering readiness,
    /// dataset scale, and overall workspace health.
    /// </summary>
    /// <returns>A point-in-time snapshot of the workspace.</returns>
    public SurfaceChartWorkspaceStatus CaptureWorkspaceStatus()
    {
        var panels = new List<SurfaceChartPanelStatus>(_panels.Count);

        foreach (var (chart, info) in _panels.Values)
        {
            var datasetEvidence = chart.Plot.CreateDatasetEvidence();
            var isReady = chart.RenderingStatus.IsReady;
            var seriesCount = datasetEvidence.Series.Count;
            var pointCount = datasetEvidence.Series.Sum(s => s.SampleCount + s.PointCount);

            panels.Add(new SurfaceChartPanelStatus(
                info.ChartId,
                info.Label,
                info.ChartKind,
                isReady,
                seriesCount,
                pointCount));
        }

        var allReady = panels.Count > 0 && panels.All(p => p.IsReady);

        return new SurfaceChartWorkspaceStatus(
            _panels.Count,
            _activeChartId,
            panels,
            LinkGroupCount: _linkGroups.Count,
            allReady);
    }

    /// <summary>
    /// Returns a bounded text block describing workspace state, including active panel,
    /// per-chart rendering status, and dataset scale.
    /// </summary>
    /// <returns>A bounded diagnostic text block.</returns>
    public string CreateWorkspaceEvidence()
    {
        var status = CaptureWorkspaceStatus();
        var activeInfo = _activeChartId is { } id && _panels.TryGetValue(id, out var entry) ? entry.Info : null;
        var linkedStates = _linkGroups.Count > 0 ? CaptureLinkedInteractionStates() : null;
        return SurfaceChartWorkspaceEvidence.Create(status, activeInfo?.RecipeContext, linkedStates);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _panels.Clear();
        _linkGroups.Clear();
        _activeChartId = null;
        _disposed = true;
    }
}
