using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a scatter plottable.
/// </summary>
public sealed class ScatterPlot3DSeries : Plot3DSeries
{
    internal ScatterPlot3DSeries(string? name, ScatterChartData data)
        : base(Plot3DSeriesKind.Scatter, name, surfaceSource: null, data, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null)
    {
    }
}
