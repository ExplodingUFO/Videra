using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds render-ready line segments from immutable line chart data.
/// </summary>
public static class LineRenderer
{
    /// <summary>
    /// Builds one render-ready line scene from immutable line chart data.
    /// Each adjacent point pair in a series becomes one <see cref="LineRenderSegment"/>.
    /// </summary>
    /// <param name="data">The source line chart dataset.</param>
    /// <returns>The render-ready line scene.</returns>
    public static LineRenderScene BuildScene(LineChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var segments = new List<LineRenderSegment>();

        foreach (var series in data.Series)
        {
            for (var i = 0; i < series.Points.Count - 1; i++)
            {
                var start = series.Points[i];
                var end = series.Points[i + 1];

                segments.Add(new LineRenderSegment(
                    new Vector3((float)start.Horizontal, (float)start.Value, (float)start.Depth),
                    new Vector3((float)end.Horizontal, (float)end.Value, (float)end.Depth),
                    series.Width,
                    start.Color ?? series.Color));
            }
        }

        return new LineRenderScene(data.SeriesCount, segments);
    }
}
