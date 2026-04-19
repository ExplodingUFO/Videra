using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// GPU 资源创建工厂接口。
/// Built-in D3D11/Vulkan/Metal backends guarantee the minimum contract around buffer creation
/// and pipeline creation. Advanced seams such as <c>CreateShader</c> and <c>CreateResourceSet</c>
/// are not a portability promise on the shipped native backends and may throw
/// <see cref="Videra.Core.Exceptions.UnsupportedOperationException"/>.
/// </summary>
public interface IResourceFactory
{
    /// <summary>
    /// 创建顶点缓冲区
    /// </summary>
    IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices);
    
    /// <summary>
    /// 创建顶点缓冲区（指定大小）
    /// </summary>
    IBuffer CreateVertexBuffer(uint sizeInBytes);
    
    /// <summary>
    /// 创建索引缓冲区
    /// </summary>
    IBuffer CreateIndexBuffer(uint[] indices);
    
    /// <summary>
    /// 创建索引缓冲区（指定大小）
    /// </summary>
    IBuffer CreateIndexBuffer(uint sizeInBytes);
    
    /// <summary>
    /// 创建 Uniform Buffer (常量缓冲区)
    /// </summary>
    IBuffer CreateUniformBuffer(uint sizeInBytes);
    
    /// <summary>
    /// 创建 Pipeline (渲染管线)。
    /// Built-in native backends adapt this richer description to the current minimum contract
    /// instead of promising full backend-specific shader/resource-layout parity.
    /// </summary>
    IPipeline CreatePipeline(PipelineDescription description);
    
    /// <summary>
    /// 创建简化的 Pipeline（用于基础渲染）
    /// </summary>
    IPipeline CreatePipeline(uint vertexSize, bool hasNormals, bool hasColors);
    
    /// <summary>
    /// 创建 Shader (着色器)。
    /// This remains an advanced seam; shipped native backends manage shaders internally and may
    /// throw <see cref="Videra.Core.Exceptions.UnsupportedOperationException"/>.
    /// </summary>
    IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint);

    /// <summary>
    /// 创建 Resource Set (资源绑定集)。
    /// This remains an advanced seam; shipped native backends may bind resources through
    /// backend-specific internal paths and throw
    /// <see cref="Videra.Core.Exceptions.UnsupportedOperationException"/>.
    /// </summary>
    IResourceSet CreateResourceSet(ResourceSetDescription description);
}

/// <summary>
/// Pipeline 描述
/// </summary>
public struct PipelineDescription
{
    public IShader VertexShader;
    public IShader FragmentShader;
    public VertexLayoutDescription VertexLayout;
    public ResourceLayoutDescription[] ResourceLayouts;
    public PrimitiveTopology Topology;
    public bool DepthTestEnabled;
    public bool DepthWriteEnabled;
}

/// <summary>
/// 顶点布局描述
/// </summary>
public struct VertexLayoutDescription
{
    public VertexElementDescription[] Elements;
}

/// <summary>
/// 顶点元素描述
/// </summary>
public struct VertexElementDescription
{
    public string Name;
    public VertexElementFormat Format;
    public uint Offset;
}

/// <summary>
/// 顶点元素格式
/// </summary>
public enum VertexElementFormat
{
    Float3,
    Float4
}

/// <summary>
/// Resource Layout 描述
/// </summary>
public struct ResourceLayoutDescription
{
    public ResourceLayoutElementDescription[] Elements;
}

/// <summary>
/// Resource Layout 元素描述
/// </summary>
public struct ResourceLayoutElementDescription
{
    public uint Binding;
    public ResourceKind Kind;
    public ShaderStage Stages;
}

/// <summary>
/// 资源类型
/// </summary>
public enum ResourceKind
{
    UniformBuffer
}

/// <summary>
/// Resource Set 描述
/// </summary>
public struct ResourceSetDescription
{
    public IResourceLayout Layout;
    public IBuffer[] Buffers;
}

/// <summary>
/// Shader 阶段
/// </summary>
[Flags]
public enum ShaderStage
{
    Vertex = 1,
    Fragment = 2
}

/// <summary>
/// 图元拓扑
/// </summary>
public enum PrimitiveTopology
{
    TriangleList,
    LineList,
    PointList
}

/// <summary>
/// 网格拓扑（用于兼容）
/// </summary>
public enum MeshTopology
{
    Triangles,
    Lines,
    Points
}
