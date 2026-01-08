using System.Numerics;
using Videra.Core.Cameras;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public class AxisRenderer : IDisposable
{
    public RgbaFloat XColor { get; set; } = RgbaFloat.Red;
    public RgbaFloat YColor { get; set; } = RgbaFloat.Green;
    public RgbaFloat ZColor { get; set; } = RgbaFloat.Blue;
    public bool IsVisible { get; set; } = true;

    public void Initialize(IResourceFactory? factory)
    {
        // TODO: 实现Axis渲染
        Console.WriteLine("[AxisRenderer] Initialized (simplified version)");
    }

    public void Draw(ICommandExecutor? executor, OrbitCamera camera, uint width, uint height)
    {
        if (!IsVisible || executor == null)
            return;
        
        // TODO: 实现Axis绘制
        // 暂时不绘制，等待Shader系统完善
    }

    public void Dispose()
    {
        // TODO: 清理资源
    }
}
