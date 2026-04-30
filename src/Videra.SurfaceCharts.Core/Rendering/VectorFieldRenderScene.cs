using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A render-ready arrow in a vector field scene.
/// </summary>
public readonly record struct VectorFieldRenderArrow(Vector3 Position, Vector3 Direction, double Magnitude, uint Color);

/// <summary>
/// Represents a render-ready vector-field snapshot containing arrows for all points.
/// </summary>
public sealed class VectorFieldRenderScene
{
    private readonly ReadOnlyCollection<VectorFieldRenderArrow> _arrowsView;

    public VectorFieldRenderScene(int arrowCount, IReadOnlyList<VectorFieldRenderArrow> arrows)
    {
        ArgumentNullException.ThrowIfNull(arrows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(arrowCount);
        ArrowCount = arrowCount;
        _arrowsView = Array.AsReadOnly(arrows.ToArray());
    }

    public int ArrowCount { get; }
    public IReadOnlyList<VectorFieldRenderArrow> Arrows => _arrowsView;
}
