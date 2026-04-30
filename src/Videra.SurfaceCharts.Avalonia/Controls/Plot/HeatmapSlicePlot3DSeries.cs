using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a heatmap slice plottable.
/// </summary>
public sealed class HeatmapSlicePlot3DSeries : Plot3DSeries
{
    internal HeatmapSlicePlot3DSeries(string? name, HeatmapSliceData data)
        : base(Plot3DSeriesKind.HeatmapSlice, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: null,
            vectorFieldData: null, heatmapSliceData: data, boxPlotData: null)
    {
    }

    /// <summary>
    /// Updates the normalized slice position (0..1).
    /// </summary>
    /// <param name="position">The new slice position.</param>
    public void SetPosition(double position)
    {
        var data = HeatmapSliceData ?? throw new InvalidOperationException("Heatmap slice series requires heatmap slice data.");
        ReplaceHeatmapSliceData(new HeatmapSliceData(data.Field, data.Axis, position, data.ColorMap));
    }

    /// <summary>
    /// Updates the color map for the heatmap slice.
    /// </summary>
    /// <param name="colorMap">The new color map.</param>
    public void SetColorMap(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        var data = HeatmapSliceData ?? throw new InvalidOperationException("Heatmap slice series requires heatmap slice data.");
        ReplaceHeatmapSliceData(new HeatmapSliceData(data.Field, data.Axis, data.Position, colorMap));
    }
}
