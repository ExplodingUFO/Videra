using System.Numerics;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// 跨平台图形后端抽象接口
/// </summary>
public interface IGraphicsBackend : IDisposable
{
    bool IsInitialized { get; }
    
    /// <summary>
    /// 初始化图形后端（D3D11/Metal/Vulkan）
    /// </summary>
    void Initialize(IntPtr windowHandle, int width, int height);
    
    /// <summary>
    /// 调整 Swapchain 大小
    /// </summary>
    void Resize(int width, int height);
    
    /// <summary>
    /// 开始渲染一帧
    /// </summary>
    void BeginFrame();
    
    /// <summary>
    /// 结束渲染并呈现
    /// </summary>
    void EndFrame();
    
    /// <summary>
    /// 设置清屏颜色
    /// </summary>
    void SetClearColor(Vector4 color);
    
    /// <summary>
    /// 创建资源工厂
    /// </summary>
    IResourceFactory GetResourceFactory();
    
    /// <summary>
    /// 获取命令执行器
    /// </summary>
    ICommandExecutor GetCommandExecutor();
}
