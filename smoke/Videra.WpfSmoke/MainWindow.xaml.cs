using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Videra.Core.Graphics;
using Videra.Core.Rendering;
using Videra.Platform.Windows;

namespace Videra.WpfSmoke;

public partial class MainWindow : Window
{
    private readonly VideraEngine _engine = new();
    private readonly RenderSessionOrchestrator _orchestrator;
    private readonly string _diagnosticsPath;
    private readonly DispatcherTimer _timeoutTimer;
    private bool _completed;

    public MainWindow()
    {
        InitializeComponent();

        _orchestrator = new RenderSessionOrchestrator(
            _engine,
            backendFactory: preference => preference == GraphicsBackendPreference.D3D11
                ? new D3D11Backend()
                : throw new NotSupportedException($"Unsupported backend preference {preference} for the WPF smoke host."));

        _diagnosticsPath = ResolveDiagnosticsPath();
        _timeoutTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _timeoutTimer.Tick += OnTimeoutElapsed;

        Loaded += OnLoaded;
        ContentRendered += OnContentRendered;
        Closed += OnClosed;

        StatusText.Text = "Waiting for the WPF host HWND.";
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _timeoutTimer.Start();
        StatusText.Text = "Waiting for layout and native host creation.";
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        TryRenderFirstFrame();
    }

    private void TryRenderFirstFrame()
    {
        if (_completed)
        {
            return;
        }

        try
        {
            var handle = ViewerHost.NativeHandle;
            if (handle == IntPtr.Zero)
            {
                StatusText.Text = "Waiting for the child HWND.";
                _ = Dispatcher.InvokeAsync(TryRenderFirstFrame, DispatcherPriority.Background);
                return;
            }

            var renderMetrics = GetRenderMetrics();

            _orchestrator.Attach(GraphicsBackendPreference.D3D11);
            _orchestrator.BindHandle(handle);
            _orchestrator.Resize(renderMetrics.Width, renderMetrics.Height, renderMetrics.RenderScale);

            if (!_orchestrator.IsReady)
            {
                StatusText.Text = $"Waiting for ready state: {_orchestrator.Snapshot.State}.";
                _ = Dispatcher.InvokeAsync(TryRenderFirstFrame, DispatcherPriority.Background);
                return;
            }

            StatusText.Text = "Backend ready. Rendering the first frame.";
            var result = _orchestrator.RenderOnce();
            if (result.Faulted)
            {
                throw result.Error ?? new InvalidOperationException("RenderOnce faulted without an exception.");
            }

            if (_orchestrator.Snapshot.LastPipelineSnapshot == null)
            {
                throw new InvalidOperationException("Render completed without a pipeline snapshot.");
            }

            WriteDiagnosticsSnapshot(renderMetrics.Width, renderMetrics.Height, renderMetrics.RenderScale, handle);
            Complete(true, null);
        }
        catch (Exception ex)
        {
            var renderMetrics = GetRenderMetrics();
            WriteDiagnosticsSnapshot(
                width: renderMetrics.Width,
                height: renderMetrics.Height,
                renderScale: renderMetrics.RenderScale,
                handle: ViewerHost.NativeHandle,
                failure: ex.Message);
            Complete(false, ex.Message);
        }
    }

    private void WriteDiagnosticsSnapshot(uint width, uint height, float renderScale, IntPtr handle, string? failure = null)
    {
        var snapshot = _orchestrator.Snapshot;
        var pipelineStages = snapshot.LastPipelineSnapshot is { StageNames: var stageNames }
            ? string.Join(", ", stageNames)
            : string.Empty;
        var diagnostics = new StringBuilder()
            .AppendLine("Videra WPF smoke diagnostics")
            .AppendLine($"State: {snapshot.State}")
            .AppendLine($"HandleState: {snapshot.HandleState}")
            .AppendLine($"RequestedBackend: {snapshot.Inputs.RequestedBackend}")
            .AppendLine($"ResolvedBackend: {snapshot.LastBackendResolution?.ResolvedPreference}")
            .AppendLine($"SoftwareFallback: {snapshot.LastBackendResolution?.IsUsingSoftwareFallback}")
            .AppendLine($"Width: {width}")
            .AppendLine($"Height: {height}")
            .AppendLine($"RenderScale: {renderScale:0.###}")
            .AppendLine($"Handle: 0x{handle.ToInt64():X}")
            .AppendLine($"Ready: {snapshot.State == RenderSessionState.Ready}")
            .AppendLine($"PipelineProfile: {snapshot.LastPipelineSnapshot?.Profile}")
            .AppendLine($"PipelineStages: {pipelineStages}")
            .AppendLine($"LastInitializationError: {snapshot.LastInitializationError?.Message ?? "<none>"}");

        if (!string.IsNullOrWhiteSpace(snapshot.LastBackendResolution?.FallbackReason))
        {
            diagnostics.AppendLine($"FallbackReason: {snapshot.LastBackendResolution.FallbackReason}");
        }

        if (!string.IsNullOrWhiteSpace(failure))
        {
            diagnostics.AppendLine($"Failure: {failure}");
        }

        var diagnosticsDirectory = Path.GetDirectoryName(_diagnosticsPath);
        if (!string.IsNullOrWhiteSpace(diagnosticsDirectory))
        {
            Directory.CreateDirectory(diagnosticsDirectory);
        }

        File.WriteAllText(_diagnosticsPath, diagnostics.ToString());
    }

    private void Complete(bool succeeded, string? failure)
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _timeoutTimer.Stop();
        StatusText.Text = succeeded
            ? $"WPF smoke passed. Diagnostics: {_diagnosticsPath}"
            : $"WPF smoke failed: {failure}";

        _ = Dispatcher.InvokeAsync(
            () => Application.Current?.Shutdown(succeeded ? 0 : 1),
            DispatcherPriority.Background);
    }

    private void OnTimeoutElapsed(object? sender, EventArgs e)
    {
        _timeoutTimer.Stop();

        if (_completed)
        {
            return;
        }

        try
        {
            var renderMetrics = GetRenderMetrics();
            WriteDiagnosticsSnapshot(
                renderMetrics.Width,
                renderMetrics.Height,
                renderMetrics.RenderScale,
                ViewerHost.NativeHandle,
                failure: "Timed out before reaching a ready and rendered state.");
        }
        catch
        {
            // Best-effort diagnostics only.
        }

        Complete(false, "Timed out before reaching a ready and rendered state.");
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _timeoutTimer.Stop();
        _orchestrator.Dispose();
        _engine.Dispose();
    }

    private (uint Width, uint Height, float RenderScale) GetRenderMetrics()
    {
        var dpi = VisualTreeHelper.GetDpi(ViewerHost);
        var width = Math.Max(1u, (uint)Math.Round(ViewerHost.ActualWidth * dpi.DpiScaleX));
        var height = Math.Max(1u, (uint)Math.Round(ViewerHost.ActualHeight * dpi.DpiScaleY));
        return (width, height, (float)dpi.DpiScaleX);
    }

    private static string ResolveDiagnosticsPath()
    {
        var configuredPath = Environment.GetEnvironmentVariable("VIDERA_WPF_SMOKE_OUTPUT");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(AppContext.BaseDirectory, "wpf-smoke-diagnostics.txt");
    }
}
