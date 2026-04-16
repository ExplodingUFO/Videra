using Videra.Core.Graphics.Abstractions;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

internal sealed class SurfacePatchTopologyCache : IDisposable
{
    private readonly Dictionary<(int Width, int Height), Entry> _entries = [];

    public SurfacePatchTopologyLease Acquire(SurfacePatchGeometry geometry, IResourceFactory resourceFactory)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(resourceFactory);

        var key = (geometry.SampleWidth, geometry.SampleHeight);
        if (!_entries.TryGetValue(key, out var entry))
        {
            var indices = geometry.Indices.ToArray();
            var indexBuffer = resourceFactory.CreateIndexBuffer(indices);
            entry = new Entry(indexBuffer, (uint)(indices.Length * sizeof(uint)), (uint)indices.Length);
            _entries.Add(key, entry);
        }

        entry.RefCount++;
        return new SurfacePatchTopologyLease(this, key, entry.IndexBuffer, entry.IndexBufferBytes, entry.IndexCount);
    }

    public void Dispose()
    {
        foreach (var entry in _entries.Values)
        {
            entry.IndexBuffer.Dispose();
        }

        _entries.Clear();
    }

    private void Release((int Width, int Height) key)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            return;
        }

        entry.RefCount--;
        if (entry.RefCount > 0)
        {
            return;
        }

        entry.IndexBuffer.Dispose();
        _entries.Remove(key);
    }

    private sealed class Entry
    {
        public Entry(IBuffer indexBuffer, uint indexBufferBytes, uint indexCount)
        {
            IndexBuffer = indexBuffer;
            IndexBufferBytes = indexBufferBytes;
            IndexCount = indexCount;
        }

        public IBuffer IndexBuffer { get; }

        public uint IndexBufferBytes { get; }

        public uint IndexCount { get; }

        public int RefCount { get; set; }
    }

    internal sealed class SurfacePatchTopologyLease : IDisposable
    {
        private readonly SurfacePatchTopologyCache _owner;
        private readonly (int Width, int Height) _key;
        private bool _disposed;

        public SurfacePatchTopologyLease(
            SurfacePatchTopologyCache owner,
            (int Width, int Height) key,
            IBuffer indexBuffer,
            uint indexBufferBytes,
            uint indexCount)
        {
            _owner = owner;
            _key = key;
            IndexBuffer = indexBuffer;
            IndexBufferBytes = indexBufferBytes;
            IndexCount = indexCount;
        }

        public IBuffer IndexBuffer { get; }

        public uint IndexBufferBytes { get; }

        public uint IndexCount { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _owner.Release(_key);
        }
    }
}
