using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class WaterfallStreamingRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.WaterfallStreamingId;
    public string Group => "Waterfall streaming";
    public string Title => "Waterfall streaming";
    public string Description => "WaterfallDataLogger3D delegating to SurfaceDataLogger3D for row streaming.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.WaterfallChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var matrix = SampleDataFactory.CreateWaterfallMatrix();
        var logger = new WaterfallDataLogger3D(matrix, fifoRowCapacity: 100);

        // Simulate streaming: append 2 batches
        for (var batch = 0; batch < 2; batch++)
        {
            var newRows = SampleDataFactory.CreateStreamingRows(matrix.Metadata.Width, 6, batch);
            logger.Append(newRows);
        }

        chart.Plot.Add.Waterfall(logger.Matrix, Title);
        chart.Plot.ColorMap = SampleDataFactory.CreateColorMap(logger.Matrix.Metadata.ValueRange);
        chart.FitToData();

        return new RecipeResult(
            Title,
            $"WaterfallDataLogger3D streaming demo. " +
            $"Delegates to SurfaceDataLogger3D internally. " +
            $"Appended {logger.AppendBatchCount} batches, {logger.TotalAppendedRowCount} total rows.",
            $"Waterfall streaming uses WaterfallDataLogger3D with {logger.RowCount} rows, " +
            $"{logger.ColumnCount} columns. FIFO capacity: {logger.FifoRowCapacity?.ToString() ?? "unlimited"}.",
            "No additional assets are used on this path.");
    }
}
