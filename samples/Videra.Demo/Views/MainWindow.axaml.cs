using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using Veldrid;
using Videra.Core.IO;
using Videra.Demo.ViewModels;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainWindowViewModel();
        DataContext = _vm;

        // 监听背景色改变，同步给 Engine
        _vm.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(MainWindowViewModel.BgColor))
            {
                var c = _vm.BgColor;
                // 转换 Avalonia Color -> Veldrid RgbaFloat
                View3D.Engine.BackgroundColor = new RgbaFloat(c.R / 255f, c.G / 255f, c.B / 255f, 1f);
            }
        };
    }

    private async void OnImportClick(object sender, RoutedEventArgs e)
    {
        // 1. 获取支持的格式
        var filters = new FilePickerFileType("3D Models")
        {
            Patterns = ModelImporter.SupportedFormats
        };

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true, // 允许选多个
            FileTypeFilter = new[] { filters }
        });

        foreach (var file in files)
        {
            var path = file.Path.LocalPath;

            // 2. 加载模型 (传入 Engine 内部的 GraphicsDevice 进行初始化)
            // 注意：Engine.InternalDevice 是我们需要暴露的一个属性
            var obj3d = await Task.Run(() => ModelImporter.Load(path, View3D.Engine.GraphicsDevice));

            // 3. 添加到 Engine 渲染列表
            View3D.Engine.AddObject(obj3d);

            // 4. 添加到 ViewModel 列表
            _vm.Models.Add(new ModelViewModel(obj3d));
        }
    }
}