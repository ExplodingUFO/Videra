using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        Console.WriteLine("[MainWindow] OnLoaded event triggered");

        View3D.BackendReady += OnBackendReady;
        TryInitializeViewModel();
    }

    private void OnBackendReady(object? sender, EventArgs e)
    {
        Console.WriteLine("[MainWindow] BackendReady received");
        Dispatcher.UIThread.Post(TryInitializeViewModel, DispatcherPriority.Background);
    }

    private void TryInitializeViewModel()
    {
        if (_viewModelInitialized)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        var factory = View3D.GetResourceFactory();
        Console.WriteLine($"[MainWindow] TopLevel: {topLevel != null}, Factory: {factory != null}");

        if (topLevel == null || factory == null)
        {
            if (DataContext == null)
            {
                DataContext = new MainWindowViewModel(null!);
                Console.WriteLine("[MainWindow] ViewModel created without importer service");
            }
            return;
        }

        var importerService = new AvaloniaModelImporter(topLevel, factory);
        var viewModel = new MainWindowViewModel(importerService);
        DataContext = viewModel;
        Console.WriteLine("[MainWindow] ViewModel created with importer service");

        try
        {
            var cube = DemoMeshFactory.CreateCube(factory);
            viewModel.SceneObjects.Add(cube);
            viewModel.SelectedObject = cube;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MainWindow] Demo cube creation failed: {ex.Message}");
        }

        _viewModelInitialized = true;
        View3D.BackendReady -= OnBackendReady;
    }
}
