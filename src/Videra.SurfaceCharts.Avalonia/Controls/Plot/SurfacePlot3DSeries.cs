using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a surface plottable.
/// </summary>
public sealed class SurfacePlot3DSeries : Plot3DSeries
{
    internal SurfacePlot3DSeries(string? name, ISurfaceTileSource source)
        : base(Plot3DSeriesKind.Surface, name, source, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null)
    {
    }
}
