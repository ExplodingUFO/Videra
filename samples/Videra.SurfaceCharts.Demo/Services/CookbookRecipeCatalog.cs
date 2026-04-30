namespace Videra.SurfaceCharts.Demo.Services;

internal sealed record CookbookRecipe(
    string Group,
    string Title,
    string Description,
    string ScenarioId,
    string? ScatterScenarioId,
    string Snippet)
{
    public override string ToString() => $"{Group}: {Title}";
}

internal static class CookbookRecipes
{
    public static IReadOnlyList<CookbookRecipe> All { get; } =
    [
        new(
            "First chart",
            "Surface from a matrix",
            "Isolated setup path: selects the in-memory first-chart source and fits the camera to the generated surface.",
            ScenarioId: SurfaceDemoScenarios.StartId,
            ScatterScenarioId: null,
            Snippet: """
                var chart = new VideraChartView();

                chart.Plot.Add.Surface(new double[,]
                {
                    { 0.0, 0.4, 0.8 },
                    { 0.2, 0.7, 1.0 },
                    { 0.1, 0.5, 0.9 },
                }, "First surface");

                chart.FitToData();
                """),
        new(
            "Styling",
            "Axes, color map, and overlay options",
            "Isolated setup path: selects the analytics proof with explicit coordinates, a separate ColorField, and chart-local overlay options.",
            ScenarioId: SurfaceDemoScenarios.AnalyticsId,
            ScatterScenarioId: null,
            Snippet: """
                chart.Plot.Axes.X.Label = "Time";
                chart.Plot.Axes.X.Unit = "s";
                chart.Plot.Axes.Y.Label = "Height";
                chart.Plot.Axes.Y.Unit = "mm";
                chart.Plot.Axes.Z.Label = "Band";
                chart.Plot.Axes.Z.Unit = "Hz";
                chart.Plot.ColorMap = new SurfaceColorMap(
                    new SurfaceValueRange(0, 1),
                    SurfaceColorMapPresets.CreateProfessional());
                chart.Plot.OverlayOptions = new SurfaceChartOverlayOptions
                {
                    ShowMinorTicks = true,
                };
                """),
        new(
            "Interactions",
            "Profile plus bounded commands",
            "Isolated setup path: keeps the first chart visible while the snippet shows chart-local interaction switches and commands.",
            ScenarioId: SurfaceDemoScenarios.StartId,
            ScatterScenarioId: null,
            Snippet: """
                chart.InteractionProfile = new SurfaceChartInteractionProfile
                {
                    IsOrbitEnabled = true,
                    IsPanEnabled = true,
                    IsDollyEnabled = true,
                    IsProbePinningEnabled = true,
                };

                chart.TryExecuteChartCommand(SurfaceChartCommand.FitToData);
                chart.TryResolveProbe(pointerPosition, out var probe);
                """),
        new(
            "Live data",
            "Latest-window scatter stream",
            "Isolated setup path: selects the FIFO-trim scatter scenario and shows retained-point counters through DataLogger3D-style live evidence.",
            ScenarioId: SurfaceDemoScenarios.ScatterId,
            ScatterScenarioId: "scatter-fifo-trim-100k",
            Snippet: """
                var live = new DataLogger3D(0xFF2F80EDu, label: "Live scatter", fifoCapacity: 10_000);
                live.Append(new ScatterColumnarData(
                    new float[] { 0f, 1f, 2f },
                    new float[] { 0.2f, 0.5f, 0.8f },
                    new float[] { 0f, 0.5f, 1f }));
                live.UseLatestWindow(2_000);

                var scatterData = new ScatterChartData(
                    new ScatterChartMetadata(
                        new SurfaceAxisDescriptor("Time", "s", 0, 10),
                        new SurfaceAxisDescriptor("Band", "Hz", 0, 10),
                        new SurfaceValueRange(0, 1)),
                    [],
                    [live.Series]);
                var evidence = live.CreateLiveViewEvidence();
                chart.Plot.Clear();
                chart.Plot.Add.Scatter(scatterData, "Live scatter");
                chart.FitToData();
                """),
        new(
            "Linked axes",
            "Explicit two-chart view link",
            "Isolated setup path: selects the waterfall proof as a second chart-family result; the snippet shows the explicit disposable two-chart link.",
            ScenarioId: SurfaceDemoScenarios.WaterfallId,
            ScatterScenarioId: null,
            Snippet: """
                var left = new VideraChartView();
                var right = new VideraChartView();

                left.Plot.Add.Surface(new double[,]
                {
                    { 0.0, 0.4, 0.8 },
                    { 0.2, 0.7, 1.0 },
                    { 0.1, 0.5, 0.9 },
                }, "Left");
                right.Plot.Add.Surface(new double[,]
                {
                    { 0.1, 0.5, 0.9 },
                    { 0.3, 0.8, 1.1 },
                    { 0.2, 0.6, 1.0 },
                }, "Right");

                using var link = left.LinkViewWith(right);
                left.Plot.Axes.X.SetBounds(0, 10);
                right.FitToData();
                """),
        new(
            "Bar",
            "Grouped bars",
            "Isolated setup path: selects the bounded Bar chart proof with three named series and five categories.",
            ScenarioId: SurfaceDemoScenarios.BarId,
            ScatterScenarioId: null,
            Snippet: """
                var data = new BarChartData(
                [
                    new BarSeries([12.0, 19.0, 3.0, 5.0, 8.0], 0xFF38BDF8u, "Series A"),
                    new BarSeries([7.0, 11.0, 15.0, 8.0, 13.0], 0xFFF97316u, "Series B"),
                    new BarSeries([5.0, 9.0, 12.0, 18.0, 6.0], 0xFF2DD4BFu, "Series C"),
                ]);

                chart.Plot.Clear();
                var bars = chart.Plot.Add.Bar(data, "Grouped bars");
                bars.SetSeriesColor(seriesIndex: 1, color: 0xFFABCDEFu);
                chart.FitToData();
                """),
        new(
            "Contour",
            "Radial scalar field",
            "Isolated setup path: selects the bounded Contour plot proof generated from a small radial scalar field.",
            ScenarioId: SurfaceDemoScenarios.ContourId,
            ScatterScenarioId: null,
            Snippet: """
                const int size = 32;
                var values = new float[size * size];
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        var dx = (x - (size - 1) / 2.0) / ((size - 1) / 2.0);
                        var dy = (y - (size - 1) / 2.0) / ((size - 1) / 2.0);
                        values[y * size + x] = (float)Math.Sqrt(dx * dx + dy * dy);
                    }
                }

                var range = new SurfaceValueRange(values.Min(), values.Max());
                var field = new SurfaceScalarField(size, size, values, range);
                chart.Plot.Clear();
                chart.Plot.Add.Contour(field, explicitLevels: [0.25f, 0.5f, 0.75f], name: "Radial contours");
                chart.FitToData();
                """),
        new(
            "Export",
            "Chart-local PNG snapshot",
            "Isolated setup path: keeps the first chart visible and uses the bounded Capture Snapshot button for the same PNG-only export path.",
            ScenarioId: SurfaceDemoScenarios.StartId,
            ScatterScenarioId: null,
            Snippet: """
                var result = await chart.Plot.SavePngAsync(
                    "artifacts/surfacecharts/first-chart.png",
                    width: 1920,
                    height: 1080);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(result.Failure?.Message);
                }
                """),
    ];
}
