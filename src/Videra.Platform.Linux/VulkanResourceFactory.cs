using Silk.NET.Vulkan;
using Videra.Core.Geometry;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal class VulkanResourceFactory : IResourceFactory
{
    private readonly Device _device;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Vk _vk;

    public VulkanResourceFactory(Device device, PhysicalDevice physicalDevice, Vk vk)
    {
        _device = device;
        _physicalDevice = physicalDevice;
        _vk = vk;
    }

    public IBuffer CreateVertexBuffer(VertexPositionNormalColor[] vertices)
    {
        // TODO: 实现 Vulkan Buffer 创建
        throw new NotImplementedException("Vulkan resource creation will be implemented");
    }

    public IBuffer CreateIndexBuffer(uint[] indices)
    {
        throw new NotImplementedException("Vulkan resource creation will be implemented");
    }

    public IBuffer CreateUniformBuffer(uint sizeInBytes)
    {
        throw new NotImplementedException("Vulkan resource creation will be implemented");
    }

    public IPipeline CreatePipeline(PipelineDescription description)
    {
        throw new NotImplementedException("Vulkan pipeline creation will be implemented");
    }

    public IShader CreateShader(ShaderStage stage, byte[] bytecode, string entryPoint)
    {
        throw new NotImplementedException("Vulkan shader creation will be implemented");
    }

    public IResourceSet CreateResourceSet(ResourceSetDescription description)
    {
        throw new NotImplementedException("Vulkan resource set creation will be implemented");
    }
}
