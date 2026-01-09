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
    public static IGraphicsBackend CreateBackend()
    {
        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        var preferNative = string.Equals(backendMode, "native", StringComparison.OrdinalIgnoreCase);
        var preferSoftware = string.IsNullOrWhiteSpace(backendMode) ||
                             string.Equals(backendMode, "software", StringComparison.OrdinalIgnoreCase);

        if (preferSoftware)
            return new SoftwareBackend();

        if (preferNative)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var windowsAssembly = System.Reflection.Assembly.Load("Videra.Platform.Windows");
                    var backendType = windowsAssembly.GetType("Videra.Platform.Windows.D3D11Backend");
                    return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    var macOSAssembly = System.Reflection.Assembly.Load("Videra.Platform.macOS");
                    var backendType = macOSAssembly.GetType("Videra.Platform.macOS.MetalBackend");
                    return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var linuxAssembly = System.Reflection.Assembly.Load("Videra.Platform.Linux");
                    var backendType = linuxAssembly.GetType("Videra.Platform.Linux.VulkanBackend");
                    return (IGraphicsBackend)Activator.CreateInstance(backendType!)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Videra] Native backend load failed, falling back to software: {ex.Message}");
            }
        }

        return new SoftwareBackend();
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
}
