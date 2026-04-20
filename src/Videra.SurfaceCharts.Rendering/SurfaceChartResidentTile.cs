using System.Collections;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartResidentTile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartResidentTile"/> class using an explicit scalar snapshot.
    /// This overload preserves the historical public contract for callers that materialize resident scalar truth themselves.
    /// </summary>
    public SurfaceChartResidentTile(
        SurfaceTile sourceTile,
        SurfaceRenderTile softwareRenderTile,
        IReadOnlyList<float> sampleValues,
        bool isResident = true)
        : this(
            sourceTile,
            softwareRenderTile,
            ResolveExplicitSampleMemory(sampleValues),
            isResident)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceChartResidentTile"/> class by borrowing scalar memory from the source tile snapshot.
    /// </summary>
    public SurfaceChartResidentTile(
        SurfaceTile sourceTile,
        SurfaceRenderTile softwareRenderTile,
        bool isResident = true)
        : this(
            sourceTile,
            softwareRenderTile,
            ResolveSourceSampleValues(sourceTile),
            isResident)
    {
    }

    private SurfaceChartResidentTile(
        SurfaceTile sourceTile,
        SurfaceRenderTile softwareRenderTile,
        ReadOnlyMemory<float> sampleValueMemory,
        bool isResident)
    {
        ArgumentNullException.ThrowIfNull(sourceTile);
        ArgumentNullException.ThrowIfNull(softwareRenderTile);

        if (softwareRenderTile.Key != sourceTile.Key)
        {
            throw new ArgumentException("Software render tile key must match the source tile.", nameof(softwareRenderTile));
        }

        if (softwareRenderTile.Bounds != sourceTile.Bounds)
        {
            throw new ArgumentException("Software render tile bounds must match the source tile.", nameof(softwareRenderTile));
        }

        if (sampleValueMemory.Length != softwareRenderTile.Geometry.VertexCount)
        {
            throw new ArgumentException("Sample values must match the geometry vertex count.", nameof(sourceTile));
        }

        Key = sourceTile.Key;
        SourceTile = sourceTile;
        SoftwareRenderTile = softwareRenderTile;
        SampleValueMemory = sampleValueMemory;
        SampleValues = new ReadOnlyMemoryListAdapter(sampleValueMemory);
        IsResident = isResident;
    }

    public SurfaceTileKey Key { get; }

    public SurfaceTile SourceTile { get; }

    public SurfaceTileBounds Bounds => SourceTile.Bounds;

    public SurfacePatchGeometry Geometry => SoftwareRenderTile.Geometry;

    public SurfaceRenderTile SoftwareRenderTile { get; }

    public IReadOnlyList<float> SampleValues { get; }

    /// <summary>
    /// Gets the resident scalar truth as direct source-backed memory.
    /// The source tile is treated as immutable; replace the tile instance if scalar contents change.
    /// </summary>
    public ReadOnlyMemory<float> SampleValueMemory { get; }

    public bool IsResident { get; }

    public SurfaceChartResidentTile WithSoftwareRenderTile(SurfaceRenderTile softwareRenderTile)
    {
        ArgumentNullException.ThrowIfNull(softwareRenderTile);

        return new SurfaceChartResidentTile(
            SourceTile,
            softwareRenderTile,
            SampleValueMemory,
            IsResident);
    }

    public SurfaceRenderTile ToRenderTile()
    {
        return SoftwareRenderTile;
    }

    private static ReadOnlyMemory<float> ResolveSourceSampleValues(SurfaceTile sourceTile)
    {
        ArgumentNullException.ThrowIfNull(sourceTile);
        return sourceTile.ColorField?.Values ?? sourceTile.Values;
    }

    private static ReadOnlyMemory<float> ResolveExplicitSampleMemory(IReadOnlyList<float> sampleValues)
    {
        ArgumentNullException.ThrowIfNull(sampleValues);
        return sampleValues.ToArray();
    }

    private sealed class ReadOnlyMemoryListAdapter : IReadOnlyList<float>
    {
        public ReadOnlyMemoryListAdapter(ReadOnlyMemory<float> memory)
        {
            Memory = memory;
        }

        public ReadOnlyMemory<float> Memory { get; }

        public int Count => Memory.Length;

        public float this[int index] => Memory.Span[index];

        public IEnumerator<float> GetEnumerator()
        {
            return new Enumerator(Memory);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class Enumerator : IEnumerator<float>
        {
            private readonly ReadOnlyMemory<float> _memory;
            private int _index = -1;

            public Enumerator(ReadOnlyMemory<float> memory)
            {
                _memory = memory;
            }

            public float Current => _memory.Span[_index];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (_index + 1 >= _memory.Length)
                {
                    return false;
                }

                _index++;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
            }
        }
    }
}
