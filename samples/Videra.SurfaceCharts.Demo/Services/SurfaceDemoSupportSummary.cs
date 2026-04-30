using System.Globalization;
using System.Runtime.InteropServices;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Demo.Services;

internal static class SurfaceDemoSupportSummary
{
    internal static string Create(SurfaceDemoSupportSummaryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.IsScatterProofActive)
        {
            var scenario = context.ScatterScenario
                ?? throw new InvalidOperationException("The scatter support summary requires an active scatter scenario.");

            return
                "SurfaceCharts support summary\n" +
                $"GeneratedUtc: {DateTimeOffset.UtcNow:O}\n" +
                "EvidenceKind: SurfaceChartsStreamingDatasetProof\n" +
                "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.\n" +
                $"ChartControl: {CreateActiveChartControlSummary(context.ActiveChartView)}\n" +
                $"EnvironmentRuntime: {CreateEnvironmentRuntimeSummary()}\n" +
                $"AssemblyIdentity: {CreateAssemblyIdentitySummary()}\n" +
                $"BackendDisplayEnvironment: {CreateBackendDisplayEnvironmentSummary()}\n" +
                $"CacheLoadFailure: {CreateCacheLoadFailureSummary(context.CacheLoadFailureMessage)}\n" +
                $"Plot path: {context.ActivePlotPathHeading}\n" +
                $"Plot details: {context.ActivePlotPathDetails}\n" +
                $"SeriesCount: {context.ScatterChartView.Plot.Series.Count}\n" +
                $"ActiveSeries: {CreateActiveSeriesSummary(context.ScatterChartView)}\n" +
                $"ChartKind: {CreateChartKindSummary(context.ScatterChartView)}\n" +
                $"ColorMap: {CreateColorMapSummary(context.ScatterChartView.Plot.ColorMap)}\n" +
                $"PrecisionProfile: {CreatePrecisionProfileSummary(context.ScatterChartView)}\n" +
                $"OutputEvidenceKind: {CreateOutputEvidenceKindSummary(context.ScatterChartView)}\n" +
                $"OutputCapabilityDiagnostics: {CreateOutputCapabilityDiagnosticsSummary(context.ScatterChartView)}\n" +
                CreateSnapshotSummary(context.LastSnapshotResult) +
                $"DatasetEvidenceKind: {CreateDatasetEvidenceKindSummary(context.ScatterChartView)}\n" +
                $"DatasetSeriesCount: {CreateDatasetSeriesCountSummary(context.ScatterChartView)}\n" +
                $"DatasetActiveSeriesIndex: {CreateDatasetActiveSeriesIndexSummary(context.ScatterChartView)}\n" +
                $"DatasetActiveSeriesMetadata: {CreateDatasetActiveSeriesMetadataSummary(context.ScatterChartView)}\n" +
                $"ScenarioId: {scenario.Id}\n" +
                $"ScenarioName: {scenario.DisplayName}\n" +
                $"ScenarioUpdateMode: {scenario.UpdateMode}\n" +
                $"ScenarioInitialPointCount: {scenario.InitialPointCount}\n" +
                $"ScenarioUpdatePointCount: {scenario.UpdatePointCount}\n" +
                $"ScenarioFifoCapacity: {FormatFifoCapacity(scenario.FifoCapacity)}\n" +
                $"ScenarioPickable: {scenario.Pickable}\n" +
                $"Chart contract: VideraChartView exposes Plot.Add.Scatter on this proof path.\n" +
                $"Plot: {CreateScatterCameraSummary(context.ActiveScatterData, context.ScatterChartView)}\n" +
                $"RenderingStatus:\n{CreateScatterRenderingDiagnosticsSummary(context.ActiveScatterData, context.ScatterChartView)}\n" +
                $"InteractionQuality: {context.ScatterChartView.InteractionQuality}\n" +
                "OverlayOptions: VideraChartView.Plot.OverlayOptions\n" +
                $"Cache asset: {context.ActiveAssetSummary}\n" +
                $"Dataset: {context.ActiveDatasetSummary}";
        }

