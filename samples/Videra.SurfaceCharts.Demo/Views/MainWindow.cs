using Avalonia.Controls;
using Avalonia.Layout;

namespace Videra.SurfaceCharts.Demo.Views;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Title = "Videra Surface Charts Demo";
        Width = 1280;
        Height = 800;
        Content = new TextBlock
        {
            Text = "Surface charts demo scaffold",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}
