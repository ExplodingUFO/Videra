namespace Videra.SurfaceCharts.Demo.Services;

internal sealed record SurfaceDemoScenario(
    string Id,
    string Label,
    string ChartKind,
    string RecipeGroup,
    string Description)
{
    public override string ToString() => Label;
}

internal static class SurfaceDemoScenarios
{
    public const string StartId = "surface-start";
    public const string CacheId = "surface-cache";
    public const string AnalyticsId = "surface-analytics";
    public const string WaterfallId = "waterfall-proof";
    public const string ScatterId = "scatter-proof";
    public const string BarId = "bar-proof";
    public const string ContourId = "contour-proof";
    public const string LineId = "line-proof";
    public const string RibbonId = "ribbon-proof";
    public const string VectorFieldId = "vector-field-proof";
    public const string HeatmapSliceId = "heatmap-slice-proof";
    public const string BoxPlotId = "box-plot-proof";
    public const string SurfaceStreamingId = "surface-streaming";
    public const string WaterfallStreamingId = "waterfall-streaming";
    public const string BarStreamingId = "bar-streaming";
    public const string MultiPlot3DId = "multiplot3d";
    public const string AnalysisWorkspaceId = "analysis-workspace";
    public const string LinkedInteractionId = "linked-interaction";
    public const string StreamingWorkspaceId = "streaming-workspace";
    public const string HistogramId = "histogram-proof";
    public const string FunctionPlotId = "function-plot-proof";

    public static IReadOnlyList<SurfaceDemoScenario> All { get; } =
    [
        new(
            StartId,
            "Start here: In-memory first chart",
            "Surface",
            "First chart",
            "Generated at runtime from a dense matrix and kept as the baseline chart path inside this demo."),
        new(
            CacheId,
            "Explore next: Cache-backed streaming",
            "Surface",
            "First chart",
            "Loads committed cache metadata and lazy viewport tiles after the first chart path renders."),
        new(
            AnalyticsId,
            "Try next: Analytics proof",
            "Surface",
            "Styling",
            "Uses explicit coordinates and an independent ColorField on the same chart shell."),
        new(
            WaterfallId,
            "Try next: Waterfall proof",
            "Waterfall",
            "Linked axes",
            "Uses explicit strip spacing while keeping the inherited interaction and overlay workflow."),
        new(
            ScatterId,
            "Try next: Scatter proof",
            "Scatter",
            "Live data",
            "Uses columnar streaming scenarios and direct camera pose truth on the same proof surface."),
        new(
            BarId,
            "Try next: Bar chart proof",
            "Bar",
            "Bar",
            "Uses grouped BarChartData with named series and category labels."),
        new(
            ContourId,
            "Try next: Contour plot proof",
            "Contour",
            "Contour",
            "Uses a radial scalar field with explicit contour levels."),
        new(
            LineId,
            "Try next: Line chart proof",
            "Line",
            "Line",
            "Uses 3D polyline with configurable color, width, and markers."),
        new(
            RibbonId,
            "Try next: Ribbon chart proof",
            "Ribbon",
            "Ribbon",
            "Uses tube/pipe geometry with configurable radius and colormap."),
        new(
            VectorFieldId,
            "Try next: Vector field proof",
            "VectorField",
            "Vector field",
            "Uses 3D arrows with magnitude-based coloring and scale control."),
        new(
            HeatmapSliceId,
            "Try next: Heatmap slice proof",
            "HeatmapSlice",
            "Heatmap slice",
            "Uses volumetric slice visualization with axis/position control."),
        new(
            BoxPlotId,
            "Try next: Box plot proof",
            "BoxPlot",
            "Box plot",
            "Uses statistical distribution visualization with grouped layout."),
        new(
            SurfaceStreamingId,
            "Try next: Surface streaming",
            "Surface",
            "Surface streaming",
            "SurfaceDataLogger3D with append/replace/FIFO semantics and live counters."),
        new(
            WaterfallStreamingId,
            "Try next: Waterfall streaming",
            "Waterfall",
            "Waterfall streaming",
            "WaterfallDataLogger3D delegating to SurfaceDataLogger3D for row streaming."),
        new(
            BarStreamingId,
            "Try next: Bar streaming",
            "Bar",
            "Bar streaming",
            "BarDataLogger3D appending new series with live series count tracking."),
        new(
            MultiPlot3DId,
            "Try next: MultiPlot3D grid",
            "MultiPlot3D",
            "MultiPlot3D",
            "MultiPlot3D 2x2 subplot grid with mixed chart types and linked camera."),
        new(
            AnalysisWorkspaceId,
            "Try next: Analysis workspace",
            "Multi-chart",
            "Analysis workspace",
            "Shows four VideraChartView instances in a bounded 2x2 grid with workspace status and evidence."),
        new(
            LinkedInteractionId,
            "Try next: Linked interaction",
            "Multi-chart",
            "Linked interaction",
            "Shows two linked VideraChartView instances with FullViewState sync and probe propagation across the link group."),
        new(
            StreamingWorkspaceId,
            "Try next: Streaming workspace",
            "Multi-chart",
            "Streaming workspace",
            "Shows two VideraChartView instances with different streaming modes (replace and append+FIFO), workspace tracking, and streaming evidence."),
        new(
            HistogramId,
            "Try next: Histogram proof",
            "Histogram",
            "Histogram",
            "Uses configurable bin count with count/density/cumulative modes."),
        new(
            FunctionPlotId,
            "Try next: Function plot proof",
            "FunctionPlot",
            "Function plot",
            "Evaluates a mathematical function over a domain with configurable sample count."),
    ];

    public static SurfaceDemoScenario Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Unknown SurfaceCharts demo scenario '{id}'.");
    }
}
