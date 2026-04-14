namespace Videra.SurfaceCharts.Avalonia.Controls;

internal interface ISurfaceChartNativeHost
{
    event Action<IntPtr>? HandleCreated;

    event Action? HandleDestroyed;

    IntPtr CurrentHandle { get; }
}
