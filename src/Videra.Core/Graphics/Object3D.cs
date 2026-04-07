using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Wireframe;

namespace Videra.Core.Graphics;

/// <summary>
/// Represents a 3D object with independent position, rotation, and scale transforms,
/// GPU-backed vertex and index buffers, and optional wireframe rendering support.
/// Implements <see cref="IDisposable"/> to release GPU resources.
/// </summary>
public partial class Object3D : IDisposable
{
    private bool _disposed;
    private MeshData? _cachedMesh;
    private bool _wireframeResourcesRequested;
    private BoundingBox3? _localBounds;

    /// <summary>
    /// Gets or sets the display name of this object, used for logging and identification.
    /// Defaults to <c>"Object"</c>.
    /// </summary>
    public string Name { get; set; } = "Object";

    // --- Independent transform properties ---

    /// <summary>
    /// Gets or sets the world-space position of this object.
    /// Defaults to <see cref="Vector3.Zero"/>.
    /// </summary>
    public Vector3 Position { get; set; } = Vector3.Zero;

    /// <summary>
    /// Gets or sets the Euler rotation of this object in radians (X = pitch, Y = yaw, Z = roll).
    /// Defaults to <see cref="Vector3.Zero"/>.
    /// </summary>
    public Vector3 Rotation { get; set; } = Vector3.Zero;

    /// <summary>
    /// Gets or sets the uniform scale applied to this object along each axis.
    /// Defaults to <see cref="Vector3.One"/>.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.One;

    // --- GPU resources (managed per-object) ---

    /// <summary>
    /// Gets the GPU vertex buffer containing <see cref="VertexPositionNormalColor"/> data,
    /// or <c>null</c> if the object has not been initialized.
    /// </summary>
    internal IBuffer? VertexBuffer { get; private set; }

    /// <summary>
    /// Gets the GPU index buffer containing triangle indices,
    /// or <c>null</c> if the object has not been initialized.
    /// </summary>
    internal IBuffer? IndexBuffer { get; private set; }

    /// <summary>
    /// Gets the GPU uniform buffer holding the current world matrix (64 bytes),
    /// or <c>null</c> if the object has not been initialized.
    /// </summary>
    internal IBuffer? WorldBuffer { get; private set; }

    /// <summary>
    /// Gets the number of indices stored in <see cref="IndexBuffer"/>.
    /// </summary>
    internal uint IndexCount { get; private set; }

    /// <summary>
    /// Gets the primitive topology (triangles, lines, or points) of the mesh data.
    /// </summary>
    internal MeshTopology Topology { get; private set; }

    // --- Wireframe rendering resources ---

    /// <summary>
    /// Gets the GPU index buffer containing extracted wireframe edge indices,
    /// or <c>null</c> if wireframe has not been initialized.
    /// </summary>
    internal IBuffer? LineIndexBuffer { get; private set; }

    /// <summary>
    /// Gets the GPU vertex buffer used exclusively for wireframe rendering
    /// (with per-vertex custom colors), or <c>null</c> if wireframe has not been initialized.
    /// </summary>
    internal IBuffer? LineVertexBuffer { get; private set; }

    /// <summary>
    /// Gets the number of indices stored in <see cref="LineIndexBuffer"/>.
    /// </summary>
    internal uint LineIndexCount { get; private set; }

    private uint[]? _cachedTriangleIndices;
    private VertexPositionNormalColor[]? _cachedVertices;

    // Compute the world matrix from SRT transforms

    /// <summary>
    /// Gets the computed world (model) matrix derived from the current
    /// <see cref="Scale"/>, <see cref="Rotation"/>, and <see cref="Position"/> values.
    /// The matrix is recomputed on every access using SRT (Scale-Rotate-Translate) order.
    /// </summary>
    public Matrix4x4 WorldMatrix =>
        Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
        Matrix4x4.CreateTranslation(Position);

    public BoundingBox3? LocalBounds => _localBounds;

    public BoundingBox3? WorldBounds => _localBounds?.Transform(WorldMatrix);

