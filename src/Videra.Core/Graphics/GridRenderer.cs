using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class GridRenderer : IDisposable
{
    private IResourceFactory? _factory;
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _worldBuffer;
    private uint _indexCount;

    public bool IsVisible { get; set; } = true;
    public float Height { get; set; } = 0.0f;
    public RgbaFloat GridColor { get; set; } = new(0.4f, 0.4f, 0.4f, 0.5f);
    public bool EnableDiagnostics { get; set; } = false;

    public void Initialize(IResourceFactory? factory)
    {
        _factory = factory;
        Rebuild(factory);
    }

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

    public void Draw(ICommandExecutor? executor, IPipeline? pipeline, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible)
            return;

        if (executor == null || pipeline == null || _vertexBuffer == null || _indexBuffer == null || _worldBuffer == null)
            return;

        if (EnableDiagnostics && _drawCallCount % 60 == 0)
            Console.WriteLine($"[GridRenderer] Drawing grid with {_indexCount} indices");
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

    public void Dispose()
    {
        DisposeBuffers();
    }
}
