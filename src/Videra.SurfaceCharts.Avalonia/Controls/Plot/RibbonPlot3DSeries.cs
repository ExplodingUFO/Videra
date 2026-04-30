using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a ribbon plottable.
/// </summary>
public sealed class RibbonPlot3DSeries : Plot3DSeries
{
    internal RibbonPlot3DSeries(string? name, RibbonChartData data)
        : base(Plot3DSeriesKind.Ribbon, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: null, ribbonData: data,
            vectorFieldData: null, heatmapSliceData: null, boxPlotData: null)
    {
    }

    /// <summary>
    /// Updates the ARGB color for all ribbon series.
    /// </summary>
    /// <param name="color">The replacement ARGB color.</param>
    public void SetColor(uint color)
    {
        var data = RibbonData ?? throw new InvalidOperationException("Ribbon series requires ribbon data.");
        var series = data.Series.ToArray();
        for (var i = 0; i < series.Length; i++)
        {
            var s = series[i];
            series[i] = new RibbonSeries(s.Points, s.Radius, color, s.Label);
        }

        ReplaceRibbonData(new RibbonChartData(series, data.Metadata));
    }

    /// <summary>
    /// Updates the tube radius for all ribbon series.
    /// </summary>
    /// <param name="radius">The replacement tube radius.</param>
    public void SetRadius(float radius)
    {
        var data = RibbonData ?? throw new InvalidOperationException("Ribbon series requires ribbon data.");
        var series = data.Series.ToArray();
        for (var i = 0; i < series.Length; i++)
        {
            var s = series[i];
            series[i] = new RibbonSeries(s.Points, radius, s.Color, s.Label);
        }

        ReplaceRibbonData(new RibbonChartData(series, data.Metadata));
    }

    /// <summary>
    /// Applies a color map to each point in the ribbon series, setting per-point colors
    /// based on the point's value (Y coordinate) mapped through the color map.
    /// </summary>
    /// <param name="colorMap">The color map to apply. Must not be null.</param>
    public void SetColormap(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        var data = RibbonData ?? throw new InvalidOperationException("Ribbon series requires ribbon data.");
        var series = data.Series.ToArray();
        for (var i = 0; i < series.Length; i++)
        {
            var s = series[i];
            var coloredPoints = new ScatterPoint[s.Points.Count];
            for (var j = 0; j < s.Points.Count; j++)
            {
                var pt = s.Points[j];
                coloredPoints[j] = new ScatterPoint(pt.Horizontal, pt.Value, pt.Depth, colorMap.Map(pt.Value));
            }
            series[i] = new RibbonSeries(coloredPoints, s.Radius, s.Color, s.Label);
        }
        ReplaceRibbonData(new RibbonChartData(series, data.Metadata));
    }
}
