using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Videra.Avalonia.Controls;
using Videra.Core.Graphics;

namespace Videra.MinimalSample.Views;

public partial class MainWindow : Window
{
    private readonly Button _frameAllButton;
    private readonly Button _reloadButton;
    private readonly Button _resetCameraButton;
    private readonly TextBlock _diagnosticsText;
    private readonly TextBlock _statusText;
    private bool _initialSceneRequested;

    public MainWindow()
    {
        InitializeComponent();

        _frameAllButton = this.FindControl<Button>("FrameAllButton")
            ?? throw new InvalidOperationException("FrameAllButton is missing.");
        _reloadButton = this.FindControl<Button>("ReloadButton")
            ?? throw new InvalidOperationException("ReloadButton is missing.");
        _resetCameraButton = this.FindControl<Button>("ResetCameraButton")
            ?? throw new InvalidOperationException("ResetCameraButton is missing.");
        _diagnosticsText = this.FindControl<TextBlock>("DiagnosticsText")
            ?? throw new InvalidOperationException("DiagnosticsText is missing.");
        _statusText = this.FindControl<TextBlock>("StatusText")
            ?? throw new InvalidOperationException("StatusText is missing.");

        View3D.Options = new VideraViewOptions
        {
            Backend =
            {
                PreferredBackend = GraphicsBackendPreference.Auto,
                AllowSoftwareFallback = true
            }
        };

        View3D.BackendReady += OnBackendReady;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed += OnInitializationFailed;

        _frameAllButton.Click += OnFrameAllClicked;
        _resetCameraButton.Click += OnResetCameraClicked;
        _reloadButton.Click += OnReloadClicked;

        Opened += OnOpened;
        Closed += OnClosed;

        SetStatus("Waiting for backend readiness.");
        RefreshDiagnostics();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _ = TryLoadSampleSceneAsync(forceReload: false);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        Closed -= OnClosed;
        View3D.BackendReady -= OnBackendReady;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
        _frameAllButton.Click -= OnFrameAllClicked;
        _resetCameraButton.Click -= OnResetCameraClicked;
        _reloadButton.Click -= OnReloadClicked;
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        _ = TryLoadSampleSceneAsync(forceReload: false);
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        RefreshDiagnostics(e.Diagnostics);
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        SetStatus($"Backend initialization failed: {e.Exception.Message}");
        RefreshDiagnostics(e.Diagnostics);
    }

    private async Task TryLoadSampleSceneAsync(bool forceReload)
    {
        if (!View3D.BackendDiagnostics.IsReady)
        {
            RefreshDiagnostics();
            return;
        }

        if (_initialSceneRequested && !forceReload)
        {
            RefreshDiagnostics();
            return;
        }

        _initialSceneRequested = true;

        try
        {
            var result = await View3D.LoadModelAsync("Assets/reference-cube.obj").ConfigureAwait(true);
            if (!result.Succeeded)
            {
                SetStatus($"LoadModelAsync failed: {result.Failure?.ErrorMessage ?? "Unknown error."}");
                RefreshDiagnostics();
                return;
            }

            var framed = View3D.FrameAll();
            SetStatus(
                $"LoadModelAsync succeeded in {result.Duration.TotalMilliseconds:N0} ms. " +
                $"FrameAll() returned {framed}.");
            RefreshDiagnostics();
        }
        catch (Exception ex)
        {
            SetStatus($"Sample flow failed: {ex.Message}");
            RefreshDiagnostics();
        }
    }

    private void OnFrameAllClicked(object? sender, EventArgs e)
    {
        var framed = View3D.FrameAll();
        SetStatus($"FrameAll() returned {framed}.");
        RefreshDiagnostics();
    }

    private void OnResetCameraClicked(object? sender, EventArgs e)
    {
        View3D.ResetCamera();
        SetStatus("ResetCamera() restored the default view.");
        RefreshDiagnostics();
    }

    private void OnReloadClicked(object? sender, EventArgs e)
    {
        _ = TryLoadSampleSceneAsync(forceReload: true);
    }

    private void SetStatus(string message)
    {
        _statusText.Text = message;
    }

    private void RefreshDiagnostics()
    {
        RefreshDiagnostics(View3D.BackendDiagnostics);
    }

    private void RefreshDiagnostics(VideraBackendDiagnostics diagnostics)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"IsReady: {diagnostics.IsReady}");
        builder.AppendLine($"RequestedBackend: {diagnostics.RequestedBackend}");
        builder.AppendLine($"ResolvedBackend: {diagnostics.ResolvedBackend}");
        builder.AppendLine($"ResolvedDisplayServer: {diagnostics.ResolvedDisplayServer ?? "Unavailable"}");
        builder.AppendLine($"IsUsingSoftwareFallback: {diagnostics.IsUsingSoftwareFallback}");
        builder.AppendLine($"FallbackReason: {diagnostics.FallbackReason ?? "None"}");
        builder.AppendLine($"NativeHostBound: {diagnostics.NativeHostBound}");
        builder.AppendLine($"LastInitializationError: {diagnostics.LastInitializationError ?? "None"}");
        _diagnosticsText.Text = builder.ToString();
    }
}
