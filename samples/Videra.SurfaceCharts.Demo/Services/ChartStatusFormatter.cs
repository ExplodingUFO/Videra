using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Bundles the state needed by <see cref="ChartStatusFormatter"/> methods.
/// </summary>
internal sealed record ChartStatusContext(
    VideraChartView ScatterChartView,
    VideraChartView ContourPlotView,
    VideraChartView BarChartView,
    VideraChartView ActiveSurfaceFamilyChartView,
    bool IsScatterProofActive,
    bool IsContourProofActive,
    ScatterChartData? ActiveScatterData,
    string ActivePlotPathHeading,
    string ActivePlotPathDetails);

/// <summary>
/// Produces status-panel text strings from chart state.
/// </summary>
internal static class ChartStatusFormatter
{
    internal static string FormatStatusText(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            var scatter = ctx.ActiveScatterData;
            return
                $"{ctx.ActivePlotPathHeading}\n" +
                $"{ctx.ActivePlotPathDetails}\n" +
                "Scatter proof is authored through VideraChartView.Plot.Add.Scatter.\n" +
                $"Current scene: {scatter?.SeriesCount ?? 0} series, {scatter?.PointCount ?? 0} points.\n" +
                $"Columnar series: {scatter?.ColumnarSeriesCount ?? 0}; Retained columnar points: {scatter?.ColumnarPointCount ?? 0}; Pickable points: {scatter?.PickablePointCount ?? 0}.\n" +
                $"Streaming appends: {scatter?.StreamingAppendBatchCount ?? 0}; FIFO capacity: {SurfaceDemoSupportSummary.FormatFifoCapacity(scatter?.ConfiguredFifoCapacity)}; Dropped points: {scatter?.StreamingDroppedPointCount ?? 0}.";
        }

        if (ctx.IsContourProofActive)
        {
            var contourStatus = ctx.ContourPlotView.ContourRenderingStatus;
            return
                $"{ctx.ActivePlotPathHeading}\n" +
                $"{ctx.ActivePlotPathDetails}\n" +
                "Contour proof is authored through VideraChartView.Plot.Add.Contour.\n" +
                $"Current scene: {contourStatus.LevelCount} levels, {contourStatus.ExtractedLineCount} lines, {contourStatus.TotalSegmentCount} segments.";
        }

        if (ctx.BarChartView.IsVisible)
        {
            var barStatus = ctx.BarChartView.BarRenderingStatus;
            return
                $"{ctx.ActivePlotPathHeading}\n" +
                $"{ctx.ActivePlotPathDetails}\n" +
                "Bar proof is authored through VideraChartView.Plot.Add.Bar.\n" +
                $"Current scene: {barStatus.SeriesCount} series, {barStatus.CategoryCount} categories, {barStatus.BarCount} bars, layout {barStatus.Layout}.";
        }

