using System.Collections.ObjectModel;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartResidentTile
{
    public SurfaceChartResidentTile(
        SurfaceTile sourceTile,
        SurfaceRenderTile softwareRenderTile,
        IReadOnlyList<float> sampleValues,
        bool isResident = true)
    {
        ArgumentNullException.ThrowIfNull(sourceTile);
        ArgumentNullException.ThrowIfNull(softwareRenderTile);
        ArgumentNullException.ThrowIfNull(sampleValues);

        if (softwareRenderTile.Key != sourceTile.Key)
        {
            throw new ArgumentException("Software render tile key must match the source tile.", nameof(softwareRenderTile));
        }

        if (softwareRenderTile.Bounds != sourceTile.Bounds)
        {
            throw new ArgumentException("Software render tile bounds must match the source tile.", nameof(softwareRenderTile));
        }

        if (sampleValues.Count != softwareRenderTile.Geometry.VertexCount)
        {
            throw new ArgumentException("Sample values must match the geometry vertex count.", nameof(sampleValues));
        }

        Key = sourceTile.Key;
        SourceTile = sourceTile;
        SoftwareRenderTile = softwareRenderTile;
        SampleValues = AsReadOnly(sampleValues);
        IsResident = isResident;
    }

    public SurfaceTileKey Key { get; }

    public SurfaceTile SourceTile { get; }

    public SurfaceTileBounds Bounds => SourceTile.Bounds;

    public SurfacePatchGeometry Geometry => SoftwareRenderTile.Geometry;

    public SurfaceRenderTile SoftwareRenderTile { get; }

    public IReadOnlyList<float> SampleValues { get; }

    public bool IsResident { get; }

    public SurfaceChartResidentTile WithSoftwareRenderTile(SurfaceRenderTile softwareRenderTile)
    {
        ArgumentNullException.ThrowIfNull(softwareRenderTile);

        return new SurfaceChartResidentTile(
            SourceTile,
            softwareRenderTile,
            SampleValues,
            IsResident);
    }

    public SurfaceRenderTile ToRenderTile()
    {
        return SoftwareRenderTile;
    }

    private static IReadOnlyList<T> AsReadOnly<T>(IReadOnlyList<T> values)
    {
        return values as ReadOnlyCollection<T> ?? Array.AsReadOnly(values.ToArray());
    }
}
