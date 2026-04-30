using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Core.Picking;

/// <summary>
/// Hit-tests pie slices by angle and radial distance.
/// </summary>
public sealed class PieProbeStrategy : ISeriesProbeStrategy
{
    private readonly PieRenderScene _scene;
    private readonly PieChartData _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="PieProbeStrategy"/> class.
    /// </summary>
    public PieProbeStrategy(PieRenderScene scene, PieChartData data)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(data);

        _scene = scene;
        _data = data;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        var px = (float)chartX;
        var pz = (float)chartZ;
        var distance = MathF.Sqrt((px * px) + (pz * pz));
        var outerRadius = _scene.SliceCount > 0 ? _scene.Slices[0].OuterRadius : 1f;

        if (distance > outerRadius * 1.1f)
        {
            return null;
        }

        var angle = MathF.Atan2(pz, px) * 180f / MathF.PI;
        if (angle < 0f) angle += 360f;

        for (var i = 0; i < _scene.SliceCount; i++)
        {
            var slice = _scene.Slices[i];
            var startAngle = slice.StartAngle % 360f;
            var endAngle = startAngle + slice.SweepAngle;

            var inRange = angle >= startAngle && angle < endAngle;
            var inRadial = distance >= slice.InnerRadius && distance <= slice.OuterRadius;

            if (!inRange || !inRadial) continue;

            var dataSlice = _data.Slices[i];
            return new SurfaceProbeInfo(
                sampleX: chartX,
                sampleY: chartZ,
                axisX: chartX,
                axisY: chartZ,
                value: dataSlice.Value,
                isApproximate: false);
        }

        return null;
    }
}
