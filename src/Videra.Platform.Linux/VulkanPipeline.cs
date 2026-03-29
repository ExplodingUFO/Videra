using Silk.NET.Vulkan;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal sealed unsafe class VulkanPipeline : IPipeline
{
    private readonly Vk _vk;
    private readonly Device _device;
    private bool _disposed;
    public Pipeline TrianglePipeline { get; }
    public Pipeline LinePipeline { get; }
    public Pipeline PointPipeline { get; }
    public PipelineLayout PipelineLayout { get; }
    public DescriptorSetLayout DescriptorSetLayout { get; }
    public DescriptorPool DescriptorPool { get; }
    public DescriptorSet DescriptorSet { get; }

    public VulkanPipeline(
        Vk vk,
        Device device,
        Pipeline trianglePipeline,
        Pipeline linePipeline,
        Pipeline pointPipeline,
        PipelineLayout pipelineLayout,
        DescriptorSetLayout descriptorSetLayout,
        DescriptorPool descriptorPool,
        DescriptorSet descriptorSet)
    {
        _vk = vk;
        _device = device;
        TrianglePipeline = trianglePipeline;
        LinePipeline = linePipeline;
        PointPipeline = pointPipeline;
        PipelineLayout = pipelineLayout;
        DescriptorSetLayout = descriptorSetLayout;
        DescriptorPool = descriptorPool;
        DescriptorSet = descriptorSet;
    }

    public Pipeline GetPipeline(uint primitiveType)
    {
        return primitiveType switch
        {
            1 => LinePipeline,
            2 => PointPipeline,
            _ => TrianglePipeline
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _vk.DestroyPipeline(_device, TrianglePipeline, null);
        _vk.DestroyPipeline(_device, LinePipeline, null);
        _vk.DestroyPipeline(_device, PointPipeline, null);
        _vk.DestroyPipelineLayout(_device, PipelineLayout, null);
        _vk.DestroyDescriptorSetLayout(_device, DescriptorSetLayout, null);
        _vk.DestroyDescriptorPool(_device, DescriptorPool, null);
    }
}
