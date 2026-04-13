using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Provides data for the <see cref="SurfaceChartView.TileRequestFailed"/> event.
/// </summary>
public sealed class SurfaceChartTileRequestFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartTileRequestFailedEventArgs"/> class.
    /// </summary>
    /// <param name="tileKey">The tile key that failed to load.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    public SurfaceChartTileRequestFailedEventArgs(SurfaceTileKey tileKey, Exception exception)
    {
        TileKey = tileKey;
        Exception = exception;
    }

    /// <summary>
    /// Gets the tile key that failed to load.
    /// </summary>
    public SurfaceTileKey TileKey { get; }

    /// <summary>
    /// Gets the exception that caused the failure.
    /// </summary>
    public Exception Exception { get; }
}
