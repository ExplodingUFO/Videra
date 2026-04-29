namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for scatter series by finding the nearest data point via brute-force XZ distance.
/// </summary>
public sealed class ScatterProbeStrategy : ISeriesProbeStrategy
{
    private readonly ScatterChartData _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatterProbeStrategy"/> class.
    /// </summary>
    /// <param name="data">The scatter chart data to probe.</param>
    public ScatterProbeStrategy(ScatterChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        ScatterPoint? nearest = null;
        var minDistSq = double.MaxValue;

        // Search point-object series
        foreach (var series in _data.Series)
        {
            foreach (var point in series.Points)
            {
                var dx = point.Horizontal - chartX;
                var dz = point.Depth - chartZ;
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = point;
                }
            }
        }

        // Search columnar series
        foreach (var series in _data.ColumnarSeries)
        {
            var xs = series.X.Span;
            var ys = series.Y.Span;
            var zs = series.Z.Span;
            for (var i = 0; i < series.Count; i++)
            {
                if (float.IsNaN(xs[i]) || float.IsNaN(ys[i]) || float.IsNaN(zs[i]))
                {
                    continue;
                }

                var dx = xs[i] - chartX;
                var dz = zs[i] - chartZ;
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nearest = new ScatterPoint(xs[i], ys[i], zs[i]);
                }
            }
        }

        if (nearest is not ScatterPoint hit)
        {
            return null;
        }

        return new SurfaceProbeInfo(
            sampleX: hit.Horizontal,
            sampleY: hit.Depth,
            axisX: hit.Horizontal,
            axisY: hit.Depth,
            value: hit.Value,
            isApproximate: false);
    }
}
