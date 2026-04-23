using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Scene;

public sealed class ImportedSceneTextureConsumptionTests
{
    [Fact]
    public void SceneObjectFactory_CreateDeferredRuntimeObjects_BakesBaseColorTextureUsingRequestedCoordinateSet()
    {
        var asset = CreateTexturedAsset(
            coordinateSet: 1,
            transform: MaterialTextureTransform.Identity,
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)]),
                new MeshTextureCoordinateSet(1, [new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)])
            ]);

        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(static vertex => IsApproximately(vertex.Color, new RgbaFloat(0f, 1f, 0f, 1f)));
    }

    [Fact]
    public void CreateDeferredRuntimeObjects_BakesBaseColorTextureTransformIntoVertexColors()
    {
        var asset = CreateTexturedAsset(
            coordinateSet: 0,
            transform: new MaterialTextureTransform(new Vector2(0.5f, 0f), Vector2.One, 0f),
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)])
            ]);

        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(static vertex => IsApproximately(vertex.Color, new RgbaFloat(0f, 1f, 0f, 1f)));
    }

    private static ImportedSceneAsset CreateTexturedAsset(
        int coordinateSet,
        MaterialTextureTransform transform,
        MeshTextureCoordinateSet[] textureCoordinateSets)
    {
        var texture = new Texture2D(
            Texture2DId.New(),
            "BaseColor",
            2,
            1,
            TextureImageFormat.Png,
            [1, 2, 3],
            [
                255, 0, 0, 255,
                0, 255, 0, 255
            ]);
        var sampler = new Sampler(
            SamplerId.New(),
            "NearestClamp",
            TextureFilter.Nearest,
            TextureFilter.Nearest,
            TextureWrapMode.ClampToEdge,
            TextureWrapMode.ClampToEdge);
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            "Textured",
            RgbaFloat.White,
            new MaterialTextureBinding(texture.Id, sampler.Id, coordinateSet, TextureColorSpace.Srgb, transform));
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            TextureCoordinateSets = textureCoordinateSets,
            Topology = MeshTopology.Triangles
        };
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), "Triangle", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), "Root", Matrix4x4.Identity, parentId: null, [primitive.Id]);
        return new ImportedSceneAsset(
            "textured.gltf",
            "textured.gltf",
            [node],
            [primitive],
            [material],
            [texture],
            [sampler]);
    }

    private static bool IsApproximately(RgbaFloat actual, RgbaFloat expected)
    {
        return MathF.Abs(actual.R - expected.R) < 0.001f &&
               MathF.Abs(actual.G - expected.G) < 0.001f &&
               MathF.Abs(actual.B - expected.B) < 0.001f &&
               MathF.Abs(actual.A - expected.A) < 0.001f;
    }
}
