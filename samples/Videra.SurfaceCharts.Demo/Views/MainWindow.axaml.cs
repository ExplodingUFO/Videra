using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Globalization;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow : Window
{
    private const string CacheManifestFileName = "sample.surfacecache.json";
    private const string CachePayloadSuffix = ".bin";

    private readonly SurfaceChartView _chartView;
    private readonly ComboBox _sourceSelector;
    private readonly Button _fitToDataButton;
    private readonly Button _resetCameraButton;
    private readonly TextBlock _statusText;
    private readonly TextBlock _viewStateText;
    private readonly TextBlock _interactionQualityText;
    private readonly TextBlock _renderingPathText;
    private readonly TextBlock _overlayOptionsText;
    private readonly TextBlock _cachePathText;
    private readonly TextBlock _datasetText;
    private readonly ISurfaceTileSource _inMemorySource;
    private readonly SurfaceColorMap _colorMap;
    private readonly string _cachePath;
    private readonly string _cachePayloadPath;
    private Task<ISurfaceTileSource>? _cacheSourceTask;
    private string _activeSourceHeading = string.Empty;
    private string _activeSourceDetails = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        _chartView = this.FindControl<SurfaceChartView>("ChartView")
            ?? throw new InvalidOperationException("Surface chart view is missing.");
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
        _renderingPathText = this.FindControl<TextBlock>("RenderingPathText")
            ?? throw new InvalidOperationException("Rendering path text control is missing.");
        _overlayOptionsText = this.FindControl<TextBlock>("OverlayOptionsText")
            ?? throw new InvalidOperationException("Overlay options text control is missing.");
        _cachePathText = this.FindControl<TextBlock>("CachePathText")
            ?? throw new InvalidOperationException("Cache path text control is missing.");
        _datasetText = this.FindControl<TextBlock>("DatasetText")
            ?? throw new InvalidOperationException("Dataset text control is missing.");

        _cachePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "sample-surface-cache",
            CacheManifestFileName);
        _cachePayloadPath = _cachePath + CachePayloadSuffix;

        _inMemorySource = CreateInMemorySource();
        _colorMap = CreateColorMap(_inMemorySource.Metadata.ValueRange);
        _chartView.OverlayOptions = CreateOverlayOptions();

        _sourceSelector.SelectedIndex = 0;
        _chartView.ColorMap = _colorMap;

        ApplySource(
            _inMemorySource,
            "Start here: In-memory first chart",
            "Start here first. Generated at runtime from a dense matrix, built with SurfacePyramidBuilder, and kept source-first inside this demo.");

        _chartView.RenderStatusChanged += OnRenderStatusChanged;
        _chartView.InteractionQualityChanged += OnInteractionQualityChanged;
        _chartView.PropertyChanged += OnChartViewPropertyChanged;
        _sourceSelector.SelectionChanged += OnSourceSelectionChanged;
        _fitToDataButton.Click += OnFitToDataClicked;
        _resetCameraButton.Click += OnResetCameraClicked;

        UpdateRenderingPathText(_chartView.RenderingStatus);
        UpdateInteractionQualityText(_chartView.InteractionQuality);
        UpdateOverlayOptionsText(_chartView.OverlayOptions);
        _cachePathText.Text = $"Manifest: {_cachePath}\nPayload sidecar: {_cachePayloadPath}";
        _datasetText.Text = CreateDatasetSummary();
        UpdateStatusText();
        UpdateViewStateText();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void OnSourceSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _ = sender;
        _ = e;

        if (_sourceSelector.SelectedIndex == 0)
        {
            ApplySource(
                _inMemorySource,
                "Start here: In-memory first chart",
                "Start here first. Generated at runtime from a dense matrix, built with SurfacePyramidBuilder, and kept source-first inside this demo.");
            return;
        }

        try
        {
            var cacheSource = await GetOrCreateCacheSourceAsync().ConfigureAwait(true);
            ApplySource(
                cacheSource,
                "Explore next: Cache-backed streaming",
                $"Advanced follow-up after the first chart renders. Loads manifest metadata from {_cachePath} and uses lazy viewport tile streaming from {_cachePayloadPath}.");
        }
        catch (Exception exception)
        {
            ApplySource(
                _inMemorySource,
                "Start here: In-memory first chart",
                $"Start here fallback. Cache-backed streaming failed to load: {exception.Message}");
            _sourceSelector.SelectedIndex = 0;
        }
    }

    private void ApplySource(ISurfaceTileSource source, string heading, string details)
    {
        _chartView.Source = source;
        _chartView.FitToData();
        _activeSourceHeading = heading;
        _activeSourceDetails = details;
        UpdateStatusText();
        UpdateViewStateText();
    }

    private void OnRenderStatusChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        UpdateRenderingPathText(_chartView.RenderingStatus);
    }

    private void OnInteractionQualityChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        UpdateInteractionQualityText(_chartView.InteractionQuality);
    }

    private void OnChartViewPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        _ = sender;

        if (e.Property == SurfaceChartView.ViewStateProperty)
        {
            UpdateStatusText();
            UpdateViewStateText();
        }
    }

    private void OnFitToDataClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _chartView.FitToData();
        UpdateStatusText();
        UpdateViewStateText();
    }

    private void OnResetCameraClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _chartView.ResetCamera();
        UpdateStatusText();
        UpdateViewStateText();
    }

    private void UpdateViewStateText()
    {
        var viewState = _chartView.ViewState;
        var dataWindow = viewState.DataWindow;
        var camera = viewState.Camera;
        _viewStateText.Text =
            $"ViewState\n" +
            $"Data window: StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}\n" +
            $"Camera: Target ({camera.Target.X:0.###}, {camera.Target.Y:0.###}, {camera.Target.Z:0.###}), Yaw {camera.YawDegrees:0.###}, Pitch {camera.PitchDegrees:0.###}, Distance {camera.Distance:0.###}";
    }

    private void UpdateInteractionQualityText(SurfaceChartInteractionQuality interactionQuality)
    {
        _interactionQualityText.Text =
            $"Current mode: {interactionQuality}\n" +
            "Interactive: lighter requests while orbit, pan, dolly, or focus input is in flight.\n" +
            "Refine: full settled requests for the current view.";
    }

    private void UpdateStatusText()
    {
        var dataWindow = _chartView.ViewState.DataWindow;
        _statusText.Text =
            $"{_activeSourceHeading}\n" +
            $"{_activeSourceDetails}\n" +
            "First-chart navigation: Left drag orbit, Right drag pan, Wheel dolly, Ctrl + Left drag focus zoom.\n" +
            $"Current window: StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}";
    }

    private void UpdateRenderingPathText(SurfaceChartRenderingStatus status)
    {
        var fallbackText = status.IsFallback
            ? $"software fallback active ({status.FallbackReason ?? "reason unavailable"})"
            : "no fallback active";
        var hostText = status.UsesNativeSurface
            ? "native surface host is active"
            : "control-owned surface is active";
        _renderingPathText.Text =
            $"Active backend: {status.ActiveBackend}\n" +
            $"Ready: {status.IsReady}\n" +
            $"Fallback: {fallbackText}\n" +
            $"Host path: {hostText}\n" +
            $"Resident tiles: {status.ResidentTileCount}";
    }

    private void UpdateOverlayOptionsText(SurfaceChartOverlayOptions overlayOptions)
    {
        _overlayOptionsText.Text =
            "Chart-local `OverlayOptions` keep formatter, minor ticks, grid plane, and axis-side behavior inside `SurfaceChartView` instead of pushing chart semantics into `VideraView`.\n" +
            $"Minor ticks: {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions})\n" +
            $"Grid plane: {overlayOptions.GridPlane}\n" +
            $"Axis side: {overlayOptions.AxisSideMode}\n" +
            "Formatter: legend and axis labels share the same chart-local numeric formatting contract.";
    }

    private static ISurfaceTileSource CreateInMemorySource()
    {
        var matrix = CreateSampleMatrix();
        return new SurfacePyramidBuilder(32, 32).Build(matrix);
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

    private static string CreateDatasetSummary()
    {
        return "The start-here in-memory path uses a generated 64x48 matrix with an overview-first pyramid. " +
               "The explore-next cache-backed path loads a committed manifest plus binary sidecar and lazily reads only the tiles needed for the current built-in interaction view.";
    }
}
