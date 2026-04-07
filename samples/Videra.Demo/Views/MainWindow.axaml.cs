using System;
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
        TryInitializeScene();
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(TryInitializeScene, DispatcherPriority.Background);
    }

    private void OnBackendStatusChanged(object? sender, VideraBackendStatusChangedEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
    }

    private void OnInitializationFailed(object? sender, VideraBackendFailureEventArgs e)
    {
        _viewModel.UpdateBackendDiagnostics(e.Diagnostics);
        _viewModel.SetStatusMessage($"Backend initialization failed: {e.Exception.Message}");
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
        View3D.BackendReady -= OnBackendReady;
    }
}
