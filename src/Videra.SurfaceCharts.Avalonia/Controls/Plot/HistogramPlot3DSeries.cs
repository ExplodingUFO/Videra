using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a histogram plottable.
/// </summary>
public sealed class HistogramPlot3DSeries : Plot3DSeries
{
    internal HistogramPlot3DSeries(string? name, HistogramData data)
        : base(Plot3DSeriesKind.Histogram, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, histogramData: data)
    {
    }
}
