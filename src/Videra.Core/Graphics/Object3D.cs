using System.Numerics;
using Veldrid;
using Videra.Core.Geometry;

namespace Videra.Core.Graphics;

public class Object3D : IDisposable
{
    public string Name { get; set; } = "Object";

    // --- 独立变换属性 ---
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero; // 欧拉角 (弧度)
    public Vector3 Scale { get; set; } = Vector3.One;

    // --- GPU 资源 (每个模型自己管理) ---
    internal DeviceBuffer VertexBuffer { get; private set; }
    internal DeviceBuffer IndexBuffer { get; private set; }
    internal DeviceBuffer WorldBuffer { get; private set; } // 存放 World 矩阵
    internal ResourceSet WorldResourceSet { get; private set; }
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
        WorldResourceSet?.Dispose();
    }

    // 初始化 GPU 资源 (由 Engine 调用)
    public void Initialize(ResourceFactory factory, GraphicsDevice gd, MeshData mesh)
    {
        Topology = mesh.Topology;
        IndexCount = (uint)mesh.Indices.Length;

        // 1. 创建顶点/索引 Buffer
        var vSize = (uint)(mesh.Vertices.Length * VertexPositionNormalColor.SizeInBytes);
        VertexBuffer = factory.CreateBuffer(new BufferDescription(vSize, BufferUsage.VertexBuffer));
        gd.UpdateBuffer(VertexBuffer, 0, mesh.Vertices);

        var iSize = (uint)(mesh.Indices.Length * sizeof(uint));
        IndexBuffer = factory.CreateBuffer(new BufferDescription(iSize, BufferUsage.IndexBuffer));
        gd.UpdateBuffer(IndexBuffer, 0, mesh.Indices);

        // 2. 创建 World Uniform Buffer (存放该对象的变换矩阵)
        WorldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
    }

    // 创建资源集 (Pipeline 创建后调用)
    public void CreateResourceSet(ResourceFactory factory, ResourceLayout worldLayout)
    {
        WorldResourceSet = factory.CreateResourceSet(new ResourceSetDescription(worldLayout, WorldBuffer));
    }

    public void UpdateUniforms(CommandList cl)
    {
        // 每次绘制前，把最新的 SRT 矩阵传给 GPU
        cl.UpdateBuffer(WorldBuffer, 0, WorldMatrix);
    }
}