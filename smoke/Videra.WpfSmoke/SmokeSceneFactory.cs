using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.WpfSmoke;

internal static class SmokeSceneFactory
{
    public const string EmissiveNormalProofObjectName = "WpfSmokeEmissiveNormalProofQuad";
    public const string MixedTransparencyProofObjectName = "WpfSmokeMixedTransparencyProof";

    public static Object3D CreateEmissiveNormalProofObject()
    {
        var sampler = new Sampler(
            SamplerId.New(),
            "NearestClamp",
            TextureFilter.Nearest,
            TextureFilter.Nearest,
            TextureWrapMode.ClampToEdge,
            TextureWrapMode.ClampToEdge);
        var emissiveTexture = new Texture2D(
            Texture2DId.New(),
            "WpfSmokeEmissive",
            2,
            1,
            TextureImageFormat.Png,
            [7, 8, 9],
            [
                0, 0, 0, 255,
                0, 255, 64, 255
            ]);
        var normalTexture = new Texture2D(
            Texture2DId.New(),
            "WpfSmokeNormal",
            2,
            1,
            TextureImageFormat.Png,
            [10, 11, 12],
            [
                128, 128, 255, 255,
                255, 64, 255, 255
            ]);
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            "WpfSmokeEmissiveNormalProof",
            new RgbaFloat(0.65f, 0.65f, 0.65f, 1f),
            emissive: new MaterialEmissive(
                Vector3.One,
                new MaterialTextureBinding(emissiveTexture.Id, sampler.Id, 0, TextureColorSpace.Srgb)),
            normalTexture: new MaterialNormalTextureBinding(
                new MaterialTextureBinding(normalTexture.Id, sampler.Id, 0, TextureColorSpace.Linear),
                1f));
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.55f, -0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(0.55f, -0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(0.55f, 0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(-0.55f, 0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Tangents =
            [
                new Vector4(Vector3.UnitX, 1f),
                new Vector4(Vector3.UnitX, 1f),
                new Vector4(Vector3.UnitX, 1f),
                new Vector4(Vector3.UnitX, 1f)
            ],
            TextureCoordinateSets =
            [
                new MeshTextureCoordinateSet(
                    0,
                    [
                        new Vector2(0f, 1f),
                        new Vector2(1f, 1f),
                        new Vector2(1f, 0f),
                        new Vector2(0f, 0f)
                    ])
            ],
            Topology = MeshTopology.Triangles
        };
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), EmissiveNormalProofObjectName, mesh, material.Id);
        var node = new SceneNode(
            SceneNodeId.New(),
            EmissiveNormalProofObjectName,
            Matrix4x4.CreateTranslation(0f, 0f, 0f),
            parentId: null,
            [primitive.Id]);
        var asset = new ImportedSceneAsset(
            "wpf-smoke-emissive-normal-proof.gltf",
            EmissiveNormalProofObjectName,
            [node],
            [primitive],
            [material],
            [emissiveTexture, normalTexture],
            [sampler]);

        return SceneObjectFactory.CreateDeferred(asset);
    }

    public static Object3D CreateMixedTransparencyProofObject()
    {
        var opaqueMaterial = new MaterialInstance(
            MaterialInstanceId.New(),
            "WpfSmokeOpaque",
            new RgbaFloat(0f, 0.8f, 0.2f, 1f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Opaque, 0f, false));
        var blendedMaterial = new MaterialInstance(
            MaterialInstanceId.New(),
            "WpfSmokeBlended",
            new RgbaFloat(0.9f, 0.1f, 0.1f, 0.5f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, true));
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(0.5f, -0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(0.5f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(-0.5f, 0.5f, 0f), Vector3.UnitZ, RgbaFloat.White),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };
        var opaquePrimitive = new MeshPrimitive(MeshPrimitiveId.New(), "opaque-quad", mesh, opaqueMaterial.Id);
        var blendedPrimitive = new MeshPrimitive(MeshPrimitiveId.New(), "blended-quad", mesh, blendedMaterial.Id);
        var node = new SceneNode(
            SceneNodeId.New(),
            MixedTransparencyProofObjectName,
            Matrix4x4.CreateTranslation(-1.55f, -0.2f, 0.5f),
            parentId: null,
            [opaquePrimitive.Id, blendedPrimitive.Id]);
        var asset = new ImportedSceneAsset(
            "wpf-smoke-mixed-transparency-proof.gltf",
            MixedTransparencyProofObjectName,
            [node],
            [opaquePrimitive, blendedPrimitive],
            [opaqueMaterial, blendedMaterial]);

        return SceneObjectFactory.CreateDeferred(asset);
    }
}
