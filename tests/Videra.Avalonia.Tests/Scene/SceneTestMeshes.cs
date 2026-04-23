using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;

namespace Videra.Avalonia.Tests.Scene;

internal static class SceneTestMeshes
{
    public static MeshData CreateTriangleMesh()
    {
        return new MeshData
        {
            Vertices =
            [
                new VertexPositionNormalColor(new Vector3(0f, 0f, 0f), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(1f, 0f, 0f), Vector3.UnitY, RgbaFloat.White),
                new VertexPositionNormalColor(new Vector3(0f, 1f, 0f), Vector3.UnitY, RgbaFloat.White)
            ],
            Indices = [0u, 1u, 2u],
            Topology = MeshTopology.Triangles
        };
    }

    public static ImportedSceneAsset CreateImportedAsset(string name = "triangle.obj")
    {
        var mesh = CreateTriangleMesh();
        var material = new MaterialInstance(MaterialInstanceId.New(), $"{name}#material0", RgbaFloat.White);
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), $"{name}#primitive0", mesh, material.Id);
        var rootNode = new SceneNode(SceneNodeId.New(), name, Matrix4x4.Identity, parentId: null, [primitive.Id]);
        return new ImportedSceneAsset(name, name, [rootNode], [primitive], [material]);
    }

    public static ImportedSceneAsset CreateMixedAlphaImportedAsset(string name = "mixed-alpha.gltf")
    {
        var mesh = CreateTriangleMesh();
        var opaqueMaterial = new MaterialInstance(MaterialInstanceId.New(), $"{name}#opaque", RgbaFloat.White);
        var blendMaterial = new MaterialInstance(
            MaterialInstanceId.New(),
            $"{name}#blend",
            new RgbaFloat(1f, 1f, 1f, 0.5f),
            alpha: new MaterialAlphaSettings(MaterialAlphaMode.Blend, 0.5f, true));
        var opaquePrimitive = new MeshPrimitive(MeshPrimitiveId.New(), $"{name}#primitive0", mesh, opaqueMaterial.Id);
        var blendPrimitive = new MeshPrimitive(MeshPrimitiveId.New(), $"{name}#primitive1", mesh, blendMaterial.Id);
        var rootNode = new SceneNode(
            SceneNodeId.New(),
            name,
            Matrix4x4.Identity,
            parentId: null,
            [opaquePrimitive.Id, blendPrimitive.Id]);

        return new ImportedSceneAsset(
            name,
            name,
            [rootNode],
            [opaquePrimitive, blendPrimitive],
            [opaqueMaterial, blendMaterial]);
    }

    public static Object3D CreateDeferredObject(string name = "Deferred")
    {
        var sceneObject = new Object3D { Name = name };
        sceneObject.PrepareDeferredMesh(CreateTriangleMesh());
        return sceneObject;
    }
}
