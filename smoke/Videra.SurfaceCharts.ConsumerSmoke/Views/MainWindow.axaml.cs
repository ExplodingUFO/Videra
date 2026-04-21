using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Processing;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.ConsumerSmoke.Views;

public partial class MainWindow : Window
{
    private readonly SurfaceChartView _chartView;
    private readonly TextBlock _statusText;
    private readonly string? _outputPath;
    private readonly string? _diagnosticsSnapshotPath;
    private readonly string? _supportSummaryPath;
    private readonly string? _tracePath;
    private readonly SurfaceMetadata _metadata;
    private readonly ISurfaceTileSource _source;
    private readonly SurfaceChartOverlayOptions _overlayOptions;
    private readonly SurfaceColorMap _colorMap;
    private readonly DispatcherTimer _readinessPollTimer;
    private bool _completed;
    private bool _completionQueued;

    public MainWindow()
    {
        InitializeComponent();

        _chartView = this.FindControl<SurfaceChartView>("ChartView")
            ?? throw new InvalidOperationException("ChartView is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("StatusText is missing.");
        _outputPath = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_OUTPUT");
        _diagnosticsSnapshotPath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "diagnostics-snapshot.txt");
        _supportSummaryPath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "surfacecharts-support-summary.txt");
        _tracePath = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_TRACE");

