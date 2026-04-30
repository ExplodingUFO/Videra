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

    /// <summary>
    /// Updates the ARGB color for one bar data series.
    /// </summary>
    /// <param name="seriesIndex">The zero-based bar series index.</param>
    /// <param name="color">The replacement ARGB color.</param>
    public void SetSeriesColor(int seriesIndex, uint color)
    {
        var data = BarData ?? throw new InvalidOperationException("Bar series requires bar data.");
        ArgumentOutOfRangeException.ThrowIfNegative(seriesIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(seriesIndex, data.Series.Count);

        var current = data.Series[seriesIndex];
        if (current.Color == color)
        {
            return;
        }

        var series = data.Series.ToArray();
        series[seriesIndex] = new BarSeries(current.Values, color, current.Label);
        ReplaceBarData(data.CategoryLabels.Count > 0
            ? new BarChartData(series, data.CategoryLabels, data.Layout)
            : new BarChartData(series, data.Layout));
    }
}