        var dataWindow = ctx.ActiveSurfaceFamilyChartView.ViewState.DataWindow;
        return
            $"{ctx.ActivePlotPathHeading}\n" +
            $"{ctx.ActivePlotPathDetails}\n" +
            "First-chart navigation: Left drag orbit, Right drag pan, Wheel dolly, Ctrl + Left drag focus zoom.\n" +
            $"Current window: StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}";
    }

    internal static string FormatViewStateText(ChartStatusContext ctx)
    {
        return $"ViewState / camera state\n{FormatViewStateSummary(ctx)}";
    }

    internal static string FormatViewStateSummary(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            return SurfaceDemoSupportSummary.CreateScatterCameraSummary(ctx.ActiveScatterData, ctx.ScatterChartView);
        }

        if (ctx.IsContourProofActive)
        {
            var contourStatus = ctx.ContourPlotView.ContourRenderingStatus;
            return $"Contour: HasSource={contourStatus.HasSource}; Levels={contourStatus.LevelCount}; Lines={contourStatus.ExtractedLineCount}; Segments={contourStatus.TotalSegmentCount}";
        }

        var viewState = ctx.ActiveSurfaceFamilyChartView.ViewState;
        var dataWindow = viewState.DataWindow;
        var camera = viewState.Camera;
        return
            $"Data window StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}; " +
            $"Camera target ({camera.Target.X:0.###}, {camera.Target.Y:0.###}, {camera.Target.Z:0.###}), Yaw {camera.YawDegrees:0.###}, Pitch {camera.PitchDegrees:0.###}, Distance {camera.Distance:0.###}";
    }

    internal static string FormatInteractionQualityText(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            var scatter = ctx.ActiveScatterData;
            return
                $"Current mode: {ctx.ScatterChartView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {scatter?.PointCount ?? 0} scatter points.";
        }

        if (ctx.IsContourProofActive)
        {
            var contourStatus = ctx.ContourPlotView.ContourRenderingStatus;
            return
                $"Current mode: {ctx.ContourPlotView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {contourStatus.ExtractedLineCount} contour lines.";
        }

        if (ctx.BarChartView.IsVisible)
        {
            var barStatus = ctx.BarChartView.BarRenderingStatus;
            return
                $"Current mode: {ctx.BarChartView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {barStatus.BarCount} bars.";
        }

        return
            $"Current mode: {ctx.ActiveSurfaceFamilyChartView.InteractionQuality}\n" +
            "Interactive: lighter requests while orbit, pan, dolly, or focus input is in flight.\n" +
            "Refine: full settled requests for the current view.";
    }

    internal static string FormatRenderingPathText(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            var scatter = ctx.ActiveScatterData;
            return
                "Plot path: VideraChartView.Plot.Add.Scatter\n" +
                $"Plot revision: {ctx.ScatterChartView.Plot.Revision}\n" +
                $"Interaction quality: {ctx.ScatterChartView.InteractionQuality}\n" +
                $"Series: {scatter?.SeriesCount ?? 0}; Points: {scatter?.PointCount ?? 0}\n" +
                $"Columnar series: {scatter?.ColumnarSeriesCount ?? 0}; Retained columnar points: {scatter?.ColumnarPointCount ?? 0}; Pickable points: {scatter?.PickablePointCount ?? 0}\n" +
                $"Streaming appends: {scatter?.StreamingAppendBatchCount ?? 0}; Replacements: {scatter?.StreamingReplaceBatchCount ?? 0}; FIFO capacity: {SurfaceDemoSupportSummary.FormatFifoCapacity(scatter?.ConfiguredFifoCapacity)}; Dropped points: {scatter?.StreamingDroppedPointCount ?? 0} (last {scatter?.LastStreamingDroppedPointCount ?? 0})";
        }

        if (ctx.IsContourProofActive)
        {
            var contourStatus = ctx.ContourPlotView.ContourRenderingStatus;
            return
                "Plot path: VideraChartView.Plot.Add.Contour\n" +
                $"Plot revision: {ctx.ContourPlotView.Plot.Revision}\n" +
                $"Interaction quality: {ctx.ContourPlotView.InteractionQuality}\n" +
                $"Has source: {contourStatus.HasSource}; Is ready: {contourStatus.IsReady}\n" +
                $"Levels: {contourStatus.LevelCount}; Lines: {contourStatus.ExtractedLineCount}; Segments: {contourStatus.TotalSegmentCount}";
        }

        if (ctx.BarChartView.IsVisible)
        {
            var barStatus = ctx.BarChartView.BarRenderingStatus;
            return
                "Plot path: VideraChartView.Plot.Add.Bar\n" +
                $"Plot revision: {ctx.BarChartView.Plot.Revision}\n" +
                $"Interaction quality: {ctx.BarChartView.InteractionQuality}\n" +
                $"Has source: {barStatus.HasSource}; Is ready: {barStatus.IsReady}\n" +
                $"Series: {barStatus.SeriesCount}; Categories: {barStatus.CategoryCount}; Bars: {barStatus.BarCount}; Layout: {barStatus.Layout}";
        }

        var surfaceStatus = ctx.ActiveSurfaceFamilyChartView.RenderingStatus;
        return
            $"Active backend: {surfaceStatus.ActiveBackend}\n" +
            $"Ready: {surfaceStatus.IsReady}\n" +
            $"Fallback: {SurfaceDemoSupportSummary.CreateFallbackText(surfaceStatus)}\n" +
            $"Host path: {SurfaceDemoSupportSummary.CreateHostText(surfaceStatus)}\n" +
            $"Resident tiles: {surfaceStatus.ResidentTileCount}";
    }

    internal static string FormatRenderingDiagnosticsText(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            return SurfaceDemoSupportSummary.CreateScatterRenderingDiagnosticsSummary(ctx.ActiveScatterData, ctx.ScatterChartView);
        }

        if (ctx.IsContourProofActive)
        {
            var contourStatus = ctx.ContourPlotView.ContourRenderingStatus;
            return
                $"HasSource: {contourStatus.HasSource}\n" +
                $"IsReady: {contourStatus.IsReady}\n" +
                $"LevelCount: {contourStatus.LevelCount}\n" +
                $"ExtractedLineCount: {contourStatus.ExtractedLineCount}\n" +
                $"TotalSegmentCount: {contourStatus.TotalSegmentCount}";
        }

        if (ctx.BarChartView.IsVisible)
        {
            var barStatus = ctx.BarChartView.BarRenderingStatus;
            return
                $"HasSource: {barStatus.HasSource}\n" +
                $"IsReady: {barStatus.IsReady}\n" +
                $"BackendKind: {barStatus.BackendKind}\n" +
                $"SeriesCount: {barStatus.SeriesCount}\n" +
                $"CategoryCount: {barStatus.CategoryCount}\n" +
                $"BarCount: {barStatus.BarCount}\n" +
                $"Layout: {barStatus.Layout}";
        }

        return SurfaceDemoSupportSummary.CreateSurfaceRenderingDiagnosticsSummary(ctx.ActiveSurfaceFamilyChartView.RenderingStatus);
    }

    internal static string FormatOverlayOptionsText(ChartStatusContext ctx)
    {
        if (ctx.IsScatterProofActive)
        {
            return
                "VideraChartView.Plot exposes `OverlayOptions`.\n" +
                "This proof path stays direct-scatter only; Plot-level presentation is shared API but scatter overlay rendering is not widened in this demo.";
        }

        if (ctx.IsContourProofActive)
        {
            return
                "VideraChartView.Plot exposes `OverlayOptions`.\n" +
                "This proof path stays direct-contour only; Plot-level presentation is shared API but contour overlay rendering is not widened in this demo.";
        }

        var overlayOptions = ctx.ActiveSurfaceFamilyChartView.Plot.OverlayOptions;
        return
            "Chart-local `OverlayOptions` keep formatter, minor ticks, grid plane, and axis-side behavior inside `VideraChartView` instead of pushing chart semantics into `VideraView`.\n" +
            $"Minor ticks: {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions})\n" +
            $"Grid plane: {overlayOptions.GridPlane}\n" +
            $"Axis side: {overlayOptions.AxisSideMode}\n" +
            "Formatter: legend and axis labels share the same chart-local numeric formatting contract.";
    }
}
