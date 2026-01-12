using Silk.NET.Vulkan;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Platform.Linux;

internal sealed unsafe class VulkanShader : IShader
{
    private readonly Vk _vk;
    private readonly Device _device;
    public ShaderModule ShaderModule { get; }

    public VulkanShader(Vk vk, Device device, ShaderModule shaderModule)
    {
        _vk = vk;
        _device = device;
        ShaderModule = shaderModule;
    }

    public void Dispose()
    {
        _vk.DestroyShaderModule(_device, ShaderModule, null);
    }
}
