using System.Numerics;

namespace Videra.Core.Inspection;

public readonly record struct VideraMeasurementAnchor(
    Vector3 WorldPoint,
    Guid? ObjectId = null)
{
    public static VideraMeasurementAnchor ForWorldPoint(Vector3 worldPoint)
    {
        return new VideraMeasurementAnchor(worldPoint, null);
    }

    public static VideraMeasurementAnchor ForObjectPoint(Guid objectId, Vector3 worldPoint)
    {
        return new VideraMeasurementAnchor(worldPoint, objectId);
    }
}
