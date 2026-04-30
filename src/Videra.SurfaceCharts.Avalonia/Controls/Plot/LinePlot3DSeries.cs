using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Host-facing handle for a line plottable.
/// </summary>
public sealed class LinePlot3DSeries : Plot3DSeries
{
    internal LinePlot3DSeries(string? name, LineChartData data)
        : base(Plot3DSeriesKind.Line, name,
            surfaceSource: null, scatterData: null, barData: null, contourData: null,
            lineData: data, ribbonData: null,
            vectorFieldData: null, heatmapSliceData: null, boxPlotData: null)
    {
    }

    /// <summary>
    /// Updates the ARGB color for all line series.
    /// </summary>
    /// <param name="color">The replacement ARGB color.</param>
    public void SetColor(uint color)
    {
        var data = LineData ?? throw new InvalidOperationException("Line series requires line data.");
        if (data.Series.Count == 1 && data.Series[0].Color == color)
        {
            return;
        }

        var series = data.Series.ToArray();
        for (var i = 0; i < series.Length; i++)
        {
            var s = series[i];
            series[i] = new LineSeries(s.Points, color, s.Width, s.Label);
        }

        ReplaceLineData(new LineChartData(series, data.Metadata));
    }

    /// <summary>
    /// Updates the line width for all line series.
    /// </summary>
    /// <param name="width">The replacement line width in pixels.</param>
    public void SetWidth(float width)
    {
        var data = LineData ?? throw new InvalidOperationException("Line series requires line data.");
        var series = data.Series.ToArray();
        for (var i = 0; i < series.Length; i++)
        {
            var s = series[i];
            series[i] = new LineSeries(s.Points, s.Color, width, s.Label);
        }

        ReplaceLineData(new LineChartData(series, data.Metadata));
    }

    /// <summary>
    /// Applies a color map to each point in the line series, setting per-point colors
    /// based on the point's value (Y coordinate) mapped through the color map.
    /// </summary>
    /// <param name="colorMap">The color map to apply. Must not be null.</param>
    public void SetColormap(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);
        var data = LineData ?? throw new InvalidOperationException("Line series requires line data.");
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
            series[i] = new LineSeries(coloredPoints, s.Color, s.Width, s.Label);
        }
        ReplaceLineData(new LineChartData(series, data.Metadata));
    }
}
