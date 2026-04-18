using System.Numerics;

namespace Videra.Core.Inspection;

public sealed class VideraMeasurement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? Label { get; set; }

    public bool IsVisible { get; set; } = true;

    public VideraMeasurementAnchor Start { get; set; }

    public VideraMeasurementAnchor End { get; set; }

    public float Distance => Vector3.Distance(Start.WorldPoint, End.WorldPoint);

    public float HeightDelta => End.WorldPoint.Y - Start.WorldPoint.Y;

    public float HorizontalDistance
    {
        get
        {
            var start = Start.WorldPoint;
            var end = End.WorldPoint;
            return Vector2.Distance(new Vector2(start.X, start.Z), new Vector2(end.X, end.Z));
        }
    }
}
