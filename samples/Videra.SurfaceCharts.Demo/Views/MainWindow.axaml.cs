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

    private readonly VideraChartView
        _surfaceChartView, _waterfallChartView, _scatterChartView, _barChartView,
        _contourPlotView, _linePlotView, _ribbonPlotView, _vectorFieldPlotView,
        _heatmapSlicePlotView, _boxPlotView, _histogramPlotView, _functionPlotView,
        _piePlotView, _ohlcPlotView, _violinPlotView, _polygonPlotView,
        _workspaceChartA, _workspaceChartB, _workspaceChartC, _workspaceChartD;
    private readonly Grid _analysisWorkspacePanel, _multiPlot3DPanel, _scatterScenarioPanel;
    private MultiPlot3D? _activeMultiPlot3D;
    private readonly Border _workspaceToolbarPanel;
    private readonly TextBlock _workspaceStatusText, _statusText, _viewStateText,
        _cookbookRecipeStatusText, _cookbookRecipeSnippetText, _interactionQualityText,
        _builtInInteractionText, _renderingPathText, _renderingDiagnosticsText,
        _overlayOptionsText, _cachePathText, _datasetText, _supportSummaryStatusText,
        _supportSummaryText;
    private readonly Button _copyWorkspaceEvidenceButton, _fitToDataButton, _resetCameraButton,
        _copyRecipeSnippetButton, _copySupportSummaryButton;
    private SurfaceChartWorkspaceService? _workspaceService;
    private SurfaceChartLinkGroup? _activeLinkGroup;
    private SurfaceChartInteractionPropagator? _activePropagator;
    private readonly ComboBox _sourceSelector, _scatterScenarioSelector, _cookbookRecipeSelector;
    private readonly ISurfaceTileSource _inMemorySource, _analyticsProofSource, _waterfallSource;
    private CacheSourceHandler _cacheHandler = null!;
    private ClipboardHandler _clipboardHandler = null!;
    private SupportSummaryService _supportSummaryService = null!;

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

        this.FindControl<Slider>("LineWidthSlider")!.ValueChanged += _parameterController.OnLineWidthChanged;
        this.FindControl<Slider>("RibbonRadiusSlider")!.ValueChanged += _parameterController.OnRibbonRadiusChanged;
        this.FindControl<Slider>("VectorFieldScaleSlider")!.ValueChanged += _parameterController.OnVectorFieldScaleChanged;
        this.FindControl<Slider>("HeatmapPositionSlider")!.ValueChanged += _parameterController.OnHeatmapPositionChanged;

        _allChartViews =
        [
            _surfaceChartView, _waterfallChartView, _scatterChartView, _barChartView,
            _contourPlotView, _linePlotView, _ribbonPlotView, _vectorFieldPlotView,
            _heatmapSlicePlotView, _boxPlotView, _histogramPlotView, _functionPlotView,
            _piePlotView, _ohlcPlotView, _violinPlotView, _polygonPlotView,
        ];

        var cachePath = Path.Combine(
            AppContext.BaseDirectory, "Assets", "sample-surface-cache",
            "sample.surfacecache.json");
        var cachePayloadPath = cachePath + ".bin";
        _cacheHandler = new CacheSourceHandler(cachePath, cachePayloadPath);

        _supportSummaryService = new SupportSummaryService(
            _scatterChartView, _contourPlotView, _barChartView,
            [_linePlotView, _ribbonPlotView, _vectorFieldPlotView, _heatmapSlicePlotView,
             _boxPlotView, _histogramPlotView, _functionPlotView, _piePlotView,
             _ohlcPlotView, _violinPlotView, _polygonPlotView],
            _supportSummaryText,
            () => ActiveSurfaceFamilyChartView,
            _viewStateText, _interactionQualityText, _renderingPathText,
            _renderingDiagnosticsText, _overlayOptionsText, _statusText,
            _builtInInteractionText, _scatterScenarioPanel, _scatterScenarioSelector);

        _clipboardHandler = new ClipboardHandler(
            () => TopLevel.GetTopLevel(this),
            _supportSummaryText, _supportSummaryStatusText,
            _cookbookRecipeSnippetText, _cookbookRecipeStatusText,
            _statusText, _workspaceStatusText,
            _supportSummaryService.GetActiveChartView,
            () => _workspaceService,
            UpdateSupportSummaryText);

        _inMemorySource = SampleDataFactory.CreateInMemorySource();
        _analyticsProofSource = SampleDataFactory.CreateAnalyticsProofSource();
        _waterfallSource = SampleDataFactory.CreateWaterfallSource();
        _recipeContext = new RecipeContext(
            _surfaceChartView, _waterfallChartView, _scatterChartView, _barChartView,
            _contourPlotView, _linePlotView, _ribbonPlotView, _vectorFieldPlotView,
            _heatmapSlicePlotView, _boxPlotView, _histogramPlotView, _functionPlotView,
            _piePlotView, _ohlcPlotView, _violinPlotView, _polygonPlotView,
            _inMemorySource, _analyticsProofSource, _waterfallSource, SetActiveChartView,
            createColorMap: CreateColorMap,
            multiPlot3DPanel: _multiPlot3DPanel, analysisWorkspacePanel: _analysisWorkspacePanel,
            workspaceToolbarPanel: _workspaceToolbarPanel,
            workspaceChartA: _workspaceChartA, workspaceChartB: _workspaceChartB,
            workspaceChartC: _workspaceChartC, workspaceChartD: _workspaceChartD,
            workspaceStatusText: _workspaceStatusText,
            setWorkspaceService: s => _workspaceService = s, setLinkGroup: g => _activeLinkGroup = g,
            setPropagator: p => _activePropagator = p, setMultiPlot3D: m => _activeMultiPlot3D = m,
            disposeWorkspaceState: DisposeWorkspaceState, hideAllCharts: HideAllCharts);

        VideraChartView[] allConfigurable =
            [.. _allChartViews, _workspaceChartA, _workspaceChartB, _workspaceChartC, _workspaceChartD];
        foreach (var view in allConfigurable)
        {
            if (!ReferenceEquals(view, _scatterChartView))
            {
                view.Plot.OverlayOptions = CreateOverlayOptions();
                view.PropertyChanged += OnChartViewPropertyChanged;
            }

            view.RenderStatusChanged += OnRenderStatusChanged;
            view.InteractionQualityChanged += OnInteractionQualityChanged;
        }

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

        _cachePathText.Text = $"Manifest: {cachePath}\nPayload sidecar: {cachePayloadPath}";
        ApplyCookbookRecipe(CookbookRecipes.All[0]);
        RefreshActiveProofTexts();
    }

    private VideraChartView ActiveSurfaceFamilyChartView
    {
        get
        {
            VideraChartView[] candidates =
            [
                _waterfallChartView, _barChartView, _linePlotView, _ribbonPlotView,
                _vectorFieldPlotView, _heatmapSlicePlotView, _boxPlotView,
                _histogramPlotView, _functionPlotView, _piePlotView,
                _ohlcPlotView, _violinPlotView, _polygonPlotView,
            ];
            return Array.Find(candidates, v => v.IsVisible) ?? _surfaceChartView;
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void OnSourceSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_sourceSelector.SelectedItem is not SurfaceDemoScenario scenario) return;
        _parameterController.UpdateParameterPanel(scenario.Id);
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

        var recipe = RecipeRegistry.Get(scenario.Id);
        if (recipe is not null && _recipeContext is not null)
            ApplyRecipeResult(recipe.Apply(_recipeContext));
    }

    private void OnScatterScenarioSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsSelectedScenario(SurfaceDemoScenarios.ScatterId))
            ApplySelectedScatterScenario(SurfaceDemoScenarios.Get(SurfaceDemoScenarios.ScatterId));
    }

    private void OnCookbookRecipeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_cookbookRecipeSelector.SelectedItem is CookbookRecipe recipe)
            ApplyCookbookRecipe(recipe);
    }

    private void ApplyCookbookRecipe(CookbookRecipe recipe)
    {
        _cookbookRecipeStatusText.Text = recipe.Description;
        _cookbookRecipeSnippetText.Text = recipe.Snippet;
        if (recipe.ScatterScenarioId is { } scenarioId)
            _scatterScenarioSelector.SelectedItem = ScatterStreamingScenarios.Get(scenarioId);
        var scenario = SurfaceDemoScenarios.Get(recipe.ScenarioId);
        if (!ReferenceEquals(_sourceSelector.SelectedItem, scenario))
            _sourceSelector.SelectedItem = scenario;
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
        => _scatterScenarioSelector.SelectedItem as ScatterStreamingScenario
            ?? ScatterStreamingScenarios.Get("scatter-replace-100k");

    private Task LoadAndApplyCacheSourceAsync(SurfaceDemoScenario scenario)
        => _cacheHandler.LoadAndApplyCacheSourceAsync(
            scenario, IsSelectedScenario, ApplySource, _surfaceChartView,
            v => _supportSummaryService.ActivePlotPathHeading = v,
            v => _supportSummaryService.ActivePlotPathDetails = v,
            v => _supportSummaryService.ActiveDatasetSummary = v,
            v => _supportSummaryService.ActiveAssetSummary = v,
            v => _datasetText.Text = v,
            RefreshActiveProofTexts);

    private void ApplySource(VideraChartView chartView, ISurfaceTileSource source,
        string heading, string details, string datasetSummary, string assetSummary)
    {
        SetActiveChartView(chartView);
        _supportSummaryService.ActiveScatterData = null;
        chartView.Plot.Clear();
        if (ReferenceEquals(chartView, _waterfallChartView))
            chartView.Plot.Add.Waterfall(source, heading);
        else chartView.Plot.Add.Surface(source, heading);
        chartView.Plot.ColorMap = CreateColorMap(source.Metadata.ValueRange);
        chartView.FitToData();
        ApplySourceState(heading, details, datasetSummary, assetSummary);
    }

    private void ApplyScatterSource(ScatterChartData source,
        string heading, string details, string datasetSummary, string assetSummary)
    {
        SetActiveChartView(_scatterChartView);
        _supportSummaryService.ActiveScatterData = source;
        _scatterChartView.Plot.Clear();
        _scatterChartView.Plot.Add.Scatter(source, heading);
        _scatterChartView.FitToData();
        ApplySourceState(heading, details, datasetSummary, assetSummary);
    }

    private void ApplyRecipeResult(RecipeResult result)
    {
        _supportSummaryService.ActiveScatterData = null;
        _supportSummaryService.ActivePlotPathHeading = result.Heading;
        _supportSummaryService.ActivePlotPathDetails = result.Details;
        _supportSummaryService.ActiveDatasetSummary = result.DatasetSummary;
        _supportSummaryService.ActiveAssetSummary = result.AssetSummary;
        _datasetText.Text = result.DatasetSummary;
        RefreshActiveProofTexts();
    }

    private void ApplySourceState(string heading, string details, string datasetSummary, string assetSummary)
    {
        _supportSummaryService.ActivePlotPathHeading = heading;
        _supportSummaryService.ActivePlotPathDetails = details;
        _supportSummaryService.ActiveDatasetSummary = datasetSummary;
        _supportSummaryService.ActiveAssetSummary = assetSummary;
        _datasetText.Text = datasetSummary;
        RefreshActiveProofTexts();
    }

    private void SetActiveChartView(VideraChartView chartView)
        => SetChartVisibility(chartView);

    private void HideAllCharts()
        => SetChartVisibility(null);

    private void SetChartVisibility(VideraChartView? active)
    {
        foreach (var view in _allChartViews) view.IsVisible = ReferenceEquals(view, active);
        _analysisWorkspacePanel.IsVisible = false;
        _workspaceToolbarPanel.IsVisible = false;
    }

    private void OnCopyWorkspaceEvidenceClicked(object? sender, RoutedEventArgs e)
        => _clipboardHandler.OnCopyWorkspaceEvidenceClicked(sender, e);

    private bool IsSelectedScenario(string scenarioId)
        => _sourceSelector.SelectedItem is SurfaceDemoScenario scenario &&
           string.Equals(scenario.Id, scenarioId, StringComparison.Ordinal);

    private void DisposeWorkspaceState()
    {
        _workspaceService?.Dispose();
        _activeLinkGroup?.Dispose();
        _activePropagator?.Dispose();
        _activeLinkGroup = null;
        _activePropagator = null;
        _activeMultiPlot3D?.Dispose();
        _multiPlot3DPanel.Children.Clear();
    }

    private void OnRenderStatusChanged(object? sender, EventArgs e)
    {
        if (IsPrimaryChartSender(sender)) RefreshActiveProofTexts();
    }

    private void OnInteractionQualityChanged(object? sender, EventArgs e)
    {
        if (IsPrimaryChartSender(sender))
            _interactionQualityText.Text = ChartStatusFormatter.FormatInteractionQualityText(CreateStatusContext());
    }

    private void OnChartViewPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == VideraChartView.ViewStateProperty &&
            (ReferenceEquals(sender, ActiveSurfaceFamilyChartView) ||
             ReferenceEquals(sender, _contourPlotView) ||
             ReferenceEquals(sender, _barChartView)))
            RefreshActiveProofTexts();
    }

    private bool IsPrimaryChartSender(object? sender)
        => ReferenceEquals(sender, ActiveSurfaceFamilyChartView) ||
           ReferenceEquals(sender, _scatterChartView) ||
           ReferenceEquals(sender, _contourPlotView);

    private void OnFitToDataClicked(object? sender, RoutedEventArgs e)
        => PerformOnActiveChart(v => v.FitToData());

    private void OnResetCameraClicked(object? sender, RoutedEventArgs e)
        => PerformOnActiveChart(v => v.ResetCamera());

    private void PerformOnActiveChart(Action<VideraChartView> action)
    {
        if (_scatterChartView.IsVisible) action(_scatterChartView);
        else if (_contourPlotView.IsVisible) action(_contourPlotView);
        else if (_barChartView.IsVisible) action(_barChartView);
        else action(ActiveSurfaceFamilyChartView);
        RefreshActiveProofTexts();
    }

    private void OnCopySupportSummaryClicked(object? sender, RoutedEventArgs e)
        => _clipboardHandler.OnCopySupportSummaryClicked(sender, e);

    private void OnCopyRecipeSnippetClicked(object? sender, RoutedEventArgs e)
        => _clipboardHandler.OnCopyRecipeSnippetClicked(sender, e);

    private void OnCaptureSnapshotClicked(object? sender, RoutedEventArgs e)
        => _clipboardHandler.OnCaptureSnapshotClicked(sender, e);

    private void RefreshActiveProofTexts()
        => _supportSummaryService.RefreshActiveProofTexts(
            GetSelectedScatterScenario(),
            _clipboardHandler.LastSnapshotResult,
            _cacheHandler.LastCacheLoadFailureMessage);

    private static SurfaceColorMap CreateColorMap(SurfaceValueRange range)
        => new(range, SurfaceColorMapPresets.CreateProfessional());

    private static SurfaceChartOverlayOptions CreateOverlayOptions() => new()
    {
        ShowMinorTicks = true,
        MinorTickDivisions = 4,
        GridPlane = SurfaceChartGridPlane.XZ,
        AxisSideMode = SurfaceChartAxisSideMode.Auto,
        LabelFormatter = static (_, value) => value.ToString("0.##", CultureInfo.InvariantCulture),
    };

    private void UpdateSupportSummaryText()
        => _supportSummaryService.UpdateSupportSummaryText(
            GetSelectedScatterScenario(),
            _clipboardHandler.LastSnapshotResult,
            _cacheHandler.LastCacheLoadFailureMessage);

    private ChartStatusContext CreateStatusContext()
        => _supportSummaryService.CreateStatusContext();

}
