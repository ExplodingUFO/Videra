using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Probe strategy for heatmap slice series — finds the containing cell and returns its value.
/// </summary>
public sealed class HeatmapSliceProbeStrategy : ISeriesProbeStrategy
{
    private readonly HeatmapSliceRenderScene _scene;

    public HeatmapSliceProbeStrategy(HeatmapSliceRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        HeatmapSliceRenderCell? nearest = null;
        var nearestDist = double.MaxValue;

        foreach (var cell in _scene.Cells)
        {
            var dx = chartX - cell.Position.X;
            var dz = chartZ - cell.Position.Z;
            var dist = Math.Sqrt(dx * dx + dz * dz);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = cell;
            }
        }

        if (nearest is null)
        {
            return null;
        }

        var hit = nearest.Value;
        return new SurfaceProbeInfo(
            hit.Position.X,
            hit.Position.Y,
            hit.Position.Z,
            0d,
            $"Slice @ ({hit.Position.X:F2}, {hit.Position.Y:F2}, {hit.Position.Z:F2})");
    }
}
