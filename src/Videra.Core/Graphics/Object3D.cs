using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;

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

    // --- 线框渲染资源 ---
    internal IBuffer? LineIndexBuffer { get; private set; }
    internal IBuffer? LineVertexBuffer { get; private set; }  // 线框专用顶点缓冲（带自定义颜色）
    internal uint LineIndexCount { get; private set; }
    private uint[]? _cachedTriangleIndices;
    private VertexPositionNormalColor[]? _cachedVertices;  // 缓存原始顶点用于线框颜色更新

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
        LineIndexBuffer?.Dispose();
        LineVertexBuffer?.Dispose();
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

            // 缓存顶点数据用于线框
            _cachedVertices = mesh.Vertices;

            // ✅ 索引 Buffer
            if ((long)mesh.Indices.Length * sizeof(uint) > uint.MaxValue)
                throw new InvalidOperationException($"Index buffer too large: {mesh.Indices.Length} indices");

            var iSize = (uint)(mesh.Indices.Length * sizeof(uint));
            
            Console.WriteLine($"[Object3D '{Name}'] Index buffer size: {iSize:N0} bytes ({iSize / 1024.0 / 1024.0:F2} MB)");

            IndexBuffer = factory.CreateIndexBuffer(iSize);
            IndexBuffer.SetData(mesh.Indices, 0);

            // ✅ 缓存三角形索引用于线框提取
            if (mesh.Topology == MeshTopology.Triangles)
            {
                _cachedTriangleIndices = mesh.Indices;
            }

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
            LineIndexBuffer?.Dispose();
            
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

    /// <summary>
    /// 初始化线框渲染资源（从三角形网格提取边缘）
    /// </summary>
    public void InitializeWireframe(IResourceFactory factory)
    {
        if (_cachedTriangleIndices == null || _cachedTriangleIndices.Length == 0)
        {
            Console.WriteLine($"[Object3D '{Name}'] Wireframe: No triangle indices available");
            return;
        }

        // 从三角形索引中提取唯一边缘
        var lineIndices = EdgeExtractor.ExtractUniqueEdges(_cachedTriangleIndices);

        if (lineIndices.Length == 0)
        {
            Console.WriteLine($"[Object3D '{Name}'] Wireframe: No edges extracted");
            return;
        }

        LineIndexCount = (uint)lineIndices.Length;

        var bufferSize = (uint)(lineIndices.Length * sizeof(uint));
        LineIndexBuffer = factory.CreateIndexBuffer(bufferSize);
        LineIndexBuffer.SetData(lineIndices, 0);

        // 创建线框顶点缓冲（初始时复制原始顶点）
        if (_cachedVertices != null)
        {
            var vertexSize = Unsafe.SizeOf<VertexPositionNormalColor>();
            var vSize = (uint)(_cachedVertices.Length * vertexSize);
            LineVertexBuffer = factory.CreateVertexBuffer(vSize);
            LineVertexBuffer.SetData(_cachedVertices, 0);
        }

        Console.WriteLine($"[Object3D '{Name}'] Wireframe: {LineIndexCount / 2} edges initialized");
    }

    /// <summary>
    /// 更新线框顶点缓冲区的颜色
    /// </summary>
    public void UpdateWireframeColor(RgbaFloat color)
    {
        if (_cachedVertices == null || LineVertexBuffer == null)
            return;

        // 创建带新颜色的顶点数组
        var coloredVertices = new VertexPositionNormalColor[_cachedVertices.Length];
        for (int i = 0; i < _cachedVertices.Length; i++)
        {
            coloredVertices[i] = new VertexPositionNormalColor(
                _cachedVertices[i].Position,
                _cachedVertices[i].Normal,
                color
            );
        }

        LineVertexBuffer.SetData(coloredVertices, 0);
    }
}