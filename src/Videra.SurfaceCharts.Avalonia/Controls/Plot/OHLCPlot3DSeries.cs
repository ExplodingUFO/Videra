using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for an OHLC/Candlestick plottable.
/// </summary>
public sealed class OHLCPlot3DSeries : Plot3DSeries
{
    internal OHLCPlot3DSeries(string? name, OHLCData data)
        : base(Plot3DSeriesKind.OHLC, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, ohlcData: data)
    {
    }
}
