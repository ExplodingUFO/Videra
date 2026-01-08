using System.Numerics;
using System.Runtime.CompilerServices;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class VideraEngine : IDisposable
{
    private readonly object _lock = new();
    private IGraphicsBackend? _backend;
    private IResourceFactory? _factory;
    private ICommandExecutor? _executor;
    
    private IPipeline? _meshPipeline;
    private IBuffer? _cameraBuffer;
    
    private uint _width, _height;
    
    private readonly List<Object3D> _sceneObjects = new();
    
    public OrbitCamera Camera { get; } = new();
    public bool IsInitialized { get; private set; }
    public RgbaFloat BackgroundColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);
    
    // 网格和坐标轴渲染器
    public GridRenderer Grid { get; } = new();
    private readonly AxisRenderer _axisRenderer = new();
    
    public bool ShowAxis
    {
        get => _axisRenderer.IsVisible;
        set => _axisRenderer.IsVisible = value;
    }

    public void Dispose()
    {
        Grid.Dispose();
        _axisRenderer.Dispose();
        ClearObjects();
        
        _meshPipeline?.Dispose();
        _cameraBuffer?.Dispose();
        _backend?.Dispose();
        
        IsInitialized = false;
    }

    public void Initialize(IGraphicsBackend backend)
    {
        if (IsInitialized) return;

        _backend = backend;
        _factory = backend.GetResourceFactory();
        _executor = backend.GetCommandExecutor();

        CreateResources();
        
        // 初始化渲染器
        Grid.Initialize(_factory);
        _axisRenderer.Initialize(_factory);
        
        IsInitialized = true;
        Console.WriteLine("[VideraEngine] Initialized successfully");
    }

    private void CreateResources()
    {
        if (_factory == null) return;

        // 创建相机uniform buffer (View + Projection = 2 * 64 bytes)
        _cameraBuffer = _factory.CreateUniformBuffer(128);

        // 创建Pipeline
        _meshPipeline = _factory.CreatePipeline(
            vertexSize: (uint)Unsafe.SizeOf<VertexPositionNormalColor>(),
            hasNormals: true,
            hasColors: true
        );

        Console.WriteLine("[VideraEngine] Resources created");
    }

    public void Resize(uint width, uint height)
    {
        if (!IsInitialized || width == 0 || height == 0)
        {
            Console.WriteLine($"[VideraEngine] Resize ignored: Init={IsInitialized}, {width}x{height}");
            return;
        }

        if (_width == width && _height == height) return;

        _width = width;
        _height = height;

        try
        {
            Console.WriteLine($"[VideraEngine] Resizing to: {width}x{height}");
            _backend?.Resize((int)width, (int)height);
            Camera.UpdateProjection(width, height);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[VideraEngine Warning] Resize failed: {ex.Message}");
        }
    }

    public void Draw()
    {
        if (!IsInitialized || _backend == null || _executor == null || _meshPipeline == null)
        {
            return;
        }

        lock (_lock)
        {
            // 开始帧
            _backend.BeginFrame();
            
            // 清除颜色和深度
            _executor.Clear(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            
            // 设置视口
            _executor.SetViewport(0, 0, _width, _height);
            
            // 更新相机数据
            if (_cameraBuffer != null)
            {
                _cameraBuffer.SetData(Camera.ViewMatrix, 0);
                _cameraBuffer.SetData(Camera.ProjectionMatrix, 64);
            }
            
            // 绑定Pipeline
            _executor.SetPipeline(_meshPipeline);
            
            // 渲染网格
            Grid.Draw(_executor, Camera, _width, _height);
            
            // 渲染所有物体
            foreach (var obj in _sceneObjects)
            {
                if (obj.VertexBuffer == null || obj.IndexBuffer == null || obj.WorldBuffer == null)
                    continue;

                // 更新物体的World矩阵
                obj.UpdateUniforms(_executor);
                
                // 绑定缓冲区
                _executor.SetVertexBuffer(obj.VertexBuffer);
                _executor.SetIndexBuffer(obj.IndexBuffer);
                
                // 绘制
                _executor.DrawIndexed(obj.IndexCount, 1, 0, 0, 0);
            }
            
            // 渲染坐标轴
            _axisRenderer.Draw(_executor, Camera, _width, _height);
            
            // 结束帧并呈现
            _backend.EndFrame();
        }
    }

    public void AddObject(Object3D obj)
    {
        lock (_lock)
        {
            _sceneObjects.Add(obj);
        }
        Console.WriteLine($"[VideraEngine] Added object: {obj.Name}");
    }

    public void RemoveObject(Object3D obj)
    {
        lock (_lock)
        {
            _sceneObjects.Remove(obj);
            obj.Dispose();
        }
        Console.WriteLine($"[VideraEngine] Removed object: {obj.Name}");
    }

    public void ClearObjects()
    {
        lock (_lock)
        {
            foreach (var obj in _sceneObjects)
            {
                obj.Dispose();
            }
            _sceneObjects.Clear();
        }
        Console.WriteLine("[VideraEngine] Cleared all objects");
    }

    // 保留这个方法以兼容旧代码，但现在不再使用
    public void UpdateMesh(MeshData mesh)
    {
        // 这个方法在新架构中已不需要，因为每个Object3D自己管理mesh
        Console.WriteLine("[VideraEngine] UpdateMesh is deprecated in new architecture");
    }
}