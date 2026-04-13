using System.Reflection;
using Avalonia;
using Avalonia.Media;
using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceScenePainterTests
{
    [Fact]
    public void ProjectTriangles_WithBuiltScene_ReturnsProjectedTriangles()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata(width: 2, height: 2);
            var tile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
            var scene = new SurfaceRenderer().BuildScene(metadata, [tile], CreateColorMap(metadata));

            var triangles = SurfaceScenePainter.ProjectTriangles(scene, new Size(320, 180));

            triangles.Should().HaveCount(2);
            triangles.Should().OnlyContain(static triangle => HasFinitePoints(triangle) && triangle.A != triangle.B);
        });
    }

    [Fact]
    public void ProjectTriangles_WithoutSceneOrTiles_ReturnsEmpty()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata(width: 2, height: 2);
            var emptyScene = new SurfaceRenderScene(metadata, []);

            SurfaceScenePainter.ProjectTriangles(null, new Size(320, 180)).Should().BeEmpty();
            SurfaceScenePainter.ProjectTriangles(emptyScene, new Size(320, 180)).Should().BeEmpty();
        });
    }

    [Fact]
    public void DrawScene_WithBuiltScene_IssuesGeometryDrawCalls()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata(width: 2, height: 2);
            var tile = SurfaceChartTestHelpers.CreateTile(metadata, new SurfaceTileKey(0, 0, 0, 0), tileValue: 12f);
            var scene = new SurfaceRenderer().BuildScene(metadata, [tile], CreateColorMap(metadata));
            var expectedTriangleCount = SurfaceScenePainter.ProjectTriangles(scene, new Size(320, 180)).Count;
            using var drawingContext = new RecordingDrawingContext();

            SurfaceScenePainter.DrawScene(drawingContext.Context, scene, new Size(320, 180));

            expectedTriangleCount.Should().BeGreaterThan(0);
            drawingContext.GeometryDrawCallCount.Should().Be(expectedTriangleCount);
        });
    }

    [Fact]
    public void DrawScene_WithoutSceneOrTiles_IssuesNoGeometryDrawCalls()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var metadata = CreateMetadata(width: 2, height: 2);
            var emptyScene = new SurfaceRenderScene(metadata, []);
            using var drawingContext = new RecordingDrawingContext();

            SurfaceScenePainter.DrawScene(drawingContext.Context, null, new Size(320, 180));
            SurfaceScenePainter.DrawScene(drawingContext.Context, emptyScene, new Size(320, 180));

            drawingContext.GeometryDrawCallCount.Should().Be(0);
        });
    }

    [Fact]
    public Task SurfaceChartView_LoadedTiles_BuildsRenderSceneConsumableByPainter()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata(width: 4, height: 4);
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new SurfaceChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            var scene = GetRenderScene(view);

            scene.Should().NotBeNull();
            SurfaceScenePainter.ProjectTriangles(scene, new Size(240, 160)).Should().HaveCount(18);
        });
    }

    [Fact]
    public Task SurfaceChartView_Render_WithLoadedTiles_IssuesGeometryDrawCalls()
    {
        return AvaloniaHeadlessTestSession.RunAsync(async () =>
        {
            var metadata = CreateMetadata(width: 4, height: 4);
            var source = new ScriptedSurfaceTileSource(metadata, defaultTileValue: 6f);
            var view = new SurfaceChartView();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));
            view.Source = source;

            await SurfaceChartTestHelpers.WaitForLoadedTileValuesAsync(view, [6f]);

            var scene = GetRenderScene(view);
            scene.Should().NotBeNull();

            var expectedTriangleCount = SurfaceScenePainter.ProjectTriangles(scene, view.Bounds.Size).Count;
            expectedTriangleCount.Should().BeGreaterThan(0);
            using var drawingContext = new RecordingDrawingContext();

            view.Render(drawingContext.Context);

            drawingContext.GeometryDrawCallCount.Should().Be(expectedTriangleCount);
        });
    }

    [Fact]
    public void SurfaceChartView_Render_WithoutRenderScene_IssuesNoGeometryDrawCalls()
    {
        AvaloniaHeadlessTestSession.Run(() =>
        {
            var view = new SurfaceChartView();
            using var drawingContext = new RecordingDrawingContext();

            view.Measure(new Size(240, 160));
            view.Arrange(new Rect(0, 0, 240, 160));

            view.Render(drawingContext.Context);

            drawingContext.GeometryDrawCallCount.Should().Be(0);
        });
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("X", unit: null, minimum: 0, maximum: width - 1),
            new SurfaceAxisDescriptor("Y", unit: null, minimum: 0, maximum: height - 1),
            new SurfaceValueRange(0, 100));
    }

    private static SurfaceColorMap CreateColorMap(SurfaceMetadata metadata)
    {
        return new SurfaceColorMap(metadata.ValueRange, new SurfaceColorMapPalette(0xFF203040u, 0xFFE0F0FFu));
    }

    private static SurfaceRenderScene? GetRenderScene(SurfaceChartView view)
    {
        var field = typeof(SurfaceChartView).GetField("_renderScene", BindingFlags.Instance | BindingFlags.NonPublic);
        field.Should().NotBeNull();
        return (SurfaceRenderScene?)field!.GetValue(view);
    }

    private static bool HasFinitePoints(ProjectedSurfaceTriangle triangle)
    {
        return double.IsFinite(triangle.A.X)
            && double.IsFinite(triangle.A.Y)
            && double.IsFinite(triangle.B.X)
            && double.IsFinite(triangle.B.Y)
            && double.IsFinite(triangle.C.X)
            && double.IsFinite(triangle.C.Y);
    }

    private sealed class RecordingDrawingContext : IDisposable
    {
        private readonly DrawingGroup _drawingGroup = new();
        private readonly DrawingContext _drawingContext;
        private int? _geometryDrawCallCount;

        public RecordingDrawingContext()
        {
            _drawingContext = _drawingGroup.Open();
        }

        public DrawingContext Context => _drawingContext;

        public int GeometryDrawCallCount
        {
            get
            {
                CompleteRecording();
                return _geometryDrawCallCount!.Value;
            }
        }

        public void Dispose()
        {
            CompleteRecording();
        }

        private void CompleteRecording()
        {
            if (_geometryDrawCallCount.HasValue)
            {
                return;
            }

            _drawingContext.Dispose();
            _geometryDrawCallCount = CountGeometryDrawings(_drawingGroup);
        }

        private static int CountGeometryDrawings(Drawing drawing)
        {
            return drawing switch
            {
                GeometryDrawing => 1,
                DrawingGroup group => group.Children.Sum(CountGeometryDrawings),
                _ => 0,
            };
        }
    }
}
