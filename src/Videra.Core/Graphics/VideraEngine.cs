using System.Runtime.CompilerServices;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Videra.Core.Cameras;
using Videra.Core.Geometry;

namespace Videra.Core.Graphics;

public class VideraEngine : IDisposable
{
    public GraphicsDevice GraphicsDevice => _gd;
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

    public void ClearObjects()
    {
        lock (_lock)
        {
            foreach (var obj in _sceneObjects) obj.Dispose();
            _sceneObjects.Clear();
        }
    }

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

        var pipeDesc = new GraphicsPipelineDescription
        {
            BlendState = BlendStateDescription.SingleOverrideBlend,
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            RasterizerState = RasterizerStateDescription.CullNone,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ResourceLayouts = new[] { _viewProjLayout, _worldLayout },
            ShaderSet = new ShaderSetDescription(new[] { vLayout }, shaders),
            Outputs = _gd.SwapchainFramebuffer.OutputDescription
        };
        _meshPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        pipeDesc.PrimitiveTopology = PrimitiveTopology.PointList;
        _pointPipeline = _factory.CreateGraphicsPipeline(pipeDesc);

        // 创建全局 VP 资源集
        _projViewBuffer =
            _factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        _projViewSet = _factory.CreateResourceSet(new ResourceSetDescription(_viewProjLayout, _projViewBuffer));

        // 初始化轴组件
        _axisRenderer.Initialize(_gd, _gd.SwapchainFramebuffer.OutputDescription);
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
        if (!IsInitialized) return;
        lock (_lock)
        {
            _cl.Begin();
            _cl.SetFramebuffer(_gd.SwapchainFramebuffer);

            // 使用自定义背景色
            _cl.ClearColorTarget(0, BackgroundColor);
            _cl.ClearDepthStencil(1f);

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
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers();
        }
    }

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