using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class AxisRenderer : IDisposable
{
    private IBuffer? _vertexBuffer;
    private IBuffer? _indexBuffer;
    private IBuffer? _worldBuffer;
    private uint _indexCount;
    
    public RgbaFloat XColor { get; set; } = RgbaFloat.Red;
    public RgbaFloat YColor { get; set; } = RgbaFloat.Green;
    public RgbaFloat ZColor { get; set; } = RgbaFloat.Blue;
    public bool IsVisible { get; set; } = true;
    public float AxisLength { get; set; } = 1.0f;

    public void Initialize(IResourceFactory? factory)
    {
        if (factory == null) return;
        
        // 创建3条轴线：X(红)、Y(绿)、Z(蓝)
        var vertices = new List<VertexPositionNormalColor>();
        var indices = new List<uint>();
        
        // X轴 (红色) - 从原点到 (1,0,0)
        vertices.Add(new VertexPositionNormalColor(Vector3.Zero, Vector3.UnitX, XColor));
        vertices.Add(new VertexPositionNormalColor(new Vector3(AxisLength, 0, 0), Vector3.UnitX, XColor));
        indices.Add(0); indices.Add(1);
        
        // Y轴 (绿色) - 从原点到 (0,1,0)
        vertices.Add(new VertexPositionNormalColor(Vector3.Zero, Vector3.UnitY, YColor));
        vertices.Add(new VertexPositionNormalColor(new Vector3(0, AxisLength, 0), Vector3.UnitY, YColor));
        indices.Add(2); indices.Add(3);
        
        // Z轴 (蓝色) - 从原点到 (0,0,1)
        vertices.Add(new VertexPositionNormalColor(Vector3.Zero, Vector3.UnitZ, ZColor));
        vertices.Add(new VertexPositionNormalColor(new Vector3(0, 0, AxisLength), Vector3.UnitZ, ZColor));
        indices.Add(4); indices.Add(5);
        
        _vertexBuffer = factory.CreateVertexBuffer(vertices.ToArray());
        _indexBuffer = factory.CreateIndexBuffer(indices.ToArray());
        _indexCount = (uint)indices.Count;
        
        // 创建world buffer并设置为单位矩阵
        _worldBuffer = factory.CreateUniformBuffer(64);
        _worldBuffer.SetData(Matrix4x4.Identity, 0);
        
        Console.WriteLine($"[AxisRenderer] Initialized with {vertices.Count} vertices, {_indexCount} indices");
    }

    public void Draw(ICommandExecutor? executor, IPipeline? pipeline, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible || executor == null || pipeline == null || _vertexBuffer == null || _indexBuffer == null || _worldBuffer == null)
            return;
        
        // 使用左下角的小视口渲染坐标轴
        float viewportSize = 100f; // 100x100像素
        executor.SetViewport(10, height - viewportSize - 10, viewportSize, viewportSize);
        
        executor.SetPipeline(pipeline);
        executor.SetVertexBuffer(_vertexBuffer, 0);
        executor.SetVertexBuffer(_worldBuffer, 2);
        executor.SetIndexBuffer(_indexBuffer);
        
        // 使用线条模式绘制坐标轴
        executor.DrawIndexed(1, _indexCount, 1, 0, 0, 0);
        
        // 恢复主视口
        executor.SetViewport(0, 0, width, height);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _worldBuffer?.Dispose();
    }
}
