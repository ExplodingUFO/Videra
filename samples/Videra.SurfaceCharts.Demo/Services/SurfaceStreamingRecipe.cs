using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class SurfaceStreamingRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.SurfaceStreamingId;
    public string Group => "Surface streaming";
    public string Title => "Surface streaming";
    public string Description => "SurfaceDataLogger3D with append/replace/FIFO semantics and live counters.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.SurfaceChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var matrix = SampleDataFactory.CreateSampleMatrix();
        var logger = new SurfaceDataLogger3D(matrix, fifoRowCapacity: 200);

        // Simulate streaming: append 3 batches of new rows
        for (var batch = 0; batch < 3; batch++)
        {
            var newRows = SampleDataFactory.CreateStreamingRows(matrix.Metadata.Width, 10, batch);
            logger.Append(newRows);
        }

        chart.Plot.Add.Surface(logger.Matrix, Title);
        chart.Plot.ColorMap = SampleDataFactory.CreateColorMap(logger.Matrix.Metadata.ValueRange);
        chart.FitToData();

        return new RecipeResult(
            Title,
            $"SurfaceDataLogger3D streaming demo. " +
            $"Appended {logger.AppendBatchCount} batches, {logger.TotalAppendedRowCount} total rows. " +
            $"FIFO capacity: {logger.FifoRowCapacity?.ToString() ?? "unlimited"}. " +
            $"Dropped rows: {logger.LastDroppedRowCount}.",
            $"Surface streaming uses SurfaceDataLogger3D with {logger.RowCount} rows, " +
            $"{logger.ColumnCount} columns. Append batches: {logger.AppendBatchCount}.",
            "No additional assets are used on this path.");
    }
}
