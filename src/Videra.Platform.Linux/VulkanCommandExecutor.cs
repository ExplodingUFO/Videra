using Silk.NET.Vulkan;
using Videra.Core.Exceptions;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal sealed unsafe class VulkanCommandExecutor : ICommandExecutor, IBufferBindingCacheInvalidator
{
    private readonly Device _device;
    private readonly CommandBuffer _commandBuffer;
    private readonly Vk _vk;
    private readonly Dictionary<VulkanBuffer, DescriptorSet> _surfaceScalarDescriptorSets = [];
    private VulkanPipeline? _pipeline;
    private VulkanBuffer? _cameraBuffer;
    private VulkanBuffer? _worldBuffer;
    private VulkanBuffer? _colorMapBuffer;
    private VulkanBuffer? _surfaceTileScalarBuffer;
    private DescriptorSet _currentDescriptorSet;

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

        if (!ReferenceEquals(_pipeline, vkPipeline))
        {
            if (_pipeline is not null)
            {
                ReleaseCachedDescriptorSets(_pipeline);
            }

            _surfaceScalarDescriptorSets.Clear();
        }

        _pipeline = vkPipeline;
        _currentDescriptorSet = _pipeline.DescriptorSet;
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

        if (index == RenderBindingSlots.Camera)
            _cameraBuffer = vkBuffer;
        else if (index == RenderBindingSlots.World)
            _worldBuffer = vkBuffer;
        else if (index == RenderBindingSlots.SurfaceColorMap)
            _colorMapBuffer = vkBuffer;
        else if (index == RenderBindingSlots.SurfaceTileScalars)
            _surfaceTileScalarBuffer = vkBuffer;

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

    public void ReleaseBuffer(IBuffer buffer)
    {
        if (buffer is not VulkanBuffer vkBuffer)
        {
            return;
        }

        if (ReferenceEquals(_surfaceTileScalarBuffer, vkBuffer))
        {
            _surfaceTileScalarBuffer = null;
        }

        if (_pipeline is null)
        {
            return;
        }

        if (!_surfaceScalarDescriptorSets.Remove(vkBuffer, out var descriptorSet))
        {
            return;
        }

        _ = _vk.FreeDescriptorSets(_device, _pipeline.DescriptorPool, 1, in descriptorSet);
        if (_currentDescriptorSet.Handle == descriptorSet.Handle)
        {
            _currentDescriptorSet = default;
        }
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

        var descriptorSet = _currentDescriptorSet.Handle != 0 ? _currentDescriptorSet : _pipeline.DescriptorSet;
        _vk.CmdBindDescriptorSets(_commandBuffer, PipelineBindPoint.Graphics, _pipeline.PipelineLayout, 0, 1, &descriptorSet, 0, null);
    }

    private void UpdateDescriptorSet()
    {
        if (_pipeline == null || _cameraBuffer == null || _worldBuffer == null)
            return;

        var descriptorWriteCount = 2u;
        if (_pipeline.UsesSurfaceChartScalarBindings)
        {
            if (_colorMapBuffer == null || _surfaceTileScalarBuffer == null)
                return;

            descriptorWriteCount = 4u;
            _currentDescriptorSet = GetOrCreateSurfaceScalarDescriptorSet(_surfaceTileScalarBuffer);
        }
        else
        {
            _currentDescriptorSet = _pipeline.DescriptorSet;
        }

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

        var writes = stackalloc WriteDescriptorSet[(int)descriptorWriteCount];
        writes[0] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _currentDescriptorSet,
            DstBinding = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            PBufferInfo = &cameraInfo
        };
        writes[1] = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = _currentDescriptorSet,
            DstBinding = 1,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            PBufferInfo = &worldInfo
        };

        if (_pipeline.UsesSurfaceChartScalarBindings)
        {
            var colorMapInfo = new DescriptorBufferInfo
            {
                Buffer = _colorMapBuffer!.NativeBuffer,
                Offset = 0,
                Range = _colorMapBuffer.SizeInBytes
            };
            var tileScalarInfo = new DescriptorBufferInfo
            {
                Buffer = _surfaceTileScalarBuffer!.NativeBuffer,
                Offset = 0,
                Range = _surfaceTileScalarBuffer.SizeInBytes
            };

            writes[2] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = _currentDescriptorSet,
                DstBinding = 2,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &colorMapInfo
            };
            writes[3] = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = _currentDescriptorSet,
                DstBinding = 3,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.UniformBuffer,
                PBufferInfo = &tileScalarInfo
            };
        }

        _vk.UpdateDescriptorSets(_device, descriptorWriteCount, writes, 0, null);
        BindDescriptorSet();
    }

    private DescriptorSet GetOrCreateSurfaceScalarDescriptorSet(VulkanBuffer scalarBuffer)
    {
        if (_pipeline is null)
            throw new InvalidOperationException("A Vulkan pipeline must be bound before allocating descriptor sets.");

        if (_surfaceScalarDescriptorSets.TryGetValue(scalarBuffer, out var descriptorSet))
        {
            return descriptorSet;
        }

        var descriptorSetLayout = _pipeline.DescriptorSetLayout;
        var allocateInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _pipeline.DescriptorPool,
            DescriptorSetCount = 1,
            PSetLayouts = &descriptorSetLayout
        };

        if (_vk.AllocateDescriptorSets(_device, in allocateInfo, out descriptorSet) != Result.Success)
        {
            throw new ResourceCreationException(
                "Failed to allocate a per-tile Vulkan descriptor set for the SurfaceCharts scalar recolor path.",
                "UpdateDescriptorSet");
        }

        _surfaceScalarDescriptorSets[scalarBuffer] = descriptorSet;
        return descriptorSet;
    }

    private void ReleaseCachedDescriptorSets(VulkanPipeline pipeline)
    {
        foreach (var descriptorSet in _surfaceScalarDescriptorSets.Values)
        {
            _ = _vk.FreeDescriptorSets(_device, pipeline.DescriptorPool, 1, in descriptorSet);
        }
    }
}
