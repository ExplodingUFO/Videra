using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class StreamingWorkspaceRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.StreamingWorkspaceId;
    public string Group => "Streaming workspace";
    public string Title => "Streaming workspace";
    public string Description => "Two VideraChartView instances with different streaming modes (replace and append+FIFO).";

    public RecipeResult Apply(RecipeContext context)
    {
        // Hide all single-chart panels, show workspace panel with 2 charts.
        context.HideAllCharts?.Invoke();
        context.AnalysisWorkspacePanel!.IsVisible = true;
        context.WorkspaceToolbarPanel!.IsVisible = true;

        // Dispose old workspace service, link group, and propagator if any.
        context.DisposeWorkspaceState?.Invoke();

        // Create new workspace service with 2 scatter charts.
        var service = new SurfaceChartWorkspaceService();
        service.RegisterCharts(new List<(VideraChartView, string, Plot3DSeriesKind)>
        {
            (context.WorkspaceChartA, "Replace Scatter", Plot3DSeriesKind.Scatter),
            (context.WorkspaceChartB, "Append+FIFO Scatter", Plot3DSeriesKind.Scatter),
        });

        // Create scatter data for chart A: replace mode, 100k points.
        var replaceScenario = ScatterStreamingScenarios.Get("scatter-replace-100k");
        var replaceData = SampleDataFactory.CreateScatterSource(replaceScenario);
        var replaceColumnarSeries = replaceData.ColumnarSeries.Count > 0 ? replaceData.ColumnarSeries[0] : null;
        context.WorkspaceChartA.Plot.Clear();
        context.WorkspaceChartA.Plot.Add.Scatter(replaceData, "Replace Scatter");
        context.WorkspaceChartA.FitToData();

        // Create scatter data for chart B: append+FIFO mode, 100k points, FIFO=100k.
        var fifoScenario = ScatterStreamingScenarios.Get("scatter-fifo-trim-100k");
        var fifoData = SampleDataFactory.CreateScatterSource(fifoScenario);
        var fifoColumnarSeries = fifoData.ColumnarSeries.Count > 0 ? fifoData.ColumnarSeries[0] : null;
        context.WorkspaceChartB.Plot.Clear();
        context.WorkspaceChartB.Plot.Add.Scatter(fifoData, "Append+FIFO Scatter");
        context.WorkspaceChartB.FitToData();

        // Hide unused workspace charts.
        context.WorkspaceChartC!.IsVisible = false;
        context.WorkspaceChartD!.IsVisible = false;

        // Register streaming status for each chart.
        var chartAId = service.GetWorkspaceStatus().Panels[0].ChartId;
        var chartBId = service.GetWorkspaceStatus().Panels[1].ChartId;

        if (replaceColumnarSeries is not null)
        {
            service.RegisterStreamingStatus(chartAId, new SurfaceChartStreamingStatus
            {
                UpdateMode = replaceScenario.UpdateMode.ToString(),
                RetainedPointCount = replaceColumnarSeries.Count,
                PickablePointCount = replaceColumnarSeries.Pickable ? replaceColumnarSeries.Count : 0,
                AppendBatchCount = replaceColumnarSeries.AppendBatchCount,
                ReplaceBatchCount = replaceColumnarSeries.ReplaceBatchCount,
                DroppedFifoPointCount = replaceColumnarSeries.TotalDroppedPointCount,
                EvidenceOnly = true,
            });
        }

        if (fifoColumnarSeries is not null)
        {
            service.RegisterStreamingStatus(chartBId, new SurfaceChartStreamingStatus
            {
                UpdateMode = fifoScenario.UpdateMode.ToString(),
                RetainedPointCount = fifoColumnarSeries.Count,
                FifoCapacity = fifoColumnarSeries.FifoCapacity,
                PickablePointCount = fifoColumnarSeries.Pickable ? fifoColumnarSeries.Count : 0,
                AppendBatchCount = fifoColumnarSeries.AppendBatchCount,
                ReplaceBatchCount = fifoColumnarSeries.ReplaceBatchCount,
                DroppedFifoPointCount = fifoColumnarSeries.TotalDroppedPointCount,
                EvidenceOnly = true,
            });
        }

        service.SetActiveChart(chartAId);
        context.SetWorkspaceService?.Invoke(service);

        // Display streaming info in the toolbar.
        var status = service.GetWorkspaceStatus();
        context.WorkspaceStatusText!.Text =
            $"Charts: {status.ChartCount} | Active: {status.ActiveChartId ?? "none"} | All ready: {status.AllReady}\n" +
            string.Join("\n", status.Panels.Select(p =>
                $"  {p.Label} ({p.ChartKind}): Ready={p.IsReady}, Series={p.SeriesCount}, Points={p.PointCount}"));

        return new RecipeResult(
            Title,
            "Two VideraChartView instances with different streaming modes (replace and append+FIFO). " +
            "Workspace tracks per-chart streaming status. Copy streaming evidence to see the full report.",
            "Streaming workspace contains two Scatter charts: one using replace mode (100k points) " +
            "and one using append+FIFO mode (100k points, FIFO=100k).",
            "No additional assets are used on this path.");
    }
}
