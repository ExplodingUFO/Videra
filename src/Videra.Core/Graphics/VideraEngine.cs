using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Videra.Core.Cameras;
using Videra.Core.Geometry;

namespace Videra.Core.Graphics;

public class VideraEngine : IDisposable
{
    private readonly object _lock = new();
    private CommandList _cl;
    private MeshTopology _currentTopology;
    private ResourceFactory _factory;
    private DeviceBuffer _indexBuffer;

    private uint _indexCount;

    private Pipeline _meshPipeline;
    private Pipeline _pointPipeline;
    private DeviceBuffer _projViewBuffer;
    private ResourceSet _projViewSet;

    private DeviceBuffer _vertexBuffer;

    public GraphicsDevice GraphicsDevice { get; private set; }

    public OrbitCamera Camera { get; } = new();
    public bool IsInitialized { get; private set; }

    public void Dispose()
    {
        Grid.Dispose();
        _axisRenderer.Dispose();
        GraphicsDevice?.Dispose();
        _factory = null;
    }

    public void ClearObjects()
    {
        lock (_lock)
        {
            foreach (var obj in _sceneObjects) obj.Dispose();
            _sceneObjects.Clear();
        }
    }

    // 初始化：平台无关，只接收 options 和 swapchainDesc
    public void Initialize(GraphicsDeviceOptions options, SwapchainDescription swapchainDesc)
    {
        if (IsInitialized) return; // 防止重复初始化

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            GraphicsDevice = GraphicsDevice.CreateMetal(options, swapchainDesc);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            GraphicsDevice = GraphicsDevice.CreateD3D11(options, swapchainDesc);
        else
            GraphicsDevice = GraphicsDevice.CreateVulkan(options, swapchainDesc);

        _factory = GraphicsDevice.ResourceFactory;
        _cl = _factory.CreateCommandList();

        CreateResources();
        IsInitialized = true;
    }

