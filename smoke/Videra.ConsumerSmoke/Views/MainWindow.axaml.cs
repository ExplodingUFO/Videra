using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;

namespace Videra.ConsumerSmoke.Views;

public partial class MainWindow : Window
{
    private readonly VideraView _view3D;
    private readonly TextBlock _statusText;
    private readonly string? _outputPath;
    private readonly string? _diagnosticsSnapshotPath;
    private bool _completed;
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

        _view3D.Options = new VideraViewOptions
        {
            Backend =
            {
                PreferredBackend = GraphicsBackendPreference.Auto,
                AllowSoftwareFallback = true
            }
        };

        _backendReadyHandler = (_, _) => _ = TryExecuteSmokeAsync();
        _openedHandler = (_, _) =>
        {
            _statusText.Text = "Waiting for backend readiness.";
            _ = RunTimeoutGuardAsync();
            _ = TryExecuteSmokeAsync();
        };

        _view3D.BackendReady += _backendReadyHandler;
        _view3D.BackendStatusChanged += (_, e) =>
        {
            _statusText.Text =
                $"IsReady={e.Diagnostics.IsReady}; ResolvedBackend={e.Diagnostics.ResolvedBackend}; " +
                $"ResolvedDisplayServer={e.Diagnostics.ResolvedDisplayServer ?? "Unavailable"}";
        };
        _view3D.InitializationFailed += (_, e) =>
        {
            CompleteAsync(
                succeeded: false,
                frameAllReturned: false,
                failure: e.Exception.Message);
        };

        Opened += _openedHandler;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task TryExecuteSmokeAsync()
    {
        if (_completed || !_view3D.BackendDiagnostics.IsReady)
        {
            return;
        }

        try
        {
            var result = await _view3D.LoadModelAsync("Assets/reference-cube.obj").ConfigureAwait(true);
            if (!result.Succeeded)
            {
                await CompleteAsync(
                    succeeded: false,
                    frameAllReturned: false,
                    failure: result.Failure?.ErrorMessage ?? "LoadModelAsync returned an unknown error.").ConfigureAwait(true);
                return;
            }

            var framed = _view3D.FrameAll();
            _view3D.ResetCamera();

            await CompleteAsync(
                succeeded: framed,
                frameAllReturned: framed,
                failure: framed ? null : "FrameAll() returned false.").ConfigureAwait(true);
        }
        catch (Exception ex)
        {
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
        _statusText.Text = succeeded
            ? "Consumer smoke passed."
            : $"Consumer smoke failed: {failure}";

        var diagnostics = _view3D.BackendDiagnostics;
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
            diagnostics.LastInitializationError,
            _diagnosticsSnapshotPath);

        if (!string.IsNullOrWhiteSpace(_outputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_outputPath!)!);
            File.WriteAllText(_outputPath!, JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        if (!string.IsNullOrWhiteSpace(_diagnosticsSnapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_diagnosticsSnapshotPath!)!);
            File.WriteAllText(_diagnosticsSnapshotPath!, VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown(succeeded ? 0 : 1);
            }
        }, DispatcherPriority.Background);

        return Task.CompletedTask;
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
        string? LastInitializationError,
        string? DiagnosticsSnapshotPath);
}
