using System.Numerics;
using Microsoft.Extensions.Logging;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

/// <summary>
/// Renders an axis-aligned ground grid using line primitives.
/// The grid is drawn at a configurable height and can be toggled on or off.
/// </summary>
public class GridRenderer : IDisposable
{
    private IResourceFactory? _factory;
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _worldBuffer;
    private uint _indexCount;
    private bool _disposed;

    /// <summary>
    /// Gets or sets whether the grid is visible during rendering.
    /// When <c>false</c>, <see cref="Draw"/> skips all rendering work.
    /// Default value is <c>true</c>.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the Y-coordinate (height) at which the grid plane is drawn.
    /// Default value is 0.0f.
    /// </summary>
    public float Height { get; set; } = 0.0f;

    /// <summary>
    /// Gets or sets the color used for all grid lines.
    /// Default is a semi-transparent gray <c>(0.4, 0.4, 0.4, 0.5)</c>.
    /// </summary>
    public RgbaFloat GridColor { get; set; } = new(0.4f, 0.4f, 0.4f, 0.5f);

    /// <summary>
    /// Gets or sets whether diagnostic logging is enabled for draw calls.
    /// When <c>true</c>, index count is logged every 60 frames.
    /// Default value is <c>false</c>.
    /// </summary>
    public bool EnableDiagnostics { get; set; } = false;

    /// <summary>
    /// Initializes the grid renderer with the specified resource factory.
    /// Builds the grid geometry and creates GPU buffers immediately.
    /// </summary>
    /// <param name="factory">The resource factory used to create vertex, index, and uniform buffers.
    /// May be <c>null</c>, in which case the renderer is initialized but no buffers are created
    /// until <see cref="Rebuild"/> is called with a valid factory.</param>
    public void Initialize(IResourceFactory? factory)
    {
        _factory = factory;
        Rebuild(factory);
    }

    /// <summary>
    /// Rebuilds the grid geometry and recreates GPU buffers.
    /// Call this after changing <see cref="Height"/>, <see cref="GridColor"/>,
    /// or when the resource factory changes.
    /// </summary>
    /// <param name="factory">An optional replacement resource factory.
    /// If <c>null</c>, the previously set factory is reused.</param>
    public void Rebuild(IResourceFactory? factory = null)
    {
        if (factory != null)
            _factory = factory;

        if (_factory == null)
            return;

        DisposeBuffers();
        BuildGrid(_factory);
    }

    private void BuildGrid(IResourceFactory factory)
    {
        const int gridSize = 10;
        const float spacing = 1.0f;
        const float halfSize = gridSize * spacing * 0.5f;

        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();
        uint vertexIndex = 0;

        for (int i = 0; i <= gridSize; i++)
        {
            float z = -halfSize + i * spacing;
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(-halfSize, Height, z),
                Vector3.UnitY,
                GridColor));
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(halfSize, Height, z),
                Vector3.UnitY,
                GridColor));

            indices.Add(vertexIndex++);
            indices.Add(vertexIndex++);
        }

        for (int i = 0; i <= gridSize; i++)
        {
            float x = -halfSize + i * spacing;
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(x, Height, -halfSize),
                Vector3.UnitY,
                GridColor));
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(x, Height, halfSize),
                Vector3.UnitY,
                GridColor));

            indices.Add(vertexIndex++);
            indices.Add(vertexIndex++);
        }

        _vertexBuffer = factory.CreateVertexBuffer(vertices.ToArray());
        _indexBuffer = factory.CreateIndexBuffer(indices.ToArray());
        _indexCount = (uint)indices.Count;

        _worldBuffer = factory.CreateUniformBuffer(64);
        _worldBuffer.SetData(Matrix4x4.Identity, 0);
    }

    private int _drawCallCount;
    private readonly ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger<GridRenderer>();

    /// <summary>
    /// Draws the grid using the specified command executor, pipeline, and camera.
    /// Does nothing if <see cref="IsVisible"/> is <c>false</c> or if any required
    /// resource is <c>null</c>.
    /// </summary>
    /// <param name="executor">The command executor used to issue draw commands. May be <c>null</c>.</param>
    /// <param name="pipeline">The rendering pipeline to bind. May be <c>null</c>.</param>
    /// <param name="camera">The orbit camera used for view-projection transforms.</param>
    /// <param name="width">The render target width in pixels.</param>
    /// <param name="height">The render target height in pixels.</param>
    public void Draw(ICommandExecutor? executor, IPipeline? pipeline, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible)
            return;

        if (executor == null || pipeline == null || _vertexBuffer == null || _indexBuffer == null || _worldBuffer == null)
            return;

        if (EnableDiagnostics && _drawCallCount % 60 == 0)
            _logger.LogDebug("[GridRenderer] Drawing grid with {IndexCount} indices", _indexCount);
        _drawCallCount++;

        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(_vertexBuffer, 0);
        executor.SetVertexBuffer(_worldBuffer, 2);
        executor.SetIndexBuffer(_indexBuffer);
        executor.DrawIndexed(1, _indexCount, 1, 0, 0, 0);
    }

    private void DisposeBuffers()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _worldBuffer?.Dispose();
        _vertexBuffer = null;
        _indexBuffer = null;
        _worldBuffer = null;
    }

    /// <summary>
    /// Releases all GPU buffers held by this renderer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeBuffers();
    }
}
