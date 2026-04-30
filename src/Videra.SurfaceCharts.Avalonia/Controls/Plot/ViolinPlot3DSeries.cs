using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a violin plottable.
/// </summary>
public sealed class ViolinPlot3DSeries : Plot3DSeries
{
    internal ViolinPlot3DSeries(string? name, ViolinData data)
        : base(Plot3DSeriesKind.Violin, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, violinData: data)
    {
    }
}
