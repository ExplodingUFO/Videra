using System.Numerics;
using FluentAssertions;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests.Rendering;

public class SurfaceRendererInputTests
{
    [Fact]
    public void BuildTile_MapsTileSamplesIntoDedicatedRenderInput()
    {
        var renderer = new SurfaceRenderer();
        var metadata = CreateMetadata(width: 4, height: 3);
        var colorMap = CreateColorMap();
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(startX: 1, startY: 1, width: 2, height: 2),
            new float[]
            {
                10f, 20f,
                30f, 40f
            },
            new SurfaceValueRange(10.0, 40.0));

        var renderTile = renderer.BuildTile(metadata, tile, colorMap);

        renderTile.Key.Should().Be(tile.Key);
        renderTile.Bounds.Should().Be(tile.Bounds);
        renderTile.Geometry.SampleWidth.Should().Be(2);
        renderTile.Geometry.SampleHeight.Should().Be(2);
        renderTile.Vertices.Should().Equal(
            new SurfaceRenderVertex(new Vector3(10f, 10f, 10f), 0xFF000000u),
            new SurfaceRenderVertex(new Vector3(20f, 20f, 10f), 0xFF555555u),
            new SurfaceRenderVertex(new Vector3(10f, 30f, 20f), 0xFFAAAAAAu),
            new SurfaceRenderVertex(new Vector3(20f, 40f, 20f), 0xFFFFFFFFu));
        renderTile.Geometry.Indices.Should().Equal(0u, 2u, 1u, 1u, 2u, 3u);
    }

    [Fact]
    public void BuildScene_ReturnsEmptySceneForEmptyTileSet()
    {
        var renderer = new SurfaceRenderer();
        var metadata = CreateMetadata(width: 4, height: 4);
        var colorMap = CreateColorMap();

        var scene = renderer.BuildScene(metadata, Array.Empty<SurfaceTile>(), colorMap);

        scene.Metadata.Should().BeSameAs(metadata);
        scene.Tiles.Should().BeEmpty();
    }

    [Fact]
    public void BuildTile_PreservesVerticesForDegenerateTileWithoutTriangles()
    {
        var renderer = new SurfaceRenderer();
        var metadata = CreateMetadata(width: 3, height: 3);
        var colorMap = CreateColorMap();
        var tile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 1),
            width: 1,
            height: 3,
            new SurfaceTileBounds(startX: 2, startY: 0, width: 1, height: 3),
            new float[]
            {
                5f,
                15f,
                25f
            },
            new SurfaceValueRange(5.0, 25.0));

        var renderTile = renderer.BuildTile(metadata, tile, colorMap);

        renderTile.Vertices.Should().HaveCount(3);
        renderTile.Geometry.Indices.Should().BeEmpty();
        renderTile.Vertices.Should().Equal(
            new SurfaceRenderVertex(new Vector3(30f, 5f, 0f), 0xFF000000u),
            new SurfaceRenderVertex(new Vector3(30f, 15f, 10f), 0xFF2B2B2Bu),
            new SurfaceRenderVertex(new Vector3(30f, 25f, 20f), 0xFF808080u));
    }

    [Fact]
    public void BuildTile_DoesNotAllowMutatingExposedVertices()
    {
        var renderer = new SurfaceRenderer();
        var renderTile = renderer.BuildTile(
            CreateMetadata(width: 2, height: 2),
            new SurfaceTile(
                new SurfaceTileKey(0, 0, 0, 0),
                width: 2,
                height: 2,
                new SurfaceTileBounds(0, 0, 2, 2),
                new float[] { 10f, 20f, 30f, 40f },
                new SurfaceValueRange(10.0, 40.0)),
            CreateColorMap());

        var act = () => ((IList<SurfaceRenderVertex>)renderTile.Vertices)[0] =
            new SurfaceRenderVertex(new Vector3(999f, 999f, 999f), 0u);

        act.Should().Throw<NotSupportedException>();
        renderTile.Vertices[0].Should().Be(new SurfaceRenderVertex(new Vector3(0f, 10f, 0f), 0xFF000000u));
    }

    [Fact]
    public void BuildScene_DoesNotAllowMutatingExposedTiles()
    {
        var renderer = new SurfaceRenderer();
        var metadata = CreateMetadata(width: 2, height: 2);
        var sourceTile = new SurfaceTile(
            new SurfaceTileKey(0, 0, 0, 0),
            width: 2,
            height: 2,
            new SurfaceTileBounds(0, 0, 2, 2),
            new float[] { 10f, 20f, 30f, 40f },
            new SurfaceValueRange(10.0, 40.0));
        var scene = renderer.BuildScene(metadata, new[] { sourceTile }, CreateColorMap());
        var replacement = renderer.BuildTile(
            metadata,
            new SurfaceTile(
                new SurfaceTileKey(0, 0, 1, 0),
                width: 2,
                height: 2,
                new SurfaceTileBounds(0, 0, 2, 2),
                new float[] { 40f, 30f, 20f, 10f },
                new SurfaceValueRange(10.0, 40.0)),
            CreateColorMap());

        var act = () => ((IList<SurfaceRenderTile>)scene.Tiles)[0] = replacement;

        act.Should().Throw<NotSupportedException>();
        scene.Tiles[0].Key.Should().Be(sourceTile.Key);
    }

    [Fact]
    public void SurfaceRenderTileCtor_RejectsBoundsThatDoNotMatchGeometryShape()
    {
        var geometry = new SurfacePatchGeometryBuilder().Build(2, 2);
        var vertices = new[]
        {
            new SurfaceRenderVertex(new Vector3(0f, 0f, 0f), 0u),
            new SurfaceRenderVertex(new Vector3(1f, 0f, 0f), 0u),
            new SurfaceRenderVertex(new Vector3(0f, 1f, 0f), 0u),
            new SurfaceRenderVertex(new Vector3(1f, 1f, 0f), 0u)
        };

        var act = () => new SurfaceRenderTile(
            new SurfaceTileKey(0, 0, 0, 0),
            new SurfaceTileBounds(0, 0, 3, 2),
            geometry,
            vertices);

        act.Should().Throw<ArgumentException>()
            .Where(ex => ex.ParamName == "bounds");
    }

    private static SurfaceMetadata CreateMetadata(int width, int height)
    {
        return new SurfaceMetadata(
            width,
            height,
            new SurfaceAxisDescriptor("Time", "s", 0.0, 30.0),
            new SurfaceAxisDescriptor("Frequency", "Hz", 0.0, 20.0),
            new SurfaceValueRange(0.0, 40.0));
    }

    private static SurfaceColorMap CreateColorMap()
    {
        return new SurfaceColorMap(
            new SurfaceValueRange(10.0, 40.0),
            new SurfaceColorMapPalette(0xFF000000u, 0xFFFFFFFFu));
    }
}
