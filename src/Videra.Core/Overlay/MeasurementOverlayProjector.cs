using System.Globalization;
using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Inspection;

namespace Videra.Core.Overlay;

public sealed record MeasurementOverlayProjection(
    Guid MeasurementId,
    Vector2 StartScreenPosition,
    Vector2 EndScreenPosition,
    string Text);

public sealed class MeasurementOverlayProjector
{
    public IReadOnlyList<MeasurementOverlayProjection> Project(
        IReadOnlyList<VideraMeasurement>? measurements,
        OrbitCamera camera,
        Vector2 viewportSize)
    {
        ArgumentNullException.ThrowIfNull(camera);

        if (measurements is null || measurements.Count == 0 || viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            return Array.Empty<MeasurementOverlayProjection>();
        }

        var projections = new List<MeasurementOverlayProjection>(measurements.Count);
        foreach (var measurement in measurements)
        {
            if (measurement is null || !measurement.IsVisible)
            {
                continue;
            }

            if (!camera.TryProjectWorldPoint(measurement.Start.WorldPoint, viewportSize, out var startScreenPoint) ||
                !camera.TryProjectWorldPoint(measurement.End.WorldPoint, viewportSize, out var endScreenPoint) ||
                !IsWithinViewport(startScreenPoint, viewportSize) ||
                !IsWithinViewport(endScreenPoint, viewportSize))
            {
                continue;
            }

            projections.Add(new MeasurementOverlayProjection(
                measurement.Id,
                startScreenPoint,
                endScreenPoint,
                FormatText(measurement)));
        }

        return projections;
    }

    private static string FormatText(VideraMeasurement measurement)
    {
        if (!string.IsNullOrWhiteSpace(measurement.Label))
        {
            return measurement.Label!;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"d {measurement.Distance:0.###} | Δy {measurement.HeightDelta:+0.###;-0.###;0}");
    }

    private static bool IsWithinViewport(Vector2 screenPoint, Vector2 viewportSize)
    {
        return screenPoint.X >= 0f &&
               screenPoint.Y >= 0f &&
               screenPoint.X <= viewportSize.X &&
               screenPoint.Y <= viewportSize.Y;
    }
}
