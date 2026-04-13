using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Videra.SurfaceCharts.Demo;

public sealed class App : Application
{
    public override void Initialize()
    {
        // Intentionally empty: the demo does not use XAML yet.
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Views.MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
