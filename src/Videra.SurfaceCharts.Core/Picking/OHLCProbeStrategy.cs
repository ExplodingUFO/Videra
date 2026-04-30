using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Core.Picking;

/// <summary>
/// Hit-tests OHLC bars by X position and Y range.
/// </summary>
public sealed class OHLCProbeStrategy : ISeriesProbeStrategy
{
    private readonly OHLCRenderScene _scene;
    private readonly OHLCData _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="OHLCProbeStrategy"/> class.
    /// </summary>
    public OHLCProbeStrategy(OHLCRenderScene scene, OHLCData data)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(data);

        _scene = scene;
        _data = data;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        OHLCRenderBar? hitBar = null;
        var minDistSq = double.MaxValue;

        foreach (var bar in _scene.Bars)
        {
            var halfWidth = (bar.BodyMax.X - bar.BodyMin.X) / 2f;
            var centerX = bar.BodyMin.X + halfWidth;
            var dx = chartX - centerX;

            if (Math.Abs(dx) <= halfWidth && chartZ >= bar.WickBottom.Y && chartZ <= bar.WickTop.Y)
            {
                var distSq = dx * dx;
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    hitBar = bar;
                }
            }
        }

        if (hitBar is not OHLCRenderBar hit)
        {
            return null;
        }

        var barIndex = -1;
        for (var i = 0; i < _scene.Bars.Count; i++)
        {
            if (_scene.Bars[i].Equals(hit))
            {
                barIndex = i;
                break;
            }
        }

        var value = barIndex >= 0 && barIndex < _data.Bars.Count ? _data.Bars[barIndex].Close : 0d;

        return new SurfaceProbeInfo(
            sampleX: chartX,
            sampleY: chartZ,
            axisX: chartX,
            axisY: chartZ,
            value: value,
            isApproximate: false);
    }
}
