using Avalonia;
using System;
using Avalonia.Win32;
using System.Runtime.InteropServices;

namespace Videra.Demo
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => ConfigurePlatformOptions(AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace());

        private static AppBuilder ConfigurePlatformOptions(AppBuilder builder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder = builder.With(new Win32PlatformOptions
                {
                    CompositionMode = new[]
                    {
                        Win32CompositionMode.RedirectionSurface
                    },
                    RenderingMode = new[]
                    {
                        Win32RenderingMode.Software
                    }
                });
            }

            return builder;
        }
    }
}