        var matrix = CreateSampleMatrix();
        _metadata = matrix.Metadata;
        _source = new SurfacePyramidBuilder(32, 32).Build(matrix);
        _colorMap = CreateColorMap(_metadata.ValueRange);
        _overlayOptions = CreateOverlayOptions();
        _readinessPollTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };

        ConfigureChart(_metadata);

        Opened += OnOpened;
        _chartView.RenderStatusChanged += OnRenderStatusChanged;
        _chartView.InteractionQualityChanged += OnInteractionQualityChanged;
        _chartView.PropertyChanged += OnChartViewPropertyChanged;
        _readinessPollTimer.Tick += OnReadinessPollTick;

        UpdateStatusText("Constructed.");
        Trace("SurfaceCharts consumer smoke window constructed.");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ConfigureChart(SurfaceMetadata metadata)
    {
        _chartView.ColorMap = _colorMap;
        _chartView.OverlayOptions = _overlayOptions;
        _chartView.Source = _source;
        _chartView.ViewState = SurfaceViewState.CreateDefault(
            metadata,
            new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));
        _chartView.FitToData();
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        Trace("Window opened.");
        ReapplyChartAfterOpen();
        UpdateStatusText("Waiting for packaged first-chart readiness.");
        _readinessPollTimer.Start();
        _ = RunTimeoutGuardAsync();
        _ = TryCompleteWhenReadyAsync();
    }

    private void OnRenderStatusChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        Trace($"Render status changed: backend={_chartView.RenderingStatus.ActiveBackend}; ready={_chartView.RenderingStatus.IsReady}; residentTiles={_chartView.RenderingStatus.ResidentTileCount}.");
        UpdateStatusText("Render status changed.");
        _ = TryCompleteWhenReadyAsync();
    }

    private void OnInteractionQualityChanged(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        UpdateStatusText("Interaction quality changed.");
    }

    private void OnChartViewPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        _ = sender;

        if (e.Property == SurfaceChartView.ViewStateProperty)
        {
            UpdateStatusText("View state changed.");
        }
    }

    private void OnReadinessPollTick(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;

        if (_completed)
        {
            _readinessPollTimer.Stop();
            return;
        }

        _ = TryCompleteWhenReadyAsync();
    }

    private void ReapplyChartAfterOpen()
    {
        _chartView.Source = null;
        ConfigureChart(_metadata);
        Trace("Reapplied chart after window open.");
    }

    private async Task TryCompleteWhenReadyAsync()
    {
        if (_completed || _completionQueued || !IsFirstChartReady())
        {
            return;
        }

        _completionQueued = true;

        try
        {
            await CompleteAsync(
                succeeded: true,
                firstChartRendered: true,
                failure: null).ConfigureAwait(true);
        }
        finally
        {
            _completionQueued = false;
        }
    }

    private bool IsFirstChartReady()
    {
        var status = _chartView.RenderingStatus;
        return status.IsReady && status.ResidentTileCount > 0;
    }

    private async Task RunTimeoutGuardAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(75)).ConfigureAwait(true);
        if (_completed)
        {
            return;
        }

        if (IsFirstChartReady())
        {
            await CompleteAsync(
                succeeded: true,
                firstChartRendered: true,
                failure: null).ConfigureAwait(true);
            return;
        }

        Trace("Timeout guard elapsed.");
        await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(true);
        if (_completed)
        {
            return;
        }

        if (IsFirstChartReady())
        {
            await CompleteAsync(
                succeeded: true,
                firstChartRendered: true,
                failure: null).ConfigureAwait(true);
            return;
        }

        await CompleteAsync(
            succeeded: false,
            firstChartRendered: false,
            failure: "SurfaceCharts consumer smoke timed out before the packaged first-chart path became ready.")
            .ConfigureAwait(true);
    }

    private async Task CompleteAsync(
        bool succeeded,
        bool firstChartRendered,
        string? failure)
    {
        if (_completed)
        {
            return;
        }

        if (!succeeded)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250)).ConfigureAwait(true);
            if (_completed)
            {
                return;
            }

            if (IsFirstChartReady())
            {
                succeeded = true;
                firstChartRendered = true;
                failure = null;
            }
        }

        _completed = true;
        _readinessPollTimer.Stop();

        var diagnostics = CreateDiagnosticsSnapshot();
        var supportSummary = CreateSupportSummary();
        var report = CreateReport(succeeded, firstChartRendered, failure);
        await PersistArtifactsAsync(report, diagnostics, supportSummary).ConfigureAwait(true);

        if (!succeeded)
        {
            for (var attempt = 0; attempt < 40 && !IsFirstChartReady(); attempt++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
            }

            if (IsFirstChartReady())
            {
                Trace("Late readiness observed after initial completion; upgrading result to success.");
                succeeded = true;
                firstChartRendered = true;
                failure = null;
                diagnostics = CreateDiagnosticsSnapshot();
                supportSummary = CreateSupportSummary();
                report = CreateReport(succeeded, firstChartRendered, failure);
                await PersistArtifactsAsync(report, diagnostics, supportSummary).ConfigureAwait(true);
            }
        }

        _statusText.Text = succeeded
            ? "Packaged SurfaceCharts first-chart consumer smoke passed."
            : $"Packaged SurfaceCharts first-chart consumer smoke failed: {failure}";

        Dispatcher.UIThread.Post(() =>
        {
            Trace($"Posting shutdown with exit code {(succeeded ? 0 : 1)}.");
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(succeeded ? 0 : 1);
            }
        }, DispatcherPriority.Background);
    }

    private ConsumerSmokeReport CreateReport(
        bool succeeded,
        bool firstChartRendered,
        string? failure)
    {
        var renderingStatus = _chartView.RenderingStatus;
        return new ConsumerSmokeReport(
            succeeded,
            firstChartRendered,
            failure,
            renderingStatus.ActiveBackend.ToString(),
            renderingStatus.IsReady,
            renderingStatus.IsFallback,
            renderingStatus.FallbackReason,
            renderingStatus.UsesNativeSurface,
            renderingStatus.ResidentTileCount,
            _chartView.InteractionQuality.ToString(),
            _diagnosticsSnapshotPath,
            _supportSummaryPath);
    }

    private async Task PersistArtifactsAsync(
        ConsumerSmokeReport report,
        string diagnostics,
        string supportSummary)
    {
        if (!string.IsNullOrWhiteSpace(_outputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath!)!);
            await File.WriteAllTextAsync(
                _outputPath!,
                JsonSerializer.Serialize(
                    report,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    })).ConfigureAwait(true);
            Trace($"Wrote report: {_outputPath}");
        }

        if (!string.IsNullOrWhiteSpace(_diagnosticsSnapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_diagnosticsSnapshotPath!)!);
            await File.WriteAllTextAsync(_diagnosticsSnapshotPath!, diagnostics).ConfigureAwait(true);
            Trace($"Wrote diagnostics snapshot: {_diagnosticsSnapshotPath}");
        }

        if (!string.IsNullOrWhiteSpace(_supportSummaryPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_supportSummaryPath!)!);
            await File.WriteAllTextAsync(_supportSummaryPath!, supportSummary).ConfigureAwait(true);
            Trace($"Wrote support summary: {_supportSummaryPath}");
        }
    }

    private void UpdateStatusText(string reason)
    {
        var status = _chartView.RenderingStatus;
        _statusText.Text =
            "Packaged first-chart path: Start here: In-memory first chart\n" +
            "Public install stack: Videra.SurfaceCharts.Avalonia + Videra.SurfaceCharts.Processing\n" +
            $"Reason: {reason}\n" +
            $"InteractionQuality: {_chartView.InteractionQuality}\n" +
            $"RenderingStatus: ActiveBackend {status.ActiveBackend}; IsReady {status.IsReady}; IsFallback {status.IsFallback}; ResidentTileCount {status.ResidentTileCount}";
    }

    private string CreateDiagnosticsSnapshot()
    {
        var status = _chartView.RenderingStatus;
        return
            "SurfaceCharts consumer smoke diagnostics\n" +
            $"Source path: Start here: In-memory first chart\n" +
            $"ActiveBackend: {status.ActiveBackend}\n" +
            $"IsReady: {status.IsReady}\n" +
            $"IsFallback: {status.IsFallback}\n" +
            $"FallbackReason: {status.FallbackReason ?? "none"}\n" +
            $"UsesNativeSurface: {status.UsesNativeSurface}\n" +
            $"ResidentTileCount: {status.ResidentTileCount}\n" +
            $"InteractionQuality: {_chartView.InteractionQuality}\n" +
            $"SupportSummaryPath: {_supportSummaryPath ?? "<unset>"}";
    }

    private string CreateSupportSummary()
    {
        var status = _chartView.RenderingStatus;
        return
            "SurfaceCharts support summary\n" +
            "Source path: Start here: In-memory first chart\n" +
            "Source details: Generated at runtime from a dense 64x48 matrix, built with SurfacePyramidBuilder, and used as the packaged first-chart smoke baseline.\n" +
            $"ViewState: {CreateViewStateSummary()}\n" +
            $"InteractionQuality: {_chartView.InteractionQuality}\n" +
            $"RenderingStatus: ActiveBackend {status.ActiveBackend}; IsReady {status.IsReady}; IsFallback {status.IsFallback}; FallbackReason {status.FallbackReason ?? "none"}; UsesNativeSurface {status.UsesNativeSurface}; ResidentTileCount {status.ResidentTileCount}\n" +
            $"OverlayOptions: {CreateOverlayOptionsSummary(_chartView.OverlayOptions)}\n" +
            "Dataset: Generated 64x48 in-memory matrix for the packaged first-chart consumer proof.";
    }

    private string CreateViewStateSummary()
    {
        var viewState = _chartView.ViewState;
        var dataWindow = viewState.DataWindow;
        var camera = viewState.Camera;
        return
            $"Data window StartX {dataWindow.StartX:0.###}, StartY {dataWindow.StartY:0.###}, Width {dataWindow.Width:0.###}, Height {dataWindow.Height:0.###}; " +
            $"Camera target ({camera.Target.X:0.###}, {camera.Target.Y:0.###}, {camera.Target.Z:0.###}), Yaw {camera.YawDegrees:0.###}, Pitch {camera.PitchDegrees:0.###}, Distance {camera.Distance:0.###}";
    }

    private static string CreateOverlayOptionsSummary(SurfaceChartOverlayOptions overlayOptions)
    {
        return
            $"Minor ticks {(overlayOptions.ShowMinorTicks ? "enabled" : "disabled")} (divisions {overlayOptions.MinorTickDivisions}); " +
            $"Grid plane {overlayOptions.GridPlane}; " +
            $"Axis side {overlayOptions.AxisSideMode}";
    }

    private static SurfaceChartOverlayOptions CreateOverlayOptions()
    {
        return new SurfaceChartOverlayOptions
        {
            ShowMinorTicks = true,
            MinorTickDivisions = 4,
            GridPlane = SurfaceChartGridPlane.XZ,
            AxisSideMode = SurfaceChartAxisSideMode.Auto,
            LabelFormatter = static (_, value) => value.ToString("0.##", CultureInfo.InvariantCulture)
        };
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

    private void Trace(string message)
    {
        if (string.IsNullOrWhiteSpace(_tracePath))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_tracePath!)!);
            File.AppendAllText(_tracePath!, $"[{DateTimeOffset.UtcNow:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Best-effort diagnostics only.
        }
    }

    private sealed record ConsumerSmokeReport(
        bool Succeeded,
        bool FirstChartRendered,
        string? Failure,
        string ActiveBackend,
        bool IsReady,
        bool IsFallback,
        string? FallbackReason,
        bool UsesNativeSurface,
        int ResidentTileCount,
        string InteractionQuality,
        string? DiagnosticsSnapshotPath,
        string? SupportSummaryPath);
}
