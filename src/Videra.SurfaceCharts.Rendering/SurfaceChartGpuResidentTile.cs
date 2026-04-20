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
        IBuffer scalarBuffer,
        uint scalarBufferBytes,
        SurfacePatchTopologyCache.SurfacePatchTopologyLease topology)
    {
        ArgumentNullException.ThrowIfNull(vertexShadow);
        ArgumentNullException.ThrowIfNull(vertexBuffer);
        ArgumentNullException.ThrowIfNull(scalarBuffer);
        ArgumentNullException.ThrowIfNull(topology);

        Key = key;
        VertexShadow = vertexShadow;
        VertexBuffer = vertexBuffer;
        VertexBufferBytes = vertexBufferBytes;
        ScalarBuffer = scalarBuffer;
        ScalarBufferBytes = scalarBufferBytes;
        Topology = topology;
    }

    public SurfaceTileKey Key { get; }

    public VertexPositionNormalColor[] VertexShadow { get; }

    public IBuffer VertexBuffer { get; }

    public uint VertexBufferBytes { get; }

    public IBuffer ScalarBuffer { get; }

    public uint ScalarBufferBytes { get; }

    public SurfacePatchTopologyCache.SurfacePatchTopologyLease Topology { get; }

    public uint ResidentBytes => VertexBufferBytes + ScalarBufferBytes + Topology.IndexBufferBytes;

    public void Dispose()
    {
        VertexBuffer.Dispose();
        ScalarBuffer.Dispose();
        Topology.Dispose();
    }
}
