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

    /// <summary>
    /// Initializes a new vector-field dataset.
    /// </summary>
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

    /// <summary>
    /// Gets the vector field sample points.
    /// </summary>
    public IReadOnlyList<VectorFieldPoint> Points => _pointsView;

    /// <summary>
    /// Gets the horizontal axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor HorizontalAxis { get; }

    /// <summary>
    /// Gets the depth axis descriptor.
    /// </summary>
    public SurfaceAxisDescriptor DepthAxis { get; }
    /// <summary>
    /// Gets the magnitude value range across all vector field points.
    /// </summary>
    public SurfaceValueRange MagnitudeRange { get; }

    /// <summary>
    /// Gets the number of vector field sample points.
    /// </summary>
    public int PointCount => _pointsView.Count;
}
