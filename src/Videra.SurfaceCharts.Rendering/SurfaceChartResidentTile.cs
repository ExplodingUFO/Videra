using System.Collections.ObjectModel;
using System.Numerics;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartResidentTile
{
    public SurfaceChartResidentTile(
        SurfaceTile sourceTile,
        SurfacePatchGeometry geometry,
        IReadOnlyList<Vector3> samplePositions,
        IReadOnlyList<float> sampleValues,
        IReadOnlyList<uint> colors,
        bool isResident = true)
    {
        ArgumentNullException.ThrowIfNull(sourceTile);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(samplePositions);
        ArgumentNullException.ThrowIfNull(sampleValues);
        ArgumentNullException.ThrowIfNull(colors);

        if (samplePositions.Count != geometry.VertexCount)
        {
            throw new ArgumentException("Sample positions must match the geometry vertex count.", nameof(samplePositions));
        }

        if (sampleValues.Count != geometry.VertexCount)
        {
            throw new ArgumentException("Sample values must match the geometry vertex count.", nameof(sampleValues));
        }

        if (colors.Count != geometry.VertexCount)
        {
            throw new ArgumentException("Colors must match the geometry vertex count.", nameof(colors));
        }

        Key = sourceTile.Key;
        SourceTile = sourceTile;
        Geometry = geometry;
        SamplePositions = AsReadOnly(samplePositions);
        SampleValues = AsReadOnly(sampleValues);
        Colors = AsReadOnly(colors);
        IsResident = isResident;
    }

    public SurfaceTileKey Key { get; }

    public SurfaceTile SourceTile { get; }

    public SurfaceTileBounds Bounds => SourceTile.Bounds;

    public SurfacePatchGeometry Geometry { get; }

    public IReadOnlyList<Vector3> SamplePositions { get; }

    public IReadOnlyList<float> SampleValues { get; }

    public IReadOnlyList<uint> Colors { get; }

    public bool IsResident { get; }

    public SurfaceChartResidentTile WithColors(IReadOnlyList<uint> colors)
    {
        ArgumentNullException.ThrowIfNull(colors);

        if (colors.Count != Geometry.VertexCount)
        {
            throw new ArgumentException("Colors must match the geometry vertex count.", nameof(colors));
        }

        return new SurfaceChartResidentTile(
            SourceTile,
            Geometry,
            SamplePositions,
            SampleValues,
            AsReadOnly(colors),
            IsResident);
    }

    public SurfaceRenderTile ToRenderTile()
    {
        var vertices = new SurfaceRenderVertex[Geometry.VertexCount];
        for (var index = 0; index < vertices.Length; index++)
        {
            vertices[index] = new SurfaceRenderVertex(SamplePositions[index], Colors[index]);
        }

        return new SurfaceRenderTile(Key, Bounds, Geometry, vertices);
    }

    private static IReadOnlyList<T> AsReadOnly<T>(IReadOnlyList<T> values)
    {
        return values as ReadOnlyCollection<T> ?? Array.AsReadOnly(values.ToArray());
    }
}
