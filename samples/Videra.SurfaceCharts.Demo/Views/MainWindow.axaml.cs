using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow : Window
{
    private const string CacheManifestFileName = "sample.surfacecache.json";
    private const string CachePayloadSuffix = ".bin";
    private const int WaterfallSourceIndex = 2;
    private const int ScatterSourceIndex = 3;

    private readonly SurfaceChartView _surfaceChartView;
    private readonly WaterfallChartView _waterfallChartView;
    private readonly ScatterChartView _scatterChartView;
    private readonly ComboBox _sourceSelector;
    private readonly Button _fitToDataButton;
    private readonly Button _resetCameraButton;
    private readonly TextBlock _statusText;
    private readonly TextBlock _viewStateText;
    private readonly TextBlock _interactionQualityText;
    private readonly TextBlock _builtInInteractionText;
    private readonly TextBlock _renderingPathText;
    private readonly TextBlock _overlayOptionsText;
    private readonly TextBlock _cachePathText;
    private readonly TextBlock _datasetText;
    private readonly Button _copySupportSummaryButton;
    private readonly TextBlock _supportSummaryStatusText;
    private readonly TextBlock _supportSummaryText;
    private readonly ISurfaceTileSource _inMemorySource;
    private readonly ISurfaceTileSource _waterfallSource;
    private readonly ScatterChartData _scatterSource;
    private readonly string _cachePath;
    private readonly string _cachePayloadPath;
    private Task<ISurfaceTileSource>? _cacheSourceTask;
    private string _activeSourceHeading = string.Empty;
    private string _activeSourceDetails = string.Empty;
    private string _activeDatasetSummary = string.Empty;
    private string _activeAssetSummary = "No additional assets are used on this path.";

    public MainWindow()
    {
        InitializeComponent();

        _surfaceChartView = this.FindControl<SurfaceChartView>("ChartView")
            ?? throw new InvalidOperationException("Surface chart view is missing.");
        _waterfallChartView = this.FindControl<WaterfallChartView>("WaterfallChartView")
            ?? throw new InvalidOperationException("Waterfall chart view is missing.");
        _scatterChartView = this.FindControl<ScatterChartView>("ScatterChartView")
            ?? throw new InvalidOperationException("ScatterChartView is missing.");
        _sourceSelector = this.FindControl<ComboBox>("SourceSelector")
            ?? throw new InvalidOperationException("Source selector is missing.");
        _fitToDataButton = this.FindControl<Button>("FitToDataButton")
            ?? throw new InvalidOperationException("FitToDataButton is missing.");
        _resetCameraButton = this.FindControl<Button>("ResetCameraButton")
            ?? throw new InvalidOperationException("ResetCameraButton is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("Status text control is missing.");
        _viewStateText = this.FindControl<TextBlock>("ViewStateText")
            ?? throw new InvalidOperationException("ViewStateText is missing.");
        _interactionQualityText = this.FindControl<TextBlock>("InteractionQualityText")
            ?? throw new InvalidOperationException("InteractionQualityText is missing.");
        _builtInInteractionText = this.FindControl<TextBlock>("BuiltInInteractionText")
            ?? throw new InvalidOperationException("BuiltInInteractionText is missing.");
        _renderingPathText = this.FindControl<TextBlock>("RenderingPathText")
            ?? throw new InvalidOperationException("Rendering path text control is missing.");
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
        _waterfallSource = CreateWaterfallSource();
        _scatterSource = CreateScatterSource();

        ConfigureSurfaceChartView(_surfaceChartView);
        ConfigureSurfaceChartView(_waterfallChartView);
        ConfigureScatterChartView(_scatterChartView);

        _sourceSelector.SelectedIndex = 0;
        ApplySource(
            _surfaceChartView,
            _inMemorySource,
            "Start here: In-memory first chart",
            "Start here first. Generated at runtime from a dense matrix, built with SurfacePyramidBuilder, and kept as the baseline chart path inside this demo.",
            "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid.",
            "No additional assets are used on this path.");

        _sourceSelector.SelectionChanged += OnSourceSelectionChanged;
        _fitToDataButton.Click += OnFitToDataClicked;
        _resetCameraButton.Click += OnResetCameraClicked;
        _copySupportSummaryButton.Click += OnCopySupportSummaryClicked;

        _cachePathText.Text = $"Manifest: {_cachePath}\nPayload sidecar: {_cachePayloadPath}";
        RefreshActiveProofTexts();
    }

    private SurfaceChartView ActiveSurfaceChartView => _waterfallChartView.IsVisible ? _waterfallChartView : _surfaceChartView;

    private bool IsScatterProofActive => _scatterChartView.IsVisible;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureSurfaceChartView(SurfaceChartView chartView)
    {
        chartView.OverlayOptions = CreateOverlayOptions();
        chartView.RenderStatusChanged += OnRenderStatusChanged;
        chartView.InteractionQualityChanged += OnInteractionQualityChanged;
        chartView.PropertyChanged += OnChartViewPropertyChanged;
    }

    private void ConfigureScatterChartView(ScatterChartView chartView)
    {
        chartView.RenderStatusChanged += OnRenderStatusChanged;
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
            ApplyScatterSource(
                _scatterSource,
                "Try next: Scatter proof",
                "Repo-owned scatter proof on the same Avalonia chart line. Uses discrete point clouds with direct camera pose truth and no `ViewState` or `OverlayOptions` seam on this path.",
                "The scatter proof builds two small series at startup so the view can report series count, point count, camera target, and camera distance without a cache or surface pyramid.",
                "No additional assets are used on this path.");
            return;
        }

        try
        {
            var cacheSource = await GetOrCreateCacheSourceAsync().ConfigureAwait(true);
            if (_sourceSelector.SelectedIndex != requestedIndex)
            {
                return;
            }

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

            ApplySource(
                _surfaceChartView,
                _inMemorySource,
                "Start here: In-memory first chart",
                $"Start here fallback. Cache-backed streaming failed to load: {exception.Message}",
                "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid.",
                "No additional assets are used on this path.");
            _sourceSelector.SelectedIndex = 0;
        }
    }

    private void ApplySource(
        SurfaceChartView chartView,
        ISurfaceTileSource source,
        string heading,
        string details,
        string datasetSummary,
        string assetSummary)
    {
        SetActiveChartView(chartView);
        chartView.ColorMap = CreateColorMap(source.Metadata.ValueRange);
        chartView.Source = source;
        chartView.FitToData();
        _activeSourceHeading = heading;
        _activeSourceDetails = details;
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
        _scatterChartView.Source = source;
        _scatterChartView.FitToData();
        _activeSourceHeading = heading;
        _activeSourceDetails = details;
        _activeDatasetSummary = datasetSummary;
        _activeAssetSummary = assetSummary;
        _datasetText.Text = datasetSummary;
        RefreshActiveProofTexts();
    }

    private void SetActiveChartView(SurfaceChartView chartView)
    {
        _surfaceChartView.IsVisible = ReferenceEquals(chartView, _surfaceChartView);
        _waterfallChartView.IsVisible = ReferenceEquals(chartView, _waterfallChartView);
        _scatterChartView.IsVisible = false;
    }

    private void SetActiveChartView(ScatterChartView chartView)
    {
        _surfaceChartView.IsVisible = false;
        _waterfallChartView.IsVisible = false;
        _scatterChartView.IsVisible = ReferenceEquals(chartView, _scatterChartView);
    }

    private void OnRenderStatusChanged(object? sender, EventArgs e)
    {
        _ = e;
        if (!ReferenceEquals(sender, ActiveSurfaceChartView) && !ReferenceEquals(sender, _scatterChartView))
        {
            return;
        }

        RefreshActiveProofTexts();
    }

    private void OnInteractionQualityChanged(object? sender, EventArgs e)
    {
        _ = e;
        if (!ReferenceEquals(sender, ActiveSurfaceChartView))
        {
            return;
        }

        UpdateInteractionQualityText();
    }

    private void OnChartViewPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (!ReferenceEquals(sender, ActiveSurfaceChartView) || e.Property != SurfaceChartView.ViewStateProperty)
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
        else
        {
            ActiveSurfaceChartView.FitToData();
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
        else
        {
            ActiveSurfaceChartView.ResetCamera();
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

    private void UpdateViewStateText()
    {
        _viewStateText.Text = $"ViewState / camera state\n{CreateViewStateSummary()}";
    }

    private void UpdateInteractionQualityText()
    {
        if (IsScatterProofActive)
        {
            _interactionQualityText.Text =
                "ScatterChartView does not expose `InteractionQuality`.\n" +
                "Use the render-status panel plus camera pose updates to track the proof path while the pointer is active.";
            return;
        }

        _interactionQualityText.Text =
            $"Current mode: {ActiveSurfaceChartView.InteractionQuality}\n" +
            "Interactive: lighter requests while orbit, pan, dolly, or focus input is in flight.\n" +
            "Refine: full settled requests for the current view.";
    }

    private void UpdateStatusText()
    {
        if (IsScatterProofActive)
        {
            var status = _scatterChartView.RenderingStatus;
            _statusText.Text =
                $"{_activeSourceHeading}\n" +
                $"{_activeSourceDetails}\n" +
                "Scatter proof navigation: Left drag orbit, Wheel dolly.\n" +
                $"Current scene: {status.SeriesCount} series, {status.PointCount} points.\n" +
                $"Camera target ({status.CameraTarget.X:0.###}, {status.CameraTarget.Y:0.###}, {status.CameraTarget.Z:0.###}), distance {status.CameraDistance:0.###}";
            return;
        }

        var dataWindow = ActiveSurfaceChartView.ViewState.DataWindow;
        _statusText.Text =
            $"{_activeSourceHeading}\n" +
            $"{_activeSourceDetails}\n" +
            "First-chart navigation: Left drag orbit, Right drag pan, Wheel dolly, Ctrl + Left drag focus zoom.\n" +
            $"Current window: StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}";
    }

    private void UpdateRenderingPathText()
    {
        if (IsScatterProofActive)
        {
            var status = _scatterChartView.RenderingStatus;
            _renderingPathText.Text =
                $"Backend kind: {status.BackendKind}\n" +
                $"Ready: {status.IsReady}\n" +
                $"Interaction active: {status.IsInteracting}\n" +
                $"View size: {status.ViewSize.Width:0.#} x {status.ViewSize.Height:0.#}\n" +
                $"Series: {status.SeriesCount}; Points: {status.PointCount}\n" +
                $"Camera target: ({status.CameraTarget.X:0.###}, {status.CameraTarget.Y:0.###}, {status.CameraTarget.Z:0.###}); Distance: {status.CameraDistance:0.###}";
            return;
        }

        var surfaceStatus = ActiveSurfaceChartView.RenderingStatus;
        _renderingPathText.Text =
            $"Active backend: {surfaceStatus.ActiveBackend}\n" +
            $"Ready: {surfaceStatus.IsReady}\n" +
            $"Fallback: {CreateFallbackText(surfaceStatus)}\n" +
            $"Host path: {CreateHostText(surfaceStatus)}\n" +
            $"Resident tiles: {surfaceStatus.ResidentTileCount}";
    }

    private void UpdateOverlayOptionsText()
    {
        if (IsScatterProofActive)
        {
            _overlayOptionsText.Text =
                "ScatterChartView does not expose `OverlayOptions`.\n" +
                "This proof path stays direct-scatter only and keeps chart-local overlay configuration out of the scatter host.";
            return;
        }

        var overlayOptions = ActiveSurfaceChartView.OverlayOptions;
        _overlayOptionsText.Text =
            "Chart-local `OverlayOptions` keep formatter, minor ticks, grid plane, and axis-side behavior inside `SurfaceChartView` instead of pushing chart semantics into `VideraView`.\n" +
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
        UpdateOverlayOptionsText();
        UpdateStatusText();
        UpdateSupportSummaryText();
    }

    private void UpdateBuiltInInteractionText()
    {
        _builtInInteractionText.Text = IsScatterProofActive
            ? "Left drag orbit. Wheel dolly. Scatter proof does not expose right-drag pan or Ctrl + Left drag focus zoom."
            : "Left drag orbit. Right drag pan. Wheel dolly. Ctrl + Left drag focus zoom.";
    }

    private static ISurfaceTileSource CreateInMemorySource()
    {
        var matrix = CreateSampleMatrix();
        return new SurfacePyramidBuilder(32, 32).Build(matrix);
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

    private static ScatterChartData CreateScatterSource()
    {
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("Horizontal", "u", 0d, 12d),
            new SurfaceAxisDescriptor("Depth", "u", 0d, 12d),
            new SurfaceValueRange(0d, 14d));

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

        return new ScatterChartData(metadata, series);
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
            new SurfaceColorMapPalette(
                0xFF08111Fu,
                0xFF154C79u,
                0xFF2DD4BFu,
                0xFFFDE68Au,
                0xFFF97316u));
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
        if (IsScatterProofActive)
        {
            var status = _scatterChartView.RenderingStatus;
            _supportSummaryText.Text =
                "SurfaceCharts support summary\n" +
                $"Source path: {_activeSourceHeading}\n" +
                $"Source details: {_activeSourceDetails}\n" +
                $"Chart contract: ScatterChartView exposes direct point data, camera pose truth, Fit to data, and Reset camera on this proof path.\n" +
                $"Camera: {CreateScatterCameraSummary(status)}\n" +
                $"RenderingStatus: BackendKind {status.BackendKind}; IsReady {status.IsReady}; IsInteracting {status.IsInteracting}; SeriesCount {status.SeriesCount}; PointCount {status.PointCount}; ViewSize {status.ViewSize.Width:0.#}x{status.ViewSize.Height:0.#}\n" +
                "InteractionQuality: not exposed on ScatterChartView\n" +
                "OverlayOptions: not exposed on ScatterChartView\n" +
                $"Cache asset: {_activeAssetSummary}\n" +
                $"Dataset: {_activeDatasetSummary}";
            return;
        }

        var surfaceStatus = ActiveSurfaceChartView.RenderingStatus;
        _supportSummaryText.Text =
            "SurfaceCharts support summary\n" +
            $"Source path: {_activeSourceHeading}\n" +
            $"Source details: {_activeSourceDetails}\n" +
            $"ViewState: {CreateViewStateSummary()}\n" +
            $"InteractionQuality: {ActiveSurfaceChartView.InteractionQuality}\n" +
            $"RenderingStatus: ActiveBackend {surfaceStatus.ActiveBackend}; IsReady {surfaceStatus.IsReady}; IsFallback {surfaceStatus.IsFallback}; FallbackReason {surfaceStatus.FallbackReason ?? "none"}; UsesNativeSurface {surfaceStatus.UsesNativeSurface}; ResidentTileCount {surfaceStatus.ResidentTileCount}\n" +
            $"OverlayOptions: {CreateOverlayOptionsSummary(ActiveSurfaceChartView.OverlayOptions)}\n" +
            $"Cache asset: {_activeAssetSummary}\n" +
            $"Dataset: {_activeDatasetSummary}";
    }

    private string CreateViewStateSummary()
    {
        if (IsScatterProofActive)
        {
            return CreateScatterCameraSummary(_scatterChartView.RenderingStatus);
        }

        var viewState = ActiveSurfaceChartView.ViewState;
        var dataWindow = viewState.DataWindow;
        var camera = viewState.Camera;
        return
            $"Data window StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}; " +
            $"Camera target ({camera.Target.X:0.###}, {camera.Target.Y:0.###}, {camera.Target.Z:0.###}), Yaw {camera.YawDegrees:0.###}, Pitch {camera.PitchDegrees:0.###}, Distance {camera.Distance:0.###}";
    }

    private static string CreateScatterCameraSummary(ScatterChartRenderingStatus status)
    {
        return
            $"Camera target ({status.CameraTarget.X:0.###}, {status.CameraTarget.Y:0.###}, {status.CameraTarget.Z:0.###}), Distance {status.CameraDistance:0.###}, SeriesCount {status.SeriesCount}, PointCount {status.PointCount}";
    }

    private static string CreateOverlayOptionsSummary(SurfaceChartOverlayOptions overlayOptions)
    {
        return
            $"Minor ticks {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions}); " +
            $"Grid plane {overlayOptions.GridPlane}; " +
            $"Axis side {overlayOptions.AxisSideMode}";
    }

    private static string CreateFallbackText(SurfaceChartRenderingStatus status)
    {
        return status.IsFallback
            ? $"software fallback active ({status.FallbackReason ?? "reason unavailable"})"
            : "no fallback active";
    }

    private static string CreateHostText(SurfaceChartRenderingStatus status)
    {
        return status.UsesNativeSurface
            ? "native surface host is active"
            : "control-owned surface is active";
    }
}
