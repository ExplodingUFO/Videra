using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class AnalysisWorkspaceRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.AnalysisWorkspaceId;
    public string Group => "Analysis workspace";
    public string Title => "Analysis workspace";
    public string Description => "Multi-chart analysis workspace with 4 charts in a 2x2 grid.";

    public RecipeResult Apply(RecipeContext context)
    {
        // Hide all single-chart panels, show workspace panel.
        context.HideAllCharts?.Invoke();
        context.AnalysisWorkspacePanel!.IsVisible = true;
        context.WorkspaceToolbarPanel!.IsVisible = true;

        // Dispose old workspace service, link group, and propagator if any.
        context.DisposeWorkspaceState?.Invoke();

        // Restore hidden workspace charts.
        context.WorkspaceChartC!.IsVisible = true;
        context.WorkspaceChartD!.IsVisible = true;

        // Create new workspace service and register 4 charts with different kinds.
        var service = new SurfaceChartWorkspaceService();
        service.RegisterCharts(new List<(VideraChartView, string, Plot3DSeriesKind)>
        {
            (context.WorkspaceChartA, "Surface A", Plot3DSeriesKind.Surface),
            (context.WorkspaceChartB, "Bar B", Plot3DSeriesKind.Bar),
            (context.WorkspaceChartC, "Scatter C", Plot3DSeriesKind.Scatter),
            (context.WorkspaceChartD, "Contour D", Plot3DSeriesKind.Contour),
        });

        // Load data into each workspace chart.
        context.WorkspaceChartA.Plot.Clear();
        context.WorkspaceChartA.Plot.Add.Surface(context.InMemorySource, "Surface A");
        context.WorkspaceChartA.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.InMemorySource.Metadata.ValueRange);
        context.WorkspaceChartA.FitToData();

        var barData = SampleDataFactory.CreateSampleBarData();
        context.WorkspaceChartB.Plot.Clear();
        context.WorkspaceChartB.Plot.Add.Bar(barData, "Bar B");
        context.WorkspaceChartB.FitToData();

        var scatterScenario = ScatterStreamingScenarios.Get("scatter-replace-100k");
        var scatterData = SampleDataFactory.CreateScatterSource(scatterScenario);
        context.WorkspaceChartC.Plot.Clear();
        context.WorkspaceChartC.Plot.Add.Scatter(scatterData, "Scatter C");
        context.WorkspaceChartC.FitToData();

        var contourField = SampleDataFactory.CreateSampleContourField();
        context.WorkspaceChartD.Plot.Clear();
        context.WorkspaceChartD.Plot.Add.Contour(contourField, "Contour D");
        context.WorkspaceChartD.FitToData();

        // Set active chart and update workspace status display.
        service.SetActiveChart(service.GetWorkspaceStatus().Panels[0].ChartId);
        context.SetWorkspaceService?.Invoke(service);

        var status = service.GetWorkspaceStatus();
        context.WorkspaceStatusText!.Text =
            $"Charts: {status.ChartCount} | Active: {status.ActiveChartId ?? "none"} | " +
            $"Link groups: {status.LinkGroupCount} | All ready: {status.AllReady}\n" +
            string.Join("\n", status.Panels.Select(p =>
                $"  {p.Label} ({p.ChartKind}): Ready={p.IsReady}, Series={p.SeriesCount}, Points={p.PointCount}"));

        return new RecipeResult(
            Title,
            "Multi-chart analysis workspace with 4 charts in a 2x2 grid. Delegates workspace state to SurfaceChartWorkspaceService.",
            "Analysis workspace contains Surface, Bar, Scatter, and Contour charts registered in a SurfaceChartWorkspace.",
            "No additional assets are used on this path.");
    }
}
