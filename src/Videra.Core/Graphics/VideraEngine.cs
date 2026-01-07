using System.Runtime.CompilerServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Videra.Core.Cameras;
using Videra.Core.Geometry;

namespace Videra.Core.Graphics;

public class VideraEngine : IDisposable
{
    #region 轴显隐

    private readonly AxisRenderer _axisRenderer = new AxisRenderer();
    private uint _width, _height; // 记录窗口大小用于视口计算

    // 对外暴露显隐控制
    public bool ShowAxis
    {
        get => _axisRenderer.IsVisible;
        set => _axisRenderer.IsVisible = value;
    }

    #endregion

    private readonly object _lock = new();
    private CommandList _cl;
    private MeshTopology _currentTopology;
    private ResourceFactory _factory;
    private GraphicsDevice _gd;
    private DeviceBuffer _indexBuffer;

    private uint _indexCount;

    private Pipeline _meshPipeline;
    private Pipeline _pointPipeline;
    private DeviceBuffer _projViewBuffer;
    private ResourceSet _projViewSet;

    private DeviceBuffer _vertexBuffer;

    public OrbitCamera Camera { get; } = new();
    public bool IsInitialized { get; private set; }

    public void Dispose()
    {
        _axisRenderer.Dispose();
        _gd?.Dispose();
        _factory = null;
    }

    // 初始化：平台无关，只接收 options 和 swapchainDesc
    public void Initialize(GraphicsDeviceOptions options, SwapchainDescription swapchainDesc, PlatformID platform)
    {
        if (platform == PlatformID.Win32NT)
            _gd = GraphicsDevice.CreateD3D11(options, swapchainDesc);
        else if (platform == PlatformID.MacOSX)
            _gd = GraphicsDevice.CreateMetal(options, swapchainDesc);
        else
            _gd = GraphicsDevice.CreateVulkan(options, swapchainDesc);

        _factory = _gd.ResourceFactory;
        _cl = _factory.CreateCommandList();

        CreateResources();
        IsInitialized = true;
    }

    private void CreateResources()
    {
        // 内置简单光照 Shader
        var vertexCode = @"
#version 450
layout(location=0) in vec3 Pos; layout(location=1) in vec3 Norm; layout(location=2) in vec4 Col;
layout(location=0) out vec4 vCol;
layout(set=0, binding=0) uniform VP { mat4 V; mat4 P; };
void main() { 
    gl_Position = P * V * vec4(Pos, 1); 
    vec3 L = normalize(vec3(1,1,1));
    float I = max(dot(normalize(Norm), L), 0.3);
    vCol = vec4(Col.rgb * I, Col.a); 
}";
        var fragCode = @"#version 450
layout(location=0) in vec4 vCol; layout(location=0) out vec4 Out;
void main() { Out = vCol; }";

        var vDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
        var fDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragCode), "main");
        var shaders = _factory.CreateFromSpirv(vDesc, fDesc);

        var vLayout = new VertexLayoutDescription(
            new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Norm", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Col", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        _projViewBuffer =
            _factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        var rLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("VP", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        _projViewSet = _factory.CreateResourceSet(new ResourceSetDescription(rLayout, _projViewBuffer));

        var pipeDesc = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[] { rLayout },
            ShaderSet = new ShaderSetDescription(new[] { vLayout }, shaders),
            Outputs = _gd.SwapchainFramebuffer.OutputDescription
        };
        _meshPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        pipeDesc.PrimitiveTopology = PrimitiveTopology.PointList;
        _pointPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        // 2. 初始化坐标轴组件
        _axisRenderer.Initialize(_gd, _gd.SwapchainFramebuffer.OutputDescription);
    }

    public void UpdateMesh(MeshData mesh)
    {
        if (!IsInitialized || mesh.Vertices.Length == 0) return;
        lock (_lock)
        {
            _currentTopology = mesh.Topology;
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();

            var vSize = (uint)(mesh.Vertices.Length * Unsafe.SizeOf<VertexPositionNormalColor>());
            _vertexBuffer = _factory.CreateBuffer(new BufferDescription(vSize, BufferUsage.VertexBuffer));
            _gd.UpdateBuffer(_vertexBuffer, 0, mesh.Vertices);

            var iSize = (uint)(mesh.Indices.Length * sizeof(uint));
            _indexBuffer = _factory.CreateBuffer(new BufferDescription(iSize, BufferUsage.IndexBuffer));
            _gd.UpdateBuffer(_indexBuffer, 0, mesh.Indices);

            _indexCount = (uint)mesh.Indices.Length;
        }
    }

    public void Resize(uint width, uint height)
    {
        if (_gd == null) return;

        _width = width;
        _height = height;

        _gd.MainSwapchain.Resize(width, height);
        Camera.UpdateProjection(width, height);
    }

    public void Draw()
    {
        if (!IsInitialized || _indexCount == 0) return;
        lock (_lock)
        {
            _cl.Begin();
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);
            _cl.ClearColorTarget(0, RgbaFloat.Black);
            _cl.ClearDepthStencil(1f);

            // A. 画主场景
            if (_indexCount > 0)
            {
                _cl.SetPipeline(_currentTopology == MeshTopology.Points ? _pointPipeline : _meshPipeline);
                _cl.UpdateBuffer(_projViewBuffer, 0, Camera.ViewMatrix);
                _cl.UpdateBuffer(_projViewBuffer, 64, Camera.ProjectionMatrix);
                _cl.SetGraphicsResourceSet(0, _projViewSet);
                _cl.SetVertexBuffer(0, _vertexBuffer);
                _cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt32);
                _cl.DrawIndexed(_indexCount, 1, 0, 0, 0);
            }

            // B. 画坐标轴 (画在最上层)
            // 直接调用组件，传入 CommandList 和上下文信息
            _axisRenderer.Draw(_cl, Camera, _width, _height);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers();
        }
    }
}