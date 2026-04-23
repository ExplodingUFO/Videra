using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.ConsumerSmoke.Views;

internal static class SmokeSceneFactory
{
    public const string EmissiveNormalProofObjectName = "ConsumerSmokeEmissiveNormalProofQuad";

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
            "ConsumerSmokeEmissive",
            2,
            1,
            TextureImageFormat.Png,
            [1, 2, 3],
            [
                0, 0, 0, 255,
                0, 255, 64, 255
            ]);
        var normalTexture = new Texture2D(
            Texture2DId.New(),
            "ConsumerSmokeNormal",
            2,
            1,
            TextureImageFormat.Png,
            [4, 5, 6],
            [
                128, 128, 255, 255,
                255, 64, 255, 255
            ]);
        var material = new MaterialInstance(
            MaterialInstanceId.New(),
            "ConsumerSmokeEmissiveNormalProof",
            new RgbaFloat(0.65f, 0.65f, 0.65f, 1f),
            emissive: new MaterialEmissive(
                Vector3.One,
                new MaterialTextureBinding(emissiveTexture.Id, sampler.Id, 0, TextureColorSpace.Srgb)),
            normalTexture: new MaterialNormalTextureBinding(
                new MaterialTextureBinding(normalTexture.Id, sampler.Id, 0, TextureColorSpace.Linear),
                1f));
        var mesh = new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(-0.55f, -0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.55f, -0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0.55f, 0.55f, 0f), Vector3.UnitZ, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(-0.55f, 0.55f, 0f), Vector3.UnitZ, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u, 0u, 2u, 3u],
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
            Matrix4x4.CreateTranslation(1.55f, 0.2f, 0.3f),
            parentId: null,
            [primitive.Id]);
        var asset = new ImportedSceneAsset(
            "consumer-smoke-emissive-normal-proof.gltf",
            EmissiveNormalProofObjectName,
            [node],
            [primitive],
            [material],
            [emissiveTexture, normalTexture],
            [sampler]);

        return SceneObjectFactory.CreateDeferred(asset);
    }
}
