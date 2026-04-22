using System.Numerics;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal sealed class WaterfallSurfaceRenderer : SurfaceRenderer
{
    private const double RowsPerStrip = 3d;
    private const float StripPitch = 2.4f;
    private const float IntraStripPitch = 0.35f;

    public override SurfaceRenderTile BuildTile(SurfaceMetadata metadata, SurfaceTile tile, SurfaceColorMap colorMap)
    {
        var baseTile = base.BuildTile(metadata, tile, colorMap);
        var vertices = new SurfaceRenderVertex[baseTile.Vertices.Count];

        for (var row = 0; row < tile.Height; row++)
        {
            var sampleY = MapTileSampleCoordinate(tile.Bounds.StartY, tile.Bounds.Height, tile.Height, row);
            var stripBase = Math.Floor(sampleY / RowsPerStrip);
            var stripIndex = (float)stripBase;
            var stripRow = (float)(sampleY - (stripBase * RowsPerStrip));
            var remappedZ = (stripIndex * StripPitch) + (stripRow * IntraStripPitch);

            for (var column = 0; column < tile.Width; column++)
            {
                var vertexIndex = checked((row * tile.Width) + column);
                var vertex = baseTile.Vertices[vertexIndex];
                vertices[vertexIndex] = new SurfaceRenderVertex(
                    new Vector3(vertex.Position.X, vertex.Position.Y, remappedZ),
                    vertex.Color);
            }
        }

        return new SurfaceRenderTile(baseTile.Key, baseTile.Bounds, baseTile.Geometry, vertices);
    }

    private static double MapTileSampleCoordinate(int start, int span, int sampleCount, int sampleIndex)
    {
        if (sampleCount <= 1)
        {
            return start + ((span - 1d) / 2d);
        }

        return start + (sampleIndex * ((span - 1d) / (sampleCount - 1d)));
    }
}
