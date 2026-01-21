using System.Numerics;

namespace Videra.Core.Graphics.Abstractions;

/// <summary>
/// 命令执行器接口（封装 CommandList/CommandBuffer）
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
    /// 绑定 Resource Set
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
    /// 设置深度状态
    /// </summary>
    void SetDepthState(bool testEnabled, bool writeEnabled);

    /// <summary>
    /// 重置深度状态为默认
    /// </summary>
    void ResetDepthState();
}
