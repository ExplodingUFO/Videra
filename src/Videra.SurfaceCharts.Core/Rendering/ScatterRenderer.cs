using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds render-ready scatter points from immutable scatter chart data.
/// </summary>
public static class ScatterRenderer
{
    /// <summary>
    /// Builds one render-ready scatter scene from immutable scatter chart data.
    /// </summary>
    /// <param name="data">The source dataset.</param>
    /// <returns>The render-ready scatter scene.</returns>
    public static ScatterRenderScene BuildScene(ScatterChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var renderSeries = new ScatterRenderSeries[data.Series.Count + data.ColumnarSeries.Count];
        for (var index = 0; index < data.Series.Count; index++)
        {
            renderSeries[index] = BuildSeries(data.Series[index]);
        }

        for (var index = 0; index < data.ColumnarSeries.Count; index++)
        {
            renderSeries[data.Series.Count + index] = BuildColumnarSeries(data.ColumnarSeries[index]);
        }

        return new ScatterRenderScene(data.Metadata, renderSeries);
    }

    private static ScatterRenderSeries BuildSeries(ScatterSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var renderPoints = new ScatterRenderPoint[series.Points.Count];
        for (var index = 0; index < series.Points.Count; index++)
        {
            var point = series.Points[index];
            renderPoints[index] = new ScatterRenderPoint(
                new Vector3(
                    ToRenderComponent(point.Horizontal, nameof(point.Horizontal)),
                    ToRenderComponent(point.Value, nameof(point.Value)),
                    ToRenderComponent(point.Depth, nameof(point.Depth))),
                point.Color ?? series.Color);
        }

        return new ScatterRenderSeries(renderPoints, series.Color, series.Label, series.ConnectPoints);
    }

    private static ScatterRenderSeries BuildColumnarSeries(ScatterColumnarSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var renderPoints = new List<ScatterRenderPoint>(series.Count);
        var x = series.X.Span;
        var y = series.Y.Span;
        var z = series.Z.Span;

        for (var index = 0; index < series.Count; index++)
        {
            if (float.IsNaN(x[index]) || float.IsNaN(y[index]) || float.IsNaN(z[index]))
            {
                continue;
            }

            renderPoints.Add(
                new ScatterRenderPoint(
                    new Vector3(x[index], y[index], z[index]),
                    series.GetColor(index),
                    series.GetSize(index)));
        }

        return new ScatterRenderSeries(renderPoints, series.Color, series.Label);
    }

    private static float ToRenderComponent(double value, string paramName)
    {
        if (value < float.MinValue || value > float.MaxValue)
        {
            throw new ArgumentOutOfRangeException(paramName, "Scatter render coordinates must fit into single-precision render space.");
        }

        return (float)value;
    }
}
