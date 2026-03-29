using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;
using Videra.Core.Styles.Services;

namespace Videra.Core.Graphics;

/// <summary>
/// Central rendering engine that orchestrates graphics back-end resources, scene objects,
/// cameras, and visual helpers (grid, axes, wireframe) to produce each frame.
/// Implements <see cref="IDisposable"/> so that all GPU resources are released deterministically.
/// </summary>
public class VideraEngine : IDisposable
{
	private readonly object _lock = new();
	private readonly ILogger _logger;
	private IGraphicsBackend? _backend;
	private IResourceFactory? _factory;
	private ICommandExecutor? _executor;

	private IPipeline? _meshPipeline;
	private IBuffer? _cameraBuffer;
	private IBuffer? _styleUniformBuffer;

	private uint _width, _height;

	private readonly List<Object3D> _sceneObjects = new();
	private readonly IRenderStyleService _styleService;

	/// <summary>
	/// Gets the orbit camera used to view the scene.
	/// </summary>
	public OrbitCamera Camera { get; } = new();

	/// <summary>
	/// Gets a value indicating whether the engine has been initialized and is ready to render.
	/// </summary>
	public bool IsInitialized { get; private set; }

	/// <summary>
	/// Gets or sets the background color used to clear the render target each frame.
	/// </summary>
	public RgbaFloat BackgroundColor { get; set; } = new(0.1f, 0.1f, 0.1f, 1f);

	/// <summary>
	/// Gets or sets the render scale factor applied during axis rendering.
	/// A value of 1.0 uses the default scale.
	/// </summary>
	public float RenderScale { get; set; } = 1f;

	/// <summary>
	/// Gets or sets a value indicating whether periodic per-frame diagnostic logging is enabled.
	/// When <see langword="true"/>, debug information is emitted every 60 frames.
	/// </summary>
	public bool EnableFrameLogging { get; set; } = false;

	/// <summary>
	/// Gets the render style service that manages visual style parameters
	/// and notifies listeners when styles change.
	/// </summary>
	public IRenderStyleService StyleService => _styleService;

	/// <summary>
	/// Gets the grid renderer responsible for drawing the reference grid in the 3D viewport.
	/// </summary>
	public GridRenderer Grid { get; } = new();

	private readonly AxisRenderer _axisRenderer = new();

	/// <summary>
	/// Gets the wireframe renderer that overlays wireframe geometry on scene objects.
	/// </summary>
	public WireframeRenderer Wireframe { get; } = new();

	/// <summary>
	/// Gets or sets a value indicating whether the origin axis helper is visible.
	/// </summary>
	public bool ShowAxis
	{
		get => _axisRenderer.IsVisible;
		set => _axisRenderer.IsVisible = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="VideraEngine"/> class.
	/// </summary>
	/// <param name="styleService">
	/// An optional <see cref="IRenderStyleService"/> implementation.
	/// When <see langword="null"/>, a default <see cref="RenderStyleService"/> is used.
	/// </param>
	/// <param name="logger">
	/// An optional <see cref="ILogger{VideraEngine}"/> for diagnostic output.
	/// When <see langword="null"/>, a no-op logger is used.
	/// </param>
	public VideraEngine(IRenderStyleService? styleService = null, ILogger<VideraEngine>? logger = null)
	{
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<VideraEngine>();
        _styleService = styleService ?? new RenderStyleService();
        _styleService.StyleChanged += OnStyleChanged;
    }

    private void OnStyleChanged(object? sender, StyleChangedEventArgs e)
    {
        // 更新 GPU Uniform Buffer
        if (_styleUniformBuffer != null)
        {
            _styleUniformBuffer.Update(e.UniformData);
        }
    }

	/// <summary>
	/// Releases all GPU resources held by the engine, including pipelines, buffers,
	/// renderers, scene objects, and the graphics back-end.
	/// Sets <see cref="IsInitialized"/> to <see langword="false"/>.
	/// </summary>
	public void Dispose()
	{
        _styleService.StyleChanged -= OnStyleChanged;
        Grid.Dispose();
        _axisRenderer.Dispose();
        Wireframe.Dispose();
        ClearObjects();

        _meshPipeline?.Dispose();
        _cameraBuffer?.Dispose();
        _styleUniformBuffer?.Dispose();
        _backend?.Dispose();

        IsInitialized = false;
    }

	/// <summary>
	/// Initializes the engine with the specified graphics back-end.
	/// Creates GPU resources (camera buffer, style buffer, mesh pipeline) and
	/// initializes all sub-renderers (grid, axis, wireframe).
	/// If the engine is already initialized this method returns immediately.
	/// </summary>
	/// <param name="backend">The platform-specific graphics back-end to use for rendering.</param>
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
        Wireframe.Initialize(_factory);
        
        IsInitialized = true;
        _logger.LogInformation("[VideraEngine] Initialized successfully");
    }

