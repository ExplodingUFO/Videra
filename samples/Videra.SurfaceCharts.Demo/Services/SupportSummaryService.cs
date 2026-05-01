using Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Services;

/// <summary>
/// Centralises support-summary text generation, status-context creation,
/// active-chart-view resolution, status-panel refresh, and the mutable
/// plot-path state that drives the support summary panel.
/// </summary>
internal sealed class SupportSummaryService
{
    private readonly VideraChartView _scatterChartView;
    private readonly VideraChartView _contourPlotView;
    private readonly VideraChartView _barChartView;
    private readonly VideraChartView[] _newChartTypeViews;
    private readonly TextBlock _supportSummaryText;
    private readonly Func<VideraChartView> _getActiveSurfaceFamilyChartView;

    // Status-panel UI references
    private readonly TextBlock _viewStateText;
    private readonly TextBlock _interactionQualityText;
    private readonly TextBlock _renderingPathText;
    private readonly TextBlock _renderingDiagnosticsText;
    private readonly TextBlock _overlayOptionsText;
    private readonly TextBlock _statusText;
    private readonly TextBlock _builtInInteractionText;
    private readonly Grid _scatterScenarioPanel;
    private readonly ComboBox _scatterScenarioSelector;

    internal SupportSummaryService(
        VideraChartView scatterChartView,
        VideraChartView contourPlotView,
        VideraChartView barChartView,
        VideraChartView[] newChartTypeViews,
        TextBlock supportSummaryText,
        Func<VideraChartView> getActiveSurfaceFamilyChartView,
        TextBlock viewStateText,
        TextBlock interactionQualityText,
        TextBlock renderingPathText,
        TextBlock renderingDiagnosticsText,
        TextBlock overlayOptionsText,
        TextBlock statusText,
        TextBlock builtInInteractionText,
        Grid scatterScenarioPanel,
        ComboBox scatterScenarioSelector)
    {
        _scatterChartView = scatterChartView;
        _contourPlotView = contourPlotView;
        _barChartView = barChartView;
        _newChartTypeViews = newChartTypeViews;
        _supportSummaryText = supportSummaryText;
        _getActiveSurfaceFamilyChartView = getActiveSurfaceFamilyChartView;
        _viewStateText = viewStateText;
        _interactionQualityText = interactionQualityText;
        _renderingPathText = renderingPathText;
        _renderingDiagnosticsText = renderingDiagnosticsText;
        _overlayOptionsText = overlayOptionsText;
        _statusText = statusText;
        _builtInInteractionText = builtInInteractionText;
        _scatterScenarioPanel = scatterScenarioPanel;
        _scatterScenarioSelector = scatterScenarioSelector;
    }

    internal string ActivePlotPathHeading { get; set; } = string.Empty;
    internal string ActivePlotPathDetails { get; set; } = string.Empty;
    internal string ActiveDatasetSummary { get; set; } = string.Empty;
    internal string ActiveAssetSummary { get; set; } = "No additional assets are used on this path.";
    internal ScatterChartData? ActiveScatterData { get; set; }

    internal bool IsNewChartTypeActive => _newChartTypeViews.Any(v => v.IsVisible);

    internal VideraChartView GetActiveChartView()
    {
        if (_scatterChartView.IsVisible)
        {
            return _scatterChartView;
        }

        if (_contourPlotView.IsVisible)
        {
            return _contourPlotView;
        }

        if (_barChartView.IsVisible)
        {
            return _barChartView;
        }

        return _getActiveSurfaceFamilyChartView();
    }

    internal ChartStatusContext CreateStatusContext()
    {
        return new ChartStatusContext(
            ScatterChartView: _scatterChartView,
            ContourPlotView: _contourPlotView,
            BarChartView: _barChartView,
            ActiveSurfaceFamilyChartView: _getActiveSurfaceFamilyChartView(),
            IsScatterProofActive: _scatterChartView.IsVisible,
            IsContourProofActive: _contourPlotView.IsVisible,
            ActiveScatterData: ActiveScatterData,
            ActivePlotPathHeading: ActivePlotPathHeading,
            ActivePlotPathDetails: ActivePlotPathDetails);
    }

    internal void UpdateSupportSummaryText(
        ScatterStreamingScenario? scatterScenario,
        PlotSnapshotResult? lastSnapshotResult,
        string? cacheLoadFailureMessage)
    {
        var statusCtx = CreateStatusContext();

        _supportSummaryText.Text = SurfaceDemoSupportSummary.Create(
            new SurfaceDemoSupportSummaryContext(
                ActiveChartView: GetActiveChartView(),
                ScatterChartView: _scatterChartView,
                ContourPlotView: _contourPlotView,
                IsScatterProofActive: _scatterChartView.IsVisible,
                IsContourProofActive: _contourPlotView.IsVisible,
                ActiveScatterData: ActiveScatterData,
                ScatterScenario: scatterScenario,
                LastSnapshotResult: lastSnapshotResult,
                CacheLoadFailureMessage: cacheLoadFailureMessage,
                ActivePlotPathHeading: ActivePlotPathHeading,
                ActivePlotPathDetails: ActivePlotPathDetails,
                ActiveDatasetSummary: ActiveDatasetSummary,
                ActiveAssetSummary: ActiveAssetSummary,
                ViewStateSummary: ChartStatusFormatter.FormatViewStateSummary(statusCtx)));
    }

    internal void RefreshActiveProofTexts(
        ScatterStreamingScenario? scatterScenario,
        PlotSnapshotResult? lastSnapshotResult,
        string? cacheLoadFailureMessage)
    {
        UpdateScatterScenarioSelectorState();
        UpdateBuiltInInteractionText();

        var ctx = CreateStatusContext();
        _viewStateText.Text = ChartStatusFormatter.FormatViewStateText(ctx);
        _interactionQualityText.Text = ChartStatusFormatter.FormatInteractionQualityText(ctx);
        _renderingPathText.Text = ChartStatusFormatter.FormatRenderingPathText(ctx);
        _renderingDiagnosticsText.Text = ChartStatusFormatter.FormatRenderingDiagnosticsText(ctx);
        _overlayOptionsText.Text = ChartStatusFormatter.FormatOverlayOptionsText(ctx);
        _statusText.Text = ChartStatusFormatter.FormatStatusText(ctx);
        UpdateSupportSummaryText(scatterScenario, lastSnapshotResult, cacheLoadFailureMessage);
    }

    internal ChartStatusContext FormatInteractionQualityContext()
        => CreateStatusContext();

    private void UpdateScatterScenarioSelectorState()
    {
        var isScatterActive = _scatterChartView.IsVisible;
        _scatterScenarioPanel.Opacity = isScatterActive ? 1d : 0.45d;
        _scatterScenarioSelector.IsEnabled = isScatterActive;
    }

    private void UpdateBuiltInInteractionText()
    {
        _builtInInteractionText.Text = _scatterChartView.IsVisible
            ? "Left drag orbit. Wheel dolly. Scatter proof does not expose right-drag pan or Ctrl + Left drag focus zoom."
            : _contourPlotView.IsVisible
            ? "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom. Contour proof shares the same interaction contract."
            : "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom.";
    }
}
