using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Inspection;

internal sealed class VideraClipPayloadService
{
    private static readonly ConditionalWeakTable<MeshPayload, CachedClipPayloads> Cache = new();

    public MeshPayload? Clip(MeshPayload sourcePayload, IReadOnlyList<VideraClipPlane> clippingPlanes)
    {
        ArgumentNullException.ThrowIfNull(sourcePayload);

        var activePlanes = NormalizePlanes(clippingPlanes);
        if (activePlanes.Length == 0)
        {
            return sourcePayload;
        }

        if (sourcePayload.Topology != MeshTopology.Triangles)
        {
            return sourcePayload;
        }

        var signature = CreateSignature(activePlanes);
        var cachedPayloads = Cache.GetOrCreateValue(sourcePayload);
        if (cachedPayloads.TryGet(signature, out var cachedPayload))
        {
            return cachedPayload;
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
            cachedPayloads.Set(signature, payload: null);
            return null;
        }

        var clippedPayload = new MeshPayload(outputVertices.ToArray(), outputIndices.ToArray(), MeshTopology.Triangles);
        cachedPayloads.Set(signature, clippedPayload);
        return clippedPayload;
    }

    private static NormalizedClipPlane[] NormalizePlanes(IReadOnlyList<VideraClipPlane>? clippingPlanes)
    {
        if (clippingPlanes is null || clippingPlanes.Count == 0)
        {
            return Array.Empty<NormalizedClipPlane>();
        }

        var normalized = new List<NormalizedClipPlane>(clippingPlanes.Count);
        foreach (var plane in clippingPlanes)
        {
            if (!plane.IsEnabled || !plane.TryGetNormalized(out var planePoint, out var planeNormal))
            {
                continue;
            }

            normalized.Add(new NormalizedClipPlane(planePoint, planeNormal));
        }

        return normalized.ToArray();
    }

    private static string CreateSignature(IReadOnlyList<NormalizedClipPlane> clippingPlanes)
    {
        var builder = new StringBuilder(clippingPlanes.Count * 48);
        foreach (var plane in clippingPlanes)
        {
            AppendFloatBits(builder, plane.Point.X);
            AppendFloatBits(builder, plane.Point.Y);
            AppendFloatBits(builder, plane.Point.Z);
            AppendFloatBits(builder, plane.Normal.X);
            AppendFloatBits(builder, plane.Normal.Y);
            AppendFloatBits(builder, plane.Normal.Z);
            builder.Append('|');
        }

        return builder.ToString();
    }

    private static List<VertexPositionNormalColor> ClipPolygonAgainstPlane(
        IReadOnlyList<VertexPositionNormalColor> polygon,
        NormalizedClipPlane plane)
    {
        var clipped = new List<VertexPositionNormalColor>();
        for (var i = 0; i < polygon.Count; i++)
        {
            var current = polygon[i];
            var previous = polygon[(i + polygon.Count - 1) % polygon.Count];
            var currentDistance = SignedDistance(current.Position, plane.Point, plane.Normal);
            var previousDistance = SignedDistance(previous.Position, plane.Point, plane.Normal);
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

    private static void AppendFloatBits(StringBuilder builder, float value)
    {
        builder.Append(BitConverter.SingleToInt32Bits(value));
        builder.Append(',');
    }

    private readonly record struct NormalizedClipPlane(Vector3 Point, Vector3 Normal);

    private sealed class CachedClipPayloads
    {
        private readonly Dictionary<string, CachedClipPayload> _entries = new(StringComparer.Ordinal);

        public bool TryGet(string signature, out MeshPayload? payload)
        {
            lock (_entries)
            {
                if (_entries.TryGetValue(signature, out var cachedPayload))
                {
                    payload = cachedPayload.Payload;
                    return true;
                }
            }

            payload = null;
            return false;
        }

        public void Set(string signature, MeshPayload? payload)
        {
            lock (_entries)
            {
                _entries[signature] = new CachedClipPayload(payload);
            }
        }
    }

    private sealed record CachedClipPayload(MeshPayload? Payload);
}
