using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow : Window
{
    private const string CacheManifestFileName = "sample.surfacecache.json";
    private const string CachePayloadSuffix = ".bin";
    private const int CacheSourceIndex = 1;
    private const int AnalyticsSourceIndex = 2;
    private const int WaterfallSourceIndex = 3;
    private const int ScatterSourceIndex = 4;
    private const int BarSourceIndex = 5;
    private const int ContourSourceIndex = 6;

    private readonly VideraChartView _surfaceChartView;
    private readonly VideraChartView _waterfallChartView;
    private readonly VideraChartView _scatterChartView;
    private readonly VideraChartView _barChartView;
    private readonly VideraChartView _contourPlotView;
    private readonly ComboBox _sourceSelector;
    private readonly ComboBox _scatterScenarioSelector;
    private readonly ComboBox _cookbookRecipeSelector;
    private readonly Button _fitToDataButton;
    private readonly Button _resetCameraButton;
    private readonly Button _copyRecipeSnippetButton;
    private readonly TextBlock _statusText;
    private readonly TextBlock _viewStateText;
    private readonly TextBlock _cookbookRecipeStatusText;
    private readonly TextBlock _cookbookRecipeSnippetText;
    private readonly TextBlock _interactionQualityText;
    private readonly TextBlock _builtInInteractionText;
    private readonly TextBlock _renderingPathText;
    private readonly TextBlock _renderingDiagnosticsText;
    private readonly TextBlock _overlayOptionsText;
    private readonly TextBlock _cachePathText;
    private readonly TextBlock _datasetText;
    private readonly Button _copySupportSummaryButton;
    private readonly TextBlock _supportSummaryStatusText;
    private readonly TextBlock _supportSummaryText;
    private readonly ISurfaceTileSource _inMemorySource;
    private readonly ISurfaceTileSource _analyticsProofSource;
    private readonly ISurfaceTileSource _waterfallSource;
    private readonly string _cachePath;
    private readonly string _cachePayloadPath;
    private Task<ISurfaceTileSource>? _cacheSourceTask;
    private string _activePlotPathHeading = string.Empty;
    private string _activePlotPathDetails = string.Empty;
    private string _activeDatasetSummary = string.Empty;
    private string _activeAssetSummary = "No additional assets are used on this path.";
    private string? _lastCacheLoadFailureMessage;
    private ScatterChartData? _activeScatterData;
    private PlotSnapshotResult? _lastSnapshotResult;

    public MainWindow()
    {
        InitializeComponent();

        _surfaceChartView = this.FindControl<VideraChartView>("ChartView")
            ?? throw new InvalidOperationException("Surface chart view is missing.");
        _waterfallChartView = this.FindControl<VideraChartView>("WaterfallPlotView")
            ?? throw new InvalidOperationException("WaterfallPlotView is missing.");
        _scatterChartView = this.FindControl<VideraChartView>("ScatterPlotView")
            ?? throw new InvalidOperationException("ScatterPlotView is missing.");
        _barChartView = this.FindControl<VideraChartView>("BarChartPlotView")
            ?? throw new InvalidOperationException("BarChartPlotView is missing.");
        _contourPlotView = this.FindControl<VideraChartView>("ContourPlotView")
            ?? throw new InvalidOperationException("ContourPlotView is missing.");
        _sourceSelector = this.FindControl<ComboBox>("SourceSelector")
            ?? throw new InvalidOperationException("Source selector is missing.");
        _scatterScenarioSelector = this.FindControl<ComboBox>("ScatterScenarioSelector")
            ?? throw new InvalidOperationException("Scatter scenario selector is missing.");
        _cookbookRecipeSelector = this.FindControl<ComboBox>("CookbookRecipeSelector")
            ?? throw new InvalidOperationException("CookbookRecipeSelector is missing.");
        _fitToDataButton = this.FindControl<Button>("FitToDataButton")
            ?? throw new InvalidOperationException("FitToDataButton is missing.");
        _resetCameraButton = this.FindControl<Button>("ResetCameraButton")
            ?? throw new InvalidOperationException("ResetCameraButton is missing.");
        _copyRecipeSnippetButton = this.FindControl<Button>("CopyRecipeSnippetButton")
            ?? throw new InvalidOperationException("CopyRecipeSnippetButton is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("Status text control is missing.");
        _viewStateText = this.FindControl<TextBlock>("ViewStateText")
            ?? throw new InvalidOperationException("ViewStateText is missing.");
        _cookbookRecipeStatusText = this.FindControl<TextBlock>("CookbookRecipeStatusText")
            ?? throw new InvalidOperationException("CookbookRecipeStatusText is missing.");
        _cookbookRecipeSnippetText = this.FindControl<TextBlock>("CookbookRecipeSnippetText")
            ?? throw new InvalidOperationException("CookbookRecipeSnippetText is missing.");
        _interactionQualityText = this.FindControl<TextBlock>("InteractionQualityText")
            ?? throw new InvalidOperationException("InteractionQualityText is missing.");
        _builtInInteractionText = this.FindControl<TextBlock>("BuiltInInteractionText")
            ?? throw new InvalidOperationException("BuiltInInteractionText is missing.");
        _renderingPathText = this.FindControl<TextBlock>("RenderingPathText")
            ?? throw new InvalidOperationException("Rendering path text control is missing.");
        _renderingDiagnosticsText = this.FindControl<TextBlock>("RenderingDiagnosticsText")
            ?? throw new InvalidOperationException("Rendering diagnostics text control is missing.");
        _overlayOptionsText = this.FindControl<TextBlock>("OverlayOptionsText")
            ?? throw new InvalidOperationException("Overlay options text control is missing.");
        _cachePathText = this.FindControl<TextBlock>("CachePathText")
            ?? throw new InvalidOperationException("Cache path text control is missing.");
        _datasetText = this.FindControl<TextBlock>("DatasetText")
            ?? throw new InvalidOperationException("Dataset text control is missing.");
        _copySupportSummaryButton = this.FindControl<Button>("CopySupportSummaryButton")
            ?? throw new InvalidOperationException("CopySupportSummaryButton is missing.");
        _supportSummaryStatusText = this.FindControl<TextBlock>("SupportSummaryStatusText")
            ?? throw new InvalidOperationException("SupportSummaryStatusText is missing.");
        _supportSummaryText = this.FindControl<TextBlock>("SupportSummaryText")
            ?? throw new InvalidOperationException("SupportSummaryText is missing.");

        _cachePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "sample-surface-cache",
            CacheManifestFileName);
        _cachePayloadPath = _cachePath + CachePayloadSuffix;

        _inMemorySource = CreateInMemorySource();
        _analyticsProofSource = CreateAnalyticsProofSource();
        _waterfallSource = CreateWaterfallSource();
        ConfigureSurfaceFamilyChartView(_surfaceChartView);
        ConfigureSurfaceFamilyChartView(_waterfallChartView);
        ConfigureScatterFamilyChartView(_scatterChartView);
        ConfigureSurfaceFamilyChartView(_barChartView);
        ConfigureSurfaceFamilyChartView(_contourPlotView);

        _scatterScenarioSelector.ItemsSource = ScatterStreamingScenarios.All;
        _scatterScenarioSelector.SelectedIndex = 0;
        _cookbookRecipeSelector.ItemsSource = CookbookRecipes.All;
        _cookbookRecipeSelector.SelectedIndex = 0;
        _sourceSelector.SelectedIndex = 0;
        ApplySource(
            _surfaceChartView,
            _inMemorySource,
            "Start here: In-memory first chart",
            "Start here first. Generated at runtime from a dense matrix, built with SurfacePyramidBuilder, and kept as the baseline chart path inside this demo.",
            "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid.",
            "No additional assets are used on this path.");

        _sourceSelector.SelectionChanged += OnSourceSelectionChanged;
        _scatterScenarioSelector.SelectionChanged += OnScatterScenarioSelectionChanged;
        _cookbookRecipeSelector.SelectionChanged += OnCookbookRecipeSelectionChanged;
        _fitToDataButton.Click += OnFitToDataClicked;
        _resetCameraButton.Click += OnResetCameraClicked;
        _copyRecipeSnippetButton.Click += OnCopyRecipeSnippetClicked;
        _copySupportSummaryButton.Click += OnCopySupportSummaryClicked;

        _cachePathText.Text = $"Manifest: {_cachePath}\nPayload sidecar: {_cachePayloadPath}";
        ApplyCookbookRecipe(CookbookRecipes.All[0]);
        RefreshActiveProofTexts();
    }

    private VideraChartView ActiveSurfaceFamilyChartView =>
        _waterfallChartView.IsVisible ? _waterfallChartView :
        _barChartView.IsVisible ? _barChartView :
        _surfaceChartView;

    private bool IsScatterProofActive => _scatterChartView.IsVisible;

    private bool IsContourProofActive => _contourPlotView.IsVisible;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureSurfaceFamilyChartView(VideraChartView chartView)
    {
        chartView.Plot.OverlayOptions = CreateOverlayOptions();
        chartView.RenderStatusChanged += OnRenderStatusChanged;
        chartView.InteractionQualityChanged += OnInteractionQualityChanged;
        chartView.PropertyChanged += OnChartViewPropertyChanged;
    }

    private void ConfigureScatterFamilyChartView(VideraChartView chartView)
    {
        chartView.RenderStatusChanged += OnRenderStatusChanged;
        chartView.InteractionQualityChanged += OnInteractionQualityChanged;
    }

    private async void OnSourceSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        var requestedIndex = _sourceSelector.SelectedIndex;

        if (requestedIndex == 0)
        {
            ApplySource(
                _surfaceChartView,
                _inMemorySource,
                "Start here: In-memory first chart",
                "Start here first. Generated at runtime from a dense matrix, built with SurfacePyramidBuilder, and kept as the baseline chart path inside this demo.",
                "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid.",
                "No additional assets are used on this path.");
            return;
        }

        if (requestedIndex == CacheSourceIndex)
        {
            await LoadAndApplyCacheSourceAsync(requestedIndex).ConfigureAwait(false);
            return;
        }

        if (requestedIndex == AnalyticsSourceIndex)
        {
            ApplySource(
                _surfaceChartView,
                _analyticsProofSource,
                "Try next: Analytics proof",
                "Repo-owned analytics proof on the same Avalonia shell. Uses explicit/non-uniform coordinates with an independent `ColorField`, keeps pinned-probe workflow (`Shift + LeftClick`), and keeps the built-in `ViewState` + `InteractionQuality` camera truth contract.",
                "The analytics proof uses a 19x13 explicit grid with non-uniform axis spacing and separate height and color scalar fields, while preserving the same VideraChartView interaction contracts.",
                "No additional assets are used on this path.");
            return;
        }

        if (requestedIndex == WaterfallSourceIndex)
        {
            ApplySource(
                _waterfallChartView,
                _waterfallSource,
                "Try next: Waterfall proof",
                "Thin proof on the same Avalonia chart shell. Uses explicit strip spacing while keeping the inherited ViewState, interaction, overlay, and rendering-status workflow aligned to the rendered geometry.",
                "The waterfall proof expands each logical strip into baseline-data-baseline rows and assigns explicit sweep coordinates so camera, picking, and overlays stay on the same geometry truth.",
                "No additional assets are used on this path.");
            return;
        }

        if (requestedIndex == ScatterSourceIndex)
        {
            ApplySelectedScatterScenario();
            return;
        }

        if (requestedIndex == BarSourceIndex)
        {
            ApplyBarSource();
            return;
        }

        if (requestedIndex == ContourSourceIndex)
        {
            ApplyContourSource();
            return;
        }

        await LoadAndApplyCacheSourceAsync(requestedIndex).ConfigureAwait(false);
    }

    private void OnScatterScenarioSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (_sourceSelector.SelectedIndex != ScatterSourceIndex)
        {
            return;
        }

        ApplySelectedScatterScenario();
    }

    private void OnCookbookRecipeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_cookbookRecipeSelector.SelectedItem is CookbookRecipe recipe)
        {
            ApplyCookbookRecipe(recipe);
        }
    }

    private void ApplyCookbookRecipe(CookbookRecipe recipe)
    {
        _cookbookRecipeStatusText.Text = recipe.Description;
        _cookbookRecipeSnippetText.Text = recipe.Snippet;

        if (recipe.ScatterScenarioId is { } scenarioId)
        {
            _scatterScenarioSelector.SelectedItem = ScatterStreamingScenarios.Get(scenarioId);
        }

        if (_sourceSelector.SelectedIndex != recipe.SourceIndex)
        {
            _sourceSelector.SelectedIndex = recipe.SourceIndex;
        }
    }

    private void ApplySelectedScatterScenario()
    {
        var scenario = GetSelectedScatterScenario();

        ApplyScatterSource(
            CreateScatterSource(scenario),
            "Try next: Scatter streaming proof",
            $"Repo-owned scatter proof on the same Avalonia chart line. Scenario `{scenario.Id}` uses {scenario.UpdateMode} columnar streaming with direct camera pose truth and no `ViewState` seam on this path.",
            $"The scatter proof uses scenario `{scenario.Id}`: {scenario.InitialPointCount:N0} initial points, {scenario.UpdatePointCount:N0} update points, FIFO capacity {SurfaceDemoSupportSummary.FormatFifoCapacity(scenario.FifoCapacity)}, Pickable {scenario.Pickable}.",
            "No additional assets are used on this path.");
    }

    private ScatterStreamingScenario GetSelectedScatterScenario()
    {
        return _scatterScenarioSelector.SelectedItem as ScatterStreamingScenario
            ?? ScatterStreamingScenarios.Get("scatter-replace-100k");
    }

    private async Task LoadAndApplyCacheSourceAsync(int requestedIndex)
    {
        try
        {
            var cacheSource = await GetOrCreateCacheSourceAsync().ConfigureAwait(true);
            if (_sourceSelector.SelectedIndex != requestedIndex)
            {
                return;
            }

            _lastCacheLoadFailureMessage = null;
            ApplySource(
                _surfaceChartView,
                cacheSource,
                "Explore next: Cache-backed streaming",
                $"Advanced follow-up after the first chart renders. Loads manifest metadata from {_cachePath} and uses lazy viewport tile streaming from {_cachePayloadPath}.",
                "The cache-backed path reads a committed manifest plus binary sidecar and only requests the tiles needed for the current view.",
                $"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}");
        }
        catch (Exception exception)
        {
            if (_sourceSelector.SelectedIndex != requestedIndex)
            {
                return;
            }

            _lastCacheLoadFailureMessage = $"{exception.GetType().Name}: {exception.Message}";
            ApplyCacheLoadFailure(exception);
        }
    }

    private void ApplyCacheLoadFailure(Exception exception)
    {
        _activePlotPathHeading = "Explore next: Cache-backed streaming";
        _activePlotPathDetails = $"Cache-backed streaming failed to load: {exception.Message}. No Plot path switch was performed.";
        _activeDatasetSummary = "Cache-backed data path unavailable. The previous chart Plot path remains active and there was no scenario/data-path fallback.";
        _activeAssetSummary = $"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}";
        _datasetText.Text = _activeDatasetSummary;
        RefreshActiveProofTexts();
    }

    private void ApplySource(
        VideraChartView chartView,
        ISurfaceTileSource source,
        string heading,
        string details,
        string datasetSummary,
        string assetSummary)
    {
        SetActiveChartView(chartView);
        _activeScatterData = null;
        chartView.Plot.Clear();
        if (ReferenceEquals(chartView, _waterfallChartView))
        {
            chartView.Plot.Add.Waterfall(source, heading);
        }
        else
        {
            chartView.Plot.Add.Surface(source, heading);
        }

        chartView.Plot.ColorMap = CreateColorMap(source.Metadata.ValueRange);
        chartView.FitToData();
        _activePlotPathHeading = heading;
        _activePlotPathDetails = details;
        _activeDatasetSummary = datasetSummary;
        _activeAssetSummary = assetSummary;
        _datasetText.Text = datasetSummary;
        RefreshActiveProofTexts();
    }

    private void ApplyScatterSource(
        ScatterChartData source,
        string heading,
        string details,
        string datasetSummary,
        string assetSummary)
    {
        SetActiveChartView(_scatterChartView);
        _activeScatterData = source;
        _scatterChartView.Plot.Clear();
        _scatterChartView.Plot.Add.Scatter(source, heading);
        _scatterChartView.FitToData();
        _activePlotPathHeading = heading;
        _activePlotPathDetails = details;
        _activeDatasetSummary = datasetSummary;
        _activeAssetSummary = assetSummary;
        _datasetText.Text = datasetSummary;
        RefreshActiveProofTexts();
    }

    private void ApplyBarSource()
    {
        var data = CreateSampleBarData();
        SetActiveChartView(_barChartView);
        _activeScatterData = null;
        _barChartView.Plot.Clear();
        _barChartView.Plot.Add.Bar(data, "Try next: Bar chart proof");
        _barChartView.FitToData();
        _activePlotPathHeading = "Try next: Bar chart proof";
        _activePlotPathDetails = "Grouped bar chart with 3 series and 5 categories. Demonstrates Plot.Add.Bar with configurable per-series colors.";
        _activeDatasetSummary = "Bar chart proof uses BarChartData with 3 BarSeries of 5 values each in Grouped layout.";
        _activeAssetSummary = "No additional assets are used on this path.";
        _datasetText.Text = _activeDatasetSummary;
        RefreshActiveProofTexts();
    }

    private void ApplyContourSource()
    {
        var field = CreateSampleContourField();
        SetActiveChartView(_contourPlotView);
        _activeScatterData = null;
        _contourPlotView.Plot.Clear();
        _contourPlotView.Plot.Add.Contour(field, "Try next: Contour plot proof");
        _contourPlotView.FitToData();
        _activePlotPathHeading = "Try next: Contour plot proof";
        _activePlotPathDetails = "Contour plot with marching squares iso-line extraction from a radial scalar field. Demonstrates Plot.Add.Contour.";
        _activeDatasetSummary = "Contour plot proof uses a 32x32 radial scalar field with 10 default contour levels.";
        _activeAssetSummary = "No additional assets are used on this path.";
        _datasetText.Text = _activeDatasetSummary;
        RefreshActiveProofTexts();
    }

    private void SetActiveChartView(VideraChartView chartView)
    {
        _surfaceChartView.IsVisible = ReferenceEquals(chartView, _surfaceChartView);
        _waterfallChartView.IsVisible = ReferenceEquals(chartView, _waterfallChartView);
        _scatterChartView.IsVisible = ReferenceEquals(chartView, _scatterChartView);
        _barChartView.IsVisible = ReferenceEquals(chartView, _barChartView);
        _contourPlotView.IsVisible = ReferenceEquals(chartView, _contourPlotView);
    }

    private void OnRenderStatusChanged(object? sender, EventArgs e)
    {
        _ = e;
        if (!ReferenceEquals(sender, ActiveSurfaceFamilyChartView) &&
            !ReferenceEquals(sender, _scatterChartView) &&
            !ReferenceEquals(sender, _contourPlotView))
        {
            return;
        }

        RefreshActiveProofTexts();
    }

    private void OnInteractionQualityChanged(object? sender, EventArgs e)
    {
        _ = e;
        if (!ReferenceEquals(sender, ActiveSurfaceFamilyChartView) &&
            !ReferenceEquals(sender, _scatterChartView) &&
            !ReferenceEquals(sender, _contourPlotView))
        {
            return;
        }

        UpdateInteractionQualityText();
    }

    private void OnChartViewPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!ReferenceEquals(sender, ActiveSurfaceFamilyChartView) &&
            !ReferenceEquals(sender, _contourPlotView) &&
            !ReferenceEquals(sender, _barChartView) ||
            e.Property != VideraChartView.ViewStateProperty)
        {
            return;
        }

        RefreshActiveProofTexts();
    }

    private void OnFitToDataClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (IsScatterProofActive)
        {
            _scatterChartView.FitToData();
        }
        else if (IsContourProofActive)
        {
            _contourPlotView.FitToData();
        }
        else if (_barChartView.IsVisible)
        {
            _barChartView.FitToData();
        }
        else
        {
            ActiveSurfaceFamilyChartView.FitToData();
        }

        RefreshActiveProofTexts();
    }

    private void OnResetCameraClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (IsScatterProofActive)
        {
            _scatterChartView.ResetCamera();
        }
        else if (IsContourProofActive)
        {
            _contourPlotView.ResetCamera();
        }
        else if (_barChartView.IsVisible)
        {
            _barChartView.ResetCamera();
        }
        else
        {
            ActiveSurfaceFamilyChartView.ResetCamera();
        }

        RefreshActiveProofTexts();
    }

    private async void OnCopySupportSummaryClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        UpdateSupportSummaryText();

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(_supportSummaryText.Text ?? string.Empty).ConfigureAwait(true);
            _supportSummaryStatusText.Text = "Copied support summary to the clipboard.";
            return;
        }

        _supportSummaryStatusText.Text = "Clipboard is unavailable. The support summary remains visible below.";
    }

    private async void OnCopyRecipeSnippetClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(_cookbookRecipeSnippetText.Text ?? string.Empty).ConfigureAwait(true);
            _cookbookRecipeStatusText.Text = "Copied cookbook recipe snippet to the clipboard.";
            return;
        }

        _cookbookRecipeStatusText.Text = "Clipboard is unavailable. The cookbook recipe snippet remains visible below.";
    }

    private async void OnCaptureSnapshotClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        try
        {
            VideraChartView chartView;
            if (IsScatterProofActive)
                chartView = _scatterChartView;
            else if (IsContourProofActive)
                chartView = _contourPlotView;
            else if (_barChartView.IsVisible)
                chartView = _barChartView;
            else
                chartView = ActiveSurfaceFamilyChartView;

            var outputPath = Path.Combine(Path.GetTempPath(), $"videra-surfacecharts-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png");
            var result = await chartView.Plot.SavePngAsync(outputPath, width: 1920, height: 1080).ConfigureAwait(true);
            _lastSnapshotResult = result;

            if (result.Succeeded)
            {
                _statusText.Text = $"PNG saved with Plot.SavePngAsync: {result.Path}";
                _supportSummaryStatusText.Text = $"PNG saved with Plot.SavePngAsync: {result.Path}";
            }
            else
            {
                _statusText.Text = $"Snapshot failed: {result.Failure?.Message}";
                _supportSummaryStatusText.Text = $"Snapshot failed: {result.Failure?.Message}";
            }
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Snapshot failed: {ex.Message}";
            _supportSummaryStatusText.Text = $"Snapshot failed: {ex.Message}";
        }
    }

    private void UpdateViewStateText()
    {
        _viewStateText.Text = $"ViewState / camera state\n{CreateViewStateSummary()}";
    }

    private void UpdateInteractionQualityText()
    {
        if (IsScatterProofActive)
        {
            var scatter = _activeScatterData;
            _interactionQualityText.Text =
                $"Current mode: {_scatterChartView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {scatter?.PointCount ?? 0} scatter points.";
            return;
        }

        if (IsContourProofActive)
        {
            var contourStatus = _contourPlotView.ContourRenderingStatus;
            _interactionQualityText.Text =
                $"Current mode: {_contourPlotView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {contourStatus.ExtractedLineCount} contour lines.";
            return;
        }

        if (_barChartView.IsVisible)
        {
            var barStatus = _barChartView.BarRenderingStatus;
            _interactionQualityText.Text =
                $"Current mode: {_barChartView.InteractionQuality}\n" +
                "Interactive: pointer navigation is active on the unified plot path.\n" +
                $"Refine: settled plot is ready for {barStatus.BarCount} bars.";
            return;
        }

        _interactionQualityText.Text =
            $"Current mode: {ActiveSurfaceFamilyChartView.InteractionQuality}\n" +
            "Interactive: lighter requests while orbit, pan, dolly, or focus input is in flight.\n" +
            "Refine: full settled requests for the current view.";
    }

    private void UpdateStatusText()
    {
        if (IsScatterProofActive)
        {
            var scatter = _activeScatterData;
            _statusText.Text =
                $"{_activePlotPathHeading}\n" +
                $"{_activePlotPathDetails}\n" +
                "Scatter proof is authored through VideraChartView.Plot.Add.Scatter.\n" +
                $"Current scene: {scatter?.SeriesCount ?? 0} series, {scatter?.PointCount ?? 0} points.\n" +
                $"Columnar series: {scatter?.ColumnarSeriesCount ?? 0}; Retained columnar points: {scatter?.ColumnarPointCount ?? 0}; Pickable points: {scatter?.PickablePointCount ?? 0}.\n" +
                $"Streaming appends: {scatter?.StreamingAppendBatchCount ?? 0}; FIFO capacity: {SurfaceDemoSupportSummary.FormatFifoCapacity(scatter?.ConfiguredFifoCapacity)}; Dropped points: {scatter?.StreamingDroppedPointCount ?? 0}.";
            return;
        }

        if (IsContourProofActive)
        {
            var contourStatus = _contourPlotView.ContourRenderingStatus;
            _statusText.Text =
                $"{_activePlotPathHeading}\n" +
                $"{_activePlotPathDetails}\n" +
                "Contour proof is authored through VideraChartView.Plot.Add.Contour.\n" +
                $"Current scene: {contourStatus.LevelCount} levels, {contourStatus.ExtractedLineCount} lines, {contourStatus.TotalSegmentCount} segments.";
            return;
        }

        if (_barChartView.IsVisible)
        {
            var barStatus = _barChartView.BarRenderingStatus;
            _statusText.Text =
                $"{_activePlotPathHeading}\n" +
                $"{_activePlotPathDetails}\n" +
                "Bar proof is authored through VideraChartView.Plot.Add.Bar.\n" +
                $"Current scene: {barStatus.SeriesCount} series, {barStatus.CategoryCount} categories, {barStatus.BarCount} bars, layout {barStatus.Layout}.";
            return;
        }

        var dataWindow = ActiveSurfaceFamilyChartView.ViewState.DataWindow;
        _statusText.Text =
            $"{_activePlotPathHeading}\n" +
            $"{_activePlotPathDetails}\n" +
            "First-chart navigation: Left drag orbit, Right drag pan, Wheel dolly, Ctrl + Left drag focus zoom.\n" +
            $"Current window: StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}";
    }

    private void UpdateRenderingPathText()
    {
        if (IsScatterProofActive)
        {
            var scatter = _activeScatterData;
            _renderingPathText.Text =
                "Plot path: VideraChartView.Plot.Add.Scatter\n" +
                $"Plot revision: {_scatterChartView.Plot.Revision}\n" +
                $"Interaction quality: {_scatterChartView.InteractionQuality}\n" +
                $"Series: {scatter?.SeriesCount ?? 0}; Points: {scatter?.PointCount ?? 0}\n" +
                $"Columnar series: {scatter?.ColumnarSeriesCount ?? 0}; Retained columnar points: {scatter?.ColumnarPointCount ?? 0}; Pickable points: {scatter?.PickablePointCount ?? 0}\n" +
                $"Streaming appends: {scatter?.StreamingAppendBatchCount ?? 0}; Replacements: {scatter?.StreamingReplaceBatchCount ?? 0}; FIFO capacity: {SurfaceDemoSupportSummary.FormatFifoCapacity(scatter?.ConfiguredFifoCapacity)}; Dropped points: {scatter?.StreamingDroppedPointCount ?? 0} (last {scatter?.LastStreamingDroppedPointCount ?? 0})";
            return;
        }

        if (IsContourProofActive)
        {
            var contourStatus = _contourPlotView.ContourRenderingStatus;
            _renderingPathText.Text =
                "Plot path: VideraChartView.Plot.Add.Contour\n" +
                $"Plot revision: {_contourPlotView.Plot.Revision}\n" +
                $"Interaction quality: {_contourPlotView.InteractionQuality}\n" +
                $"Has source: {contourStatus.HasSource}; Is ready: {contourStatus.IsReady}\n" +
                $"Levels: {contourStatus.LevelCount}; Lines: {contourStatus.ExtractedLineCount}; Segments: {contourStatus.TotalSegmentCount}";
            return;
        }

        if (_barChartView.IsVisible)
        {
            var barStatus = _barChartView.BarRenderingStatus;
            _renderingPathText.Text =
                "Plot path: VideraChartView.Plot.Add.Bar\n" +
                $"Plot revision: {_barChartView.Plot.Revision}\n" +
                $"Interaction quality: {_barChartView.InteractionQuality}\n" +
                $"Has source: {barStatus.HasSource}; Is ready: {barStatus.IsReady}\n" +
                $"Series: {barStatus.SeriesCount}; Categories: {barStatus.CategoryCount}; Bars: {barStatus.BarCount}; Layout: {barStatus.Layout}";
            return;
        }

        var surfaceStatus = ActiveSurfaceFamilyChartView.RenderingStatus;
        _renderingPathText.Text =
            $"Active backend: {surfaceStatus.ActiveBackend}\n" +
            $"Ready: {surfaceStatus.IsReady}\n" +
            $"Fallback: {SurfaceDemoSupportSummary.CreateFallbackText(surfaceStatus)}\n" +
            $"Host path: {SurfaceDemoSupportSummary.CreateHostText(surfaceStatus)}\n" +
            $"Resident tiles: {surfaceStatus.ResidentTileCount}";
    }

    private void UpdateRenderingDiagnosticsText()
    {
        if (IsScatterProofActive)
        {
            _renderingDiagnosticsText.Text = SurfaceDemoSupportSummary.CreateScatterRenderingDiagnosticsSummary(_activeScatterData, _scatterChartView);
            return;
        }

        if (IsContourProofActive)
        {
            var contourStatus = _contourPlotView.ContourRenderingStatus;
            _renderingDiagnosticsText.Text =
                $"HasSource: {contourStatus.HasSource}\n" +
                $"IsReady: {contourStatus.IsReady}\n" +
                $"LevelCount: {contourStatus.LevelCount}\n" +
                $"ExtractedLineCount: {contourStatus.ExtractedLineCount}\n" +
                $"TotalSegmentCount: {contourStatus.TotalSegmentCount}";
            return;
        }

        if (_barChartView.IsVisible)
        {
            var barStatus = _barChartView.BarRenderingStatus;
            _renderingDiagnosticsText.Text =
                $"HasSource: {barStatus.HasSource}\n" +
                $"IsReady: {barStatus.IsReady}\n" +
                $"BackendKind: {barStatus.BackendKind}\n" +
                $"SeriesCount: {barStatus.SeriesCount}\n" +
                $"CategoryCount: {barStatus.CategoryCount}\n" +
                $"BarCount: {barStatus.BarCount}\n" +
                $"Layout: {barStatus.Layout}";
            return;
        }

        _renderingDiagnosticsText.Text = SurfaceDemoSupportSummary.CreateSurfaceRenderingDiagnosticsSummary(ActiveSurfaceFamilyChartView.RenderingStatus);
    }

    private void UpdateOverlayOptionsText()
    {
        if (IsScatterProofActive)
        {
            _overlayOptionsText.Text =
                "VideraChartView.Plot exposes `OverlayOptions`.\n" +
                "This proof path stays direct-scatter only; Plot-level presentation is shared API but scatter overlay rendering is not widened in this demo.";
            return;
        }

        if (IsContourProofActive)
        {
            _overlayOptionsText.Text =
                "VideraChartView.Plot exposes `OverlayOptions`.\n" +
                "This proof path stays direct-contour only; Plot-level presentation is shared API but contour overlay rendering is not widened in this demo.";
            return;
        }

        var overlayOptions = ActiveSurfaceFamilyChartView.Plot.OverlayOptions;
        _overlayOptionsText.Text =
            "Chart-local `OverlayOptions` keep formatter, minor ticks, grid plane, and axis-side behavior inside `VideraChartView` instead of pushing chart semantics into `VideraView`.\n" +
            $"Minor ticks: {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions})\n" +
            $"Grid plane: {overlayOptions.GridPlane}\n" +
            $"Axis side: {overlayOptions.AxisSideMode}\n" +
            "Formatter: legend and axis labels share the same chart-local numeric formatting contract.";
    }

    private void RefreshActiveProofTexts()
    {
        UpdateBuiltInInteractionText();
        UpdateViewStateText();
        UpdateInteractionQualityText();
        UpdateRenderingPathText();
        UpdateRenderingDiagnosticsText();
        UpdateOverlayOptionsText();
        UpdateStatusText();
        UpdateSupportSummaryText();
    }

    private void UpdateBuiltInInteractionText()
    {
        _builtInInteractionText.Text = IsScatterProofActive
            ? "Left drag orbit. Wheel dolly. Scatter proof does not expose right-drag pan or Ctrl + Left drag focus zoom."
            : IsContourProofActive
            ? "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom. Contour proof shares the same interaction contract."
            : "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom.";
    }

    private static ISurfaceTileSource CreateInMemorySource()
    {
        var matrix = CreateSampleMatrix();
        return new SurfacePyramidBuilder(32, 32).Build(matrix);
    }

    private static ISurfaceTileSource CreateAnalyticsProofSource()
    {
        var matrix = CreateAnalyticsProofMatrix();
        return new InMemorySurfaceTileSource(
            matrix,
            matrix.Metadata,
            [new SurfacePyramidLevel(0, 0, matrix)],
            maxTileWidth: 32,
            maxTileHeight: 32,
            detailLevelX: 0,
            detailLevelY: 0,
            reductionKernel: new ManagedSurfaceTileReductionKernel());
    }

    private static ISurfaceTileSource CreateWaterfallSource()
    {
        var matrix = CreateWaterfallMatrix();
        return new InMemorySurfaceTileSource(
            matrix,
            matrix.Metadata,
            [new SurfacePyramidLevel(0, 0, matrix)],
            maxTileWidth: 32,
            maxTileHeight: 32,
            detailLevelX: 0,
            detailLevelY: 0,
            reductionKernel: new ManagedSurfaceTileReductionKernel());
    }

    private static BarChartData CreateSampleBarData()
    {
        return new BarChartData(
        [
            new BarSeries([12.0, 19.0, 3.0, 5.0, 8.0], 0xFF38BDF8u, "Series A"),
            new BarSeries([7.0, 11.0, 15.0, 8.0, 13.0], 0xFFF97316u, "Series B"),
            new BarSeries([5.0, 9.0, 12.0, 18.0, 6.0], 0xFF2DD4BFu, "Series C"),
        ]);
    }

    private static ContourChartData CreateSampleContourField()
    {
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
        return new ContourChartData(field);
    }

    private static SurfaceMatrix CreateAnalyticsProofMatrix()
    {
        const int width = 19;
        const int height = 13;
        var values = new float[width * height];
        var colorValues = new float[width * height];
        var horizontalCoordinates = new double[]
        {
            0.0d,
            0.14d,
            0.33d,
            0.54d,
            0.88d,
            1.19d,
            1.55d,
            1.96d,
            2.41d,
            2.96d,
            3.34d,
            3.88d,
            4.31d,
            4.91d,
            5.45d,
            6.05d,
            6.74d,
            7.68d,
            8.52d,
        };

        var verticalCoordinates = new double[]
        {
            -1.1d,
            -0.68d,
            -0.22d,
            0.09d,
            0.35d,
            0.66d,
            1.02d,
            1.41d,
            1.85d,
            2.31d,
            2.92d,
            3.48d,
            4.14d,
        };

        var valueIndex = 0;
        for (var y = 0; y < height; y++)
        {
            var axisY = verticalCoordinates[y];
            for (var x = 0; x < width; x++)
            {
                var axisX = horizontalCoordinates[x];
                var heightValue = Math.Tanh(
                    Math.Sin((axisX * 0.72d) + (axisY * 0.63d)) +
                    (0.48d * Math.Cos((axisX * 0.51d) - (axisY * 0.78d))));
                var colorValue = Math.Tanh(
                    (0.72d * Math.Cos((axisX * 0.38d) + (axisY * 0.27d))) +
                    (0.33d * Math.Sin((axisX * 0.14d) + (axisY * 1.08d))));

                values[valueIndex] = (float)heightValue;
                colorValues[valueIndex] = (float)colorValue;
                valueIndex++;
            }
        }

        var metadata = new SurfaceMetadata(
            new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates),
            new SurfaceAxisDescriptor("Distance", "km", horizontalCoordinates[0], horizontalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Depth", "m", verticalCoordinates[0], verticalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(-1d, 1d));

        var heightField = new SurfaceScalarField(width, height, values, metadata.ValueRange);
        var colorField = new SurfaceScalarField(width, height, colorValues, metadata.ValueRange);

        return new SurfaceMatrix(
            metadata,
            heightField,
            colorField);
    }

    private static ScatterChartData CreateScatterSource(ScatterStreamingScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var xMaximum = (scenario.InitialPointCount + scenario.UpdatePointCount) * 0.01d;
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Horizontal", "u", 0d, xMaximum),
            new SurfaceAxisDescriptor("Depth", "u", -20d, 20d),
            new SurfaceValueRange(-20d, 20d));

        var series = new[]
        {
            new ScatterSeries(
                new[]
                {
                    new ScatterPoint(1.2d, 2.1d, 1.0d, 0xFF5EEAD4u),
                    new ScatterPoint(2.7d, 3.8d, 2.2d),
                    new ScatterPoint(4.4d, 4.6d, 3.1d),
                    new ScatterPoint(5.8d, 5.0d, 4.0d),
                },
                0xFF38BDF8u,
                "Signal"),
            new ScatterSeries(
                new[]
                {
                    new ScatterPoint(7.1d, 7.8d, 5.6d, 0xFFF59E0Bu),
                    new ScatterPoint(8.4d, 8.6d, 6.8d),
                    new ScatterPoint(9.6d, 9.2d, 8.1d),
                    new ScatterPoint(10.8d, 11.0d, 9.3d),
                },
                0xFFF97316u,
                "Cluster"),
        };

        return new ScatterChartData(metadata, series, [ScatterStreamingScenarios.CreateSeries(scenario)]);
    }

    private Task<ISurfaceTileSource> GetOrCreateCacheSourceAsync()
    {
        return _cacheSourceTask ??= LoadCacheSourceAsync();
    }

    private async Task<ISurfaceTileSource> LoadCacheSourceAsync()
    {
        var reader = await SurfaceCacheReader.ReadAsync(_cachePath).ConfigureAwait(false);
        return new SurfaceCacheTileSource(reader);
    }

    private static SurfaceMatrix CreateSampleMatrix()
    {
        const int width = 64;
        const int height = 48;
        var values = new float[width * height];
        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        var index = 0;

        for (var y = 0; y < height; y++)
        {
            var normalizedY = (double)y / (height - 1);
            var centeredY = (normalizedY * 2d) - 1d;

            for (var x = 0; x < width; x++)
            {
                var normalizedX = (double)x / (width - 1);
                var centeredX = (normalizedX * 2d) - 1d;

                var ripple = Math.Sin(centeredX * Math.PI * 2.75d) * Math.Cos(centeredY * Math.PI * 2.25d);
                var hill = Math.Exp(-2.8d * ((centeredX * centeredX) + (centeredY * centeredY)));
                var slope = centeredY * 0.35d;
                var value = (ripple * 0.6d) + (hill * 1.45d) - slope;

                values[index++] = (float)value;
                minimum = Math.Min(minimum, value);
                maximum = Math.Max(maximum, value);
            }
        }

        var metadata = new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0d, 180d),
            new SurfaceAxisDescriptor("Frequency", "kHz", 0d, 48d),
            new SurfaceValueRange(minimum, maximum));

        return new SurfaceMatrix(metadata, values);
    }

    private static SurfaceMatrix CreateWaterfallMatrix()
    {
        const int width = 72;
        const int stripCount = 12;
        const int rowsPerStrip = 3;
        const double signalRowOffset = 0.15d;
        const double trailingBaselineOffset = 0.3d;
        var height = stripCount * rowsPerStrip;
        var values = new float[width * height];
        var horizontalCoordinates = new double[width];
        var verticalCoordinates = new double[height];
        var maximum = 0d;

        for (var x = 0; x < width; x++)
        {
            horizontalCoordinates[x] = (180d * x) / (width - 1d);
        }

        for (var strip = 0; strip < stripCount; strip++)
        {
            var baselineTop = strip * rowsPerStrip;
            var signalRow = baselineTop + 1;
            var baselineBottom = baselineTop + 2;
            verticalCoordinates[baselineTop] = strip;
            verticalCoordinates[signalRow] = strip + signalRowOffset;
            verticalCoordinates[baselineBottom] = strip + trailingBaselineOffset;
            var stripWeight = 1d + (strip * 0.045d);
            var hotspot = 0.12d + (strip * 0.055d);

            for (var x = 0; x < width; x++)
            {
                var normalizedX = (double)x / (width - 1);
                var wave = Math.Sin((normalizedX * Math.PI * (2.4d + (strip * 0.08d))) + (strip * 0.35d));
                var harmonic = Math.Cos((normalizedX * Math.PI * 5.6d) - (strip * 0.18d)) * 0.18d;
                var spike = Math.Exp(-44d * Math.Pow(normalizedX - hotspot, 2d));
                var envelope = 0.92d - (normalizedX * 0.25d);
                var value = Math.Max(0d, ((wave * 0.48d) + harmonic + (spike * 1.15d)) * stripWeight * envelope);

                values[(baselineTop * width) + x] = 0f;
                values[(signalRow * width) + x] = (float)value;
                values[(baselineBottom * width) + x] = 0f;
                maximum = Math.Max(maximum, value);
            }
        }

        var geometry = new SurfaceExplicitGrid(horizontalCoordinates, verticalCoordinates);
        var metadata = new SurfaceMetadata(
            geometry,
            new SurfaceAxisDescriptor("Time", "s", horizontalCoordinates[0], horizontalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceAxisDescriptor("Sweep", unit: null, minimum: verticalCoordinates[0], maximum: verticalCoordinates[^1], SurfaceAxisScaleKind.ExplicitCoordinates),
            new SurfaceValueRange(0d, maximum));

        return new SurfaceMatrix(metadata, values);
    }

    private static SurfaceColorMap CreateColorMap(SurfaceValueRange range)
    {
        return new SurfaceColorMap(
            range,
            SurfaceColorMapPresets.CreateProfessional());
    }

    private static SurfaceChartOverlayOptions CreateOverlayOptions()
    {
        return new SurfaceChartOverlayOptions
        {
            ShowMinorTicks = true,
            MinorTickDivisions = 4,
            GridPlane = SurfaceChartGridPlane.XZ,
            AxisSideMode = SurfaceChartAxisSideMode.Auto,
            LabelFormatter = static (_, value) => value.ToString("0.##", CultureInfo.InvariantCulture),
        };
    }

    private void UpdateSupportSummaryText()
    {
        _supportSummaryText.Text = SurfaceDemoSupportSummary.Create(
            new SurfaceDemoSupportSummaryContext(
                ActiveChartView: GetActiveChartView(),
                ScatterChartView: _scatterChartView,
                ContourPlotView: _contourPlotView,
                IsScatterProofActive: IsScatterProofActive,
                IsContourProofActive: IsContourProofActive,
                ActiveScatterData: _activeScatterData,
                ScatterScenario: GetSelectedScatterScenario(),
                LastSnapshotResult: _lastSnapshotResult,
                CacheLoadFailureMessage: _lastCacheLoadFailureMessage,
                ActivePlotPathHeading: _activePlotPathHeading,
                ActivePlotPathDetails: _activePlotPathDetails,
                ActiveDatasetSummary: _activeDatasetSummary,
                ActiveAssetSummary: _activeAssetSummary,
                ViewStateSummary: CreateViewStateSummary()));
    }

    private VideraChartView GetActiveChartView()
    {
        if (IsScatterProofActive)
        {
            return _scatterChartView;
        }

        if (IsContourProofActive)
        {
            return _contourPlotView;
        }

        if (_barChartView.IsVisible)
        {
            return _barChartView;
        }

        return ActiveSurfaceFamilyChartView;
    }

    private string CreateViewStateSummary()
    {
        if (IsScatterProofActive)
        {
            return SurfaceDemoSupportSummary.CreateScatterCameraSummary(_activeScatterData, _scatterChartView);
        }

        if (IsContourProofActive)
        {
            var contourStatus = _contourPlotView.ContourRenderingStatus;
            return $"Contour: HasSource={contourStatus.HasSource}; Levels={contourStatus.LevelCount}; Lines={contourStatus.ExtractedLineCount}; Segments={contourStatus.TotalSegmentCount}";
        }

        var viewState = ActiveSurfaceFamilyChartView.ViewState;
        var dataWindow = viewState.DataWindow;
        var camera = viewState.Camera;
        return
            $"Data window StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}; " +
            $"Camera target ({camera.Target.X:0.###}, {camera.Target.Y:0.###}, {camera.Target.Z:0.###}), Yaw {camera.YawDegrees:0.###}, Pitch {camera.PitchDegrees:0.###}, Distance {camera.Distance:0.###}";
    }
}
