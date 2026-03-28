using Avalonia;
using System;
using Avalonia.Win32;
using System.Runtime.InteropServices;
using Videra.Core.Graphics;

namespace Videra.Demo
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => ConfigurePlatformOptions(AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace());

        private static AppBuilder ConfigurePlatformOptions(AppBuilder builder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Environment.SetEnvironmentVariable("VIDERA_BACKEND", GraphicsBackendPreference.D3D11.ToString().ToLowerInvariant());

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
