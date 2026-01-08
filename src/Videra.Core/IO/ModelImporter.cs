using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using SharpGLTF.Schema2;

namespace Videra.Core.IO;

public static class ModelImporter
{
    public static string[] SupportedFormats => new[]
        { "*.gltf", "*.glb", "*.obj" };

    public static Object3D Load(string filePath, IResourceFactory factory)
    {
        try
        {
            Console.WriteLine($"[ModelImporter] Loading: {filePath}");
            
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            
            MeshData meshData = ext switch
            {
                ".gltf" or ".glb" => LoadWithSharpGLTF(filePath),
                ".obj" => LoadSimpleObj(filePath),
                _ => throw new NotSupportedException($"Format {ext} not supported")
            };
            
            Console.WriteLine($"[ModelImporter] Loaded {meshData.Vertices.Length} vertices, {meshData.Indices.Length} indices");

            var obj = new Object3D
            {
                Name = Path.GetFileName(filePath)
            };

            Console.WriteLine($"[ModelImporter] Initializing GPU resources...");
            obj.Initialize(factory, meshData);
            Console.WriteLine($"[ModelImporter] ✓ Success");

            return obj;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ModelImporter] ✗ Failed: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    private static MeshData LoadWithSharpGLTF(string filePath)
    {
        var model = ModelRoot.Load(filePath);
        
        var allVertices = new List<VertexPositionNormalColor>();
        var allIndices = new List<uint>();
        uint indexOffset = 0;

        foreach (var mesh in model.LogicalMeshes)
        {
            Console.WriteLine($"[SharpGLTF] Processing mesh: {mesh.Name}");
            
            foreach (var primitive in mesh.Primitives)
            {
                // 获取顶点数据
                var positions = primitive.GetVertexAccessor("POSITION")?.AsVector3Array();
                var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
                var colors = primitive.GetVertexAccessor("COLOR_0")?.AsVector4Array();
                
                if (positions == null)
                {
                    Console.WriteLine("[SharpGLTF] Skipping primitive without positions");
                    continue;
                }

                // 获取材质颜色
                var material = primitive.Material;
                var baseColor = material?.FindChannel("BaseColor")?.Parameter ?? Vector4.One;
                var defaultColor = new RgbaFloat(baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);

                // 处理顶点
                for (int i = 0; i < positions.Count; i++)
                {
                    var pos = positions[i];
                    var normal = normals != null && i < normals.Count 
                        ? normals[i] 
                        : Vector3.UnitY;
                    
                    var color = colors != null && i < colors.Count
                        ? new RgbaFloat(colors[i].X, colors[i].Y, colors[i].Z, colors[i].W)
                        : defaultColor;

                    allVertices.Add(new VertexPositionNormalColor(pos, normal, color));
                }

                // 处理索引
                var indices = primitive.GetIndices();
                foreach (var idx in indices)
                {
                    if (indexOffset + idx > uint.MaxValue)
                    {
                        Console.WriteLine("[SharpGLTF] Index overflow, stopping");
                        break;
                    }
                    allIndices.Add(indexOffset + (uint)idx);
                }

                indexOffset += (uint)positions.Count;
                Console.WriteLine($"[SharpGLTF] Processed {positions.Count} vertices, current offset: {indexOffset}");
            }
        }

        Console.WriteLine($"[SharpGLTF] Total: {allVertices.Count} vertices, {allIndices.Count} indices");

        return new MeshData
        {
            Vertices = allVertices.ToArray(),
            Indices = allIndices.ToArray(),
            Topology = MeshTopology.Triangles
        };
    }

    // 简单的 OBJ 加载器（备用）
    private static MeshData LoadSimpleObj(string path)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var finalVertices = new List<VertexPositionNormalColor>();
        var finalIndices = new List<uint>();

        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v" when parts.Length >= 4:
                    vertices.Add(new Vector3(
                        float.Parse(parts[1]),
                        float.Parse(parts[2]),
                        float.Parse(parts[3])));
                    break;

                case "vn" when parts.Length >= 4:
                    normals.Add(new Vector3(
                        float.Parse(parts[1]),
                        float.Parse(parts[2]),
                        float.Parse(parts[3])));
                    break;

                case "f" when parts.Length >= 4:
                    for (int i = 1; i <= 3; i++)
                    {
                        var indices = parts[i].Split('/');
                        var vIdx = int.Parse(indices[0]) - 1;
                        var nIdx = indices.Length > 2 && !string.IsNullOrEmpty(indices[2]) 
                            ? int.Parse(indices[2]) - 1 
                            : 0;

                        var v = vertices[vIdx];
                        var n = nIdx < normals.Count ? normals[nIdx] : Vector3.UnitY;
                        
                        finalVertices.Add(new VertexPositionNormalColor(
                            v, n, new RgbaFloat(0.7f, 0.7f, 0.7f, 1f)));
                        finalIndices.Add((uint)finalVertices.Count - 1);
                    }
                    break;
            }
        }

        return new MeshData
        {
            Vertices = finalVertices.ToArray(),
            Indices = finalIndices.ToArray(),
            Topology = MeshTopology.Triangles
        };
    }
}