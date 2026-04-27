using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;
#if VIDERA_CONSUMER_SMOKE_GLTF
using Videra.Import.Gltf;
#endif
#if VIDERA_CONSUMER_SMOKE_OBJ
using Videra.Import.Obj;
#endif

namespace Videra.ConsumerSmoke.Views;

public partial class MainWindow : Window
{
    private static readonly JsonSerializerOptions ReportJsonOptions = new()
    {
        WriteIndented = true
    };

#if VIDERA_CONSUMER_SMOKE_GLTF
    private const string ModelFormat = "Gltf";
    private const string ModelPath = "Assets/reference-triangle.gltf";
#elif VIDERA_CONSUMER_SMOKE_OBJ
    private const string ModelFormat = "Obj";
    private const string ModelPath = "Assets/reference-cube.obj";
#else
    private const string ModelFormat = "None";
    private const string? ModelPath = null;
#endif
    private readonly VideraView _view3D;
    private readonly TextBlock _statusText;
    private readonly string? _outputPath;
    private readonly string? _diagnosticsSnapshotPath;
    private readonly string? _inspectionSnapshotPath;
    private readonly string? _inspectionBundlePath;
    private readonly string? _tracePath;
    private readonly string _scenario;
    private readonly string _packageVersion;
    private readonly string[] _packageIds;
    private readonly int _lightingProofHoldSeconds;
    private bool _completed;
    private bool _executionStarted;
    private EventHandler? _backendReadyHandler;
    private EventHandler? _openedHandler;

