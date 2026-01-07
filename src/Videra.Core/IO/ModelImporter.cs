using System.Numerics;
using Assimp;
using Veldrid;
using Videra.Core.Geometry;

namespace Videra.Core.IO;

public static class ModelImporter
{
    public static MeshData Load(string filePath)
    {
        // 这里封装了之前讨论过的“中文路径替身法”和 Assimp 解析逻辑
        // 为了篇幅简洁，我保留核心逻辑结构
        return LoadWithAssimp(filePath);
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