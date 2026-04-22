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

        var renderSeries = new ScatterRenderSeries[data.Series.Count];
        for (var index = 0; index < data.Series.Count; index++)
        {
            renderSeries[index] = BuildSeries(data.Series[index]);
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

        return new ScatterRenderSeries(renderPoints, series.Label, series.ConnectPoints);
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
