using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Videra.Core.IO;

namespace Videra.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void LoadBtn_Click(object sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false });
        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            // 核心功能调用：IO -> Engine
            var mesh = await Task.Run(() => ModelImporter.Load(path));
            View3D.Engine.UpdateMesh(mesh);
        }
    }

    private void OnAxisToggled(object sender, RoutedEventArgs e)
    {
        if (View3D?.Engine != null && sender is CheckBox cb)
        {
            View3D.Engine.ShowAxis = cb.IsChecked ?? false;
        }
    }
}