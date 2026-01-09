using System.Runtime.InteropServices;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;

namespace Videra.Core.Graphics;

/// <summary>
/// å¹³å°åŽç«¯å·¥åŽ‚ - æ ¹æ®è¿è¡Œæ—¶å¹³å°åˆ›å»ºå¯¹åº”çš„å›¾å½¢åŽç«¯
/// </summary>
public static class GraphicsBackendFactory
{
    /// <summary>
    /// åˆ›å»ºå½“å‰å¹³å°å¯¹åº”çš„å›¾å½¢åŽç«?
    /// </summary>
    public static IGraphicsBackend CreateBackend(GraphicsBackendPreference preference = GraphicsBackendPreference.Auto)
    {
        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        Console.WriteLine($"[GraphicsBackendFactory] Preference={preference}, Env={backendMode ?? "<null>"}");
        if (preference == GraphicsBackendPreference.Auto && !string.IsNullOrWhiteSpace(backendMode))
            preference = ParsePreference(backendMode);

        if (preference == GraphicsBackendPreference.Software)
            return new SoftwareBackend();

        return preference switch
        {
            GraphicsBackendPreference.D3D11 => TryCreateD3D11() ?? new SoftwareBackend(),
            GraphicsBackendPreference.Vulkan => TryCreateVulkan() ?? new SoftwareBackend(),
            GraphicsBackendPreference.Metal => TryCreateMetal() ?? new SoftwareBackend(),
            _ => TryCreatePlatformDefault() ?? new SoftwareBackend()
        };
    }

    /// <summary>
    /// èŽ·å–å½“å‰å¹³å°åç§°
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

    private static IGraphicsBackend? TryCreatePlatformDefault()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return TryCreateD3D11();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return TryCreateMetal();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return TryCreateVulkan();

        return null;
    }

    private static IGraphicsBackend? TryCreateD3D11()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("[Videra] D3D11 backend only supported on Windows.");
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
            Console.WriteLine($"[Videra] D3D11 backend load failed: {ex.Message}");
            return null;
        }
    }

    private static IGraphicsBackend? TryCreateMetal()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("[Videra] Metal backend only supported on macOS.");
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
            Console.WriteLine($"[Videra] Metal backend load failed: {ex.Message}");
            return null;
        }
    }

    private static IGraphicsBackend? TryCreateVulkan()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("[Videra] Vulkan backend is only wired for Linux/X11 right now.");
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
            Console.WriteLine($"[Videra] Vulkan backend load failed: {ex.Message}");
            return null;
        }
    }
}
