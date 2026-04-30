using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Probe strategy for vector field series — finds the nearest arrow by XZ distance.
/// </summary>
public sealed class VectorFieldProbeStrategy : ISeriesProbeStrategy
{
    private readonly VectorFieldRenderScene _scene;
    private const double SnapRadius = 0.1;

    public VectorFieldProbeStrategy(VectorFieldRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        VectorFieldRenderArrow? nearest = null;
        var nearestDist = double.MaxValue;

        foreach (var arrow in _scene.Arrows)
        {
            var dx = chartX - arrow.Position.X;
            var dz = chartZ - arrow.Position.Z;
            var dist = Math.Sqrt(dx * dx + dz * dz);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = arrow;
            }
        }

        if (nearest is null || nearestDist > SnapRadius)
        {
            return null;
        }

        var hit = nearest.Value;
        return new SurfaceProbeInfo(
            sampleX: hit.Position.X,
            sampleY: hit.Position.Z,
            axisX: hit.Position.X,
            axisY: hit.Position.Z,
            value: hit.Magnitude,
            isApproximate: false);
    }
}
