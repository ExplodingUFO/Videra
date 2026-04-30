using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a waterfall plottable.
/// </summary>
public sealed class WaterfallPlot3DSeries : Plot3DSeries
{
    internal WaterfallPlot3DSeries(string? name, ISurfaceTileSource source)
        : base(Plot3DSeriesKind.Waterfall, name, source, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null)
    {
    }
}
