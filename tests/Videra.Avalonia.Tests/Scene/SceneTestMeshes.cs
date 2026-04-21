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
        var primitive = new MeshPrimitive(MeshPrimitiveId.New(), $"{name}#primitive0", mesh);
        var rootNode = new SceneNode(SceneNodeId.New(), name, Matrix4x4.Identity, parentId: null, [primitive.Id]);
        return new ImportedSceneAsset(name, name, [rootNode], [primitive]);
    }

    public static Object3D CreateDeferredObject(string name = "Deferred")
    {
        var sceneObject = new Object3D { Name = name };
        sceneObject.PrepareDeferredMesh(CreateTriangleMesh());
        return sceneObject;
    }
}
