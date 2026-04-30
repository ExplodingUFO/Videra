using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Probe strategy for box plot series — finds the nearest category by X position.
/// </summary>
public sealed class BoxPlotProbeStrategy : ISeriesProbeStrategy
{
    private readonly BoxPlotRenderScene _scene;

    public BoxPlotProbeStrategy(BoxPlotRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        if (_scene.CategoryCount == 0)
        {
            return null;
        }

        // Find nearest category by X position
        var bestIndex = 0;
        var bestDist = double.MaxValue;
        for (var i = 0; i < _scene.CategoryCount; i++)
        {
            var dist = Math.Abs(chartX - i);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestDist > 1.0)
        {
            return null;
        }

        // Find the box for this category
        BoxPlotRenderBox? box = null;
        foreach (var b in _scene.Boxes)
        {
            if (Math.Abs(b.Position.X - bestIndex) < 0.01)
            {
                box = b;
                break;
            }
        }

        if (box is null)
        {
            return null;
        }

        var medianY = box.Value.Position.Y + box.Value.Size.Y / 2;
        return new SurfaceProbeInfo(
            sampleX: box.Value.Position.X,
            sampleY: box.Value.Position.Z,
            axisX: box.Value.Position.X,
            axisY: box.Value.Position.Z,
            value: medianY,
            isApproximate: false);
    }
}