    public MainWindow()
    {
        InitializeComponent();

        _view3D = this.FindControl<VideraView>("View3D")
            ?? throw new InvalidOperationException("View3D is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("StatusText is missing.");
        _outputPath = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_OUTPUT");
        _diagnosticsSnapshotPath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "diagnostics-snapshot.txt");
        _inspectionSnapshotPath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "inspection-snapshot.png");
        _inspectionBundlePath = string.IsNullOrWhiteSpace(_outputPath)
            ? null
            : Path.Combine(Path.GetDirectoryName(_outputPath!)!, "inspection-bundle");
        _tracePath = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_TRACE");
        _scenario = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_SCENARIO") ?? ModelFormat;
        _packageVersion = Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_PACKAGE_VERSION") ?? string.Empty;
        _packageIds = (Environment.GetEnvironmentVariable("VIDERA_CONSUMER_SMOKE_PACKAGE_IDS") ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _lightingProofHoldSeconds = ResolveLightingProofHoldSeconds();
        Trace("MainWindow constructed.");

        var options = new VideraViewOptions
        {
            Backend =
            {
                PreferredBackend = GraphicsBackendPreference.Auto,
                AllowSoftwareFallback = true
            }
        };
#if VIDERA_CONSUMER_SMOKE_GLTF
        options.UseModelImporter(GltfModelImporter.Create());
#endif
#if VIDERA_CONSUMER_SMOKE_OBJ
        options.UseModelImporter(ObjModelImporter.Create());
#endif
        _view3D.Options = options;

        _backendReadyHandler = (_, _) => _ = TryExecuteSmokeAsync();
        _openedHandler = (_, _) =>
        {
            Trace("Window opened.");
            _statusText.Text = "Waiting for backend readiness.";
            _ = RunTimeoutGuardAsync();
            _ = TryExecuteSmokeAsync();
        };

        _view3D.BackendReady += _backendReadyHandler;
        _view3D.BackendStatusChanged += (_, e) =>
        {
            var displayServerCompatibility = VideraDiagnosticsSnapshotFormatter.DescribeDisplayServerCompatibility(e.Diagnostics);
            Trace(
                $"BackendStatusChanged: ready={e.Diagnostics.IsReady}; backend={e.Diagnostics.ResolvedBackend}; " +
                $"display={e.Diagnostics.ResolvedDisplayServer ?? "Unavailable"}; " +
                $"displayCompatibility={displayServerCompatibility}; fallback={e.Diagnostics.IsUsingSoftwareFallback}.");
            _statusText.Text =
                $"IsReady={e.Diagnostics.IsReady}; ResolvedBackend={e.Diagnostics.ResolvedBackend}; " +
                $"ResolvedDisplayServer={e.Diagnostics.ResolvedDisplayServer ?? "Unavailable"}";
        };
        _view3D.InitializationFailed += (_, e) =>
        {
            Trace($"Initialization failed: {e.Exception}");
            CompleteAsync(
                succeeded: false,
                frameAllReturned: false,
                failure: e.Exception.Message);
        };

        Opened += _openedHandler;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task TryExecuteSmokeAsync()
    {
        Trace($"TryExecuteSmokeAsync entered. completed={_completed}; started={_executionStarted}; isReady={_view3D.BackendDiagnostics.IsReady}.");
        if (_completed || _executionStarted || !_view3D.BackendDiagnostics.IsReady)
        {
            return;
        }

        _executionStarted = true;
        try
        {
#if VIDERA_CONSUMER_SMOKE_GLTF || VIDERA_CONSUMER_SMOKE_OBJ
            Trace($"LoadModelAsync starting: {ModelPath}");
            var result = await _view3D.LoadModelAsync(ModelPath).ConfigureAwait(true);
            if (!result.Succeeded)
            {
                Trace($"LoadModelAsync failed: {result.Failure?.ErrorMessage ?? "Unknown"}");
                await CompleteAsync(
                    succeeded: false,
                    frameAllReturned: false,
                    failure: result.Failure?.ErrorMessage ?? "LoadModelAsync returned an unknown error.").ConfigureAwait(true);
                return;
            }

            Trace("LoadModelAsync succeeded.");
#else
            Trace("Viewer-only smoke path selected; skipping importer-backed LoadModelAsync.");
#endif

            _view3D.AddObject(SmokeSceneFactory.CreateEmissiveNormalProofObject());
            Trace($"Added emissive/normal proof object: {SmokeSceneFactory.EmissiveNormalProofObjectName}.");
            _view3D.AddObject(SmokeSceneFactory.CreateMixedTransparencyProofObject());
            Trace($"Added mixed-transparency proof object: {SmokeSceneFactory.MixedTransparencyProofObjectName}.");
            var framed = _view3D.FrameAll();
            Trace($"FrameAll returned {framed}.");
            _view3D.ResetCamera();
            Trace("ResetCamera completed.");

            string? snapshotFailure = null;
            if (framed && !string.IsNullOrWhiteSpace(_inspectionSnapshotPath))
            {
                Trace($"ExportSnapshotAsync starting: {_inspectionSnapshotPath}");
                var snapshot = await _view3D.ExportSnapshotAsync(_inspectionSnapshotPath).ConfigureAwait(true);
                if (!snapshot.Succeeded)
                {
                    snapshotFailure = snapshot.Failure?.Message ?? "Snapshot export failed.";
                    Trace($"ExportSnapshotAsync failed: {snapshotFailure}");
                }
                else
                {
                    Trace($"ExportSnapshotAsync succeeded: {_inspectionSnapshotPath}");
                }
            }

            string? bundleFailure = null;
            if (framed && !string.IsNullOrWhiteSpace(_inspectionBundlePath))
            {
                Trace($"VideraInspectionBundleService.ExportAsync starting: {_inspectionBundlePath}");
                var bundle = await VideraInspectionBundleService.ExportAsync(_view3D, _inspectionBundlePath).ConfigureAwait(true);
                if (!bundle.Succeeded)
                {
                    bundleFailure = bundle.FailureMessage ?? "Inspection bundle export failed.";
                    Trace($"VideraInspectionBundleService.ExportAsync failed: {bundleFailure}");
                }
                else
                {
                    Trace($"VideraInspectionBundleService.ExportAsync succeeded: {_inspectionBundlePath}");
                }
            }

            await CompleteAsync(
                succeeded: framed && snapshotFailure is null && bundleFailure is null,
                frameAllReturned: framed,
                failure: !framed
                    ? "FrameAll() returned false."
                    : snapshotFailure ?? bundleFailure).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Trace($"TryExecuteSmokeAsync threw: {ex}");
            await CompleteAsync(
                succeeded: false,
                frameAllReturned: false,
                failure: ex.Message).ConfigureAwait(true);
        }
    }

    private async Task RunTimeoutGuardAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(true);
        if (_completed)
        {
            return;
        }

        Trace("Timeout guard elapsed.");
        await CompleteAsync(
            succeeded: false,
            frameAllReturned: false,
            failure: "Consumer smoke timed out before reaching a ready, framed scene.").ConfigureAwait(true);
    }

