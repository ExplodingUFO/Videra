using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a function plot plottable.
/// </summary>
public sealed class FunctionPlot3DSeries : Plot3DSeries
{
    internal FunctionPlot3DSeries(string? name, FunctionPlotData data)
        : base(Plot3DSeriesKind.FunctionPlot, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, functionPlotData: data)
    {
    }
}
