using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class GridRenderer : IDisposable
{
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _worldBuffer; // 添加world buffer
    private uint _indexCount;
    
    // 测试用：简单三角形
    private IBuffer? _testVertexBuffer;
    private IBuffer? _testIndexBuffer;
    private bool _useTestTriangle = true; // 开启测试模式
    
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
        
        // 创建world buffer并设置为单位矩阵
        _worldBuffer = factory.CreateUniformBuffer(64);
        _worldBuffer.SetData(Matrix4x4.Identity, 0);
        
        // 创建测试三角形：直接使用NDC坐标 (-1到1)
        // 这个三角形不需要MVP变换，应该能直接显示
        var testVertices = new VertexPositionNormalColor[]
        {
            new(new Vector3(-0.5f, -0.5f, 0), Vector3.UnitZ, new RgbaFloat(1, 0, 0, 1)), // 左下红色
            new(new Vector3(0.5f, -0.5f, 0), Vector3.UnitZ, new RgbaFloat(0, 1, 0, 1)),  // 右下绿色
            new(new Vector3(0, 0.5f, 0), Vector3.UnitZ, new RgbaFloat(0, 0, 1, 1))      // 上蓝色
        };
        var testIndices = new uint[] { 0, 1, 2 };
        
        _testVertexBuffer = factory.CreateVertexBuffer(testVertices);
        _testIndexBuffer = factory.CreateIndexBuffer(testIndices);
        
        Console.WriteLine($"[GridRenderer] Initialized with {vertices.Count} vertices, {_indexCount} indices");
        Console.WriteLine($"[GridRenderer] Test triangle created (NDC coordinates)");
    }

    private int _drawCallCount = 0;
    
    public void Draw(ICommandExecutor? executor, IPipeline? pipeline, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible)
        {
            if (_drawCallCount % 60 == 0)
                Console.WriteLine("[GridRenderer] Skipped - not visible");
            _drawCallCount++;
            return;
        }
        
        if (executor == null || pipeline == null || _vertexBuffer == null || _indexBuffer == null || _worldBuffer == null)
        {
            if (_drawCallCount % 60 == 0)
                Console.WriteLine($"[GridRenderer] Skipped - missing resources: executor={executor != null}, pipeline={pipeline != null}, vb={_vertexBuffer != null}, ib={_indexBuffer != null}, wb={_worldBuffer != null}");
            _drawCallCount++;
            return;
        }
        
        if (_drawCallCount % 60 == 0)
            Console.WriteLine($"[GridRenderer] Drawing grid with {_indexCount} indices");
        _drawCallCount++;
        
        // 如果测试模式开启，绘制测试三角形
        if (_useTestTriangle && _testVertexBuffer != null && _testIndexBuffer != null)
        {
            if (_drawCallCount % 60 == 0)
                Console.WriteLine("[GridRenderer] Drawing TEST TRIANGLE (NDC coordinates, no MVP)");
            
            executor.SetPipeline(pipeline);
            executor.SetVertexBuffer(_testVertexBuffer, 0);
            executor.SetVertexBuffer(_worldBuffer, 2); // 单位矩阵
            executor.SetIndexBuffer(_testIndexBuffer);
            
            // 使用三角形模式绘制测试
            executor.DrawIndexed(3, 3, 1, 0, 0, 0); // primitiveType=3 (triangle)
            return; // 暂时只绘制测试三角形
        }
        
        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(_vertexBuffer, 0); // 顶点数据在索引 0
        executor.SetVertexBuffer(_worldBuffer, 2);  // World矩阵在索引 2（单位矩阵）
        executor.SetIndexBuffer(_indexBuffer);
        
        // 使用线条模式绘制网格（MTLPrimitiveTypeLine = 1）
        executor.DrawIndexed(1, _indexCount, 1, 0, 0, 0);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _worldBuffer?.Dispose();
    }
}
