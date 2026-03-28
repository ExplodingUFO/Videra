using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;

namespace Videra.Core.Graphics;

/// <summary>
/// 平台后端工厂 - 根据运行时平台创建对应的图形后端
/// </summary>
public static class GraphicsBackendFactory
{
    /// <summary>
    /// 创建当前平台对应的图形后端
    /// </summary>
    public static IGraphicsBackend CreateBackend(GraphicsBackendPreference preference = GraphicsBackendPreference.Auto, ILoggerFactory? loggerFactory = null)
    {
        var logger = loggerFactory?.CreateLogger("GraphicsBackendFactory")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("GraphicsBackendFactory");

        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        logger.LogInformation("[GraphicsBackendFactory] Preference={Preference}, Env={EnvVar}", preference, backendMode ?? "<null>");
        if (preference == GraphicsBackendPreference.Auto && !string.IsNullOrWhiteSpace(backendMode))
            preference = ParsePreference(backendMode);

        if (preference == GraphicsBackendPreference.Software)
            return new SoftwareBackend();

        return preference switch
        {
            GraphicsBackendPreference.D3D11 => TryCreateD3D11(logger) ?? new SoftwareBackend(),
            GraphicsBackendPreference.Vulkan => TryCreateVulkan(logger) ?? new SoftwareBackend(),
            GraphicsBackendPreference.Metal => TryCreateMetal(logger) ?? new SoftwareBackend(),
            _ => TryCreatePlatformDefault(logger) ?? new SoftwareBackend()
        };
    }

    /// <summary>
    /// 获取当前平台名称
    /// </summary>
    public static string GetPlatformName()
    {
        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        if (string.IsNullOrWhiteSpace(backendMode) ||
            string.Equals(backendMode, "software", StringComparison.OrdinalIgnoreCase))
        {
            return "Software (CPU)";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows (Direct3D 11)";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macOS (Metal)";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux (Vulkan)";

        return "Unknown Platform";
    }

    private static GraphicsBackendPreference ParsePreference(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "software" => GraphicsBackendPreference.Software,
            "d3d" => GraphicsBackendPreference.D3D11,
            "d3d11" => GraphicsBackendPreference.D3D11,
            "vulkan" => GraphicsBackendPreference.Vulkan,
            "vk" => GraphicsBackendPreference.Vulkan,
            "metal" => GraphicsBackendPreference.Metal,
            "native" => GraphicsBackendPreference.Auto,
            "auto" => GraphicsBackendPreference.Auto,
            _ => GraphicsBackendPreference.Auto
        };
    }

    private static IGraphicsBackend? TryCreatePlatformDefault(ILogger logger)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return TryCreateD3D11(logger);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return TryCreateMetal(logger);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return TryCreateVulkan(logger);

        return null;
    }

    private static IGraphicsBackend? TryCreateD3D11(ILogger logger)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            logger.LogWarning("[Videra] D3D11 backend only supported on Windows.");
            return null;
        }

        try
        {
            var windowsAssembly = System.Reflection.Assembly.Load("Videra.Platform.Windows");
            var backendType = windowsAssembly.GetType("Videra.Platform.Windows.D3D11Backend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Videra] D3D11 backend load failed: {Error}", ex.Message);
            return null;
        }
    }

    private static IGraphicsBackend? TryCreateMetal(ILogger logger)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            logger.LogWarning("[Videra] Metal backend only supported on macOS.");
            return null;
        }

        try
        {
            var macOSAssembly = System.Reflection.Assembly.Load("Videra.Platform.macOS");
            var backendType = macOSAssembly.GetType("Videra.Platform.macOS.MetalBackend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Videra] Metal backend load failed: {Error}", ex.Message);
            return null;
        }
    }

    private static IGraphicsBackend? TryCreateVulkan(ILogger logger)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            logger.LogWarning("[Videra] Vulkan backend is only wired for Linux/X11 right now.");
            return null;
        }

        try
        {
            var linuxAssembly = System.Reflection.Assembly.Load("Videra.Platform.Linux");
            var backendType = linuxAssembly.GetType("Videra.Platform.Linux.VulkanBackend");
            return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Videra] Vulkan backend load failed: {Error}", ex.Message);
            return null;
        }
    }
}
