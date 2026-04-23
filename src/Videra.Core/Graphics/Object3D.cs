using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Scene;
using Videra.Core.Graphics.Wireframe;
using Videra.Core.Inspection;

namespace Videra.Core.Graphics;

/// <summary>
/// Represents a 3D object with independent position, rotation, and scale transforms,
/// GPU-backed vertex and index buffers, and optional wireframe rendering support.
/// Implements <see cref="IDisposable"/> to release GPU resources.
/// </summary>
public partial class Object3D : IDisposable
{
    private bool _disposed;
    private MeshPayload? _sourceMeshPayload;
    private MeshPayload? _meshPayload;
    private bool _wireframeResourcesRequested;
    private BoundingBox3? _localBounds;

    /// <summary>
    /// Gets the stable identity of this scene object.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

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

    internal IBuffer? AlphaMaskBuffer { get; private set; }

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

    internal MeshPayload? MeshPayload => _meshPayload;

    internal MeshPayload? SourceMeshPayload => _sourceMeshPayload;

    internal MeshPayloadSegment[] Segments => _meshPayload?.Segments ?? Array.Empty<MeshPayloadSegment>();

    internal MaterialAlphaSettings MaterialAlpha { get; private set; } = MaterialAlphaSettings.Opaque;

    internal CpuMeshRetentionPolicy CpuMeshRetentionPolicy { get; private set; } = CpuMeshRetentionPolicy.KeepForReuploadAndPicking;

    internal bool HasPreparedMesh => _meshPayload is not null;

    internal bool CanRecreateGraphicsResources => _sourceMeshPayload is not null;

    internal bool HasOpaqueGeometry => HasGeometryForPass(transparentPass: false);

    internal bool HasTransparentGeometry => HasGeometryForPass(transparentPass: true);

    internal long ApproximateGpuBytes
    {
        get
        {
            return _meshPayload?.ApproximateGpuBytes ?? 0L;
        }
    }

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
        AlphaMaskBuffer?.Dispose();
        LineIndexBuffer?.Dispose();
        LineVertexBuffer?.Dispose();