    /// <summary>
    /// Releases all GPU resources (vertex, index, world, and wireframe buffers) held by this object.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        ReleaseGpuResources();
        GC.SuppressFinalize(this);
    }

    internal void ReleaseGpuResources()
    {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
        WorldBuffer?.Dispose();
        LineIndexBuffer?.Dispose();
        LineVertexBuffer?.Dispose();

        VertexBuffer = null;
        IndexBuffer = null;
        WorldBuffer = null;
        LineIndexBuffer = null;
        LineVertexBuffer = null;
        IndexCount = 0;
        LineIndexCount = 0;
    }

    // Initialize GPU resources (called by Engine)

    /// <summary>
    /// Creates and populates GPU buffers from the supplied <paramref name="mesh"/> data.
    /// Allocates vertex, index, and world-matrix uniform buffers on the GPU.
    /// On failure, any partially created resources are disposed before the exception propagates.
    /// </summary>
    /// <param name="factory">The resource factory used to create GPU buffers.</param>
    /// <param name="mesh">The mesh data containing vertices, indices, and topology.</param>
    /// <param name="logger">
    /// An optional logger for diagnostic output. When <c>null</c>, a no-op logger is used.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="mesh"/> has null or empty vertex/index data.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the vertex or index data exceeds the maximum GPU buffer size (<see cref="uint.MaxValue"/> bytes).
    /// </exception>
    public void Initialize(IResourceFactory factory, MeshData mesh, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(mesh);
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<Object3D>();
        try
        {
            // 验证输入
            if (mesh.Vertices == null || mesh.Vertices.Length == 0)
                throw new ArgumentException("Invalid mesh data");

            if (mesh.Indices == null || mesh.Indices.Length == 0)
                throw new ArgumentException("Invalid index data");

            Log.Initializing(log, Name, mesh.Vertices.Length, mesh.Indices.Length);

            _cachedMesh = new MeshData
            {
                Vertices = (VertexPositionNormalColor[])mesh.Vertices.Clone(),
                Indices = (uint[])mesh.Indices.Clone(),
                Topology = mesh.Topology
            };
            _localBounds = BoundingBox3.FromVertices(mesh.Vertices);
            _wireframeResourcesRequested = false;
            ReleaseGpuResources();
            Topology = mesh.Topology;
            IndexCount = (uint)mesh.Indices.Length;

            // 安全的大小计算
            var vertexSize = Unsafe.SizeOf<VertexPositionNormalColor>();

            if ((long)mesh.Vertices.Length * vertexSize > uint.MaxValue)
                throw new InvalidOperationException($"Vertex buffer too large: {mesh.Vertices.Length} vertices");

            var vSize = (uint)(mesh.Vertices.Length * vertexSize);

            Log.VertexBufferSize(log, Name, vSize, vSize / 1024.0 / 1024.0);

            VertexBuffer = factory.CreateVertexBuffer(vSize);
            VertexBuffer.SetData(mesh.Vertices, 0);

            // 缓存顶点数据用于线框
            _cachedVertices = (VertexPositionNormalColor[])mesh.Vertices.Clone();

            // 索引 Buffer
            if ((long)mesh.Indices.Length * sizeof(uint) > uint.MaxValue)
                throw new InvalidOperationException($"Index buffer too large: {mesh.Indices.Length} indices");

            var iSize = (uint)(mesh.Indices.Length * sizeof(uint));

            Log.IndexBufferSize(log, Name, iSize, iSize / 1024.0 / 1024.0);

            IndexBuffer = factory.CreateIndexBuffer(iSize);
            IndexBuffer.SetData(mesh.Indices, 0);

            // 缓存三角形索引用于线框提取
            if (mesh.Topology == MeshTopology.Triangles)
            {
                _cachedTriangleIndices = (uint[])mesh.Indices.Clone();
            }
            else
            {
                _cachedTriangleIndices = null;
            }

            // World Uniform Buffer
            WorldBuffer = factory.CreateUniformBuffer(64);

            Log.Initialized(log, Name);
        }
        catch (Exception ex)
        {
            Log.InitializationFailed(log, Name, ex.Message, ex);

            // 清理已创建的资源
            ReleaseGpuResources();
            _localBounds = null;

            throw;
        }
    }

    /// <summary>
    /// Uploads the current <see cref="WorldMatrix"/> to the GPU world-matrix uniform buffer,
    /// ensuring the shader receives the latest transform before the next draw call.
    /// </summary>
    /// <param name="executor">The command executor owning the active GPU context (reserved for future use).</param>
    public void UpdateUniforms(ICommandExecutor executor)
    {
        ArgumentNullException.ThrowIfNull(executor);

        // 每次绘制前，把最新的 SRT 矩阵传给 GPU
        if (WorldBuffer != null)
        {
            WorldBuffer.SetData(WorldMatrix, 0);
        }
    }

    /// <summary>
    /// Initializes wireframe rendering resources by extracting unique edges from the
    /// cached triangle indices. The extracted edges are uploaded to a dedicated GPU
    /// index buffer, and a copy of the original vertices is placed in a separate
    /// wireframe vertex buffer so that per-edge colors can be applied independently.
    /// Does nothing if the mesh topology is not <see cref="MeshTopology.Triangles"/>
    /// or if no edges could be extracted.
    /// </summary>
    /// <param name="factory">The resource factory used to create GPU buffers.</param>
    /// <param name="logger">
    /// An optional logger for diagnostic output. When <c>null</c>, a no-op logger is used.
    /// </param>
    public void InitializeWireframe(IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<Object3D>();
        _wireframeResourcesRequested = true;
        LineIndexBuffer?.Dispose();
        LineVertexBuffer?.Dispose();
        LineIndexBuffer = null;
        LineVertexBuffer = null;
        LineIndexCount = 0;

        if (_cachedTriangleIndices == null || _cachedTriangleIndices.Length == 0)
        {
            Log.WireframeNoTriangleIndices(log, Name);
            return;
        }

        // 从三角形索引中提取唯一边缘
        var lineIndices = EdgeExtractor.ExtractUniqueEdges(_cachedTriangleIndices);

        if (lineIndices.Length == 0)
        {
            Log.WireframeNoEdges(log, Name);
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

        Log.WireframeInitialized(log, Name, LineIndexCount / 2);
    }

    internal void RecreateGraphicsResources(IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (_cachedMesh == null)
        {
            return;
        }

        var restoreWireframe = _wireframeResourcesRequested;
        Initialize(factory, _cachedMesh, logger);
        if (restoreWireframe)
        {
            InitializeWireframe(factory, logger);
        }
    }

    /// <summary>
    /// Updates the wireframe vertex buffer with the specified <paramref name="color"/>,
    /// replacing the color component of every cached vertex while preserving position
    /// and normal data. Does nothing if wireframe resources have not been initialized.
    /// </summary>
    /// <param name="color">The RGBA color to apply to all wireframe vertices.</param>
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

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "[Object3D '{Name}'] Initializing: {VertexCount} verts, {IndexCount} indices")]
        public static partial void Initializing(ILogger logger, string name, int vertexCount, int indexCount);

        [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "[Object3D '{Name}'] Vertex buffer size: {BufferSize:N0} bytes ({BufferSizeMB:F2} MB)")]
        public static partial void VertexBufferSize(ILogger logger, string name, uint bufferSize, double bufferSizeMB);

        [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[Object3D '{Name}'] Index buffer size: {BufferSize:N0} bytes ({BufferSizeMB:F2} MB)")]
        public static partial void IndexBufferSize(ILogger logger, string name, uint bufferSize, double bufferSizeMB);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "[Object3D '{Name}'] Initialized successfully")]
        public static partial void Initialized(ILogger logger, string name);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "[Object3D '{Name}'] Initialization failed: {Error}")]
        public static partial void InitializationFailed(ILogger logger, string name, string error, Exception exception);

        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "[Object3D '{Name}'] Wireframe: No triangle indices available")]
        public static partial void WireframeNoTriangleIndices(ILogger logger, string name);

        [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "[Object3D '{Name}'] Wireframe: No edges extracted")]
        public static partial void WireframeNoEdges(ILogger logger, string name);

        [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "[Object3D '{Name}'] Wireframe: {EdgeCount} edges initialized")]
        public static partial void WireframeInitialized(ILogger logger, string name, uint edgeCount);
    }
}
