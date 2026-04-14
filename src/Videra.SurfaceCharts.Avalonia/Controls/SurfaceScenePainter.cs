using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal readonly record struct ProjectedSurfaceTriangle(Point A, Point B, Point C, uint Color, double SortKey);

internal static class SurfaceScenePainter
{
    public static void DrawScene(DrawingContext context, SurfaceRenderScene? scene, Size viewSize)
    {
        ArgumentNullException.ThrowIfNull(context);

        var triangles = ProjectTriangles(scene, viewSize);
        if (triangles.Count == 0)
        {
            return;
        }

        Dictionary<uint, IBrush> brushCache = [];

        foreach (var triangle in triangles)
        {
            if (!brushCache.TryGetValue(triangle.Color, out var brush))
            {
                brush = new SolidColorBrush(ToColor(triangle.Color));
                brushCache.Add(triangle.Color, brush);
            }

            context.DrawGeometry(brush, null, CreateTriangleGeometry(triangle));
        }
    }

    internal static IReadOnlyList<ProjectedSurfaceTriangle> ProjectTriangles(SurfaceRenderScene? scene, Size viewSize)
    {
        if (scene is null
            || scene.Tiles.Count == 0
            || viewSize.Width <= 0d
            || viewSize.Height <= 0d)
        {
            return Array.Empty<ProjectedSurfaceTriangle>();
        }

        var projectedTiles = new (SurfaceRenderTile Tile, RawProjection[] Vertices)[scene.Tiles.Count];
        var hasProjectedVertex = false;
        var minRawX = double.PositiveInfinity;
        var maxRawX = double.NegativeInfinity;
        var minRawY = double.PositiveInfinity;
        var maxRawY = double.NegativeInfinity;

        for (var tileIndex = 0; tileIndex < scene.Tiles.Count; tileIndex++)
        {
            var tile = scene.Tiles[tileIndex];
            var rawVertices = new RawProjection[tile.Vertices.Count];

            for (var vertexIndex = 0; vertexIndex < tile.Vertices.Count; vertexIndex++)
            {
                var rawProjection = ProjectRaw(tile.Vertices[vertexIndex].Position);
                rawVertices[vertexIndex] = rawProjection;

                minRawX = Math.Min(minRawX, rawProjection.X);
                maxRawX = Math.Max(maxRawX, rawProjection.X);
                minRawY = Math.Min(minRawY, rawProjection.Y);
                maxRawY = Math.Max(maxRawY, rawProjection.Y);
                hasProjectedVertex = true;
            }

            projectedTiles[tileIndex] = (tile, rawVertices);
        }

        if (!hasProjectedVertex)
        {
            return Array.Empty<ProjectedSurfaceTriangle>();
        }

        var transform = CreateTransform(viewSize, minRawX, maxRawX, minRawY, maxRawY);
        List<ProjectedSurfaceTriangle> triangles = [];

        foreach (var (tile, rawVertices) in projectedTiles)
        {
            var indices = tile.Geometry.Indices;
            for (var index = 0; index <= indices.Count - 3; index += 3)
            {
                var first = rawVertices[(int)indices[index]];
                var second = rawVertices[(int)indices[index + 1]];
                var third = rawVertices[(int)indices[index + 2]];

                var firstPoint = transform.Apply(first);
                var secondPoint = transform.Apply(second);
                var thirdPoint = transform.Apply(third);

                if (IsDegenerate(firstPoint, secondPoint, thirdPoint))
                {
                    continue;
                }

                triangles.Add(
                    new ProjectedSurfaceTriangle(
                        firstPoint,
                        secondPoint,
                        thirdPoint,
                        BlendTriangleColor(
                            tile.Vertices[(int)indices[index]].Color,
                            tile.Vertices[(int)indices[index + 1]].Color,
                            tile.Vertices[(int)indices[index + 2]].Color),
                        (first.Y + second.Y + third.Y) / 3d));
            }
        }

        triangles.Sort(static (left, right) => left.SortKey.CompareTo(right.SortKey));
        return triangles;
    }

    private static RawProjection ProjectRaw(System.Numerics.Vector3 position)
    {
        return new RawProjection(
            X: position.X - position.Z,
            Y: ((position.X + position.Z) * 0.5d) - position.Y);
    }

    private static ProjectionTransform CreateTransform(
        Size viewSize,
        double minRawX,
        double maxRawX,
        double minRawY,
        double maxRawY)
    {
        var spanX = Math.Max(maxRawX - minRawX, 1d);
        var spanY = Math.Max(maxRawY - minRawY, 1d);
        var availableWidth = Math.Max(viewSize.Width * 0.9d, 1d);
        var availableHeight = Math.Max(viewSize.Height * 0.9d, 1d);
        var scale = Math.Min(availableWidth / spanX, availableHeight / spanY);
        var projectedWidth = spanX * scale;
        var projectedHeight = spanY * scale;
        var offsetX = (viewSize.Width - projectedWidth) / 2d - (minRawX * scale);
        var offsetY = (viewSize.Height - projectedHeight) / 2d - (minRawY * scale);

        return new ProjectionTransform(scale, offsetX, offsetY);
    }

    private static bool IsDegenerate(Point firstPoint, Point secondPoint, Point thirdPoint)
    {
        var area = Math.Abs(
            (firstPoint.X * (secondPoint.Y - thirdPoint.Y))
            + (secondPoint.X * (thirdPoint.Y - firstPoint.Y))
            + (thirdPoint.X * (firstPoint.Y - secondPoint.Y)));
        return area <= 0.0001d;
    }

    private static uint BlendTriangleColor(uint first, uint second, uint third)
    {
        var alpha = (((first >> 24) & 0xFFu) + ((second >> 24) & 0xFFu) + ((third >> 24) & 0xFFu)) / 3u;
        var red = (((first >> 16) & 0xFFu) + ((second >> 16) & 0xFFu) + ((third >> 16) & 0xFFu)) / 3u;
        var green = (((first >> 8) & 0xFFu) + ((second >> 8) & 0xFFu) + ((third >> 8) & 0xFFu)) / 3u;
        var blue = ((first & 0xFFu) + (second & 0xFFu) + (third & 0xFFu)) / 3u;

        return (alpha << 24)
            | (red << 16)
            | (green << 8)
            | blue;
    }

    private static Color ToColor(uint argb)
    {
        return Color.FromArgb(
            (byte)((argb >> 24) & 0xFFu),
            (byte)((argb >> 16) & 0xFFu),
            (byte)((argb >> 8) & 0xFFu),
            (byte)(argb & 0xFFu));
    }

    private static StreamGeometry CreateTriangleGeometry(ProjectedSurfaceTriangle triangle)
    {
        var geometry = new StreamGeometry();
        using var context = geometry.Open();
        context.BeginFigure(triangle.A, isFilled: true);
        context.LineTo(triangle.B);
        context.LineTo(triangle.C);
        context.EndFigure(isClosed: true);
        return geometry;
    }

    private readonly record struct RawProjection(double X, double Y);

    private readonly record struct ProjectionTransform(double Scale, double OffsetX, double OffsetY)
    {
        public Point Apply(RawProjection rawProjection)
        {
            return new Point(
                x: OffsetX + (rawProjection.X * Scale),
                y: OffsetY + (rawProjection.Y * Scale));
        }
    }
}