    private void CreateResources()
    {
        // 增加 World 矩阵输入 (Set 1)
        var vertexCode = @"
#version 450
layout(location=0) in vec3 Pos; layout(location=1) in vec3 Norm; layout(location=2) in vec4 Col;
layout(location=0) out vec4 vCol;

// Set 0: 全局相机 (View + Projection)
layout(set=0, binding=0) uniform ViewProj { mat4 V; mat4 P; };
// Set 1: 物体变换 (World)
layout(set=1, binding=0) uniform World { mat4 W; };

void main() { 
    // P * V * W * Pos
    gl_Position = P * V * W * vec4(Pos, 1); 
    
    vec3 L = normalize(vec3(1,1,1));
    // 简单的法线变换 (注意：严格来说应该用 Normal Matrix)
    vec3 worldNorm = mat3(W) * Norm; 
    float I = max(dot(normalize(worldNorm), L), 0.3);
    vCol = vec4(Col.rgb * I, Col.a); 
}";
        var fragCode = @"#version 450
layout(location=0) in vec4 vCol; layout(location=0) out vec4 Out;
void main() { Out = vCol; }";

        var vDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
        var fDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragCode), "main");
        var shaders = _factory.CreateFromSpirv(vDesc, fDesc);


        // Set 0: VP
        _viewProjLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ViewProj", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        // Set 1: World (新增)
        _worldLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("World", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        var vLayout = new VertexLayoutDescription(
            new VertexElementDescription("Pos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Norm", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Col", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        _projViewBuffer =
            _factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        var rLayout = _factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("VP", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

        _projViewSet = _factory.CreateResourceSet(new ResourceSetDescription(rLayout, _projViewBuffer));
        
        // 1. 【核心修复】显式强开启深度测试
        // 不要用默认构造函数，必须指定参数
        var depthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,      // 开启测试：比较 Z 值
            depthWriteEnabled: true,     // 开启写入：把 Z 值存入缓冲
            comparisonKind: ComparisonKind.LessEqual // 越小越近（标准模式）
        );

        // 2. 【核心修复】动态获取 OutputDescription
        // 不要硬编码 B8G8R8A8，而是直接问 Swapchain 它到底是什么格式
        // 这样能同时兼容 Windows(DirectX) 和 Mac(Metal)
        OutputDescription outputDesc = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
    
        // 双重检查：确保 OutputDescription 里真的包含深度格式
        if (outputDesc.DepthAttachment == null)
        {
            // 如果这里进来了，说明 Swapchain 创建时没带深度，那是 VideraView 的问题
            // 但我们这里先强制修正它，防止崩溃
            Console.WriteLine("[Videra Warning] Framebuffer missing depth! Forcing logic...");
            outputDesc.DepthAttachment = new OutputAttachmentDescription(PixelFormat.D32_Float_S8_UInt);
        }
        
        var pipeDesc = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = depthStencilState,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[] { _viewProjLayout, _worldLayout },
            ShaderSet = new ShaderSetDescription(new[] { vLayout }, shaders),
            Outputs = outputDesc
        };
        _meshPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        pipeDesc.PrimitiveTopology = PrimitiveTopology.PointList;;
        _pointPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        // 创建全局 VP 资源集
        _projViewBuffer =
            _factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _projViewSet = _factory.CreateResourceSet(new ResourceSetDescription(_viewProjLayout, _projViewBuffer));

        // 初始化轴组件
        _axisRenderer.Initialize(GraphicsDevice, GraphicsDevice.SwapchainFramebuffer.OutputDescription);
        Grid.Initialize(GraphicsDevice, GraphicsDevice.SwapchainFramebuffer.OutputDescription);
    }

    // --- 新增：场景操作 API ---
    public void AddObject(Object3D obj)
    {
        // 确保该对象创建了对应的 ResourceSet
        if (obj.WorldResourceSet == null)
            obj.CreateResourceSet(_factory, _worldLayout);

        lock (_lock)
        {
            _sceneObjects.Add(obj);
        }
    }

    public void RemoveObject(Object3D obj)
    {
        lock (_lock)
        {
            _sceneObjects.Remove(obj);
            obj.Dispose();
        }
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
            GraphicsDevice.UpdateBuffer(_vertexBuffer, 0, mesh.Vertices);

            var iSize = (uint)(mesh.Indices.Length * sizeof(uint));
            _indexBuffer = _factory.CreateBuffer(new BufferDescription(iSize, BufferUsage.IndexBuffer));
            GraphicsDevice.UpdateBuffer(_indexBuffer, 0, mesh.Indices);

            _indexCount = (uint)mesh.Indices.Length;
        }
    }

    public void Resize(uint width, uint height)
    {
        if (!IsInitialized || GraphicsDevice == null || width == 0 || height == 0)
        {
            Console.WriteLine($"[Videra] Resize ignored: Init={IsInitialized}, {width}x{height}");
            return;
        }

        if (_width == width && _height == height) return;

        _width = width;
        _height = height;

        try
        {
            Console.WriteLine($"[Videra] Resizing to: {width}x{height}");
            GraphicsDevice.MainSwapchain.Resize(width, height);
            Camera.UpdateProjection(width, height);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Videra Warning] Resize failed: {ex.Message}");
        }
    }

    public void Draw()
    {
        if (!IsInitialized) return;
        lock (_lock)
        {
            _cl.Begin();
            _cl.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            
            // =========================================================
            // 【修复关键点】必须设置 Viewport！
            // 否则 GPU 不知道画在哪里，或者画在 0x0 的区域里
            // =========================================================
            _cl.SetViewport(0, new Viewport(0, 0, _width, _height, 0, 1));
            _cl.SetFullScissorRects(); // 设置裁剪区域为全屏

            // 使用自定义背景色
            _cl.ClearColorTarget(0, BackgroundColor);
            _cl.ClearDepthStencil(1f);

            // 画网格 (通常在物体之前或之后都可以，因为有深度测试)
            // 建议放在物体之前画，这样如果有物体在网格下方，逻辑更清晰
            Grid.Draw(_cl, Camera);

            // 更新全局相机
            _cl.UpdateBuffer(_projViewBuffer, 0, Camera.ViewMatrix);
            _cl.UpdateBuffer(_projViewBuffer, 64, Camera.ProjectionMatrix);


            // 遍历渲染所有物体
            foreach (var obj in _sceneObjects)
            {
                // 1. 切换 Pipeline (如果不同物体有不同拓扑)
                _cl.SetPipeline(_meshPipeline);

                // 2. 绑定全局 Set 0 (相机)
                _cl.SetGraphicsResourceSet(0, _projViewSet);

                // 3. 更新该物体的 World 矩阵
                obj.UpdateUniforms(_cl);

                // 4. 绑定物体 Set 1 (World)
                _cl.SetGraphicsResourceSet(1, obj.WorldResourceSet);

                // 5. 绑定 Mesh
                _cl.SetVertexBuffer(0, obj.VertexBuffer);
                _cl.SetIndexBuffer(obj.IndexBuffer, IndexFormat.UInt32);

                // 6. Draw
                _cl.DrawIndexed(obj.IndexCount, 1, 0, 0, 0);
            }

            _axisRenderer.Draw(_cl, Camera, _width, _height);
            _cl.End();
            GraphicsDevice.SubmitCommands(_cl);
            GraphicsDevice.SwapBuffers();
        }
    }

    #region 平面网格

    // 1. 实例化组件

    // 2. 暴露给外部控制的属性
    public GridRenderer Grid { get; } = new();

    #endregion

    #region 轴显隐

    private readonly AxisRenderer _axisRenderer = new();
    private uint _width, _height; // 记录窗口大小用于视口计算

    // 对外暴露显隐控制
    public bool ShowAxis
    {
        get => _axisRenderer.IsVisible;
        set => _axisRenderer.IsVisible = value;
    }

    #endregion

    #region 场景管理

    // --- 新增：场景管理 ---
    private readonly List<Object3D> _sceneObjects = new();
    public RgbaFloat BackgroundColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f); // 默认深灰

    // Pipelines & Layouts
    private ResourceLayout _viewProjLayout;
    private ResourceLayout _worldLayout; // 新增：每个物体独有的 World 矩阵布局

    #endregion
}