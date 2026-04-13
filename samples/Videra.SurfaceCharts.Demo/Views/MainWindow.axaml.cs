using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;

namespace Videra.SurfaceCharts.Demo.Views;

public partial class MainWindow : Window
{
    private readonly SurfaceChartView _chartView;
    private readonly ComboBox _sourceSelector;
    private readonly TextBlock _statusText;
    private readonly TextBlock _cachePathText;
    private readonly TextBlock _datasetText;
    private readonly ISurfaceTileSource _inMemorySource;
    private readonly Task<ISurfaceTileSource> _cacheSourceTask;
    private readonly SurfaceColorMap _colorMap;
    private readonly SurfaceViewport _viewport;
    private readonly string _cachePath;

    public MainWindow()
    {
        InitializeComponent();

        _chartView = this.FindControl<SurfaceChartView>("ChartView")
            ?? throw new InvalidOperationException("Surface chart view is missing.");
        _sourceSelector = this.FindControl<ComboBox>("SourceSelector")
            ?? throw new InvalidOperationException("Source selector is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("Status text control is missing.");
        _cachePathText = this.FindControl<TextBlock>("CachePathText")
            ?? throw new InvalidOperationException("Cache path text control is missing.");
        _datasetText = this.FindControl<TextBlock>("DatasetText")
            ?? throw new InvalidOperationException("Dataset text control is missing.");

        _cachePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "sample-surface-cache",
            "sample.surfacecache.json");

        _inMemorySource = CreateInMemorySource();
        _cacheSourceTask = LoadCacheSourceAsync();
        _colorMap = CreateColorMap(_inMemorySource.Metadata.ValueRange);
        _viewport = new SurfaceViewport(0, 0, _inMemorySource.Metadata.Width, _inMemorySource.Metadata.Height);

        _chartView.Viewport = _viewport;
        _chartView.ColorMap = _colorMap;
        _sourceSelector.SelectedIndex = 0;
        _sourceSelector.SelectionChanged += OnSourceSelectionChanged;

        _cachePathText.Text = _cachePath;
        _datasetText.Text = CreateDatasetSummary();
        ApplySource(
            _inMemorySource,
            "In-memory example",
            "Generated at runtime from a dense matrix and built with SurfacePyramidBuilder.");
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
            ApplySource(_inMemorySource, "In-memory example", "Generated at runtime from a dense matrix and built with SurfacePyramidBuilder.");
            return;
        }

        try
        {
            var cacheSource = await _cacheSourceTask.ConfigureAwait(true);
            ApplySource(cacheSource, "Cache-backed example", $"Loaded from the committed cache asset at {_cachePath}.");
        }
        catch (Exception exception)
        {
            ApplySource(_inMemorySource, "In-memory example", $"Cache-backed example failed to load: {exception.Message}");
            _sourceSelector.SelectedIndex = 0;
        }
    }

    private void ApplySource(ISurfaceTileSource source, string heading, string details)
    {
        _chartView.Source = source;
        UpdateStatusText(heading, details);
    }

    private void UpdateStatusText(string heading, string? details = null)
    {
        _statusText.Text = details is null
            ? heading
            : $"{heading}\n{details}";
    }

    private static ISurfaceTileSource CreateInMemorySource()
    {
        var matrix = CreateSampleMatrix();
        return new SurfacePyramidBuilder(32, 32).Build(matrix);
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

    private static string CreateDatasetSummary()
    {
        return "The in-memory path uses a generated 64x48 matrix with an overview-first pyramid. " +
               "The cache-backed path reads the same sample family from a committed JSON cache file.";
    }
}
