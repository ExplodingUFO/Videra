using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class Object3D : IDisposable
{
    public string Name { get; set; } = "Object";

    // --- 独立变换属性 ---
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero; // 欧拉角 (弧度)
    public Vector3 Scale { get; set; } = Vector3.One;

    // --- GPU 资源 (每个模型自己管理) ---
    internal IBuffer? VertexBuffer { get; private set; }
    internal IBuffer? IndexBuffer { get; private set; }
    internal IBuffer? WorldBuffer { get; private set; } // 存放 World 矩阵
    internal uint IndexCount { get; private set; }
    internal MeshTopology Topology { get; private set; }

    // 计算世界矩阵
    public Matrix4x4 WorldMatrix =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
        Matrix4x4.CreateTranslation(Position);

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
        WorldBuffer?.Dispose();
    }

    // 初始化 GPU 资源 (由 Engine 调用)
    public void Initialize(IResourceFactory factory, MeshData mesh)
    {
        try
        {
            // ✅ 验证输入
            if (mesh == null || mesh.Vertices == null || mesh.Vertices.Length == 0)
                throw new ArgumentException("Invalid mesh data");

            if (mesh.Indices == null || mesh.Indices.Length == 0)
                throw new ArgumentException("Invalid index data");

            Console.WriteLine($"[Object3D '{Name}'] Initializing: {mesh.Vertices.Length} verts, {mesh.Indices.Length} indices");

            Topology = mesh.Topology;
            IndexCount = (uint)mesh.Indices.Length;

            // ✅ 安全的大小计算
            var vertexSize = Unsafe.SizeOf<VertexPositionNormalColor>();
            
            if ((long)mesh.Vertices.Length * vertexSize > uint.MaxValue)
                throw new InvalidOperationException($"Vertex buffer too large: {mesh.Vertices.Length} vertices");

            var vSize = (uint)(mesh.Vertices.Length * vertexSize);
            
            Console.WriteLine($"[Object3D '{Name}'] Vertex buffer size: {vSize:N0} bytes ({vSize / 1024.0 / 1024.0:F2} MB)");

            VertexBuffer = factory.CreateVertexBuffer(vSize);
            VertexBuffer.SetData(mesh.Vertices, 0);

            // ✅ 索引 Buffer
            if ((long)mesh.Indices.Length * sizeof(uint) > uint.MaxValue)
                throw new InvalidOperationException($"Index buffer too large: {mesh.Indices.Length} indices");

            var iSize = (uint)(mesh.Indices.Length * sizeof(uint));
            
            Console.WriteLine($"[Object3D '{Name}'] Index buffer size: {iSize:N0} bytes ({iSize / 1024.0 / 1024.0:F2} MB)");

            IndexBuffer = factory.CreateIndexBuffer(iSize);
            IndexBuffer.SetData(mesh.Indices, 0);

            // ✅ World Uniform Buffer
            WorldBuffer = factory.CreateUniformBuffer(64);

            Console.WriteLine($"[Object3D '{Name}'] ✓ Initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Object3D '{Name}'] ✗ Initialization failed: {ex.Message}");
            
            // 清理已创建的资源
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            WorldBuffer?.Dispose();
            
            throw;
        }
    }

    public void UpdateUniforms(ICommandExecutor executor)
    {
        // 每次绘制前，把最新的 SRT 矩阵传给 GPU
        if (WorldBuffer != null)
        {
            WorldBuffer.SetData(WorldMatrix, 0);
        }
    }
}