        if (context.IsContourProofActive)
        {
            var contourStatus = context.ContourPlotView.ContourRenderingStatus;
            return
                "SurfaceCharts support summary\n" +
                $"GeneratedUtc: {DateTimeOffset.UtcNow:O}\n" +
                "EvidenceKind: SurfaceChartsContourDatasetProof\n" +
                "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.\n" +
                $"ChartControl: {CreateActiveChartControlSummary(context.ActiveChartView)}\n" +
                $"EnvironmentRuntime: {CreateEnvironmentRuntimeSummary()}\n" +
                $"AssemblyIdentity: {CreateAssemblyIdentitySummary()}\n" +
                $"BackendDisplayEnvironment: {CreateBackendDisplayEnvironmentSummary()}\n" +
                $"CacheLoadFailure: {CreateCacheLoadFailureSummary(context.CacheLoadFailureMessage)}\n" +
                $"Plot path: {context.ActivePlotPathHeading}\n" +
                $"Plot details: {context.ActivePlotPathDetails}\n" +
                $"SeriesCount: {context.ContourPlotView.Plot.Series.Count}\n" +
                $"ActiveSeries: {CreateActiveSeriesSummary(context.ContourPlotView)}\n" +
                $"ChartKind: {CreateChartKindSummary(context.ContourPlotView)}\n" +
                $"ColorMap: {CreateColorMapSummary(context.ContourPlotView.Plot.ColorMap)}\n" +
                $"PrecisionProfile: {CreatePrecisionProfileSummary(context.ContourPlotView)}\n" +
                $"OutputEvidenceKind: {CreateOutputEvidenceKindSummary(context.ContourPlotView)}\n" +
                $"OutputCapabilityDiagnostics: {CreateOutputCapabilityDiagnosticsSummary(context.ContourPlotView)}\n" +
                CreateSnapshotSummary(context.LastSnapshotResult) +
                $"DatasetEvidenceKind: {CreateDatasetEvidenceKindSummary(context.ContourPlotView)}\n" +
                $"DatasetSeriesCount: {CreateDatasetSeriesCountSummary(context.ContourPlotView)}\n" +
                $"DatasetActiveSeriesIndex: {CreateDatasetActiveSeriesIndexSummary(context.ContourPlotView)}\n" +
                $"DatasetActiveSeriesMetadata: {CreateDatasetActiveSeriesMetadataSummary(context.ContourPlotView)}\n" +
                $"ContourRenderingStatus: HasSource {contourStatus.HasSource}; IsReady {contourStatus.IsReady}; Levels {contourStatus.LevelCount}; Lines {contourStatus.ExtractedLineCount}; Segments {contourStatus.TotalSegmentCount}\n" +
                $"InteractionQuality: {context.ContourPlotView.InteractionQuality}\n" +
                $"RenderingStatus:\nContour: HasSource={contourStatus.HasSource}; IsReady={contourStatus.IsReady}; Levels={contourStatus.LevelCount}; Lines={contourStatus.ExtractedLineCount}; Segments={contourStatus.TotalSegmentCount}\n" +
                $"OverlayOptions: {CreateOverlayOptionsSummary(context.ContourPlotView.Plot.OverlayOptions)}\n" +
                $"Cache asset: {context.ActiveAssetSummary}\n" +
                $"Dataset: {context.ActiveDatasetSummary}";
        }