    private void CreateResources()
    {
        if (_factory == null) return;

        // 创建相机uniform buffer (View + Projection = 2 * 64 bytes)
        _cameraBuffer = _factory.CreateUniformBuffer(128);

        // 创建风格 uniform buffer (128 bytes)
        _styleUniformBuffer = _factory.CreateUniformBuffer(128);
        _styleUniformBuffer.Update(_styleService.CurrentParameters.ToUniformData());

        // 创建Pipeline
        _meshPipeline = _factory.CreatePipeline(
            vertexSize: (uint)Unsafe.SizeOf<VertexPositionNormalColor>(),
            hasNormals: true,
            hasColors: true
        );

        _logger.LogInformation("[VideraEngine] Resources created");
    }

	/// <summary>
	/// Resizes the internal render target and updates the camera projection matrix.
	/// Ignored when the engine is not initialized, when dimensions are zero, or when
	/// the size has not changed.
	/// </summary>
	/// <param name="width">The new render target width in pixels.</param>
	/// <param name="height">The new render target height in pixels.</param>
	public void Resize(uint width, uint height)
	{
        if (!IsInitialized || width == 0 || height == 0)
        {
            _logger.LogDebug("[VideraEngine] Resize ignored: Init={IsInitialized}, {Width}x{Height}", IsInitialized, $"{width}x{height}");
            return;
        }

        if (_width == width && _height == height) return;

        _width = width;
        _height = height;

        try
        {
            _logger.LogInformation("[VideraEngine] Resizing to: {Width}x{Height}", width, height);
            _backend?.Resize((int)width, (int)height);
            Camera.UpdateProjection(width, height);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[VideraEngine] Resize failed: {Error}", ex.Message);
        }
    }

    private int _frameCount = 0;
    
