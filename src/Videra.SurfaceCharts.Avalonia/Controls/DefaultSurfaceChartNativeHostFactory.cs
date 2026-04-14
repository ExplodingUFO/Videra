namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed class DefaultSurfaceChartNativeHostFactory : ISurfaceChartNativeHostFactory
{
    public ISurfaceChartNativeHost? CreateHost()
    {
        if (OperatingSystem.IsWindows())
        {
            return new SurfaceChartNativeHost();
        }

        if (OperatingSystem.IsLinux())
        {
            return new SurfaceChartLinuxNativeHost();
        }

        if (OperatingSystem.IsMacOS())
        {
            return new SurfaceChartMacOSNativeHost();
        }

        return null;
    }
}