    private Task CompleteAsync(
        bool succeeded,
        bool frameAllReturned,
        string? failure)
    {
        if (_completed)
        {
            return Task.CompletedTask;
        }

        _completed = true;
        Trace($"CompleteAsync starting. succeeded={succeeded}; frameAllReturned={frameAllReturned}; failure={failure ?? "<none>"}");
        _statusText.Text = succeeded
            ? "Consumer smoke passed."
            : $"Consumer smoke failed: {failure}";

        var diagnostics = _view3D.BackendDiagnostics;
        var displayServerCompatibility = VideraDiagnosticsSnapshotFormatter.DescribeDisplayServerCompatibility(diagnostics);
        var report = new ConsumerSmokeReport(
            succeeded,
            frameAllReturned,
            failure,
            diagnostics.RequestedBackend.ToString(),
            diagnostics.ResolvedBackend.ToString(),
            diagnostics.IsReady,
            diagnostics.IsUsingSoftwareFallback,
            diagnostics.FallbackReason,
            diagnostics.NativeHostBound,
            diagnostics.ResolvedDisplayServer,
            diagnostics.DisplayServerFallbackUsed,
            diagnostics.DisplayServerFallbackReason,
            displayServerCompatibility,
            diagnostics.LastInitializationError,
            _diagnosticsSnapshotPath,
            _inspectionSnapshotPath,
            _inspectionBundlePath,
            _scenario,
            _packageVersion,
            _packageIds,
            ModelFormat,
            ModelPath,
            SmokeSceneFactory.EmissiveNormalProofObjectName,
            _lightingProofHoldSeconds);

        if (!string.IsNullOrWhiteSpace(_outputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath!)!);
            File.WriteAllText(_outputPath!, JsonSerializer.Serialize(report, ReportJsonOptions));
            Trace($"Wrote report: {_outputPath}");
        }

        if (!string.IsNullOrWhiteSpace(_diagnosticsSnapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_diagnosticsSnapshotPath!)!);
            File.WriteAllText(_diagnosticsSnapshotPath!, VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
            Trace($"Wrote diagnostics snapshot: {_diagnosticsSnapshotPath}");
        }

        _ = ShutdownAsync(succeeded);

        return Task.CompletedTask;
    }

    private async Task ShutdownAsync(bool succeeded)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (succeeded && _lightingProofHoldSeconds > 0)
        {
            Trace($"Lighting proof hold active for {_lightingProofHoldSeconds} seconds before shutdown.");
            _statusText.Text = $"Consumer smoke passed. Proof hold active for {_lightingProofHoldSeconds} seconds.";
            await Task.Delay(TimeSpan.FromSeconds(_lightingProofHoldSeconds)).ConfigureAwait(true);
        }

        Trace($"Posting shutdown with exit code {(succeeded ? 0 : 1)}.");
        desktop.Shutdown(succeeded ? 0 : 1);
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
            File.AppendAllText(
                _tracePath!,
                $"[{DateTimeOffset.UtcNow:O}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Best-effort diagnostics only.
        }
    }

    private sealed record ConsumerSmokeReport(
        bool Succeeded,
        bool FrameAllReturned,
        string? Failure,
        string RequestedBackend,
        string ResolvedBackend,
        bool IsReady,
        bool IsUsingSoftwareFallback,
        string? FallbackReason,
        bool NativeHostBound,
        string? ResolvedDisplayServer,
        bool DisplayServerFallbackUsed,
        string? DisplayServerFallbackReason,
        string DisplayServerCompatibility,
        string? LastInitializationError,
        string? DiagnosticsSnapshotPath,
        string? InspectionSnapshotPath,
        string? InspectionBundlePath,
        string Scenario,
        string PackageVersion,
        string[] PackageIds,
        string ConsumerModelFormat,
        string? ImportedModelPath,
        string EmissiveNormalProofObjectName,
        int LightingProofHoldSeconds);
}