	/// <summary>
	/// Renders a single frame: clears the back-end, updates camera and style uniforms,
	/// draws the grid, solid scene objects, wireframe overlays, and axis helper,
	/// then presents the result.
	/// This method is thread-safe; calls are serialized via an internal lock.
	/// </summary>
	public void Draw()
	{
        if (!IsInitialized || _backend == null || _executor == null || _meshPipeline == null)
        {
            return;
        }

        lock (_lock)
        {
            _frameCount++;
            bool shouldLog = EnableFrameLogging && _frameCount % 60 == 0; // 每秒记录一次（60 FPS）
            
            if (shouldLog)
                _logger.LogDebug("[VideraEngine] Frame {FrameCount}: Drawing {ObjectCount} objects, Grid visible: {GridVisible}", _frameCount, _sceneObjects.Count, Grid.IsVisible);
            
            // 更新后端的清除颜色
            _backend.SetClearColor(new Vector4(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A));
            
            // 开始帧
            _backend.BeginFrame();
            
            // 清除颜色和深度
            _executor.Clear(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
            
            // 设置视口
            _executor.SetViewport(0, 0, _width, _height);
            
            // 更新相机数据
            if (_cameraBuffer != null)
            {
                _cameraBuffer.Update(new CameraUniform(Camera.ViewMatrix, Camera.ProjectionMatrix));

                if (shouldLog)
                {
                    _logger.LogDebug("[VideraEngine] Camera Position: {Position}, Target: {Target}", Camera.Position, Camera.Target);
                    _logger.LogDebug("[VideraEngine] Camera Yaw: {Yaw:F2}, Pitch: {Pitch:F2}", Camera.Yaw, Camera.Pitch);

                    // 输出View矩阵的第一行来验证数据
                    var viewMatrix = Camera.ViewMatrix;
                    _logger.LogDebug("[VideraEngine] ViewMatrix M11={M11:F2}, M12={M12:F2}, M13={M13:F2}, M14={M14:F2}", viewMatrix.M11, viewMatrix.M12, viewMatrix.M13, viewMatrix.M14);

                    var projMatrix = Camera.ProjectionMatrix;
                    _logger.LogDebug("[VideraEngine] ProjMatrix M11={M11:F2}, M22={M22:F2}, M33={M33:F2}, M43={M43:F2}", projMatrix.M11, projMatrix.M22, projMatrix.M33, projMatrix.M43);
                }
            }
            
            // 绑定Pipeline用于模型渲染
            _executor.SetPipeline(_meshPipeline);
            
            // 绑定相机 uniform buffer 到索引 1（shader中的 [[buffer(1)]]）
            if (_cameraBuffer != null)
            {
                _executor.SetVertexBuffer(_cameraBuffer, 1);
                if (shouldLog)
                    _logger.LogDebug("[VideraEngine] Camera uniform buffer bound to index 1");
            }

            // 绑定风格 uniform buffer 到索引 3（shader中的 [[buffer(3)]]）
            if (_styleUniformBuffer != null)
            {
                _executor.SetVertexBuffer(_styleUniformBuffer, 3);
            }

            // 渲染网格（传入pipeline）
            if (shouldLog)
                _logger.LogDebug("[VideraEngine] Calling Grid.Draw, IsVisible={GridVisible}", Grid.IsVisible);
            Grid.Draw(_executor, _meshPipeline, Camera, _width, _height);
            
            // 绑定Pipeline用于模型渲染
            _executor.SetPipeline(_meshPipeline);

            // 渲染所有物体（WireframeOnly模式下跳过实体渲染）
            bool shouldRenderSolid = Wireframe.ShouldRenderSolid();

            if (shouldLog && _sceneObjects.Count > 0)
                _logger.LogDebug("[VideraEngine] Starting to render {ObjectCount} objects, shouldRenderSolid={ShouldRenderSolid}", _sceneObjects.Count, shouldRenderSolid);

            if (shouldRenderSolid)
            {
                foreach (var obj in _sceneObjects)
                {
                    if (obj.VertexBuffer == null || obj.IndexBuffer == null || obj.WorldBuffer == null)
                    {
                        if (shouldLog)
                            _logger.LogDebug("[VideraEngine] Skipping object '{ObjectName}' - missing buffers", obj.Name);
                        continue;
                    }

                    // 更新物体的World矩阵
                    obj.UpdateUniforms(_executor);

                    // 绑定缓冲区
                    _executor.SetVertexBuffer(obj.VertexBuffer, 0); // 顶点数据在索引 0
                    _executor.SetVertexBuffer(obj.WorldBuffer, 2);  // World矩阵在索引 2
                    _executor.SetIndexBuffer(obj.IndexBuffer);

                    if (shouldLog)
                        _logger.LogDebug("[VideraEngine] Drawing object '{ObjectName}' with {IndexCount} indices", obj.Name, obj.IndexCount);

                    // 绘制
                    switch (obj.Topology)
                    {
                        case MeshTopology.Lines:
                            _executor.DrawIndexed(1, obj.IndexCount, 1, 0, 0, 0);
                            break;
                        case MeshTopology.Points:
                            _executor.DrawIndexed(2, obj.IndexCount, 1, 0, 0, 0);
                            break;
                        default:
                            _executor.DrawIndexed(obj.IndexCount, 1, 0, 0, 0);
                            break;
                    }
                }
            }

            // 渲染线框
            if (Wireframe.Mode != WireframeMode.None)
            {
                Wireframe.RenderWireframes(_sceneObjects, _executor, _meshPipeline, Camera, _width, _height);
            }

            // 渲染坐标轴
            _axisRenderer.Draw(_executor, _meshPipeline, Camera, _width, _height, RenderScale);
            
            // 结束帧并呈现
            _backend.EndFrame();
        }
    }

	/// <summary>
	/// Adds a 3D object to the scene. If the engine has been initialized the object's
	/// wireframe buffers are created immediately.
	/// </summary>
	/// <param name="obj">The <see cref="Object3D"/> to add to the scene.</param>
	public void AddObject(Object3D obj)
	{
        lock (_lock)
        {
            _sceneObjects.Add(obj);

            // 自动初始化线框缓冲（如果工厂可用）
            if (_factory != null)
            {
                obj.InitializeWireframe(_factory);
            }
        }
        _logger.LogInformation("[VideraEngine] Added object: {ObjectName}", obj.Name);
    }

	/// <summary>
	/// Removes a 3D object from the scene and disposes its GPU resources.
	/// </summary>
	/// <param name="obj">The <see cref="Object3D"/> to remove.</param>
	public void RemoveObject(Object3D obj)
	{
        lock (_lock)
        {
            _sceneObjects.Remove(obj);
            obj.Dispose();
        }
        _logger.LogInformation("[VideraEngine] Removed object: {ObjectName}", obj.Name);
    }

	/// <summary>
	/// Removes all objects from the scene and disposes their GPU resources.
	/// </summary>
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
        _logger.LogInformation("[VideraEngine] Cleared all objects");
    }

    // 保留这个方法以兼容旧代码，但现在不再使用
	/// <summary>
	/// This method is deprecated in the current architecture where each
	/// <see cref="Object3D"/> manages its own mesh. Calling it logs a warning.
	/// </summary>
	/// <param name="mesh">Mesh data (ignored).</param>
	public void UpdateMesh(MeshData mesh)
	{
        // 这个方法在新架构中已不需要，因为每个Object3D自己管理mesh
        _logger.LogWarning("[VideraEngine] UpdateMesh is deprecated in new architecture");
    }
}