        var surfaceStatus = context.ActiveChartView.RenderingStatus;
        return
            "SurfaceCharts support summary\n" +
            $"GeneratedUtc: {DateTimeOffset.UtcNow:O}\n" +
            "EvidenceKind: SurfaceChartsDatasetProof\n" +
            "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.\n" +
            $"ChartControl: {CreateActiveChartControlSummary(context.ActiveChartView)}\n" +
            $"EnvironmentRuntime: {CreateEnvironmentRuntimeSummary()}\n" +
            $"AssemblyIdentity: {CreateAssemblyIdentitySummary()}\n" +
            $"BackendDisplayEnvironment: {CreateBackendDisplayEnvironmentSummary()}\n" +
            $"CacheLoadFailure: {CreateCacheLoadFailureSummary(context.CacheLoadFailureMessage)}\n" +
            $"Plot path: {context.ActivePlotPathHeading}\n" +
            $"Plot details: {context.ActivePlotPathDetails}\n" +
            $"SeriesCount: {context.ActiveChartView.Plot.Series.Count}\n" +
            $"ActiveSeries: {CreateActiveSeriesSummary(context.ActiveChartView)}\n" +
            $"ChartKind: {CreateChartKindSummary(context.ActiveChartView)}\n" +
            $"ColorMap: {CreateColorMapSummary(context.ActiveChartView.Plot.ColorMap)}\n" +
            $"PrecisionProfile: {CreatePrecisionProfileSummary(context.ActiveChartView)}\n" +
            $"OutputEvidenceKind: {CreateOutputEvidenceKindSummary(context.ActiveChartView)}\n" +
            $"OutputCapabilityDiagnostics: {CreateOutputCapabilityDiagnosticsSummary(context.ActiveChartView)}\n" +
            CreateSnapshotSummary(context.LastSnapshotResult) +
            $"DatasetEvidenceKind: {CreateDatasetEvidenceKindSummary(context.ActiveChartView)}\n" +
            $"DatasetSeriesCount: {CreateDatasetSeriesCountSummary(context.ActiveChartView)}\n" +
            $"DatasetActiveSeriesIndex: {CreateDatasetActiveSeriesIndexSummary(context.ActiveChartView)}\n" +
            $"DatasetActiveSeriesMetadata: {CreateDatasetActiveSeriesMetadataSummary(context.ActiveChartView)}\n" +
            $"ViewState: {context.ViewStateSummary}\n" +
            $"InteractionQuality: {context.ActiveChartView.InteractionQuality}\n" +
            $"RenderingStatus:\n{CreateSurfaceRenderingDiagnosticsSummary(surfaceStatus)}\n" +
            $"OverlayOptions: {CreateOverlayOptionsSummary(context.ActiveChartView.Plot.OverlayOptions)}\n" +
            $"Cache asset: {context.ActiveAssetSummary}\n" +
            $"Dataset: {context.ActiveDatasetSummary}";
    }

    internal static string CreateScatterCameraSummary(ScatterChartData? scatter, VideraChartView chartView)
    {
        _ = scatter;
        var status = chartView.ScatterRenderingStatus;
        return
            $"PlotRevision {chartView.Plot.Revision}, SeriesCount {status.SeriesCount}, PointCount {status.PointCount}, ColumnarSeriesCount {status.ColumnarSeriesCount}, ColumnarPointCount {status.ColumnarPointCount}, PickablePointCount {status.PickablePointCount}, StreamingAppendBatchCount {status.StreamingAppendBatchCount}, ConfiguredFifoCapacity {FormatFifoCapacity(status.ConfiguredFifoCapacity == 0 ? null : status.ConfiguredFifoCapacity)}";
    }

    internal static string CreateSurfaceRenderingDiagnosticsSummary(SurfaceChartRenderingStatus status)
    {
        return
            $"ActiveBackend: {status.ActiveBackend}\n" +
            $"IsReady: {status.IsReady}\n" +
            $"IsFallback: {status.IsFallback}\n" +
            $"FallbackReason: {status.FallbackReason ?? "none"}\n" +
            $"UsesNativeSurface: {status.UsesNativeSurface}\n" +
            $"ResidentTileCount: {status.ResidentTileCount}\n" +
            $"VisibleTileCount: {status.VisibleTileCount}\n" +
            $"ResidentTileBytes: {status.ResidentTileBytes} (measured by GPU resources or estimated from software tile geometry)\n" +
            $"Fallback status: {CreateFallbackText(status)}\n" +
            $"Host path: {CreateHostText(status)}";
    }

    internal static string CreateScatterRenderingDiagnosticsSummary(ScatterChartData? scatter, VideraChartView chartView)
    {
        _ = scatter;
        var status = chartView.ScatterRenderingStatus;
        return
            $"PlotRevision: {chartView.Plot.Revision}\n" +
            $"LastRefreshRevision: {chartView.LastRefreshRevision}\n" +
            $"InteractionQuality: {status.InteractionQuality}\n" +
            $"SeriesCount: {status.SeriesCount}\n" +
            $"PointCount: {status.PointCount}\n" +
            $"ColumnarSeriesCount: {status.ColumnarSeriesCount}\n" +
            $"ColumnarPointCount: {status.ColumnarPointCount}\n" +
            $"PickablePointCount: {status.PickablePointCount}\n" +
            $"StreamingAppendBatchCount: {status.StreamingAppendBatchCount}\n" +
            $"StreamingReplaceBatchCount: {status.StreamingReplaceBatchCount}\n" +
            $"StreamingDroppedPointCount: {status.StreamingDroppedPointCount}\n" +
            $"LastStreamingDroppedPointCount: {status.LastStreamingDroppedPointCount}\n" +
            $"ConfiguredFifoCapacity: {FormatFifoCapacity(status.ConfiguredFifoCapacity == 0 ? null : status.ConfiguredFifoCapacity)}";
    }

    internal static string FormatFifoCapacity(int? configuredFifoCapacity)
    {
        return configuredFifoCapacity is { } capacity ? capacity.ToString(CultureInfo.InvariantCulture) : "unbounded";
    }

    private static string CreateSnapshotSummary(PlotSnapshotResult? snapshotResult)
    {
        return
            $"SnapshotStatus: {CreateSnapshotStatusSummary(snapshotResult)}\n" +
            $"SnapshotPath: {CreateSnapshotPathSummary(snapshotResult)}\n" +
            $"SnapshotWidth: {CreateSnapshotWidthSummary(snapshotResult)}\n" +
            $"SnapshotHeight: {CreateSnapshotHeightSummary(snapshotResult)}\n" +
            $"SnapshotFormat: {CreateSnapshotFormatSummary(snapshotResult)}\n" +
            $"SnapshotBackground: {CreateSnapshotBackgroundSummary(snapshotResult)}\n" +
            $"SnapshotOutputEvidenceKind: {CreateSnapshotOutputEvidenceKindSummary(snapshotResult)}\n" +
            $"SnapshotDatasetEvidenceKind: {CreateSnapshotDatasetEvidenceKindSummary(snapshotResult)}\n" +
            $"SnapshotActiveSeriesIdentity: {CreateSnapshotActiveSeriesIdentitySummary(snapshotResult)}\n" +
            $"SnapshotCreatedUtc: {CreateSnapshotCreatedUtcSummary(snapshotResult)}\n";
    }

    private static string CreateActiveChartControlSummary(VideraChartView chartView)
    {
        var type = chartView.GetType();
        return $"{type.Name} ({type.FullName})";
    }

    private static string CreateEnvironmentRuntimeSummary()
    {
        return
            $"{RuntimeInformation.FrameworkDescription}; " +
            $"OS {RuntimeInformation.OSDescription}; " +
            $"ProcessArchitecture {RuntimeInformation.ProcessArchitecture}; " +
            $"OSArchitecture {RuntimeInformation.OSArchitecture}";
    }

    private static string CreateAssemblyIdentitySummary()
    {
        return
            $"Demo {CreateAssemblyIdentity(typeof(SurfaceDemoSupportSummary))}; " +
            $"Avalonia {CreateAssemblyIdentity(typeof(VideraChartView))}; " +
            $"Processing {CreateAssemblyIdentity(typeof(SurfaceCacheReader))}; " +
            $"Rendering {CreateAssemblyIdentity(typeof(SurfaceChartRenderingStatus))}";
    }

    private static string CreateAssemblyIdentity(Type type)
    {
        var name = type.Assembly.GetName();
        return $"{name.Name} {name.Version}";
    }

    private static string CreateBackendDisplayEnvironmentSummary()
    {
        return
            $"VIDERA_BACKEND={GetEnvironmentValue("VIDERA_BACKEND")}; " +
            $"DISPLAY={GetEnvironmentValue("DISPLAY")}; " +
            $"WAYLAND_DISPLAY={GetEnvironmentValue("WAYLAND_DISPLAY")}; " +
            $"XDG_SESSION_TYPE={GetEnvironmentValue("XDG_SESSION_TYPE")}";
    }

    private static string GetEnvironmentValue(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? "unset" : value;
    }

    private static string CreateCacheLoadFailureSummary(string? cacheLoadFailureMessage)
    {
        return cacheLoadFailureMessage ?? "none";
    }

    private static string CreateActiveSeriesSummary(VideraChartView chartView)
    {
        var activeSeries = chartView.Plot.ActiveSeries;
        if (activeSeries is null)
        {
            return "none";
        }

        return
            $"Index {chartView.Plot.IndexOf(activeSeries)}, " +
            $"Kind {activeSeries.Kind}, " +
            $"Name {FormatSeriesName(activeSeries.Name)}";
    }

    private static string CreateChartKindSummary(VideraChartView chartView)
    {
        return chartView.Plot.ActiveSeries?.Kind.ToString() ?? "none";
    }

    private static string CreateColorMapSummary(SurfaceColorMap? colorMap)
    {
        if (colorMap is null)
        {
            return "none";
        }

        return
            $"PaletteStops {colorMap.Palette.Count}, " +
            $"Range {colorMap.Range.Minimum.ToString("0.###", CultureInfo.InvariantCulture)}.." +
            $"{colorMap.Range.Maximum.ToString("0.###", CultureInfo.InvariantCulture)}";
    }

    private static string CreatePrecisionProfileSummary(VideraChartView chartView)
    {
        return SurfaceChartOverlayEvidenceFormatter.DescribePrecisionProfile(chartView.Plot.OverlayOptions);
    }

    private static string CreateOutputEvidenceKindSummary(VideraChartView chartView)
    {
        return CreateOutputEvidence(chartView).EvidenceKind;
    }

    private static string CreateOutputCapabilityDiagnosticsSummary(VideraChartView chartView)
    {
        var diagnostics = CreateOutputEvidence(chartView).OutputCapabilityDiagnostics;
        return string.Join(
            "; ",
            diagnostics.Select(diagnostic =>
                $"{diagnostic.Capability}={diagnostic.DiagnosticCode};Supported={diagnostic.IsSupported}"));
    }

    private static string CreateDatasetEvidenceKindSummary(VideraChartView chartView)
    {
        return chartView.Plot.CreateDatasetEvidence().EvidenceKind;
    }

    private static string CreateDatasetSeriesCountSummary(VideraChartView chartView)
    {
        return chartView.Plot.CreateDatasetEvidence().Series.Count.ToString(CultureInfo.InvariantCulture);
    }

    private static string CreateDatasetActiveSeriesIndexSummary(VideraChartView chartView)
    {
        return chartView.Plot.CreateDatasetEvidence().ActiveSeriesIndex.ToString(CultureInfo.InvariantCulture);
    }

    private static string CreateDatasetActiveSeriesMetadataSummary(VideraChartView chartView)
    {
        var evidence = chartView.Plot.CreateDatasetEvidence();
        var active = evidence.Series.FirstOrDefault(series => series.IsActive);
        if (active is null)
        {
            return "none";
        }

        return active.Kind switch
        {
            Plot3DSeriesKind.Surface or Plot3DSeriesKind.Waterfall =>
                $"{active.Identity}; Samples {active.Width}x{active.Height}; Count {active.SampleCount}; " +
                $"ValueRange {FormatValueRange(active.ValueRange)}; Sampling {active.SamplingProfile}",
            Plot3DSeriesKind.Scatter =>
                $"{active.Identity}; Points {active.PointCount}; Series {active.SeriesCount}; " +
                $"ColumnarSeries {active.ColumnarSeriesCount}; PickablePoints {active.PickablePointCount}; " +
                $"FifoCapacity {FormatFifoCapacity(active.ConfiguredFifoCapacity == 0 ? null : active.ConfiguredFifoCapacity)}",
            Plot3DSeriesKind.Bar =>
                $"{active.Identity}; Categories {active.PointCount}; Series {active.SeriesCount}; " +
                $"Sampling {active.SamplingProfile}",
            Plot3DSeriesKind.Contour =>
                $"{active.Identity}; Field {active.Width}x{active.Height}; Samples {active.SampleCount}; " +
                $"Sampling {active.SamplingProfile}",
            _ => active.Identity,
        };
    }

    private static Plot3DOutputEvidence CreateOutputEvidence(VideraChartView chartView)
    {
        return chartView.Plot.CreateOutputEvidence(chartView.RenderingStatus, chartView.ScatterRenderingStatus, chartView.BarRenderingStatus, chartView.ContourRenderingStatus);
    }

    private static string FormatValueRange(SurfaceValueRangeDatasetEvidence? valueRange)
    {
        return valueRange is null
            ? "none"
            : $"{valueRange.Minimum.ToString("G6", CultureInfo.InvariantCulture)}..{valueRange.Maximum.ToString("G6", CultureInfo.InvariantCulture)}";
    }

    private static string FormatSeriesName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? "unnamed" : name;
    }

    private static string CreateOverlayOptionsSummary(SurfaceChartOverlayOptions overlayOptions)
    {
        return
            $"Minor ticks {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions}); " +
            $"Grid plane {overlayOptions.GridPlane}; " +
            $"Axis side {overlayOptions.AxisSideMode}";
    }

    internal static string CreateFallbackText(SurfaceChartRenderingStatus status)
    {
        return status.IsFallback
            ? $"software fallback active ({status.FallbackReason ?? "reason unavailable"})"
            : "no fallback active";
    }

    internal static string CreateHostText(SurfaceChartRenderingStatus status)
    {
        return status.UsesNativeSurface
            ? "native surface host is active"
            : "control-owned surface is active";
    }

    private static string CreateSnapshotStatusSummary(PlotSnapshotResult? snapshotResult)
    {
        if (snapshotResult is null)
        {
            return "none";
        }

        return snapshotResult.Succeeded ? "present" : "failed";
    }

    private static string CreateSnapshotPathSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Path ?? "none";
    }

    private static string CreateSnapshotWidthSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.Width.ToString(CultureInfo.InvariantCulture) ?? "none";
    }

    private static string CreateSnapshotHeightSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.Height.ToString(CultureInfo.InvariantCulture) ?? "none";
    }

    private static string CreateSnapshotFormatSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.Format.ToString() ?? "none";
    }

    private static string CreateSnapshotBackgroundSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.Background.ToString() ?? "none";
    }

    private static string CreateSnapshotOutputEvidenceKindSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.OutputEvidenceKind ?? "none";
    }

    private static string CreateSnapshotDatasetEvidenceKindSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.DatasetEvidenceKind ?? "none";
    }

    private static string CreateSnapshotActiveSeriesIdentitySummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.ActiveSeriesIdentity ?? "none";
    }

    private static string CreateSnapshotCreatedUtcSummary(PlotSnapshotResult? snapshotResult)
    {
        return snapshotResult?.Manifest?.CreatedUtc.ToString("O") ?? "none";
    }
}

internal sealed record SurfaceDemoSupportSummaryContext(
    VideraChartView ActiveChartView,
    VideraChartView ScatterChartView,
    VideraChartView ContourPlotView,
    bool IsScatterProofActive,
    bool IsContourProofActive,
    ScatterChartData? ActiveScatterData,
    ScatterStreamingScenario? ScatterScenario,
    PlotSnapshotResult? LastSnapshotResult,
    string? CacheLoadFailureMessage,
    string ActivePlotPathHeading,
    string ActivePlotPathDetails,
    string ActiveDatasetSummary,
    string ActiveAssetSummary,
    string ViewStateSummary);
