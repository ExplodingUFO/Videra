using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.WpfSmoke;

internal static class SmokeSceneFactory
{
    public static Object3D CreateWhiteQuad(IResourceFactory factory, float halfExtent = 0.8f)
    {
        var vertices = new[]
        {
            new VertexPositionNormalColor(new Vector3(-halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(halfExtent, -halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
            new VertexPositionNormalColor(new Vector3(-halfExtent, halfExtent, 0f), Vector3.UnitZ, RgbaFloat.White),
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        var mesh = new MeshData
        {
            Vertices = vertices,
            Indices = indices,
            Topology = MeshTopology.Triangles
        };

        var quad = new Object3D { Name = "WpfSmokeQuad" };
        quad.Initialize(factory, mesh);
        return quad;
    }
}
