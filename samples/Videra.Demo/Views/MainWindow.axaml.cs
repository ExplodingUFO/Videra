using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Videra.Core.Graphics;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    private bool _viewModelInitialized;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        View3D.BackendReady += OnBackendReady;
        TryInitializeViewModel();
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(TryInitializeViewModel, DispatcherPriority.Background);
    }

    private void TryInitializeViewModel()
    {
        if (_viewModelInitialized)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        var factory = View3D.GetResourceFactory();
        var backendLabel = View3D.PreferredBackend == GraphicsBackendPreference.Auto
            ? "Auto (Windows native preferred)"
            : View3D.PreferredBackend.ToString();

        if (DataContext is not MainWindowViewModel viewModel)
        {
            viewModel = new MainWindowViewModel(null!);
            DataContext = viewModel;
        }

        if (topLevel == null || factory == null)
        {
            viewModel.SetBackendStatus(false, backendLabel, "等待渲染后端和资源工厂准备完成...");
            return;
        }

        var importerService = new AvaloniaModelImporter(topLevel, factory);
        viewModel = new MainWindowViewModel(importerService);
        DataContext = viewModel;
        viewModel.SetBackendStatus(true, backendLabel, "渲染后端已就绪，已加载默认演示立方体。Windows 下优先验证 D3D11 路径。");

        try
        {
            var cube = DemoMeshFactory.CreateCube(factory);
            viewModel.SceneObjects.Add(cube);
            viewModel.SelectedObject = cube;
        }
        catch (Exception ex)
        {
            viewModel.SetStatusMessage($"默认演示模型创建失败：{ex.Message}");
        }

        _viewModelInitialized = true;
        View3D.BackendReady -= OnBackendReady;
    }
}
