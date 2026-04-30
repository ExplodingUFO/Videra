namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for histogram series by hit-testing bin bounding boxes.
/// </summary>
public sealed class HistogramProbeStrategy : ISeriesProbeStrategy
{
    private readonly HistogramRenderScene _scene;
    private readonly HistogramData _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistogramProbeStrategy"/> class.
    /// </summary>
    public HistogramProbeStrategy(HistogramRenderScene scene, HistogramData data)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(data);
        _scene = scene;
        _data = data;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        HistogramRenderBin? hitBin = null;
        var minDistSq = double.MaxValue;

        foreach (var bin in _scene.Bins)
        {
            var halfWidth = bin.Size.X / 2f;
            var dx = chartX - (bin.Position.X + halfWidth);
            var dz = chartZ - bin.Position.Z;

            if (Math.Abs(dx) <= halfWidth && Math.Abs(dz) <= bin.Size.Z / 2f)
            {
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    hitBin = bin;
                }
            }
        }

        if (hitBin is not HistogramRenderBin hit)
        {
            return null;
        }

        var binIndex = -1;
        for (var i = 0; i < _scene.Bins.Count; i++)
        {
            if (_scene.Bins[i].Equals(hit))
            {
                binIndex = i;
                break;
            }
        }

        var value = binIndex >= 0 && binIndex < _data.Bins.Count ? _data.Bins[binIndex] : 0;

        return new SurfaceProbeInfo(
            sampleX: hit.Position.X + (hit.Size.X / 2f),
            sampleY: hit.Position.Z,
            axisX: hit.Position.X + (hit.Size.X / 2f),
            axisY: hit.Position.Z,
            value: value,
            isApproximate: false);
    }
}
