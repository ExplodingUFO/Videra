using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Resolves probes for ribbon series by finding the nearest ribbon segment within a snap radius
/// that accounts for the tube radius.
/// </summary>
public sealed class RibbonProbeStrategy : ISeriesProbeStrategy
{
    private readonly RibbonRenderScene _scene;
    private readonly double _snapRadius;

    /// <summary>
    /// Initializes a new instance of the <see cref="RibbonProbeStrategy"/> class.
    /// </summary>
    /// <param name="scene">The ribbon render scene to probe.</param>
    /// <param name="snapRadius">The base snap radius added to each segment's tube radius. Defaults to 0.1.</param>
    public RibbonProbeStrategy(RibbonRenderScene scene, double snapRadius = 0.1)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scene = scene;
        _snapRadius = snapRadius;
    }

    /// <inheritdoc />
    public SurfaceProbeInfo? TryResolve(double chartX, double chartZ, SurfaceMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var nearestIndex = -1;
        var minDist = double.MaxValue;

        for (var i = 0; i < _scene.Segments.Count; i++)
        {
            var segment = _scene.Segments[i];
            var dist = PointToSegmentDistance(
                chartX, chartZ,
                segment.Start.X, segment.Start.Z,
                segment.End.X, segment.End.Z);

            // Effective snap distance includes the tube radius
            var effectiveSnap = _snapRadius + segment.Radius;
            if (dist < minDist && dist <= effectiveSnap)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        if (nearestIndex < 0)
        {
            return null;
        }

        var nearestSegment = _scene.Segments[nearestIndex];

        // Project cursor onto the nearest segment to get the interpolation parameter
        var t = ProjectOntoSegment(
            chartX, chartZ,
            nearestSegment.Start.X, nearestSegment.Start.Z,
            nearestSegment.End.X, nearestSegment.End.Z);

        var sampleX = nearestSegment.Start.X + (t * (nearestSegment.End.X - nearestSegment.Start.X));
        var sampleZ = nearestSegment.Start.Z + (t * (nearestSegment.End.Z - nearestSegment.Start.Z));

        // Interpolate Y value at the projection point
        var interpolatedY = nearestSegment.Start.Y + (t * (nearestSegment.End.Y - nearestSegment.Start.Y));

        return new SurfaceProbeInfo(
            sampleX: sampleX,
            sampleY: sampleZ,
            axisX: sampleX,
            axisY: sampleZ,
            value: interpolatedY,
            isApproximate: false);
    }

    /// <summary>
    /// Computes the shortest distance from point (px, pz) to the line segment (ax, az)-(bx, bz) in XZ plane.
    /// </summary>
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
            // Degenerate segment -- return point-to-point distance
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

    /// <summary>
    /// Projects point (px, pz) onto the line segment (ax, az)-(bx, bz) and returns the interpolation parameter t in [0, 1].
    /// </summary>
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
