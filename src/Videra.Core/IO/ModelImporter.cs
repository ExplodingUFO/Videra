using System.Numerics;
using Assimp;
using Veldrid;
using Videra.Core.Geometry;
using Videra.Core.Graphics;

namespace Videra.Core.IO;

public static class ModelImporter
{
    /// <summary>
    ///     支持的格式
    /// </summary>
    public static string[] SupportedFormats => new[]
        { "*.obj", "*.stl", "*.3mf", "*.ply", "*.xyz", "*.stp", "*.gltf", "*.glb" };

    // 返回 Object3D 而不是 MeshData
    public static Object3D Load(string filePath, GraphicsDevice gd)
    {
        // 1. 调用之前的 LoadWithAssimp 获取 MeshData
        var meshData = LoadWithAssimp(filePath); // (复用之前的逻辑)

        // 2. 包装成 Object3D
        var obj = new Object3D
        {
            Name = Path.GetFileName(filePath)
        };

        // 3. 立即初始化显存资源
        if (gd != null) obj.Initialize(gd.ResourceFactory, gd, meshData);

        return obj;
    }

    private static MeshData LoadWithAssimp(string originalPath)
    {
        var importer = new AssimpContext();
        var steps = PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals |
                    PostProcessSteps.FlipUVs | PostProcessSteps.JoinIdenticalVertices |
                    PostProcessSteps.PreTransformVertices;

        Scene scene = null;
        var loadPath = originalPath;
        var isTempFile = false;

        // 处理中文路径乱码问题
        if (originalPath.Any(c => c > 127))
            try
            {
                var ext = Path.GetExtension(originalPath);
                var tempName = $"temp_{Guid.NewGuid():N}{ext}";
                loadPath = Path.Combine(Path.GetDirectoryName(originalPath) ?? "", tempName);
                File.Copy(originalPath, loadPath, true);
                isTempFile = true;
            }
            catch
            {
                loadPath = originalPath;
            }

        try
        {
            scene = importer.ImportFile(loadPath, steps);
        }
        finally
        {
            if (isTempFile && File.Exists(loadPath))
                try
                {
                    File.Delete(loadPath);
                }
                catch
                {
                }
        }

        if (scene == null || scene.RootNode == null) throw new Exception("Import failed");

        var allVertices = new List<VertexPositionNormalColor>();
        var allIndices = new List<uint>();
        uint indexOffset = 0;

        foreach (var mesh in scene.Meshes)
        {
            var baseColor = new RgbaFloat(0.7f, 0.7f, 0.7f, 1f);
            if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < scene.MaterialCount)
            {
                var c = scene.Materials[mesh.MaterialIndex].ColorDiffuse;
                baseColor = new RgbaFloat(c.R, c.G, c.B, c.A <= 0.01f ? 1f : c.A);
            }

            for (var i = 0; i < mesh.VertexCount; i++)
            {
                var v = mesh.Vertices[i];
                var n = mesh.Normals[i];
                var col = baseColor;
                if (mesh.HasVertexColors(0))
                {
                    var vc = mesh.VertexColorChannels[0][i];
                    col = new RgbaFloat(vc.R, vc.G, vc.B, vc.A);
                }

                allVertices.Add(new VertexPositionNormalColor(new Vector3(v.X, v.Y, v.Z), new Vector3(n.X, n.Y, n.Z),
                    col));
            }

            if (mesh.PrimitiveType == PrimitiveType.Point)
                for (var i = 0; i < mesh.VertexCount; i++)
                    allIndices.Add(indexOffset + (uint)i);
            else
                foreach (var f in mesh.Faces)
                    if (f.IndexCount == 3)
                    {
                        allIndices.Add(indexOffset + (uint)f.Indices[0]);
                        allIndices.Add(indexOffset + (uint)f.Indices[1]);
                        allIndices.Add(indexOffset + (uint)f.Indices[2]);
                    }

            indexOffset += (uint)mesh.VertexCount;
        }

        return new MeshData
        {
            Vertices = allVertices.ToArray(),
            Indices = allIndices.ToArray(),
            Topology = Path.GetExtension(originalPath).ToLower() == ".ply"
                ? MeshTopology.Points
                : MeshTopology.Triangles
        };
    }
}