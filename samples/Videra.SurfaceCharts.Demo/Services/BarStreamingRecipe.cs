using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class BarStreamingRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.BarStreamingId;
    public string Group => "Bar streaming";
    public string Title => "Bar streaming";
    public string Description => "BarDataLogger3D appending new series with live series count tracking.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.BarChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var data = SampleDataFactory.CreateSampleBarData();
        var logger = new BarDataLogger3D(data);

        // Simulate streaming: append 2 batches of new series
        logger.Append(
            new BarSeries([9.0, 14.0, 7.0, 11.0, 5.0], 0xFFE74C3Cu, "Series D"),
            new BarSeries([6.0, 10.0, 13.0, 9.0, 16.0], 0xFF9B59B6u, "Series E"));

        chart.Plot.Add.Bar(logger.Data, Title);
        chart.FitToData();

        return new RecipeResult(
            Title,
            $"BarDataLogger3D streaming demo. " +
            $"Appended {logger.AppendBatchCount} batches, {logger.TotalAppendedSeriesCount} total series. " +
            $"Current series count: {logger.SeriesCount}.",
            $"Bar streaming uses BarDataLogger3D with {logger.SeriesCount} series, " +
            $"{logger.CategoryCount} categories. Append batches: {logger.AppendBatchCount}.",
            "No additional assets are used on this path.");
    }
}
