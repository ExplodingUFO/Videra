using System.Numerics;
using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Styles.Services;

namespace Videra.Core.Graphics;

/// <summary>
/// Central rendering engine that orchestrates graphics back-end resources, scene objects,
/// cameras, and visual helpers (grid, axes, wireframe) to produce each frame.
/// Implements <see cref="IDisposable"/> so that all GPU resources are released deterministically.
/// </summary>
public partial class VideraEngine : IDisposable
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
    private bool _disposed;

    private readonly List<Object3D> _sceneObjects = new();
    private readonly IRenderStyleService _styleService;

    /// <summary>
    /// Gets the orbit camera used to view the scene.
    /// </summary>
    public OrbitCamera Camera { get; } = new();

    internal IReadOnlyList<Object3D> SceneObjects => _sceneObjects;

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
    public bool EnableFrameLogging { get; set; }

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

    /// <summary>
    /// Adds a 3D object to the scene. If the engine has been initialized the object's
    /// wireframe buffers are created immediately.
    /// </summary>
    /// <param name="obj">The <see cref="Object3D"/> to add to the scene.</param>
    public void AddObject(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        lock (_lock)
        {
            _sceneObjects.Add(obj);

            if (_factory != null)
            {
                obj.InitializeWireframe(_factory);
            }
        }
        Log.ObjectAdded(_logger, obj.Name);
    }

    /// <summary>
    /// Removes a 3D object from the scene and disposes its GPU resources.
    /// </summary>
    /// <param name="obj">The <see cref="Object3D"/> to remove.</param>
    public void RemoveObject(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        lock (_lock)
        {
            _sceneObjects.Remove(obj);
            obj.Dispose();
        }
        Log.ObjectRemoved(_logger, obj.Name);
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
        Log.ObjectsCleared(_logger);
    }

    // 保留这个方法以兼容旧代码，但现在不再使用
    /// <summary>
    /// This method is deprecated in the current architecture where each
    /// <see cref="Object3D"/> manages its own mesh. Calling it logs a warning.
    /// </summary>
    /// <param name="mesh">Mesh data (ignored).</param>
    [Obsolete("UpdateMesh is deprecated. Each Object3D manages its own mesh in the current architecture.")]
    public void UpdateMesh(MeshData mesh)
    {
        Log.UpdateMeshDeprecated(_logger);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[VideraEngine] Initialized successfully")]
        public static partial void Initialized(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "[VideraEngine] Suspended current graphics backend")]
        public static partial void Suspended(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "[VideraEngine] Resources created")]
        public static partial void ResourcesCreated(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "[VideraEngine] Resize ignored: Init={IsInitialized}, {Width}x{Height}")]
        public static partial void ResizeIgnored(ILogger logger, bool isInitialized, uint width, uint height);

        [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "[VideraEngine] Resizing to: {Width}x{Height}")]
        public static partial void Resizing(ILogger logger, uint width, uint height);

        [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "[VideraEngine] Resize failed: {Error}")]
        public static partial void ResizeFailed(ILogger logger, string error, Exception exception);

        [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "[VideraEngine] Frame {FrameCount}: Drawing {ObjectCount} objects, Grid visible: {GridVisible}")]
        public static partial void DrawingFrame(ILogger logger, int frameCount, int objectCount, bool gridVisible);

        [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "[VideraEngine] Camera Position: {Position}, Target: {Target}")]
        public static partial void CameraPosition(ILogger logger, Vector3 position, Vector3 target);

        [LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "[VideraEngine] Camera Yaw: {Yaw:F2}, Pitch: {Pitch:F2}")]
        public static partial void CameraRotation(ILogger logger, float yaw, float pitch);

        [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "[VideraEngine] ViewMatrix M11={M11:F2}, M12={M12:F2}, M13={M13:F2}, M14={M14:F2}")]
        public static partial void ViewMatrix(ILogger logger, float m11, float m12, float m13, float m14);

        [LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "[VideraEngine] ProjMatrix M11={M11:F2}, M22={M22:F2}, M33={M33:F2}, M43={M43:F2}")]
        public static partial void ProjectionMatrix(ILogger logger, float m11, float m22, float m33, float m43);

        [LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "[VideraEngine] Camera uniform buffer bound to index 1")]
        public static partial void CameraUniformBound(ILogger logger);

        [LoggerMessage(EventId = 13, Level = LogLevel.Debug, Message = "[VideraEngine] Calling Grid.Draw, IsVisible={GridVisible}")]
        public static partial void DrawingGrid(ILogger logger, bool gridVisible);

        [LoggerMessage(EventId = 14, Level = LogLevel.Debug, Message = "[VideraEngine] Starting to render {ObjectCount} objects, shouldRenderSolid={ShouldRenderSolid}")]
        public static partial void StartingObjectRender(ILogger logger, int objectCount, bool shouldRenderSolid);

        [LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "[VideraEngine] Skipping object '{ObjectName}' - missing buffers")]
        public static partial void SkippingObjectMissingBuffers(ILogger logger, string objectName);

        [LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "[VideraEngine] Drawing object '{ObjectName}' with {IndexCount} indices")]
        public static partial void DrawingObject(ILogger logger, string objectName, uint indexCount);

        [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "[VideraEngine] Added object: {ObjectName}")]
        public static partial void ObjectAdded(ILogger logger, string objectName);

        [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "[VideraEngine] Removed object: {ObjectName}")]
        public static partial void ObjectRemoved(ILogger logger, string objectName);

        [LoggerMessage(EventId = 19, Level = LogLevel.Information, Message = "[VideraEngine] Cleared all objects")]
        public static partial void ObjectsCleared(ILogger logger);

        [LoggerMessage(EventId = 20, Level = LogLevel.Warning, Message = "[VideraEngine] UpdateMesh is deprecated in new architecture")]
        public static partial void UpdateMeshDeprecated(ILogger logger);
    }
}
