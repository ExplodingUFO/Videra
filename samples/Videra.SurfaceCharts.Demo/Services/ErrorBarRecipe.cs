using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

internal sealed class ErrorBarRecipe : IChartRecipe
{
    public string ScenarioId => SurfaceDemoScenarios.ErrorBarId;
    public string Group => "Error bar";
    public string Title => "Error bar proof";
    public string Description => "Scatter plot with asymmetric X/Y error bars.";

    public RecipeResult Apply(RecipeContext context)
    {
        var chart = context.ScatterChartView;
        context.SetActiveChartView(chart);
        chart.Plot.Clear();

        var xs = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var ys = new double[] { 2.1, 3.8, 4.6, 5.0, 4.2, 6.5, 7.8, 8.6, 6.2 };
        var scatterPoints = xs.Select((x, i) => new ScatterPoint(x, ys[i], 0d)).ToArray();
        var scatterData = new ScatterChartData(
            new ScatterChartMetadata(
                new SurfaceAxisDescriptor("X", "u", 0d, 10d),
                new SurfaceAxisDescriptor("Y", "u", 0d, 10d),
                new SurfaceValueRange(0d, 10d)),
            [new ScatterSeries(scatterPoints, 0xFF38BDF8u, "Measurements")]);

        chart.Plot.Add.Scatter(scatterData, Title);

        var rng = new Random(42);
        var errors = new ErrorBarData(
            scatterPoints.Select(_ => new ErrorBarEntry(
                xErrorLow: 0.2 + (rng.NextDouble() * 0.3),
                xErrorHigh: 0.2 + (rng.NextDouble() * 0.3),
                yErrorLow: 0.3 + (rng.NextDouble() * 0.4),
                yErrorHigh: 0.3 + (rng.NextDouble() * 0.4))).ToArray(),
            color: 0xCCFFFFFFu,
            capSize: 6d,
            lineWidth: 1.5d);
        chart.Plot.Add.ErrorBar(errors);

        chart.FitToData();
        return new RecipeResult(
            Title,
            "Scatter plot with asymmetric X/Y error bars. Demonstrates ErrorBarData with per-point error values, configurable cap size, and color.",
            "Error bar proof shows 9 scatter points with random asymmetric errors in both X and Y dimensions.",
            "No additional assets are used on this path.");
    }
}
