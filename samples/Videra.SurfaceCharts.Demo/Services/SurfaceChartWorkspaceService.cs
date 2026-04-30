using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Demo-owned workspace state helper that manages chart registration, status, and evidence
/// for the AnalysisWorkspace and LinkedInteraction scenarios.
/// </summary>
internal sealed class SurfaceChartWorkspaceService : IDisposable
{
    private readonly SurfaceChartWorkspace _workspace = new();

    /// <summary>
    /// Registers charts with the workspace, creating panel info for each.
    /// </summary>
    /// <param name="chartEntries">Chart view, label, and kind triples to register.</param>
    public void RegisterCharts(IReadOnlyList<(VideraChartView Chart, string Label, Plot3DSeriesKind Kind)> chartEntries)
    {
        foreach (var entry in chartEntries)
        {
            var info = new SurfaceChartPanelInfo(
                Guid.NewGuid().ToString("N"),
                entry.Label,
                entry.Kind);
            _workspace.Register(entry.Chart, info);
        }
    }

    /// <summary>
    /// Registers a link group and optional propagator with the workspace.
    /// </summary>
    public void RegisterLinkGroup(SurfaceChartLinkGroup group, SurfaceChartInteractionPropagator? propagator = null)
    {
        _workspace.RegisterLinkGroup(group, propagator);
    }

    /// <summary>
    /// Registers streaming status for a chart in the workspace.
    /// </summary>
    public void RegisterStreamingStatus(string chartId, SurfaceChartStreamingStatus status)
    {
        _workspace.RegisterStreamingStatus(chartId, status);
    }

    /// <summary>
    /// Sets the active (focused) chart in the workspace.
    /// </summary>
    public void SetActiveChart(string chartId) => _workspace.SetActiveChart(chartId);

    /// <summary>
    /// Captures a point-in-time snapshot of workspace status.
    /// </summary>
    public SurfaceChartWorkspaceStatus GetWorkspaceStatus() => _workspace.CaptureWorkspaceStatus();

    /// <summary>
    /// Captures linked interaction states from all registered propagators.
    /// </summary>
    public IReadOnlyList<SurfaceChartLinkedInteractionState> GetLinkedInteractionStates() =>
        _workspace.CaptureLinkedInteractionStates();

    /// <summary>
    /// Creates bounded diagnostic text describing the workspace state.
    /// </summary>
    public string GetWorkspaceEvidence() => _workspace.CreateWorkspaceEvidence();

    /// <inheritdoc />
    public void Dispose() => _workspace.Dispose();
}
