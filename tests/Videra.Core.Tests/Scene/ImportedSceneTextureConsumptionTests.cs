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

    [Fact]
    public void CreateDeferredRuntimeObjects_AppliesOcclusionTextureUsingRequestedCoordinateSet()
    {
        var asset = CreateOccludedAsset(
            coordinateSet: 1,
            transform: MaterialTextureTransform.Identity,
            strength: 1f,
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)]),
                new MeshTextureCoordinateSet(1, [new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)])
            ]);

        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(static vertex => IsApproximately(vertex.Color, RgbaFloat.Black));
    }

    [Fact]
    public void CreateDeferredRuntimeObjects_AppliesOcclusionTextureTransform()
    {
        var asset = CreateOccludedAsset(
            coordinateSet: 0,
            transform: new MaterialTextureTransform(new Vector2(0.5f, 0f), Vector2.One, 0f),
            strength: 1f,
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)])
            ]);

        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(static vertex => IsApproximately(vertex.Color, RgbaFloat.Black));
    }

    [Fact]
    public void CreateDeferredRuntimeObjects_BakesEmissiveTextureIntoVertexColorsAndMatchesPayload()
    {
        var asset = CreateEmissiveAsset(
            coordinateSet: 1,
            transform: MaterialTextureTransform.Identity,
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)]),
                new MeshTextureCoordinateSet(1, [new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)])
            ]);

        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        sceneObject.MeshPayload.Should().NotBeNull();
        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();

        var expected = new RgbaFloat(0f, 1f, 0f, 1f);
        sceneObject.MeshPayload!.Vertices.Should().OnlyContain(vertex => IsApproximately(vertex.Color, expected));
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(vertex => IsApproximately(vertex.Color, expected));
        runtimeObjects[0].MeshPayload!.Vertices.Select(static vertex => vertex.Color)
            .Should().Equal(sceneObject.MeshPayload!.Vertices.Select(static vertex => vertex.Color));
    }

    [Fact]
    public void CreateDeferredRuntimeObjects_BakesNormalTextureIntoVertexNormalsAndMatchesPayload()
    {
        var asset = CreateNormalMappedAsset(
            coordinateSet: 1,
            transform: MaterialTextureTransform.Identity,
            textureCoordinateSets:
            [
                new MeshTextureCoordinateSet(0, [new Vector2(0.25f, 0f), new Vector2(0.25f, 0f), new Vector2(0.25f, 0f)]),
                new MeshTextureCoordinateSet(1, [new Vector2(0.75f, 0f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f)])
            ]);

        var sceneObject = SceneObjectFactory.CreateDeferred(asset);
        var runtimeObjects = SceneObjectFactory.CreateDeferredRuntimeObjects(asset);

        sceneObject.MeshPayload.Should().NotBeNull();
        runtimeObjects.Should().ContainSingle();
        runtimeObjects[0].MeshPayload.Should().NotBeNull();

        var expected = Vector3.Normalize(new Vector3(1f, -1f, 1f));
        sceneObject.MeshPayload!.Vertices.Should().OnlyContain(vertex => IsApproximately(vertex.Normal, expected));
        runtimeObjects[0].MeshPayload!.Vertices.Should().OnlyContain(vertex => IsApproximately(vertex.Normal, expected));
        runtimeObjects[0].MeshPayload!.Vertices.Select(static vertex => vertex.Normal)
            .Should().Equal(sceneObject.MeshPayload!.Vertices.Select(static vertex => vertex.Normal));
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

    private static ImportedSceneAsset CreateEmissiveAsset(
        int coordinateSet,
        MaterialTextureTransform transform,
        MeshTextureCoordinateSet[] textureCoordinateSets)
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
                new MaterialTextureBinding(emissiveTexture.Id, sampler.Id, coordinateSet, TextureColorSpace.Srgb, transform)));
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.Black)
            ],
            Indices = [0u, 1u, 2u],
            TextureCoordinateSets = textureCoordinateSets,
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

    private static ImportedSceneAsset CreateNormalMappedAsset(
        int coordinateSet,
        MaterialTextureTransform transform,
        MeshTextureCoordinateSet[] textureCoordinateSets)
    {
        var normalTexture = new Texture2D(
            Texture2DId.New(),
            "Normal",
            2,
            1,
            TextureImageFormat.Png,
            [20, 21, 22],
            [
                128, 128, 255, 255,
                255, 0, 255, 255
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
            "NormalMapped",
            RgbaFloat.White,
            normalTexture: new MaterialNormalTextureBinding(
                new MaterialTextureBinding(normalTexture.Id, sampler.Id, coordinateSet, TextureColorSpace.Linear, transform),
                1f));
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Tangents =
            [
                new Vector4(Vector3.UnitX, 1f),
                new Vector4(Vector3.UnitX, 1f),
                new Vector4(Vector3.UnitX, 1f)
            ],
            TextureCoordinateSets = textureCoordinateSets,
            Topology = MeshTopology.Triangles
        };
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), "Triangle", mesh, material.Id);
        var node = new SceneNode(SceneNodeId.New(), "Root", Matrix4x4.Identity, parentId: null, [primitive.Id]);
        return new ImportedSceneAsset(
            "normal.gltf",
            "normal.gltf",
            [node],
            [primitive],
            [material],
            [normalTexture],
            [sampler]);
    }

    private static ImportedSceneAsset CreateOccludedAsset(
        int coordinateSet,
        MaterialTextureTransform transform,
        float strength,
        MeshTextureCoordinateSet[] textureCoordinateSets)
    {
        var texture = new Texture2D(
            Texture2DId.New(),
            "Occlusion",
            2,
            1,
            TextureImageFormat.Png,
            [4, 5, 6],
            [
                255, 255, 255, 255,
                0, 0, 0, 255
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
            "Occluded",
            RgbaFloat.White,
            occlusionTexture: new MaterialOcclusionTextureBinding(
                new MaterialTextureBinding(texture.Id, sampler.Id, coordinateSet, TextureColorSpace.Linear, transform),
                strength));
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
            "occluded.gltf",
            "occluded.gltf",
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

    private static bool IsApproximately(Vector3 actual, Vector3 expected)
    {
        return MathF.Abs(actual.X - expected.X) < 0.001f &&
               MathF.Abs(actual.Y - expected.Y) < 0.001f &&
               MathF.Abs(actual.Z - expected.Z) < 0.001f;
    }
}
