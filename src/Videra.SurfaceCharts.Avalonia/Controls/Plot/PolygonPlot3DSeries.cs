using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a polygon plottable.
/// </summary>
public sealed class PolygonPlot3DSeries : Plot3DSeries
{
    internal PolygonPlot3DSeries(string? name, PolygonData data)
        : base(Plot3DSeriesKind.Polygon, name, surfaceSource: null, scatterData: null, barData: null, contourData: null, lineData: null, ribbonData: null, vectorFieldData: null, heatmapSliceData: null, boxPlotData: null, polygonData: data)
    {
    }
}
