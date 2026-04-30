using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a contour plottable.
/// </summary>
public sealed class ContourPlot3DSeries : Plot3DSeries
{
    internal ContourPlot3DSeries(string? name, ContourChartData data)
        : base(Plot3DSeriesKind.Contour, name, surfaceSource: null, scatterData: null, barData: null, data, lineData: null, ribbonData: null)
    {
    }
}
