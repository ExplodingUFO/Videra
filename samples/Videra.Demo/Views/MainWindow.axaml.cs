using Avalonia.Controls;
using Avalonia.Interactivity;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var factory = View3D.GetResourceFactory();

        if (topLevel != null && factory != null)
        {
            var importerService = new AvaloniaModelImporter(topLevel, factory);
            DataContext = new MainWindowViewModel(importerService);
        }
        else
        {
            // Fallback: 如果初始化失败，至少创建空的 ViewModel
            DataContext = new MainWindowViewModel(null!);
        }
    }
}