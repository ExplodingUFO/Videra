using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

internal sealed class SurfaceChartGpuResidentTile : IDisposable
{
    public SurfaceChartGpuResidentTile(
        SurfaceTileKey key,
        VertexPositionNormalColor[] vertexShadow,
        IBuffer vertexBuffer,
        uint vertexBufferBytes,
        SurfacePatchTopologyCache.SurfacePatchTopologyLease topology)
    {
        ArgumentNullException.ThrowIfNull(vertexShadow);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(topology);

        Key = key;
        VertexShadow = vertexShadow;
        VertexBuffer = vertexBuffer;
        VertexBufferBytes = vertexBufferBytes;
        Topology = topology;
    }

    public SurfaceTileKey Key { get; }

    public VertexPositionNormalColor[] VertexShadow { get; }

    public IBuffer VertexBuffer { get; }

    public uint VertexBufferBytes { get; }

    public SurfacePatchTopologyCache.SurfacePatchTopologyLease Topology { get; }

    public uint ResidentBytes => VertexBufferBytes + Topology.IndexBufferBytes;

    public void UpdateColors(IReadOnlyList<float> sampleValues, SurfaceColorMapLut colorMapLut)
    {
        ArgumentNullException.ThrowIfNull(sampleValues);
        ArgumentNullException.ThrowIfNull(colorMapLut);

        if (sampleValues.Count != VertexShadow.Length)
        {
            throw new ArgumentException("Sample-value count must match the resident vertex count.", nameof(sampleValues));
        }

        for (var index = 0; index < VertexShadow.Length; index++)
        {
            VertexShadow[index].Color = ToRgbaFloat(colorMapLut.Map(sampleValues[index]));
        }

        VertexBuffer.UpdateArray(VertexShadow);
    }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        Topology.Dispose();
    }

    private static RgbaFloat ToRgbaFloat(uint argb)
    {
        var a = ((argb >> 24) & 0xFF) / 255f;
        var r = ((argb >> 16) & 0xFF) / 255f;
        var g = ((argb >> 8) & 0xFF) / 255f;
        var b = (argb & 0xFF) / 255f;
        return new RgbaFloat(r, g, b, a);
    }
}
