using System.Numerics;

namespace Videra.Core.Inspection;

public readonly record struct VideraClipPlane(
    Vector3 Point,
    Vector3 Normal,
    bool IsEnabled = true)
{
    public static VideraClipPlane FromPointNormal(Vector3 point, Vector3 normal, bool isEnabled = true)
    {
        return new VideraClipPlane(point, normal, isEnabled);
    }

    internal bool TryGetNormalized(out Vector3 point, out Vector3 normal)
    {
        point = Point;
        if (!IsEnabled || Normal.LengthSquared() <= float.Epsilon)
        {
            normal = Vector3.Zero;
            return false;
        }

        normal = Vector3.Normalize(Normal);
        return true;
    }
}
