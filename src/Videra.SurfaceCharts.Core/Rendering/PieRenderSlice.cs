using System.Numerics;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Render-ready representation of a single pie slice.
/// </summary>
public readonly record struct PieRenderSlice(
    Vector3 Center,
    float InnerRadius,
    float OuterRadius,
    float StartAngle,
    float SweepAngle,
    float ExplodeDistance,
    uint Color);
