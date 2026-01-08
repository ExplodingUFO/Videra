using Silk.NET.Vulkan;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal class VulkanCommandExecutor : ICommandExecutor
{
    private readonly Device _device;
    private readonly CommandBuffer _commandBuffer;
    private readonly Vk _vk;

    public VulkanCommandExecutor(Device device, CommandBuffer commandBuffer, Vk vk)
    {
        _device = device;
        _commandBuffer = commandBuffer;
        _vk = vk;
    }

    public void SetPipeline(IPipeline pipeline)
    {
        throw new NotImplementedException();
    }

    public void SetVertexBuffer(IBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        throw new NotImplementedException();
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
        throw new NotImplementedException();
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        _vk.CmdDrawIndexed(_commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        _vk.CmdDraw(_commandBuffer, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void SetViewport(float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        var viewport = new Viewport
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            MinDepth = minDepth,
            MaxDepth = maxDepth
        };
        _vk.CmdSetViewport(_commandBuffer, 0, 1, in viewport);
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        var scissor = new Rect2D
        {
            Offset = new Offset2D(x, y),
            Extent = new Extent2D((uint)width, (uint)height)
        };
        _vk.CmdSetScissor(_commandBuffer, 0, 1, in scissor);
    }
}
