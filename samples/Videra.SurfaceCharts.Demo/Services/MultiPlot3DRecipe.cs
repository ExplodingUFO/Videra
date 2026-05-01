using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class MultiPlot3DRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.MultiPlot3DId;
    public string Group => "MultiPlot3D";
    public string Title => "MultiPlot3D grid";
    public string Description => "MultiPlot3D 2x2 subplot grid with mixed chart types and linked camera.";

    public RecipeResult Apply(RecipeContext context)
    {
        context.HideAllCharts?.Invoke();
        context.MultiPlot3DPanel!.IsVisible = true;

        // Dispose old MultiPlot3D if any
        context.DisposeWorkspaceState?.Invoke();

        // Create 2x2 MultiPlot3D grid
        var grid = new MultiPlot3D(2, 2);

        // Fill with different chart types
        grid.GetPlot(0, 0).Add.Surface(context.InMemorySource, "Surface");
        grid.GetPlot(0, 0).ColorMap = SampleDataFactory.CreateColorMap(context.InMemorySource.Metadata.ValueRange);

        var barData = SampleDataFactory.CreateSampleBarData();
        grid.GetPlot(0, 1).Add.Bar(barData, "Bar");

        var lineXs = SampleDataFactory.CreateSampleLineXs();
        var lineYs = SampleDataFactory.CreateSampleLineYs();
        var lineZs = SampleDataFactory.CreateSampleLineZs();
        grid.GetPlot(1, 0).Add.Line(lineXs, lineYs, lineZs, "Line");

        var contourField = SampleDataFactory.CreateSampleContourField();
        grid.GetPlot(1, 1).Add.Contour(contourField, "Contour");

        grid.FitAllToData();

        // Link all cells with camera-only policy
        grid.LinkAll(SurfaceChartLinkPolicy.CameraOnly);

        context.MultiPlot3DPanel!.Children.Add(grid);
        context.SetMultiPlot3D?.Invoke(grid);

        return new RecipeResult(
            Title,
            "MultiPlot3D 2x2 subplot grid with 4 different chart types (Surface, Bar, Line, Contour). " +
            "All cells linked with CameraOnly policy -- orbit/zoom on one mirrors to all.",
            $"MultiPlot3D grid: {grid.Rows}x{grid.Columns} = {grid.CellCount} cells.",
            "No additional assets are used on this path.");
    }
}
