using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Linq;
using System.Threading.Tasks;
using Veldrid;
using Videra.Core.IO;
using Videra.Demo.Services;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // 等待 View 加载完毕，确保能获取到 TopLevel 和 Engine
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender,RoutedEventArgs e)
    {
        // 1. 获取 TopLevel (用于文件弹窗)
        var topLevel = TopLevel.GetTopLevel(this);

        // 2. 获取 Engine (用于创建显存资源)
        // 注意：View3D 是 XAML 中定义的 VideraView 的 Name
        var engine = View3D.Engine;

        if (topLevel != null && engine != null)
        {
            // 3. 创建服务实现
            var importerService = new AvaloniaModelImporter(topLevel, engine);

            // 4. 创建 ViewModel 并注入服务
            DataContext = new MainWindowViewModel(importerService);
        }
    }
}