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
            "Line",
            "3D polyline",
            "Isolated setup path: creates a 3D polyline from coordinate arrays with configurable color and width.",
            ScenarioId: SurfaceDemoScenarios.LineId,
            ScatterScenarioId: null,
            Snippet: """
                var xs = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                var ys = new double[] { 0, 1.5, 0.5, 2.0, 1.0, 2.5, 1.5, 3.0, 2.0 };
                var zs = new double[] { 0, 0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5, 4.0 };

                chart.Plot.Clear();
                chart.Plot.Add.Line(xs, ys, zs, "Sine wave");
                chart.FitToData();
                """),
        new(
            "Ribbon",
            "Tube geometry",
            "Isolated setup path: creates a 3D ribbon/tube from coordinate arrays with configurable radius.",
            ScenarioId: SurfaceDemoScenarios.RibbonId,
            ScatterScenarioId: null,
            Snippet: """
                var xs = new double[] { 0, 1, 2, 3, 4, 5 };
                var ys = new double[] { 0, 1.0, 0.5, 1.5, 0.8, 2.0 };
                var zs = new double[] { 0, 0.5, 1.0, 1.5, 2.0, 2.5 };

                chart.Plot.Clear();
                chart.Plot.Add.Ribbon(xs, ys, zs, radius: 0.15f, "Helix ribbon");
                chart.FitToData();
                """),
        new(
            "Vector field",
            "Arrow field",
            "Isolated setup path: creates a 3D vector field from position and direction arrays with magnitude-based coloring.",
            ScenarioId: SurfaceDemoScenarios.VectorFieldId,
            ScatterScenarioId: null,
            Snippet: """
                var xs = new double[] { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
                var ys = new double[] { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
                var zs = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                var dxs = new double[] { 0.5, 0, -0.5, 0.5, 0, -0.5, 0.5, 0, -0.5 };
                var dys = new double[] { 0, 0.5, 0, 0, 0.5, 0, 0, 0.5, 0 };
                var dzs = new double[] { 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3, 0.3 };

                chart.Plot.Clear();
                chart.Plot.Add.VectorField(xs, ys, zs, dxs, dys, dzs, "Wind field");
                chart.FitToData();
                """),
        new(
            "Heatmap slice",
            "Volumetric slice",
            "Isolated setup path: creates a heatmap slice from a 2D scalar field at a specified axis position.",
            ScenarioId: SurfaceDemoScenarios.HeatmapSliceId,
            ScatterScenarioId: null,
            Snippet: """
                var values = new double[16, 16];
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                        values[x, y] = Math.Sin(x * 0.4) * Math.Cos(y * 0.3);

                chart.Plot.Clear();
                chart.Plot.Add.HeatmapSlice(values, HeatmapSliceAxis.Z, 0.5, "XY slice");
                chart.FitToData();
                """),
        new(
            "Box plot",
            "Statistical distribution",
            "Isolated setup path: creates a 3D box plot showing statistical distributions with grouped layout.",
            ScenarioId: SurfaceDemoScenarios.BoxPlotId,
            ScatterScenarioId: null,
            Snippet: """
                var data = new BoxPlotData([
                    new BoxPlotCategory("Group A", min: 2, q1: 5, median: 8, q3: 12, max: 18, outliers: [1, 20]),
                    new BoxPlotCategory("Group B", min: 4, q1: 7, median: 10, q3: 14, max: 19),
                    new BoxPlotCategory("Group C", min: 1, q1: 3, median: 6, q3: 9, max: 15, outliers: [0, 17]),
                ]);

                chart.Plot.Clear();
                chart.Plot.Add.BoxPlot(data, "Distribution comparison");
                chart.FitToData();
                """),
        new(
            "MultiPlot3D",
            "2x2 subplot grid",
            "Isolated setup path: creates a MultiPlot3D 2x2 grid with mixed chart types and linked camera.",
            ScenarioId: SurfaceDemoScenarios.MultiPlot3DId,
            ScatterScenarioId: null,
            Snippet: """
                var grid = new MultiPlot3D(2, 2);

                grid.GetPlot(0, 0).Add.Surface(surfaceMatrix, "Surface");
                grid.GetPlot(0, 1).Add.Bar(barData, "Bar");
                grid.GetPlot(1, 0).Add.Line(xs, ys, zs, "Line");
                grid.GetPlot(1, 1).Add.Contour(contourField, "Contour");

                grid.FitAllToData();
                using var link = grid.LinkAll(SurfaceChartLinkPolicy.CameraOnly);
                """),
        new(
            "Multi-chart",
            "Analysis workspace",
            "Host-owned workspace coordinating multiple VideraChartView instances for comparison.",
            ScenarioId: SurfaceDemoScenarios.AnalysisWorkspaceId,
            ScatterScenarioId: null,
            Snippet: """
                var workspace = new SurfaceChartWorkspace();
                workspace.Register(chartA, new SurfaceChartPanelInfo(
                    ChartId: "chart-a",
                    Label: "Surface A",
                    ChartKind: Plot3DSeriesKind.Surface));
                workspace.Register(chartB, new SurfaceChartPanelInfo(
                    ChartId: "chart-b",
                    Label: "Contour B",
                    ChartKind: Plot3DSeriesKind.Contour));
                var evidence = workspace.CreateWorkspaceEvidence();
                """),
        new(
            "Linked interaction",
            "Link group with probe propagation",
            "Host-owned link group synchronizing view state and propagating probes across charts.",
            ScenarioId: SurfaceDemoScenarios.LinkedInteractionId,
            ScatterScenarioId: null,
            Snippet: """
                using var linkGroup = new SurfaceChartLinkGroup(SurfaceChartLinkPolicy.FullViewState);
                linkGroup.Add(chartA);
                linkGroup.Add(chartB);

                var propagator = new SurfaceChartInteractionPropagator(
                    linkGroup, propagateProbe: true);
                propagator.PropagateProbe(chartA, screenPosition);
                """),
        new(
            "Surface streaming",
            "SurfaceDataLogger3D",
            "Isolated setup path: demonstrates SurfaceDataLogger3D with append (new rows), replace (full swap), and FIFO (row-cap) semantics.",
            ScenarioId: SurfaceDemoScenarios.SurfaceStreamingId,
            ScatterScenarioId: null,
            Snippet: """
                var matrix = new SurfaceMatrix(metadata, initialValues);
                var logger = new SurfaceDataLogger3D(matrix, fifoRowCapacity: 100);

                // Append new rows
                logger.Append(newRows);
                Console.WriteLine($"Rows: {logger.RowCount}, Appended: {logger.TotalAppendedRowCount}");

                // Replace entire matrix
                logger.Replace(newMatrix);

                // FIFO: oldest rows auto-trimmed when capacity exceeded
                logger.Append(moreRows);
                Console.WriteLine($"Dropped: {logger.LastDroppedRowCount}");
                """),
        new(
            "Waterfall streaming",
            "WaterfallDataLogger3D",
            "Isolated setup path: demonstrates WaterfallDataLogger3D delegating to SurfaceDataLogger3D for row streaming.",
            ScenarioId: SurfaceDemoScenarios.WaterfallStreamingId,
            ScatterScenarioId: null,
            Snippet: """
                var matrix = new SurfaceMatrix(metadata, initialValues);
                var logger = new WaterfallDataLogger3D(matrix, fifoRowCapacity: 50);

                // Same API as SurfaceDataLogger3D
                logger.Append(newRows);
                logger.Replace(newMatrix);
                Console.WriteLine($"Rows: {logger.RowCount}");
                """),
        new(
            "Bar streaming",
            "BarDataLogger3D",
            "Isolated setup path: demonstrates BarDataLogger3D with append (new series), replace (full swap), and series count tracking.",
            ScenarioId: SurfaceDemoScenarios.BarStreamingId,
            ScatterScenarioId: null,
            Snippet: """
                var data = new BarChartData(initialSeries);
                var logger = new BarDataLogger3D(data);

                // Append new series
                logger.Append(newSeries);
                Console.WriteLine($"Series: {logger.SeriesCount}, Appended: {logger.TotalAppendedSeriesCount}");

                // Replace entire data
                logger.Replace(newData);
                Console.WriteLine($"Replaced: {logger.ReplaceBatchCount}");
                """),
        new(
            "Streaming",
            "Streaming workspace with evidence",
            "Multiple charts with different streaming modes and workspace evidence tracking.",
            ScenarioId: SurfaceDemoScenarios.StreamingWorkspaceId,
            ScatterScenarioId: "scatter-fifo-trim-100k",
            Snippet: """
                var live = new DataLogger3D(0xFF2F80EDu, label: "Live", fifoCapacity: 10_000);
                live.Append(data);
                live.UseLatestWindow(2_000);

                workspace.RegisterStreamingStatus("live", new SurfaceChartStreamingStatus
                {
                    UpdateMode = "Append",
                    RetainedPointCount = live.Count,
                    FifoCapacity = live.FifoCapacity,
                    EvidenceOnly = true,
                });
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