        VertexBuffer = null;
        IndexBuffer = null;
        WorldBuffer = null;
        AlphaMaskBuffer = null;
        LineIndexBuffer = null;
        LineVertexBuffer = null;
        IndexCount = 0;
        LineIndexCount = 0;
    }

    internal void PrepareDeferredMesh(MeshData mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        PrepareDeferredMesh(MeshPayload.FromMesh(mesh, cloneArrays: true));
    }

    internal void PrepareDeferredMesh(MeshPayload payload)
    {
        PrepareDeferredMesh(payload, CpuMeshRetentionPolicy.KeepForReuploadAndPicking);
    }

    internal void PrepareDeferredMesh(MeshPayload payload, CpuMeshRetentionPolicy retentionPolicy)
    {
        ArgumentNullException.ThrowIfNull(payload);

        _sourceMeshPayload = payload;
        _meshPayload = payload;
        CpuMeshRetentionPolicy = retentionPolicy;
        _localBounds = payload.LocalBounds;
        _wireframeResourcesRequested = false;
        ReleaseGpuResources();
        Topology = payload.Topology;
        IndexCount = (uint)payload.Indices.Length;
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
        ArgumentNullException.ThrowIfNull(mesh);
        Initialize(factory, MeshPayload.FromMesh(mesh, cloneArrays: true), logger, resetWireframeRequest: true);
    }

    private void Initialize(
        IResourceFactory factory,
        MeshPayload payload,
        ILogger? logger,
        bool resetWireframeRequest)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(payload);
        var log = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<Object3D>();
        try
        {
            Log.Initializing(log, Name, payload.Vertices.Length, payload.Indices.Length);

            _sourceMeshPayload = payload;
            _meshPayload = payload;
            _localBounds = payload.LocalBounds;
            if (resetWireframeRequest)
            {
                _wireframeResourcesRequested = false;
            }
            ReleaseGpuResources();
            Topology = payload.Topology;
            IndexCount = (uint)payload.Indices.Length;

            // 安全的大小计算
            var vertexSize = Unsafe.SizeOf<VertexPositionNormalColor>();

            if ((long)payload.Vertices.Length * vertexSize > uint.MaxValue)
                throw new InvalidOperationException($"Vertex buffer too large: {payload.Vertices.Length} vertices");

            var vSize = (uint)(payload.Vertices.Length * vertexSize);

            Log.VertexBufferSize(log, Name, vSize, vSize / 1024.0 / 1024.0);

            VertexBuffer = factory.CreateVertexBuffer(vSize);
            VertexBuffer.SetData(payload.Vertices, 0);

            // 索引 Buffer
            if ((long)payload.Indices.Length * sizeof(uint) > uint.MaxValue)
                throw new InvalidOperationException($"Index buffer too large: {payload.Indices.Length} indices");

            var iSize = (uint)(payload.Indices.Length * sizeof(uint));

            Log.IndexBufferSize(log, Name, iSize, iSize / 1024.0 / 1024.0);

            IndexBuffer = factory.CreateIndexBuffer(iSize);
            IndexBuffer.SetData(payload.Indices, 0);

            // World Uniform Buffer
            WorldBuffer = factory.CreateUniformBuffer(64);
            AlphaMaskBuffer = factory.CreateUniformBuffer(16);
            AlphaMaskBuffer.Update(ObjectAlphaMaskUniformData.From(MaterialAlpha));

            Log.Initialized(log, Name);
        }
        catch (Exception ex)
        {
            Log.InitializationFailed(log, Name, ex.Message, ex);

            // 清理已创建的资源
            ReleaseGpuResources();
            if (resetWireframeRequest)
            {
                _localBounds = null;
                _meshPayload = null;
            }

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

        if (AlphaMaskBuffer != null)
        {
            AlphaMaskBuffer.SetData(ObjectAlphaMaskUniformData.From(MaterialAlpha), 0);
        }
    }

    internal void ApplyMaterialAlpha(MaterialAlphaSettings alpha)
    {
        MaterialAlpha = alpha;

        if (AlphaMaskBuffer != null)
        {
            AlphaMaskBuffer.SetData(ObjectAlphaMaskUniformData.From(MaterialAlpha), 0);
        }
    }

    private bool HasGeometryForPass(bool transparentPass)
    {
        var segments = Segments;
        if (segments.Length == 0)
        {
            return transparentPass
                ? MaterialAlpha.Mode == MaterialAlphaMode.Blend
                : MaterialAlpha.Mode != MaterialAlphaMode.Blend;
        }

        for (var i = 0; i < segments.Length; i++)
        {
            var isTransparent = segments[i].Alpha.Mode == MaterialAlphaMode.Blend;
            if (isTransparent == transparentPass)
            {
                return true;
            }
        }

        return false;
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

        if (_meshPayload is null ||
            _meshPayload.Topology != MeshTopology.Triangles ||
            _meshPayload.Indices.Length == 0)
        {
            Log.WireframeNoTriangleIndices(log, Name);
            return;
        }

        // 从三角形索引中提取唯一边缘
        var lineIndices = EdgeExtractor.ExtractUniqueEdges(_meshPayload.Indices);

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
        var vertexSize = Unsafe.SizeOf<VertexPositionNormalColor>();
        var vSize = (uint)(_meshPayload.Vertices.Length * vertexSize);
        LineVertexBuffer = factory.CreateVertexBuffer(vSize);
        LineVertexBuffer.SetData(_meshPayload.Vertices, 0);

        Log.WireframeInitialized(log, Name, LineIndexCount / 2);
    }

    internal void RecreateGraphicsResources(IResourceFactory factory, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (_meshPayload == null)
        {
            return;
        }

        var restoreWireframe = _wireframeResourcesRequested;
        Initialize(factory, _meshPayload, logger, resetWireframeRequest: false);
        if (restoreWireframe)
        {
            InitializeWireframe(factory, logger);
        }
    }

    internal void ApplyClippingPlanes(IReadOnlyList<VideraClipPlane> clippingPlanes)
    {
        if (_sourceMeshPayload is null)
        {
            return;
        }

        var payloadService = new VideraClipPayloadService();
        var nextPayload = payloadService.Clip(_sourceMeshPayload, clippingPlanes);
        if (ReferenceEquals(nextPayload, _meshPayload))
        {
            return;
        }

        _meshPayload = nextPayload;
        _localBounds = nextPayload?.LocalBounds;
        _wireframeResourcesRequested = false;
        ReleaseGpuResources();
        Topology = nextPayload?.Topology ?? _sourceMeshPayload.Topology;
        IndexCount = (uint)(nextPayload?.Indices.Length ?? 0);
        LineIndexCount = 0;
    }

    /// <summary>
    /// Updates the wireframe vertex buffer with the specified <paramref name="color"/>,
    /// replacing the color component of every cached vertex while preserving position
    /// and normal data. Does nothing if wireframe resources have not been initialized.
    /// </summary>
    /// <param name="color">The RGBA color to apply to all wireframe vertices.</param>
    public void UpdateWireframeColor(RgbaFloat color)
    {
        if (_meshPayload is null || LineVertexBuffer == null)
            return;

        // 创建带新颜色的顶点数组
        var sourceVertices = _meshPayload.Vertices;
        var coloredVertices = new VertexPositionNormalColor[sourceVertices.Length];
        for (int i = 0; i < sourceVertices.Length; i++)
        {
            coloredVertices[i] = new VertexPositionNormalColor(
                sourceVertices[i].Position,
                sourceVertices[i].Normal,
                color
            );
        }

        LineVertexBuffer.SetData(coloredVertices, 0);
    }

    internal bool TryCreateColoredWireframeVertices(
        RgbaFloat color,
        out VertexPositionNormalColor[] vertices)
    {
        if (_meshPayload is null || _meshPayload.Vertices.Length == 0)
        {
            vertices = Array.Empty<VertexPositionNormalColor>();
            return false;
        }

        var sourceVertices = _meshPayload.Vertices;
        vertices = new VertexPositionNormalColor[sourceVertices.Length];
        for (var i = 0; i < sourceVertices.Length; i++)
        {
            vertices[i] = new VertexPositionNormalColor(
                sourceVertices[i].Position,
                sourceVertices[i].Normal,
                color);
        }

        return true;
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
