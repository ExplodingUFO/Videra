using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a bar plottable.
/// </summary>
public sealed class BarPlot3DSeries : Plot3DSeries
{
    internal BarPlot3DSeries(string? name, BarChartData data)
        : base(Plot3DSeriesKind.Bar, name, surfaceSource: null, scatterData: null, data, contourData: null)
    {
    }
}
