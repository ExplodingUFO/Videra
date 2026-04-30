using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
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
    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly VideraChartView _chartView;
    private readonly TextBlock _statusText;
    private readonly string? _outputPath;
    private readonly string? _diagnosticsSnapshotPath;
    private readonly string? _supportSummaryPath;
    private readonly string? _chartSnapshotPath;
    private readonly string? _tracePath;
    private readonly SurfaceMetadata _metadata;
    private readonly ISurfaceTileSource _source;
    private readonly SurfaceChartOverlayOptions _overlayOptions;
    private readonly SurfaceColorMap _colorMap;
    private readonly DispatcherTimer _readinessPollTimer;
    private readonly int _lightingProofHoldSeconds;
    private BarChartRenderingStatus _barRenderingStatusEvidence = new();
    private ContourChartRenderingStatus _contourRenderingStatusEvidence = new();
    private bool _openedChartApplied;
    private bool _completed;
    private bool _completionQueued;
    private PlotSnapshotResult? _snapshotResult;

    public MainWindow()
    {
        InitializeComponent();

        _chartView = this.FindControl<VideraChartView>("ChartView")
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
        _chartSnapshotPath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "chart-snapshot.png");
        _tracePath = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_TRACE");
        _lightingProofHoldSeconds = ResolveLightingProofHoldSeconds();

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
        _chartView.Plot.Clear();
        var firstChartSeries = _chartView.Plot.Add.Surface(_source, "Start here: In-memory first chart");
        _chartView.Plot.ColorMap = _colorMap;
        _chartView.Plot.OverlayOptions = _overlayOptions;
        _chartView.ViewState = SurfaceViewState.CreateDefault(
            metadata,
            new SurfaceDataWindow(0d, 0d, metadata.Width, metadata.Height));

        // Add Bar series for smoke validation
        AddBarSeries();
        _barRenderingStatusEvidence = _chartView.BarRenderingStatus;

        // Add Contour series for smoke validation
        AddContourSeries();
        _contourRenderingStatusEvidence = _chartView.ContourRenderingStatus;

        _chartView.Plot.Move(firstChartSeries, _chartView.Plot.Series.Count - 1);
        _chartView.FitToData();
    }

    private void AddBarSeries()
    {
        var barData = new BarChartData(
        [
            new BarSeries([10.0, 20.0, 15.0, 25.0], 0xFF38BDF8u, "Smoke Bar A"),
            new BarSeries([8.0, 14.0, 22.0, 18.0], 0xFFF97316u, "Smoke Bar B"),
        ]);
        _chartView.Plot.Add.Bar(barData, "Smoke bar chart");
    }

    private void AddContourSeries()
    {
        const int size = 16;
        var values = new double[size, size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - (size - 1) / 2.0;
                var dy = y - (size - 1) / 2.0;
                values[x, y] = Math.Sqrt(dx * dx + dy * dy);
            }
        }
        _chartView.Plot.Add.Contour(values, "Smoke contour plot");
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        Trace("Window opened.");
        ReapplyChartAfterOpen();
        _openedChartApplied = true;
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

        if (e.Property == VideraChartView.ViewStateProperty)
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

    private async Task TryCompleteWhenReadyAsync()
    {
        if (_completed || _completionQueued || !IsFirstChartReady())
        {
            return;
        }

        _completionQueued = true;

        try
        {
            _snapshotResult = await CaptureSnapshotAsync().ConfigureAwait(true);
            await CompleteAsync(
                succeeded: true,
                firstChartRendered: true,
                failure: null).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Trace($"Snapshot capture failed: {ex.Message}");
            await CompleteAsync(
                succeeded: false,
                firstChartRendered: true,
                failure: $"SurfaceCharts consumer smoke snapshot capture failed: {ex.Message}")
                .ConfigureAwait(true);
        }
        finally
        {
            _completionQueued = false;
        }
    }

    private bool IsFirstChartReady()
    {
        return _openedChartApplied && IsRenderingStatusReady();
    }

    private bool IsRenderingStatusReady()
    {
        var status = _chartView.RenderingStatus;
        return status.IsReady && status.ResidentTileCount > 0;
    }

    private void ReapplyChartAfterOpen()
    {
        _chartView.Plot.Clear();
        ConfigureChart(_metadata);
        Trace("Reapplied chart after window open.");
    }

    private async Task<PlotSnapshotResult?> CaptureSnapshotAsync()
    {
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);
        return await _chartView.Plot.CaptureSnapshotAsync(request).ConfigureAwait(true);
    }

    private static int ResolveLightingProofHoldSeconds()
    {
        var value = Environment.GetEnvironmentVariable("VIDERA_LIGHTING_PROOF_HOLD_SECONDS");
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (!int.TryParse(value, out var seconds) || seconds < 0)
        {
            throw new InvalidOperationException("VIDERA_LIGHTING_PROOF_HOLD_SECONDS must be a non-negative whole number.");
        }

        return seconds;
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

        if (succeeded && _snapshotResult is null)
        {
            try
            {
                _snapshotResult = await CaptureSnapshotAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                Trace($"Snapshot capture failed: {ex.Message}");
                succeeded = false;
                failure = $"SurfaceCharts consumer smoke snapshot capture failed: {ex.Message}";
            }
        }

        if (succeeded && _snapshotResult is not { Succeeded: true })
        {
            succeeded = false;
            failure = "SurfaceCharts consumer smoke snapshot capture did not succeed.";
        }

        await PersistChartSnapshotAsync(succeeded).ConfigureAwait(true);

        var diagnostics = CreateDiagnosticsSnapshot();
        var supportSummary = CreateSupportSummary();
        var report = CreateReport(succeeded, firstChartRendered, failure);
        await PersistArtifactsAsync(report, diagnostics, supportSummary).ConfigureAwait(true);

        if (!succeeded)
        {
            for (var attempt = 0; attempt < 80 && !IsFirstChartReady(); attempt++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
            }

            if (IsFirstChartReady())
            {
                Trace("Late readiness observed after initial completion; upgrading result to success.");
                succeeded = true;
                firstChartRendered = true;
                failure = null;
                if (_snapshotResult is not { Succeeded: true })
                {
                    _snapshotResult = await CaptureSnapshotAsync().ConfigureAwait(true);
                }

                await PersistChartSnapshotAsync(succeeded).ConfigureAwait(true);
                diagnostics = CreateDiagnosticsSnapshot();
                supportSummary = CreateSupportSummary();
                report = CreateReport(succeeded, firstChartRendered, failure);
                await PersistArtifactsAsync(report, diagnostics, supportSummary).ConfigureAwait(true);
            }
        }

        _statusText.Text = succeeded
            ? "Packaged SurfaceCharts first-chart consumer smoke passed."
            : $"Packaged SurfaceCharts first-chart consumer smoke failed: {failure}";

        _ = ShutdownAsync(succeeded);
    }

    private async Task ShutdownAsync(bool succeeded)
    {
        if (succeeded && _lightingProofHoldSeconds > 0)
        {
            Trace($"Lighting proof hold active for {_lightingProofHoldSeconds} seconds before shutdown.");
            _statusText.Text = $"SurfaceCharts consumer smoke passed. Proof hold active for {_lightingProofHoldSeconds} seconds.";
            await Task.Delay(TimeSpan.FromSeconds(_lightingProofHoldSeconds)).ConfigureAwait(true);
        }

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
            renderingStatus.VisibleTileCount,
            renderingStatus.ResidentTileBytes,
            _chartView.InteractionQuality.ToString(),
            _diagnosticsSnapshotPath,
            _supportSummaryPath,
            CreateReportChartSnapshotPath());
    }

    private string? CreateReportChartSnapshotPath()
    {
        return HasPersistedChartSnapshot() ? _chartSnapshotPath : null;
    }

    private async Task PersistChartSnapshotAsync(bool succeeded)
    {
        if (string.IsNullOrWhiteSpace(_chartSnapshotPath))
        {
            return;
        }

        if (!succeeded)
        {
            return;
        }

        if (_snapshotResult is not { Succeeded: true } || string.IsNullOrWhiteSpace(_snapshotResult.Path))
        {
            throw new InvalidOperationException("SurfaceCharts consumer smoke did not produce a successful plot snapshot.");
        }

        if (!File.Exists(_snapshotResult.Path))
        {
            throw new FileNotFoundException("SurfaceCharts consumer smoke snapshot output was not found.", _snapshotResult.Path);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_chartSnapshotPath!)!);
        using var source = File.OpenRead(_snapshotResult.Path);
        using var destination = File.Create(_chartSnapshotPath!);
        await source.CopyToAsync(destination).ConfigureAwait(true);
        Trace($"Wrote chart snapshot: {_chartSnapshotPath}");
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
                JsonSerializer.Serialize(report, ReportJsonOptions)).ConfigureAwait(true);
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
            $"RenderingStatus: ActiveBackend {status.ActiveBackend}; IsReady {status.IsReady}; IsFallback {status.IsFallback}; ResidentTileCount {status.ResidentTileCount}; VisibleTileCount {status.VisibleTileCount}; ResidentTileBytes {status.ResidentTileBytes}";
    }

    private string CreateDiagnosticsSnapshot()
    {
        var status = _chartView.RenderingStatus;
        return
            "SurfaceCharts consumer smoke diagnostics\n" +
            $"Plot path: Start here: In-memory first chart\n" +
            $"ActiveBackend: {status.ActiveBackend}\n" +
            $"IsReady: {status.IsReady}\n" +
            $"IsFallback: {status.IsFallback}\n" +
            $"FallbackReason: {status.FallbackReason ?? "none"}\n" +
            $"UsesNativeSurface: {status.UsesNativeSurface}\n" +
            $"ResidentTileCount: {status.ResidentTileCount}\n" +
            $"VisibleTileCount: {status.VisibleTileCount}\n" +
            $"ResidentTileBytes: {status.ResidentTileBytes}\n" +
            $"InteractionQuality: {_chartView.InteractionQuality}\n" +
            $"SupportSummaryPath: {_supportSummaryPath ?? "<unset>"}\n" +
            $"SnapshotStatus: {CreateSnapshotStatusSummary()}";
    }

    private string CreateSupportSummary()
    {
        var status = _chartView.RenderingStatus;
        return
            "SurfaceCharts support summary\n" +
            $"GeneratedUtc: {DateTimeOffset.UtcNow:O}\n" +
            "EvidenceKind: SurfaceChartsDatasetProof\n" +
            "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.\n" +
            $"ChartControl: {CreateChartControlSummary()}\n" +
            $"EnvironmentRuntime: {CreateEnvironmentRuntimeSummary()}\n" +
            $"AssemblyIdentity: {CreateAssemblyIdentitySummary()}\n" +
            $"BackendDisplayEnvironment: {CreateBackendDisplayEnvironmentSummary()}\n" +
            "Plot path: Start here: In-memory first chart\n" +
            "Plot details: Generated at runtime from a dense 64x48 matrix, built with SurfacePyramidBuilder, and used as the packaged first-chart smoke baseline.\n" +
            $"SeriesCount: {_chartView.Plot.Series.Count}\n" +
            $"ActiveSeries: {CreateActiveSeriesSummary(_chartView)}\n" +
            $"ChartKind: {CreateChartKindSummary(_chartView)}\n" +
            $"ColorMap: {CreateColorMapSummary(_chartView.Plot.ColorMap)}\n" +
            $"PrecisionProfile: {CreatePrecisionProfileSummary(_chartView)}\n" +
            $"OutputEvidenceKind: {CreateOutputEvidenceKindSummary(_chartView)}\n" +
            $"OutputCapabilityDiagnostics: {CreateOutputCapabilityDiagnosticsSummary(_chartView)}\n" +
            $"SnapshotStatus: {CreateSnapshotStatusSummary()}\n" +
            $"SnapshotPath: {CreateSnapshotPathSummary()}\n" +
            $"SnapshotWidth: {_snapshotResult?.Manifest?.Width.ToString(CultureInfo.InvariantCulture) ?? "none"}\n" +
            $"SnapshotHeight: {_snapshotResult?.Manifest?.Height.ToString(CultureInfo.InvariantCulture) ?? "none"}\n" +
            $"SnapshotFormat: {_snapshotResult?.Manifest?.Format.ToString() ?? "none"}\n" +
            $"SnapshotBackground: {_snapshotResult?.Manifest?.Background.ToString() ?? "none"}\n" +
            $"SnapshotOutputEvidenceKind: {_snapshotResult?.Manifest?.OutputEvidenceKind ?? "none"}\n" +
            $"SnapshotDatasetEvidenceKind: {_snapshotResult?.Manifest?.DatasetEvidenceKind ?? "none"}\n" +
            $"SnapshotActiveSeriesIdentity: {_snapshotResult?.Manifest?.ActiveSeriesIdentity ?? "none"}\n" +
            $"SnapshotCreatedUtc: {_snapshotResult?.Manifest?.CreatedUtc.ToString("O") ?? "none"}\n" +
            $"DatasetEvidenceKind: {CreateDatasetEvidenceKindSummary(_chartView)}\n" +
            $"DatasetSeriesCount: {CreateDatasetSeriesCountSummary(_chartView)}\n" +
            $"DatasetActiveSeriesIndex: {CreateDatasetActiveSeriesIndexSummary(_chartView)}\n" +
            $"DatasetActiveSeriesMetadata: {CreateDatasetActiveSeriesMetadataSummary(_chartView)}\n" +
            $"ViewState: {CreateViewStateSummary()}\n" +
            $"InteractionQuality: {_chartView.InteractionQuality}\n" +
            $"RenderingStatus: ActiveBackend {status.ActiveBackend}; IsReady {status.IsReady}; IsFallback {status.IsFallback}; FallbackReason {status.FallbackReason ?? "none"}; UsesNativeSurface {status.UsesNativeSurface}; ResidentTileCount {status.ResidentTileCount}; VisibleTileCount {status.VisibleTileCount}; ResidentTileBytes {status.ResidentTileBytes}\n" +
            $"BarRenderingStatus: HasSource {_barRenderingStatusEvidence.HasSource}; IsReady {_barRenderingStatusEvidence.IsReady}; Series {_barRenderingStatusEvidence.SeriesCount}; Categories {_barRenderingStatusEvidence.CategoryCount}; Layout {_barRenderingStatusEvidence.Layout}\n" +
            $"ContourRenderingStatus: HasSource {_contourRenderingStatusEvidence.HasSource}; IsReady {_contourRenderingStatusEvidence.IsReady}; Levels {_contourRenderingStatusEvidence.LevelCount}; Lines {_contourRenderingStatusEvidence.ExtractedLineCount}\n" +
            $"OverlayOptions: {CreateOverlayOptionsSummary(_chartView.Plot.OverlayOptions)}\n" +
            $"InteractivityCrosshairEnabled: {_chartView.Plot.OverlayOptions.ShowCrosshair}\n" +
            $"InteractivityTooltipOffset: ({_chartView.Plot.OverlayOptions.TooltipOffset.X}, {_chartView.Plot.OverlayOptions.TooltipOffset.Y})\n" +
            $"InteractivityProbeStrategies: Surface, Bar, Contour\n" +
            $"InteractivityKeyboardShortcuts: enabled\n" +
            $"InteractivityToolbarButtons: not included in packaged consumer smoke\n" +
            $"SmokeCoverage: packaged first-chart readiness plus PNG snapshot evidence; repository demo UX and scatter/live-data scenarios are out of scope\n" +
            "Dataset: Generated 64x48 in-memory matrix for the packaged first-chart consumer proof.";
    }

    private string CreateChartControlSummary()
    {
        var chartType = _chartView.GetType();
        return $"{chartType.Name} ({chartType.FullName})";
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
            $"ConsumerSmoke {CreateAssemblyIdentity(typeof(MainWindow))}; " +
            $"Avalonia {CreateAssemblyIdentity(typeof(VideraChartView))}; " +
            $"Processing {CreateAssemblyIdentity(typeof(SurfacePyramidBuilder))}; " +
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
        return chartView.Plot.CreateOutputEvidence(
            chartView.RenderingStatus,
            chartView.ScatterRenderingStatus,
            chartView.BarRenderingStatus,
            chartView.ContourRenderingStatus);
    }

    private static string FormatValueRange(SurfaceValueRangeDatasetEvidence? valueRange)
    {
        return valueRange is null
            ? "none"
            : $"{valueRange.Minimum.ToString("G6", CultureInfo.InvariantCulture)}..{valueRange.Maximum.ToString("G6", CultureInfo.InvariantCulture)}";
    }

    private static string FormatFifoCapacity(int? configuredFifoCapacity)
    {
        return configuredFifoCapacity is { } capacity ? capacity.ToString(CultureInfo.InvariantCulture) : "unbounded";
    }

    private static string FormatSeriesName(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? "unnamed" : name;
    }

    private static string GetEnvironmentValue(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        return string.IsNullOrWhiteSpace(value) ? "unset" : value;
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

    private string CreateSnapshotStatusSummary()
    {
        if (_snapshotResult is null)
        {
            return "none";
        }

        return _snapshotResult.Succeeded ? "present" : "failed";
    }

    private string CreateSnapshotPathSummary()
    {
        if (HasPersistedChartSnapshot())
        {
            return _chartSnapshotPath!;
        }

        if (_snapshotResult is { Succeeded: true } && !string.IsNullOrWhiteSpace(_snapshotResult.Path))
        {
            return _snapshotResult.Path;
        }

        return "none";
    }

    private bool HasPersistedChartSnapshot()
    {
        return !string.IsNullOrWhiteSpace(_chartSnapshotPath) && File.Exists(_chartSnapshotPath);
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
        int VisibleTileCount,
        long ResidentTileBytes,
        string InteractionQuality,
        string? DiagnosticsSnapshotPath,
        string? SupportSummaryPath,
        string? ChartSnapshotPath);
}
