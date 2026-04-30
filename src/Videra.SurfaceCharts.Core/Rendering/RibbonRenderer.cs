using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds render-ready ribbon segments from immutable ribbon chart data.
/// </summary>
public static class RibbonRenderer
{
    /// <summary>
    /// The default number of sides for the tube cross-section polygon.
    /// </summary>
    private const int DefaultSides = 8;

    /// <summary>
    /// Builds one render-ready ribbon scene from immutable ribbon chart data.
    /// Each adjacent point pair in a series becomes one <see cref="RibbonRenderSegment"/>.
    /// </summary>
    /// <param name="data">The source ribbon chart dataset.</param>
    /// <returns>The render-ready ribbon scene.</returns>
    public static RibbonRenderScene BuildScene(RibbonChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var segments = new List<RibbonRenderSegment>();

        foreach (var series in data.Series)
        {
            for (var i = 0; i < series.Points.Count - 1; i++)
            {
                var start = series.Points[i];
                var end = series.Points[i + 1];

                segments.Add(new RibbonRenderSegment(
                    new Vector3((float)start.Horizontal, (float)start.Value, (float)start.Depth),
                    new Vector3((float)end.Horizontal, (float)end.Value, (float)end.Depth),
                    series.Radius,
                    DefaultSides,
                    start.Color ?? series.Color));
            }
        }

        return new RibbonRenderScene(data.SeriesCount, segments);
    }
}
