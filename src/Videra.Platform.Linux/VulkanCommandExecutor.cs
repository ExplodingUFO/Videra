using Silk.NET.Vulkan;
using Videra.Core.Exceptions;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal sealed unsafe class VulkanCommandExecutor : ICommandExecutor
{
    private readonly Device _device;
    private readonly CommandBuffer _commandBuffer;
    private readonly Vk _vk;
    private VulkanPipeline? _pipeline;
    private VulkanBuffer? _cameraBuffer;
    private VulkanBuffer? _worldBuffer;

    public VulkanCommandExecutor(Device device, CommandBuffer commandBuffer, Vk vk)
    {
        _device = device;
        _commandBuffer = commandBuffer;
        _vk = vk;
    }

    public void SetPipeline(IPipeline pipeline)
    {
        if (pipeline is not VulkanPipeline vkPipeline)
            throw new ArgumentException("Pipeline must be a VulkanPipeline");

        _pipeline = vkPipeline;
        BindPipeline(0);
        BindDescriptorSet();
    }

    public void SetVertexBuffer(IBuffer buffer, uint index = 0)
    {
        if (buffer is not VulkanBuffer vkBuffer)
            throw new ArgumentException("Buffer must be a VulkanBuffer");

        if (index == 0)
        {
            var nativeBuffer = vkBuffer.NativeBuffer;
            ulong offset = 0;
            _vk.CmdBindVertexBuffers(_commandBuffer, 0, 1, &nativeBuffer, &offset);
            return;
        }

        if (index == 1)
            _cameraBuffer = vkBuffer;
        else if (index == 2)
            _worldBuffer = vkBuffer;

        UpdateDescriptorSet();
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        if (buffer is not VulkanBuffer vkBuffer)
            throw new ArgumentException("Buffer must be a VulkanBuffer");

        _vk.CmdBindIndexBuffer(_commandBuffer, vkBuffer.NativeBuffer, 0, IndexType.Uint32);
    }

    public void SetResourceSet(uint slot, IResourceSet resourceSet)
    {
        throw new UnsupportedOperationException(
            "Resource sets are managed directly through descriptor set binding in the Vulkan backend.",
            "SetResourceSet",
            "Linux");
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        _vk.CmdDrawIndexed(_commandBuffer, indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void DrawIndexed(uint primitiveType, uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        BindPipeline(primitiveType);
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

    public void Clear(float r, float g, float b, float a)
    {
    }

    public void SetDepthState(bool testEnabled, bool writeEnabled)
    {
        // Vulkan depth state is managed through pipeline state objects
        // Dynamic depth state would require VK_DYNAMIC_STATE_DEPTH_TEST_ENABLE extension
    }

    public void ResetDepthState()
    {
        // No-op for Vulkan - depth state is managed through pipeline state
    }

    private void BindPipeline(uint primitiveType)
    {
        if (_pipeline == null)
            return;

        var pipeline = _pipeline.GetPipeline(primitiveType);
        _vk.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, pipeline);
        BindDescriptorSet();
    }

    private void BindDescriptorSet()
    {
        if (_pipeline == null)
            return;

        var descriptorSet = _pipeline.DescriptorSet;
        _vk.CmdBindDescriptorSets(_commandBuffer, PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, 1, &descriptorSet, 0, null);
    }

    private void UpdateDescriptorSet()
    {
        if (_pipeline == null || _cameraBuffer == null || _worldBuffer == null)
            return;

        var cameraInfo = new DescriptorBufferInfo
        {
            Buffer = _cameraBuffer.NativeBuffer,
            Offset = 0,
            Range = _cameraBuffer.SizeInBytes
        };

        var worldInfo = new DescriptorBufferInfo
        {
            Buffer = _worldBuffer.NativeBuffer,
            Offset = 0,
            Range = _worldBuffer.SizeInBytes
        };

        var writes = stackalloc WriteDescriptorSet[2];
        writes[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _pipeline.DescriptorSet,
            DstBinding = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            PBufferInfo = &cameraInfo
        };
        writes[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _pipeline.DescriptorSet,
            DstBinding = 1,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            PBufferInfo = &worldInfo
        };

        _vk.UpdateDescriptorSets(_device, 2, writes, 0, null);
    }
}
