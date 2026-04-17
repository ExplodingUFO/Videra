using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Win32;

namespace Videra.MinimalSample;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => ConfigurePlatformOptions(AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace());

    private static AppBuilder ConfigurePlatformOptions(AppBuilder builder)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return builder;
        }

        return builder.With(new Win32PlatformOptions
        {
            CompositionMode =
            [
                Win32CompositionMode.RedirectionSurface
            ],
            RenderingMode =
            [
                Win32RenderingMode.Software
            ]
        });
    }
}
