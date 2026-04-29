using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for contour series by finding the nearest contour line segment.
/// </summary>
public sealed class ContourProbeStrategy : ISeriesProbeStrategy
{
    private readonly ContourRenderScene _scene;
    private readonly double _snapRadius;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContourProbeStrategy"/> class.
    /// </summary>
    /// <param name="scene">The contour render scene to probe.</param>
    /// <param name="snapRadius">The maximum distance from a contour line to trigger a probe hit. Defaults to 0.05.</param>
    public ContourProbeStrategy(ContourRenderScene scene, double snapRadius = 0.05)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        ContourLine? nearestLine = null;
        ContourSegment nearestSegment = default;
        var minDist = double.MaxValue;

        foreach (var line in _scene.Lines)
        {
            foreach (var segment in line.Segments)
            {
                var dist = PointToSegmentDistance(
                    chartX, chartZ,
                    segment.Start.X, segment.Start.Z,
                    segment.End.X, segment.End.Z);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearestLine = line;
                    nearestSegment = segment;
                }
            }
        }

        if (nearestLine is null || minDist > _snapRadius)
        {
            return null;
        }

        // Project cursor onto the nearest segment to get the sample position
        var t = ProjectOntoSegment(
            chartX, chartZ,
            nearestSegment.Start.X, nearestSegment.Start.Z,
            nearestSegment.End.X, nearestSegment.End.Z);
        var sampleX = nearestSegment.Start.X + (t * (nearestSegment.End.X - nearestSegment.Start.X));
        var sampleZ = nearestSegment.Start.Z + (t * (nearestSegment.End.Z - nearestSegment.Start.Z));

        return new SurfaceProbeInfo(
            sampleX: sampleX,
            sampleY: sampleZ,
            axisX: sampleX,
            axisY: sampleZ,
            value: nearestLine.IsoValue,
            isApproximate: false);
    }

    private static double PointToSegmentDistance(
        double px, double pz,
        double ax, double az,
        double bx, double bz)
    {
        var abx = bx - ax;
        var abz = bz - az;
        var apx = px - ax;
        var apz = pz - az;

        var abLenSq = (abx * abx) + (abz * abz);
        if (abLenSq < 1e-12)
        {
            // Degenerate segment — return point-to-point distance
            var ddx = px - ax;
            var ddz = pz - az;
            return Math.Sqrt((ddx * ddx) + (ddz * ddz));
        }

        var t = Math.Clamp(((apx * abx) + (apz * abz)) / abLenSq, 0d, 1d);
        var projX = ax + (t * abx);
        var projZ = az + (t * abz);
        var dx = px - projX;
        var dz = pz - projZ;
        return Math.Sqrt((dx * dx) + (dz * dz));
    }

    private static double ProjectOntoSegment(
        double px, double pz,
        double ax, double az,
        double bx, double bz)
    {
        var abx = bx - ax;
        var abz = bz - az;
        var apx = px - ax;
        var apz = pz - az;

        var abLenSq = (abx * abx) + (abz * abz);
        if (abLenSq < 1e-12)
        {
            return 0d;
        }

        return Math.Clamp(((apx * abx) + (apz * abz)) / abLenSq, 0d, 1d);
    }
}
