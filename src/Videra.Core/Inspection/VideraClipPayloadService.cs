using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

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
        var outputTangents = sourcePayload.Tangents.Length == 0 ? null : new List<Vector4>();
        var outputSegments = sourcePayload.Segments.Length == 0
            ? null
            : new List<MeshPayloadSegment>(sourcePayload.Segments.Length);

        if (sourcePayload.Segments.Length == 0)
        {
            ClipTriangleRange(
                sourcePayload,
                0,
                (uint)sourcePayload.Indices.Length,
                activePlanes,
                outputVertices,
                outputTangents,
                outputIndices,
                outputSegments);
        }
        else
        {
            foreach (var segment in sourcePayload.Segments)
            {
                ClipTriangleRange(
                    sourcePayload,
                    segment.StartIndex,
                    segment.IndexCount,
                    activePlanes,
                    outputVertices,
                    outputTangents,
                    outputIndices,
                    outputSegments,
                    segment.Alpha);
            }
        }

        if (outputVertices.Count == 0 || outputIndices.Count == 0)
        {
            cachedPayloads.Set(signature, payload: null);
            return null;
        }

        var clippedPayload = new MeshPayload(
            outputVertices.ToArray(),
            outputIndices.ToArray(),
            MeshTopology.Triangles,
            outputTangents?.ToArray(),
            segments: outputSegments?.ToArray());
        cachedPayloads.Set(signature, clippedPayload);
        return clippedPayload;
    }

    private static void ClipTriangleRange(
        MeshPayload sourcePayload,
        uint startIndex,
        uint indexCount,
        IReadOnlyList<NormalizedClipPlane> activePlanes,
        ICollection<VertexPositionNormalColor> outputVertices,
        ICollection<Vector4>? outputTangents,
        ICollection<uint> outputIndices,
        ICollection<MeshPayloadSegment>? outputSegments,
        MaterialAlphaSettings? alpha = null)
    {
        var segmentStart = (uint)outputIndices.Count;
        var segmentOutputIndexCount = 0u;

        for (var i = startIndex; i + 2 < startIndex + indexCount; i += 3)
        {
            var polygon = new List<VertexPositionNormalColor>
            {
                sourcePayload.Vertices[sourcePayload.Indices[i]],
                sourcePayload.Vertices[sourcePayload.Indices[i + 1]],
                sourcePayload.Vertices[sourcePayload.Indices[i + 2]]
            };
            var tangents = outputTangents is null
                ? null
                : new List<Vector4>
                {
                    sourcePayload.Tangents[sourcePayload.Indices[i]],
                    sourcePayload.Tangents[sourcePayload.Indices[i + 1]],
                    sourcePayload.Tangents[sourcePayload.Indices[i + 2]]
                };

            foreach (var plane in activePlanes)
            {
                polygon = ClipPolygonAgainstPlane(polygon, tangents, plane, out tangents);
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
            foreach (var vertex in polygon)
            {
                outputVertices.Add(vertex);
            }
            if (outputTangents is not null && tangents is not null)
            {
                foreach (var tangent in tangents)
                {
                    outputTangents.Add(tangent);
                }
            }
            for (var vertexIndex = 1; vertexIndex < polygon.Count - 1; vertexIndex++)
            {
                outputIndices.Add(baseIndex);
                outputIndices.Add(baseIndex + (uint)vertexIndex);
                outputIndices.Add(baseIndex + (uint)vertexIndex + 1u);
                segmentOutputIndexCount += 3u;
            }
        }

        if (outputSegments is not null && segmentOutputIndexCount > 0)
        {
            outputSegments.Add(new MeshPayloadSegment(segmentStart, segmentOutputIndexCount, alpha ?? MaterialAlphaSettings.Opaque));
        }
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
        IReadOnlyList<Vector4>? tangents,
        NormalizedClipPlane plane,
        out List<Vector4>? clippedTangents)
    {
        var clipped = new List<VertexPositionNormalColor>();
        clippedTangents = tangents is null ? null : new List<Vector4>();
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
                var intersected = Intersect(previous, current, previousDistance, currentDistance,
                    tangents is null ? default : tangents[(i + polygon.Count - 1) % polygon.Count],
                    tangents is null ? default : tangents[i]);
                clipped.Add(intersected.Vertex);
                clippedTangents?.Add(intersected.Tangent);
            }

            if (currentInside)
            {
                clipped.Add(current);
                if (tangents is not null)
                {
                    clippedTangents!.Add(tangents[i]);
                }
            }
        }

        return clipped;
    }

    private static float SignedDistance(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        return Vector3.Dot(point - planePoint, planeNormal);
    }

    private static (VertexPositionNormalColor Vertex, Vector4 Tangent) Intersect(
        VertexPositionNormalColor previous,
        VertexPositionNormalColor current,
        float previousDistance,
        float currentDistance,
        Vector4 previousTangent,
        Vector4 currentTangent)
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

        var tangent = Vector4.Lerp(previousTangent, currentTangent, t);
        return (new VertexPositionNormalColor(position, normal, color), tangent);
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
