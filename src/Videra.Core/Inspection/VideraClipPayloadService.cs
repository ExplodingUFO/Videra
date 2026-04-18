using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Inspection;

internal sealed class VideraClipPayloadService
{
    public MeshPayload? Clip(MeshPayload sourcePayload, IReadOnlyList<VideraClipPlane> clippingPlanes)
    {
        ArgumentNullException.ThrowIfNull(sourcePayload);

        var activePlanes = clippingPlanes?
            .Select(static plane => plane)
            .Where(static plane => plane.IsEnabled)
            .ToArray() ?? Array.Empty<VideraClipPlane>();

        if (activePlanes.Length == 0)
        {
            return sourcePayload;
        }

        if (sourcePayload.Topology != MeshTopology.Triangles)
        {
            return sourcePayload;
        }

        var outputVertices = new List<VertexPositionNormalColor>();
        var outputIndices = new List<uint>();

        for (var i = 0; i + 2 < sourcePayload.Indices.Length; i += 3)
        {
            var polygon = new List<VertexPositionNormalColor>
            {
                sourcePayload.Vertices[sourcePayload.Indices[i]],
                sourcePayload.Vertices[sourcePayload.Indices[i + 1]],
                sourcePayload.Vertices[sourcePayload.Indices[i + 2]]
            };

            foreach (var plane in activePlanes)
            {
                polygon = ClipPolygonAgainstPlane(polygon, plane);
                if (polygon.Count == 0)
                {
                    break;
                }
            }

            if (polygon.Count < 3)
            {
                continue;
            }

            var baseIndex = (uint)outputVertices.Count;
            outputVertices.AddRange(polygon);
            for (var vertexIndex = 1; vertexIndex < polygon.Count - 1; vertexIndex++)
            {
                outputIndices.Add(baseIndex);
                outputIndices.Add(baseIndex + (uint)vertexIndex);
                outputIndices.Add(baseIndex + (uint)vertexIndex + 1u);
            }
        }

        if (outputVertices.Count == 0 || outputIndices.Count == 0)
        {
            return null;
        }

        return new MeshPayload(outputVertices.ToArray(), outputIndices.ToArray(), MeshTopology.Triangles);
    }

    private static List<VertexPositionNormalColor> ClipPolygonAgainstPlane(
        IReadOnlyList<VertexPositionNormalColor> polygon,
        VideraClipPlane plane)
    {
        if (!plane.TryGetNormalized(out var planePoint, out var planeNormal))
        {
            return polygon.ToList();
        }

        var clipped = new List<VertexPositionNormalColor>();
        for (var i = 0; i < polygon.Count; i++)
        {
            var current = polygon[i];
            var previous = polygon[(i + polygon.Count - 1) % polygon.Count];
            var currentDistance = SignedDistance(current.Position, planePoint, planeNormal);
            var previousDistance = SignedDistance(previous.Position, planePoint, planeNormal);
            var currentInside = currentDistance >= 0f;
            var previousInside = previousDistance >= 0f;

            if (currentInside != previousInside)
            {
                clipped.Add(Intersect(previous, current, previousDistance, currentDistance));
            }

            if (currentInside)
            {
                clipped.Add(current);
            }
        }

        return clipped;
    }

    private static float SignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        return Vector3.Dot(point - planePoint, planeNormal);
    }

    private static VertexPositionNormalColor Intersect(
        VertexPositionNormalColor previous,
        VertexPositionNormalColor current,
        float previousDistance,
        float currentDistance)
    {
        var denominator = previousDistance - currentDistance;
        var t = Math.Abs(denominator) <= float.Epsilon ? 0f : previousDistance / denominator;
        t = Math.Clamp(t, 0f, 1f);

        var position = Vector3.Lerp(previous.Position, current.Position, t);
        var normal = Vector3.Normalize(Vector3.Lerp(previous.Normal, current.Normal, t));
        if (normal.LengthSquared() <= float.Epsilon)
        {
            normal = previous.Normal;
        }

        var color = new RgbaFloat(
            Lerp(previous.Color.R, current.Color.R, t),
            Lerp(previous.Color.G, current.Color.G, t),
            Lerp(previous.Color.B, current.Color.B, t),
            Lerp(previous.Color.A, current.Color.A, t));

        return new VertexPositionNormalColor(position, normal, color);
    }

    private static float Lerp(float start, float end, float amount) => start + ((end - start) * amount);
}
