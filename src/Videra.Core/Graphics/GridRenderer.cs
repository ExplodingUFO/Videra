using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class GridRenderer : IDisposable
{
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private uint _indexCount;
    
    public bool IsVisible { get; set; } = true;
    public float Height { get; set; } = 0.0f;
    public RgbaFloat GridColor { get; set; } = new(0.4f, 0.4f, 0.4f, 0.5f);

    public void Initialize(IResourceFactory? factory)
    {
        if (factory == null) return;
        
        // 创建简单的网格 (10x10)
        const int gridSize = 10;
        const float spacing = 1.0f;
        const float halfSize = gridSize * spacing * 0.5f;
        
        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();
        uint vertexIndex = 0;
        
        // 水平线
        for (int i = 0; i <= gridSize; i++)
        {
            float z = -halfSize + i * spacing;
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(-halfSize, Height, z),
                Vector3.UnitY,
                new RgbaFloat(GridColor.R, GridColor.G, GridColor.B, GridColor.A)
            ));
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(halfSize, Height, z),
                Vector3.UnitY,
                new RgbaFloat(GridColor.R, GridColor.G, GridColor.B, GridColor.A)
            ));
            
            indices.Add(vertexIndex++);
            indices.Add(vertexIndex++);
        }
        
        // 垂直线
        for (int i = 0; i <= gridSize; i++)
        {
            float x = -halfSize + i * spacing;
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(x, Height, -halfSize),
                Vector3.UnitY,
                new RgbaFloat(GridColor.R, GridColor.G, GridColor.B, GridColor.A)
            ));
            vertices.Add(new VertexPositionNormalColor(
                new Vector3(x, Height, halfSize),
                Vector3.UnitY,
                new RgbaFloat(GridColor.R, GridColor.G, GridColor.B, GridColor.A)
            ));
            
            indices.Add(vertexIndex++);
            indices.Add(vertexIndex++);
        }
        
        _vertexBuffer = factory.CreateVertexBuffer(vertices.ToArray());
        _indexBuffer = factory.CreateIndexBuffer(indices.ToArray());
        _indexCount = (uint)indices.Count;
        
        Console.WriteLine($"[GridRenderer] Initialized with {vertices.Count} vertices, {_indexCount} indices");
    }

    public void Draw(ICommandExecutor? executor, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible || executor == null || _vertexBuffer == null || _indexBuffer == null)
            return;
        
        executor.SetVertexBuffer(_vertexBuffer);
        executor.SetIndexBuffer(_indexBuffer);
        
        // 使用线条模式绘制网格
        // 注意：Metal需要特殊处理线条，这里先用三角形模式
        executor.DrawIndexed(_indexCount, 1, 0, 0, 0);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }
}
