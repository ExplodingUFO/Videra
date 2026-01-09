using System.Collections.Generic;
using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Demo.Services;

public static class DemoMeshFactory
{
    public static Object3D CreateCube(IResourceFactory factory, float size = 2f)
    {
        var half = size * 0.5f;
        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();

        AddFace(vertices, indices,
            new Vector3(half, -half, -half),
            new Vector3(half, -half, half),
            new Vector3(half, half, half),
            new Vector3(half, half, -half),
            Vector3.UnitX,
            new RgbaFloat(0.9f, 0.2f, 0.2f, 1f));

        AddFace(vertices, indices,
            new Vector3(-half, -half, half),
            new Vector3(-half, -half, -half),
            new Vector3(-half, half, -half),
            new Vector3(-half, half, half),
            -Vector3.UnitX,
            new RgbaFloat(0.6f, 0.1f, 0.1f, 1f));

        AddFace(vertices, indices,
            new Vector3(-half, half, -half),
            new Vector3(half, half, -half),
            new Vector3(half, half, half),
            new Vector3(-half, half, half),
            Vector3.UnitY,
            new RgbaFloat(0.2f, 0.9f, 0.2f, 1f));

        AddFace(vertices, indices,
            new Vector3(-half, -half, half),
            new Vector3(half, -half, half),
            new Vector3(half, -half, -half),
            new Vector3(-half, -half, -half),
            -Vector3.UnitY,
            new RgbaFloat(0.1f, 0.6f, 0.1f, 1f));

        AddFace(vertices, indices,
            new Vector3(-half, -half, half),
            new Vector3(-half, half, half),
            new Vector3(half, half, half),
            new Vector3(half, -half, half),
            Vector3.UnitZ,
            new RgbaFloat(0.2f, 0.2f, 0.9f, 1f));

        AddFace(vertices, indices,
            new Vector3(half, -half, -half),
            new Vector3(half, half, -half),
            new Vector3(-half, half, -half),
            new Vector3(-half, -half, -half),
            -Vector3.UnitZ,
            new RgbaFloat(0.1f, 0.1f, 0.6f, 1f));

        var mesh = new MeshData
        {
            Vertices = vertices.ToArray(),
            Indices = indices.ToArray(),
            Topology = MeshTopology.Triangles
        };

        var cube = new Object3D { Name = "Demo Cube" };
        cube.Initialize(factory, mesh);
        return cube;
    }

    private static void AddFace(
        List<VertexPositionNormalColor> vertices,
        List<uint> indices,
        Vector3 v0,
        Vector3 v1,
        Vector3 v2,
        Vector3 v3,
        Vector3 normal,
        RgbaFloat color)
    {
        var start = (uint)vertices.Count;
        vertices.Add(new VertexPositionNormalColor(v0, normal, color));
        vertices.Add(new VertexPositionNormalColor(v1, normal, color));
        vertices.Add(new VertexPositionNormalColor(v2, normal, color));
        vertices.Add(new VertexPositionNormalColor(v3, normal, color));

        indices.Add(start);
        indices.Add(start + 1);
        indices.Add(start + 2);
        indices.Add(start);
        indices.Add(start + 2);
        indices.Add(start + 3);
    }
}
