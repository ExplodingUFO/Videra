using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed class SurfaceChartTileRequestFailedEventArgs : EventArgs
{
    public SurfaceChartTileRequestFailedEventArgs(SurfaceTileKey tileKey, Exception exception)
    {
        TileKey = tileKey;
        Exception = exception;
    }

    public SurfaceTileKey TileKey { get; }

    public Exception Exception { get; }
}
