using System.Numerics;
using FluentAssertions;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Xunit;

namespace Videra.Core.Tests.Graphics;

public sealed class ImportedSceneDeferredBakingTests
{
    [Fact]
    public void SceneObjectFactory_CreateDeferred_BakesEmissiveTextureIntoVertexColors()
    {
        var asset = CreateEmissiveAsset();

        var sceneObject = SceneObjectFactory.CreateDeferred(asset);

        sceneObject.MeshPayload.Should().NotBeNull();
        sceneObject.MeshPayload!.Vertices.Should().OnlyContain(vertex => IsApproximately(vertex.Color, new RgbaFloat(0f, 1f, 0f, 1f)));
    }

    private static ImportedSceneAsset CreateEmissiveAsset()
    {
        var emissiveTexture = new Texture2D(
            Texture2DId.New(),
            "Emissive",
            2,
            1,
            TextureImageFormat.Png,
            [10, 11, 12],
            [
                0, 0, 0, 255,
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
            "Emissive",
            RgbaFloat.White,
            emissive: new MaterialEmissive(
                Vector3.One,
                new MaterialTextureBinding(emissiveTexture.Id, sampler.Id, 0, TextureColorSpace.Srgb)));
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black)
            ],
            Indices = [0u, 1u, 2u],
            TextureCoordinateSets =
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)])
            ],
            Topology = MeshTopology.Triangles
        };
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), "Triangle", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), "Root", Matrix4x4.Identity, parentId: null, [primitive.Id]);
        return new ImportedSceneAsset(
            "emissive.gltf",
            "emissive.gltf",
            [node],
            [primitive],
            [material],
            [emissiveTexture],
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
