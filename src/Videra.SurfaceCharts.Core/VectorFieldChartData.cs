using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A single vector field sample point.
/// </summary>
public readonly record struct VectorFieldPoint(Vector3 Position, Vector3 Direction, double Magnitude);

/// <summary>
/// Represents one immutable vector-field dataset.
/// </summary>
public sealed class VectorFieldChartData
{
    private readonly ReadOnlyCollection<VectorFieldPoint> _pointsView;

    public VectorFieldChartData(
        IReadOnlyList<VectorFieldPoint> points,
        SurfaceAxisDescriptor horizontalAxis,
        SurfaceAxisDescriptor depthAxis,
        SurfaceValueRange magnitudeRange)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentNullException.ThrowIfNull(horizontalAxis);
        ArgumentNullException.ThrowIfNull(depthAxis);
        _pointsView = Array.AsReadOnly(points.ToArray());
        HorizontalAxis = horizontalAxis;
        DepthAxis = depthAxis;
        MagnitudeRange = magnitudeRange;
    }

    public IReadOnlyList<VectorFieldPoint> Points => _pointsView;
    public SurfaceAxisDescriptor HorizontalAxis { get; }
    public SurfaceAxisDescriptor DepthAxis { get; }
    public SurfaceValueRange MagnitudeRange { get; }
    public int PointCount => _pointsView.Count;
}
