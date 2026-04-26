using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Videra.Avalonia.Controls;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    private readonly DemoSceneBootstrapper _sceneBootstrapper;
    private readonly MainWindowViewModel _viewModel;
    private bool _viewModelInitialized;

    public MainWindow()
        : this(new MainWindowViewModel(), new DemoSceneBootstrapper())
    {
    }

    public MainWindow(MainWindowViewModel viewModel, DemoSceneBootstrapper sceneBootstrapper)
    {
        _viewModel = viewModel;
        _sceneBootstrapper = sceneBootstrapper;
        InitializeComponent();
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        View3D.BackendReady -= OnBackendReady;
        View3D.BackendReady += OnBackendReady;
        View3D.BackendStatusChanged -= OnBackendStatusChanged;
        View3D.BackendStatusChanged += OnBackendStatusChanged;
        View3D.InitializationFailed -= OnInitializationFailed;
        View3D.InitializationFailed += OnInitializationFailed;
        _viewModel.UpdateBackendDiagnostics(View3D.BackendDiagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        TryInitializeScene();
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(TryInitializeScene, DispatcherPriority.Background);
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        _viewModel.SetBackendInitializationFailed(e.Exception.Message);
    }

    private void TryInitializeScene()
    {
        if (_viewModelInitialized)
        {
            return;
        }

        var initialized = _sceneBootstrapper.TryInitialize(
            _viewModel,
            TopLevel.GetTopLevel(this),
            View3D);

        if (!initialized)
        {
            return;
        }

        _viewModelInitialized = true;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        View3D.BackendReady -= OnBackendReady;
    }

    private async void OnCopyDiagnosticsBundleClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        await CopySupportTextAsync(_viewModel.DiagnosticsBundle, "Copied diagnostics bundle to the clipboard.").ConfigureAwait(true);
    }

    private async void OnCopyMinimalReproClicked(object? sender, RoutedEventArgs e)
    {
        _ = sender;
        _ = e;
        _viewModel.UpdateRenderCapabilities(View3D.RenderCapabilities);
        await CopySupportTextAsync(_viewModel.MinimalReproduction, "Copied minimal reproduction metadata to the clipboard.").ConfigureAwait(true);
    }

    private async Task CopySupportTextAsync(string text, string successMessage)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.Clipboard is null)
        {
            _viewModel.SetStatusMessage("Clipboard is unavailable.");
            return;
        }

        await topLevel.Clipboard.SetTextAsync(text).ConfigureAwait(true);
        _viewModel.SetStatusMessage(successMessage);
    }
}
