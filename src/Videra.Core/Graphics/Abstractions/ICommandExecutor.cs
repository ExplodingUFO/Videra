using System.Numerics;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// 命令执行器接口（封装 CommandList/CommandBuffer）。
/// Built-in D3D11/Vulkan/Metal backends share the portable path around direct buffer binding,
/// draw submission, viewport/scissor, clear, and best-effort depth-state toggles. Advanced
/// resource-set binding is not a common portability promise and may throw
/// <see cref="Videra.Core.Exceptions.UnsupportedOperationException"/>.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// 设置当前 Pipeline
    /// </summary>
    void SetPipeline(IPipeline pipeline);
    
    /// <summary>
    /// 设置顶点缓冲区
    /// </summary>
    void SetVertexBuffer(IBuffer buffer, uint index = 0);
    
    /// <summary>
    /// 设置索引缓冲区
    /// </summary>
    void SetIndexBuffer(IBuffer buffer);
    
    /// <summary>
    /// 绑定 Resource Set。
    /// Shipped native backends do not expose this as a common portable capability and may throw
    /// <see cref="Videra.Core.Exceptions.UnsupportedOperationException"/>.
    /// </summary>
    void SetResourceSet(uint slot, IResourceSet resourceSet);
    
    /// <summary>
    /// 绘制索引图元
    /// </summary>
    void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0);
    
    /// <summary>
    /// 绘制索引图元（指定图元类型）
    /// </summary>
    void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0);
    
    /// <summary>
    /// 绘制非索引图元
    /// </summary>
    void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0);
    
    /// <summary>
    /// 设置 Viewport
    /// </summary>
    void SetViewport(float x, float y, float width, float height, float minDepth = 0f, float maxDepth = 1f);
    
    /// <summary>
    /// 设置 Scissor Rect
    /// </summary>
    void SetScissorRect(int x, int y, int width, int height);
    
    /// <summary>
    /// 清除颜色和深度缓冲
    /// </summary>
    void Clear(float r, float g, float b, float a);

    /// <summary>
    /// 设置深度状态。
    /// Backends that cannot mutate depth state dynamically may treat this as a best-effort hint
    /// and rely on their default pipeline/frame depth configuration.
    /// </summary>
    void SetDepthState(bool testEnabled, bool writeEnabled);

    /// <summary>
    /// 重置深度状态为默认。
    /// The portable expectation is to return to the backend's standard frame depth behavior.
    /// </summary>
    void ResetDepthState();
}
