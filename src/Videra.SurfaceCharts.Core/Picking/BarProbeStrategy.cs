using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for bar chart series by hit-testing bar bounding boxes.
/// </summary>
public sealed class BarProbeStrategy : ISeriesProbeStrategy
{
    private readonly BarRenderScene _scene;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarProbeStrategy"/> class.
    /// </summary>
    /// <param name="scene">The bar render scene to probe.</param>
    public BarProbeStrategy(BarRenderScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        BarRenderBar? hitBar = null;
        var minDistSq = double.MaxValue;

        foreach (var bar in _scene.Bars)
        {
            var halfWidth = bar.Size.X / 2f;
            var halfDepth = bar.Size.Z / 2f;

            // Check if chartX, chartZ is within the bar's XZ footprint
            var dx = chartX - bar.Position.X;
            var dz = chartZ - bar.Position.Z;

            if (Math.Abs(dx) <= halfWidth && Math.Abs(dz) <= halfDepth)
            {
                // Inside bar footprint — prefer the bar with smallest distance to center
                var distSq = (dx * dx) + (dz * dz);
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    hitBar = bar;
                }
            }
        }

        if (hitBar is not BarRenderBar hit)
        {
            return null;
        }

        // Bar value is the height (Y size), bar position Y is the base
        var value = hit.Position.Y + hit.Size.Y;

        return new SurfaceProbeInfo(
            sampleX: hit.Position.X,
            sampleY: hit.Position.Z,
            axisX: hit.Position.X,
            axisY: hit.Position.Z,
            value: value,
            isApproximate: false);
    }
}
