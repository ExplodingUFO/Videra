using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

/// <summary>
/// 平台后端工厂 - 根据运行时平台创建对应的图形后端
/// </summary>
public static class GraphicsBackendFactory
{
    /// <summary>
    /// 创建当前平台对应的图形后端
    /// </summary>
    public static IGraphicsBackend CreateBackend()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 动态加载 Windows D3D11 后端
            var windowsAssembly = System.Reflection.Assembly.Load("Videra.Platform.Windows");
            var backendType = windowsAssembly.GetType("Videra.Platform.Windows.D3D11Backend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // 动态加载 macOS Metal 后端
            var macOSAssembly = System.Reflection.Assembly.Load("Videra.Platform.macOS");
            var backendType = macOSAssembly.GetType("Videra.Platform.macOS.MetalBackend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // 动态加载 Linux Vulkan 后端
            var linuxAssembly = System.Reflection.Assembly.Load("Videra.Platform.Linux");
            var backendType = linuxAssembly.GetType("Videra.Platform.Linux.VulkanBackend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
        }
    }

    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    public static string GetPlatformName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows (Direct3D 11)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS (Metal)";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux (Vulkan)";
        else
            return "Unknown Platform";
    }
}
