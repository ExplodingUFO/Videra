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
            lineData: null, ribbonData: data)
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
}
