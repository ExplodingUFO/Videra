using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class LinkedInteractionRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.LinkedInteractionId;
    public string Group => "Linked interaction";
    public string Title => "Linked interaction";
    public string Description => "Two linked VideraChartView instances with FullViewState sync and probe propagation.";

    public RecipeResult Apply(RecipeContext context)
    {
        // Hide all single-chart panels, show workspace panel with 2 charts.
        context.HideAllCharts?.Invoke();
        context.AnalysisWorkspacePanel!.IsVisible = true;
        context.WorkspaceToolbarPanel!.IsVisible = true;

        // Dispose old workspace service, link group, and propagator if any.
        context.DisposeWorkspaceState?.Invoke();

        // Create new workspace service with 2 surface charts.
        var service = new SurfaceChartWorkspaceService();
        service.RegisterCharts(new List<(VideraChartView, string, Plot3DSeriesKind)>
        {
            (context.WorkspaceChartA, "Linked Surface A", Plot3DSeriesKind.Surface),
            (context.WorkspaceChartB, "Linked Surface B", Plot3DSeriesKind.Surface),
        });

        // Load the same data into both charts.
        context.WorkspaceChartA.Plot.Clear();
        context.WorkspaceChartA.Plot.Add.Surface(context.InMemorySource, "Linked Surface A");
        context.WorkspaceChartA.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.InMemorySource.Metadata.ValueRange);
        context.WorkspaceChartA.FitToData();

        context.WorkspaceChartB.Plot.Clear();
        context.WorkspaceChartB.Plot.Add.Surface(context.InMemorySource, "Linked Surface B");
        context.WorkspaceChartB.Plot.ColorMap = SampleDataFactory.CreateColorMap(context.InMemorySource.Metadata.ValueRange);
        context.WorkspaceChartB.FitToData();

        // Hide unused workspace charts.
        context.WorkspaceChartC!.IsVisible = false;
        context.WorkspaceChartD!.IsVisible = false;

        // Create link group with FullViewState policy and probe propagation.
        var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
        linkGroup.Add(context.WorkspaceChartA);
        linkGroup.Add(context.WorkspaceChartB);

        var propagator = new SurfaceChartInteractionPropagator(
            linkGroup,
            propagateProbe: true);

        // Register link group with workspace.
        service.RegisterLinkGroup(linkGroup, propagator);
        service.SetActiveChart(service.GetWorkspaceStatus().Panels[0].ChartId);

        context.SetLinkGroup?.Invoke(linkGroup);
        context.SetPropagator?.Invoke(propagator);
        context.SetWorkspaceService?.Invoke(service);

        // Display link group info in the toolbar.
        var status = service.GetWorkspaceStatus();
        var linkedStates = service.GetLinkedInteractionStates();
        var stateText = linkedStates.Count > 0
            ? $"Policy: {linkedStates[0].Policy} | Members: {linkedStates[0].MemberCount} | " +
              $"Probe: {(linkedStates[0].PropagateProbe ? "active" : "inactive")}"
            : "No link groups";

        context.WorkspaceStatusText!.Text =
            $"Charts: {status.ChartCount} | Active: {status.ActiveChartId ?? "none"} | " +
            $"Link groups: {status.LinkGroupCount} | All ready: {status.AllReady}\n" +
            $"Link: {stateText}\n" +
            string.Join("\n", status.Panels.Select(p =>
                $"  {p.Label} ({p.ChartKind}): Ready={p.IsReady}, Series={p.SeriesCount}, Points={p.PointCount}"));

        return new RecipeResult(
            Title,
            "Two linked VideraChartView instances with FullViewState synchronization and probe propagation. " +
            "Orbit/zoom on one chart mirrors to the other. Hover probe forwards across linked charts.",
            "Linked interaction workspace contains two Surface charts sharing the same data, linked via FullViewState policy.",
            "No additional assets are used on this path.");
    }
}
