using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// A pie/donut chart series.
/// </summary>
public sealed class PiePlot3DSeries : Plot3DSeries
{
    internal PiePlot3DSeries(string? name, PieChartData data)
        : base(Plot3DSeriesKind.Pie, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, pieData: data)
    {
    }
}
