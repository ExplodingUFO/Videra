using Avalonia;
using Avalonia.Media;
using System.Numerics;
using Videra.SurfaceCharts.Avalonia.Controls.Overlay;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls;

internal readonly record struct ProjectedSurfaceTriangle(Point A, Point B, Point C, uint Color, double SortKey);

internal static class SurfaceScenePainter
{
    public static void DrawScene(DrawingContext context, SurfaceRenderScene? scene, Size viewSize)
    {
        ArgumentNullException.ThrowIfNull(context);

        var projection = SurfaceChartProjection.Create(scene, viewSize);
        DrawScene(context, scene, projection);
    }

    internal static void DrawScene(DrawingContext context, SurfaceRenderScene? scene, SurfaceChartProjection? projection)
    {
        ArgumentNullException.ThrowIfNull(context);

        var triangles = ProjectTriangles(scene, projection);
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

    /// <summary>
    /// Draws contour lines from a contour render scene using the specified projection.
    /// </summary>
    internal static void DrawContourLines(
        DrawingContext context,
        ContourRenderScene? contourScene,
        SurfaceChartProjection? projection,
        IPen? pen = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (contourScene is null || contourScene.Lines.Count == 0 || projection is null)
        {
            return;
        }

        pen ??= new Pen(Brushes.DarkGray, 1.0);

        foreach (var line in contourScene.Lines)
        {
            if (line.Segments.Count == 0)
            {
                continue;
            }

            var geometry = new StreamGeometry();
            using (var geometryContext = geometry.Open())
            {
                var first = true;
                foreach (var segment in line.Segments)
                {
                    // Convert normalized coordinates to model coordinates
                    var startModel = NormalizedToModel(segment.Start, contourScene.Metadata);
                    var endModel = NormalizedToModel(segment.End, contourScene.Metadata);

                    var startScreen = projection.Project(startModel);
                    var endScreen = projection.Project(endModel);

                    if (first)
                    {
                        geometryContext.BeginFigure(startScreen, isFilled: false);
                        first = false;
                    }

                    geometryContext.LineTo(endScreen);
                }

                if (!first)
                {
                    geometryContext.EndFigure(isClosed: false);
                }
            }

            context.DrawGeometry(null, pen, geometry);
        }
    }

    private static Vector3 NormalizedToModel(Vector3 normalized, SurfaceMetadata metadata)
    {
        // Normalized coordinates are in grid space (0..width-1, 0..height-1)
        // Model coordinates use the axis ranges
        var x = (float)(metadata.HorizontalAxis.Minimum + normalized.X * (metadata.HorizontalAxis.Maximum - metadata.HorizontalAxis.Minimum) / (metadata.Width - 1));
        var y = normalized.Y; // Y (height) stays as-is
        var z = (float)(metadata.VerticalAxis.Minimum + normalized.Z * (metadata.VerticalAxis.Maximum - metadata.VerticalAxis.Minimum) / (metadata.Height - 1));
        return new Vector3(x, y, z);
    }

    internal static IReadOnlyList<ProjectedSurfaceTriangle> ProjectTriangles(SurfaceRenderScene? scene, Size viewSize)
    {
        var projection = SurfaceChartProjection.Create(scene, viewSize);
        return ProjectTriangles(scene, projection);
    }

    internal static IReadOnlyList<ProjectedSurfaceTriangle> ProjectTriangles(SurfaceRenderScene? scene, SurfaceChartProjection? projection)
    {
        if (scene is null
            || scene.Tiles.Count == 0
            || projection is null)
        {
            return Array.Empty<ProjectedSurfaceTriangle>();
        }

        List<ProjectedSurfaceTriangle> triangles = [];

        foreach (var tile in scene.Tiles)
        {
            var projectedVertices = new SurfaceChartProjectedPoint[tile.Vertices.Count];
            for (var vertexIndex = 0; vertexIndex < tile.Vertices.Count; vertexIndex++)
            {
                projectedVertices[vertexIndex] = projection.ProjectPoint(tile.Vertices[vertexIndex].Position);
            }

            var indices = tile.Geometry.Indices;
            for (var index = 0; index <= indices.Count - 3; index += 3)
            {
                var first = projectedVertices[(int)indices[index]];
                var second = projectedVertices[(int)indices[index + 1]];
                var third = projectedVertices[(int)indices[index + 2]];

                var firstPoint = first.ScreenPoint;
                var secondPoint = second.ScreenPoint;
                var thirdPoint = third.ScreenPoint;

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
                        (first.SortKey + second.SortKey + third.SortKey) / 3d));
            }
        }

        triangles.Sort(static (left, right) => left.SortKey.CompareTo(right.SortKey));
        return triangles;
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

}
