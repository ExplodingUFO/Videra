using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls.Interaction;
using Videra.SurfaceCharts.Avalonia.Controls.Workspace;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Demo.Services;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow : Window
{
    private const string CacheManifestFileName = "sample.surfacecache.json";
    private const string CachePayloadSuffix = ".bin";

    private readonly VideraChartView _surfaceChartView;
    private readonly VideraChartView _waterfallChartView;
    private readonly VideraChartView _scatterChartView;
    private readonly VideraChartView _barChartView;
    private readonly VideraChartView _contourPlotView;
    private readonly VideraChartView _linePlotView;
    private readonly VideraChartView _ribbonPlotView;
    private readonly VideraChartView _vectorFieldPlotView;
    private readonly VideraChartView _heatmapSlicePlotView;
    private readonly VideraChartView _boxPlotView;
    private readonly VideraChartView _histogramPlotView;
    private readonly VideraChartView _functionPlotView;
    private readonly VideraChartView _piePlotView;
    private readonly VideraChartView _ohlcPlotView;
    private readonly VideraChartView _violinPlotView;
    private readonly VideraChartView _polygonPlotView;
    private readonly VideraChartView _workspaceChartA;
    private readonly VideraChartView _workspaceChartB;
    private readonly VideraChartView _workspaceChartC;
    private readonly VideraChartView _workspaceChartD;
    private readonly Grid _analysisWorkspacePanel;
    private readonly Grid _multiPlot3DPanel;
    private MultiPlot3D? _activeMultiPlot3D;
    private readonly Border _workspaceToolbarPanel;
    private readonly TextBlock _workspaceStatusText;
    private readonly Button _copyWorkspaceEvidenceButton;
    private SurfaceChartWorkspaceService? _workspaceService;
    private SurfaceChartLinkGroup? _activeLinkGroup;
    private SurfaceChartInteractionPropagator? _activePropagator;
    private readonly ComboBox _sourceSelector;
    private readonly Grid _scatterScenarioPanel;
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

    private readonly RecipeParameterController _parameterController;
    private readonly VideraChartView[] _allChartViews;
    private RecipeContext? _recipeContext;

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
        _linePlotView = this.FindControl<VideraChartView>("LinePlotView")
            ?? throw new InvalidOperationException("LinePlotView is missing.");
        _ribbonPlotView = this.FindControl<VideraChartView>("RibbonPlotView")
            ?? throw new InvalidOperationException("RibbonPlotView is missing.");
        _vectorFieldPlotView = this.FindControl<VideraChartView>("VectorFieldPlotView")
            ?? throw new InvalidOperationException("VectorFieldPlotView is missing.");
        _heatmapSlicePlotView = this.FindControl<VideraChartView>("HeatmapSlicePlotView")
            ?? throw new InvalidOperationException("HeatmapSlicePlotView is missing.");
        _boxPlotView = this.FindControl<VideraChartView>("BoxPlotView")
            ?? throw new InvalidOperationException("BoxPlotView is missing.");
        _histogramPlotView = this.FindControl<VideraChartView>("HistogramPlotView")
            ?? throw new InvalidOperationException("HistogramPlotView is missing.");
        _functionPlotView = this.FindControl<VideraChartView>("FunctionPlotView")
            ?? throw new InvalidOperationException("FunctionPlotView is missing.");
        _piePlotView = this.FindControl<VideraChartView>("PiePlotView")
            ?? throw new InvalidOperationException("PiePlotView is missing.");
        _ohlcPlotView = this.FindControl<VideraChartView>("OHLCPlotView")
            ?? throw new InvalidOperationException("OHLCPlotView is missing.");
        _violinPlotView = this.FindControl<VideraChartView>("ViolinPlotView")
            ?? throw new InvalidOperationException("ViolinPlotView is missing.");
        _polygonPlotView = this.FindControl<VideraChartView>("PolygonPlotView")
            ?? throw new InvalidOperationException("PolygonPlotView is missing.");
        _workspaceChartA = this.FindControl<VideraChartView>("WorkspaceChartA")
            ?? throw new InvalidOperationException("WorkspaceChartA is missing.");
        _workspaceChartB = this.FindControl<VideraChartView>("WorkspaceChartB")
            ?? throw new InvalidOperationException("WorkspaceChartB is missing.");
        _workspaceChartC = this.FindControl<VideraChartView>("WorkspaceChartC")
            ?? throw new InvalidOperationException("WorkspaceChartC is missing.");
        _workspaceChartD = this.FindControl<VideraChartView>("WorkspaceChartD")
            ?? throw new InvalidOperationException("WorkspaceChartD is missing.");
        _analysisWorkspacePanel = this.FindControl<Grid>("AnalysisWorkspacePanel")
            ?? throw new InvalidOperationException("AnalysisWorkspacePanel is missing.");
        _multiPlot3DPanel = this.FindControl<Grid>("MultiPlot3DPanel")
            ?? throw new InvalidOperationException("MultiPlot3DPanel is missing.");
        _workspaceToolbarPanel = this.FindControl<Border>("WorkspaceToolbarPanel")
            ?? throw new InvalidOperationException("WorkspaceToolbarPanel is missing.");
        _workspaceStatusText = this.FindControl<TextBlock>("WorkspaceStatusText")
            ?? throw new InvalidOperationException("WorkspaceStatusText is missing.");
        _copyWorkspaceEvidenceButton = this.FindControl<Button>("CopyWorkspaceEvidenceButton")
            ?? throw new InvalidOperationException("CopyWorkspaceEvidenceButton is missing.");
        _sourceSelector = this.FindControl<ComboBox>("SourceSelector")
            ?? throw new InvalidOperationException("Source selector is missing.");
        _scatterScenarioPanel = this.FindControl<Grid>("ScatterScenarioPanel")
            ?? throw new InvalidOperationException("Scatter scenario panel is missing.");
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
        _parameterController = new RecipeParameterController(
            this.FindControl<Border>("RecipeParameterPanel")
                ?? throw new InvalidOperationException("RecipeParameterPanel is missing."),
            this.FindControl<StackPanel>("LineParamGroup")
                ?? throw new InvalidOperationException("LineParamGroup is missing."),
            this.FindControl<StackPanel>("RibbonParamGroup")
                ?? throw new InvalidOperationException("RibbonParamGroup is missing."),
            this.FindControl<StackPanel>("VectorFieldParamGroup")
                ?? throw new InvalidOperationException("VectorFieldParamGroup is missing."),
            this.FindControl<StackPanel>("HeatmapParamGroup")
                ?? throw new InvalidOperationException("HeatmapParamGroup is missing."),
            this.FindControl<TextBlock>("LineWidthText")
                ?? throw new InvalidOperationException("LineWidthText is missing."),
            this.FindControl<TextBlock>("RibbonRadiusText")
                ?? throw new InvalidOperationException("RibbonRadiusText is missing."),
            this.FindControl<TextBlock>("VectorFieldScaleText")
                ?? throw new InvalidOperationException("VectorFieldScaleText is missing."),
            this.FindControl<TextBlock>("HeatmapPositionText")
                ?? throw new InvalidOperationException("HeatmapPositionText is missing."));

        _allChartViews =
        [
            _surfaceChartView, _waterfallChartView, _scatterChartView, _barChartView,
            _contourPlotView, _linePlotView, _ribbonPlotView, _vectorFieldPlotView,
            _heatmapSlicePlotView, _boxPlotView, _histogramPlotView, _functionPlotView,
            _piePlotView, _ohlcPlotView, _violinPlotView, _polygonPlotView,
        ];

        _cachePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "sample-surface-cache",
            CacheManifestFileName);
        _cachePayloadPath = _cachePath + CachePayloadSuffix;

        _inMemorySource = SampleDataFactory.CreateInMemorySource();
        _analyticsProofSource = SampleDataFactory.CreateAnalyticsProofSource();
        _waterfallSource = SampleDataFactory.CreateWaterfallSource();

        _recipeContext = new RecipeContext(
            _surfaceChartView, _waterfallChartView, _scatterChartView, _barChartView,
            _contourPlotView, _linePlotView, _ribbonPlotView, _vectorFieldPlotView,
            _heatmapSlicePlotView, _boxPlotView, _histogramPlotView, _functionPlotView,
            _piePlotView, _ohlcPlotView, _violinPlotView, _polygonPlotView,
            _inMemorySource, _analyticsProofSource, _waterfallSource,
            SetActiveChartView,
            createColorMap: CreateColorMap,
            multiPlot3DPanel: _multiPlot3DPanel,
            analysisWorkspacePanel: _analysisWorkspacePanel,
            workspaceToolbarPanel: _workspaceToolbarPanel,
            workspaceChartA: _workspaceChartA,
            workspaceChartB: _workspaceChartB,
            workspaceChartC: _workspaceChartC,
            workspaceChartD: _workspaceChartD,
            workspaceStatusText: _workspaceStatusText,
            setWorkspaceService: s => _workspaceService = s,
            setLinkGroup: g => _activeLinkGroup = g,
            setPropagator: p => _activePropagator = p,
            setMultiPlot3D: m => _activeMultiPlot3D = m,
            disposeWorkspaceState: () =>
            {
                _workspaceService?.Dispose();
                _activeLinkGroup?.Dispose();
                _activePropagator?.Dispose();
                _activeLinkGroup = null;
                _activePropagator = null;
                _activeMultiPlot3D?.Dispose();
                _multiPlot3DPanel.Children.Clear();
            },
            hideAllCharts: HideAllCharts);

        ConfigureSurfaceFamilyChartView(_surfaceChartView);
        ConfigureSurfaceFamilyChartView(_waterfallChartView);
        ConfigureScatterFamilyChartView(_scatterChartView);
        ConfigureSurfaceFamilyChartView(_barChartView);
        ConfigureSurfaceFamilyChartView(_contourPlotView);
        ConfigureSurfaceFamilyChartView(_linePlotView);
        ConfigureSurfaceFamilyChartView(_ribbonPlotView);
        ConfigureSurfaceFamilyChartView(_vectorFieldPlotView);
        ConfigureSurfaceFamilyChartView(_heatmapSlicePlotView);
        ConfigureSurfaceFamilyChartView(_boxPlotView);
        ConfigureSurfaceFamilyChartView(_histogramPlotView);
        ConfigureSurfaceFamilyChartView(_functionPlotView);
        ConfigureSurfaceFamilyChartView(_piePlotView);
        ConfigureSurfaceFamilyChartView(_ohlcPlotView);
        ConfigureSurfaceFamilyChartView(_violinPlotView);
        ConfigureSurfaceFamilyChartView(_polygonPlotView);
        ConfigureSurfaceFamilyChartView(_workspaceChartA);
        ConfigureSurfaceFamilyChartView(_workspaceChartB);
        ConfigureSurfaceFamilyChartView(_workspaceChartC);
        ConfigureSurfaceFamilyChartView(_workspaceChartD);

        _sourceSelector.ItemsSource = SurfaceDemoScenarios.All;
        _scatterScenarioSelector.ItemsSource = ScatterStreamingScenarios.All;
        _scatterScenarioSelector.SelectedIndex = 0;
        _cookbookRecipeSelector.ItemsSource = CookbookRecipes.All;
        _cookbookRecipeSelector.SelectedIndex = 0;
        var startScenario = SurfaceDemoScenarios.Get(SurfaceDemoScenarios.StartId);
        _sourceSelector.SelectedItem = startScenario;
        ApplySource(
            _surfaceChartView,
            _inMemorySource,
            startScenario.Label,
            $"Start here first. {startScenario.Description}",
            "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid.",
            "No additional assets are used on this path.");

        _sourceSelector.SelectionChanged += OnSourceSelectionChanged;
        _scatterScenarioSelector.SelectionChanged += OnScatterScenarioSelectionChanged;
        _cookbookRecipeSelector.SelectionChanged += OnCookbookRecipeSelectionChanged;
        _fitToDataButton.Click += OnFitToDataClicked;
        _resetCameraButton.Click += OnResetCameraClicked;
        _copyRecipeSnippetButton.Click += OnCopyRecipeSnippetClicked;
        _copySupportSummaryButton.Click += OnCopySupportSummaryClicked;
        _copyWorkspaceEvidenceButton.Click += OnCopyWorkspaceEvidenceClicked;

        _cachePathText.Text = $"Manifest: {_cachePath}\nPayload sidecar: {_cachePayloadPath}";
        ApplyCookbookRecipe(CookbookRecipes.All[0]);
        RefreshActiveProofTexts();
    }

    private VideraChartView ActiveSurfaceFamilyChartView =>
        _waterfallChartView.IsVisible ? _waterfallChartView :
        _barChartView.IsVisible ? _barChartView :
        _linePlotView.IsVisible ? _linePlotView :
        _ribbonPlotView.IsVisible ? _ribbonPlotView :
        _vectorFieldPlotView.IsVisible ? _vectorFieldPlotView :
        _heatmapSlicePlotView.IsVisible ? _heatmapSlicePlotView :
        _boxPlotView.IsVisible ? _boxPlotView :
        _histogramPlotView.IsVisible ? _histogramPlotView :
        _functionPlotView.IsVisible ? _functionPlotView :
        _piePlotView.IsVisible ? _piePlotView :
        _ohlcPlotView.IsVisible ? _ohlcPlotView :
        _violinPlotView.IsVisible ? _violinPlotView :
        _polygonPlotView.IsVisible ? _polygonPlotView :
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
        if (_sourceSelector.SelectedItem is not SurfaceDemoScenario scenario)
        {
            return;
        }

        _parameterController.UpdateParameterPanel(scenario.Id);

        // Complex scenarios that need special handling
        if (scenario.Id == SurfaceDemoScenarios.CacheId)
        {
            await LoadAndApplyCacheSourceAsync(scenario).ConfigureAwait(false);
            return;
        }

        if (scenario.Id == SurfaceDemoScenarios.ScatterId)
        {
            ApplySelectedScatterScenario(scenario);
            return;
        }

        // Recipe-driven scenarios
        var recipe = RecipeRegistry.Get(scenario.Id);
        if (recipe is not null && _recipeContext is not null)
        {
            _activeScatterData = null;
            var result = recipe.Apply(_recipeContext);
            _activePlotPathHeading = result.Heading;
            _activePlotPathDetails = result.Details;
            _activeDatasetSummary = result.DatasetSummary;
            _activeAssetSummary = result.AssetSummary;
            _datasetText.Text = result.DatasetSummary;
            RefreshActiveProofTexts();
        }
    }

    private void OnScatterScenarioSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _ = sender;
        _ = e;
        if (!IsSelectedScenario(SurfaceDemoScenarios.ScatterId))
        {
            return;
        }

        ApplySelectedScatterScenario(SurfaceDemoScenarios.Get(SurfaceDemoScenarios.ScatterId));
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

        var scenario = SurfaceDemoScenarios.Get(recipe.ScenarioId);
        if (!ReferenceEquals(_sourceSelector.SelectedItem, scenario))
        {
            _sourceSelector.SelectedItem = scenario;
        }
    }

    private void ApplySelectedScatterScenario(SurfaceDemoScenario demoScenario)
    {
        var scenario = GetSelectedScatterScenario();

        ApplyScatterSource(
            SampleDataFactory.CreateScatterSource(scenario),
            demoScenario.Label.Replace("Scatter proof", "Scatter streaming proof", StringComparison.Ordinal),
            $"Repo-owned scatter proof on the same Avalonia chart line. Scenario `{scenario.Id}` uses {scenario.UpdateMode} columnar streaming with direct camera pose truth and no `ViewState` seam on this path.",
            $"The scatter proof uses scenario `{scenario.Id}`: {scenario.InitialPointCount:N0} initial points, {scenario.UpdatePointCount:N0} update points, FIFO capacity {SurfaceDemoSupportSummary.FormatFifoCapacity(scenario.FifoCapacity)}, Pickable {scenario.Pickable}.",
            "No additional assets are used on this path.");
    }

    private ScatterStreamingScenario GetSelectedScatterScenario()
    {
        return _scatterScenarioSelector.SelectedItem as ScatterStreamingScenario
            ?? ScatterStreamingScenarios.Get("scatter-replace-100k");
    }

    private async Task LoadAndApplyCacheSourceAsync(SurfaceDemoScenario scenario)
    {
        try
        {
            var cacheSource = await GetOrCreateCacheSourceAsync().ConfigureAwait(true);
            if (!IsSelectedScenario(scenario.Id))
            {
                return;
            }

            _lastCacheLoadFailureMessage = null;
            ApplySource(
                _surfaceChartView,
                cacheSource,
                scenario.Label,
                $"Advanced follow-up after the first chart renders. Loads manifest metadata from {_cachePath} and uses lazy viewport tile streaming from {_cachePayloadPath}.",
                "The cache-backed path reads a committed manifest plus binary sidecar and only requests the tiles needed for the current view.",
                $"Manifest {_cachePath}; Payload sidecar {_cachePayloadPath}");
        }
        catch (Exception exception)
        {
            if (!IsSelectedScenario(scenario.Id))
            {
                return;
            }

            _lastCacheLoadFailureMessage = $"{exception.GetType().Name}: {exception.Message}";
            ApplyCacheLoadFailure(scenario, exception);
        }
    }

    private void ApplyCacheLoadFailure(SurfaceDemoScenario scenario, Exception exception)
    {
        _activePlotPathHeading = scenario.Label;
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


    private void SetActiveChartView(VideraChartView chartView)
    {
        foreach (var view in _allChartViews) view.IsVisible = ReferenceEquals(view, chartView);
        _analysisWorkspacePanel.IsVisible = false;
        _workspaceToolbarPanel.IsVisible = false;
    }

    private void HideAllCharts()
    {
        foreach (var view in _allChartViews) view.IsVisible = false;
        _analysisWorkspacePanel.IsVisible = false;
        _workspaceToolbarPanel.IsVisible = false;
    }

    private async void OnCopyWorkspaceEvidenceClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_workspaceService is null)
        {
            _workspaceStatusText.Text = "No workspace is active.";
            return;
        }

        var evidence = _workspaceService.GetWorkspaceEvidence();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(evidence).ConfigureAwait(true);
            _workspaceStatusText.Text = "Copied workspace evidence to clipboard.";
            return;
        }

        _workspaceStatusText.Text = "Clipboard is unavailable. Workspace evidence remains in memory.";
    }

    private bool IsSelectedScenario(string scenarioId)
    {
        return _sourceSelector.SelectedItem is SurfaceDemoScenario scenario &&
            string.Equals(scenario.Id, scenarioId, StringComparison.Ordinal);
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

        _interactionQualityText.Text = ChartStatusFormatter.FormatInteractionQualityText(CreateStatusContext());
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
                _supportSummaryStatusText.Text = $"Snapshot state: PNG captured at {result.Path}. The support summary now includes SnapshotStatus and SnapshotPath.";
            }
            else
            {
                _statusText.Text = $"Snapshot failed: {result.Failure?.Message}";
                _supportSummaryStatusText.Text = $"Snapshot state: capture failed. SnapshotStatus remains failed in the support summary.";
            }

            UpdateSupportSummaryText();
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Snapshot failed: {ex.Message}";
            _supportSummaryStatusText.Text = $"Snapshot state: capture failed before a snapshot result was created. {ex.Message}";
            UpdateSupportSummaryText();
        }
    }

    private void RefreshActiveProofTexts()
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
        UpdateSupportSummaryText();
    }

    private void UpdateScatterScenarioSelectorState()
    {
        var isScatterActive = IsScatterProofActive;
        _scatterScenarioPanel.Opacity = isScatterActive ? 1d : 0.45d;
        _scatterScenarioSelector.IsEnabled = isScatterActive;
    }

    private void UpdateBuiltInInteractionText()
    {
        _builtInInteractionText.Text = IsScatterProofActive
            ? "Left drag orbit. Wheel dolly. Scatter proof does not expose right-drag pan or Ctrl + Left drag focus zoom."
            : IsContourProofActive
            ? "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom. Contour proof shares the same interaction contract."
            : "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom.";
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
                ViewStateSummary: ChartStatusFormatter.FormatViewStateSummary(CreateStatusContext())));
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

    private bool IsNewChartTypeActive =>
        _linePlotView.IsVisible || _ribbonPlotView.IsVisible ||
        _vectorFieldPlotView.IsVisible || _heatmapSlicePlotView.IsVisible ||
        _boxPlotView.IsVisible || _histogramPlotView.IsVisible ||
        _functionPlotView.IsVisible || _piePlotView.IsVisible ||
        _ohlcPlotView.IsVisible || _violinPlotView.IsVisible ||
        _polygonPlotView.IsVisible;

    private ChartStatusContext CreateStatusContext()
    {
        return new ChartStatusContext(
            ScatterChartView: _scatterChartView,
            ContourPlotView: _contourPlotView,
            BarChartView: _barChartView,
            ActiveSurfaceFamilyChartView: ActiveSurfaceFamilyChartView,
            IsScatterProofActive: IsScatterProofActive,
            IsContourProofActive: IsContourProofActive,
            ActiveScatterData: _activeScatterData,
            ActivePlotPathHeading: _activePlotPathHeading,
            ActivePlotPathDetails: _activePlotPathDetails);
    }

}
