using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

#if VIDERA_WINDOWS_BACKEND
using Videra.Platform.Windows;
#endif

#if VIDERA_LINUX_BACKEND
using Videra.Platform.Linux;
#endif

#if VIDERA_MACOS_BACKEND
using Videra.Platform.macOS;
#endif

namespace Videra.Avalonia.Composition;

/// <summary>
/// Avalonia-side backend resolver. Composition belongs here rather than in Videra.Core.
/// </summary>
public sealed class AvaloniaGraphicsBackendResolver : IGraphicsBackendResolver
{
    private static readonly AvaloniaGraphicsBackendResolver Instance = new();
    private static bool _registered;

    public IGraphicsBackend? CreateBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null)
    {
        return preference switch
        {
            GraphicsBackendPreference.D3D11 => TryCreateD3D11(loggerFactory),
            GraphicsBackendPreference.Vulkan => TryCreateVulkan(loggerFactory),
            GraphicsBackendPreference.Metal => TryCreateMetal(loggerFactory),
            _ => TryCreatePlatformDefault(loggerFactory)
        };
    }

    internal static void EnsureRegistered()
    {
        if (_registered)
            return;

        GraphicsBackendFactory.ConfigureResolver(Instance);
        _registered = true;
    }

    private static IGraphicsBackend? TryCreatePlatformDefault(ILoggerFactory? loggerFactory)
    {
        if (OperatingSystem.IsWindows())
            return TryCreateD3D11(loggerFactory);
        if (OperatingSystem.IsLinux())
            return TryCreateVulkan(loggerFactory);
        if (OperatingSystem.IsMacOS())
            return TryCreateMetal(loggerFactory);

        return null;
    }

    private static IGraphicsBackend? TryCreateD3D11(ILoggerFactory? loggerFactory)
    {
#if VIDERA_WINDOWS_BACKEND
        if (OperatingSystem.IsWindows())
            return new D3D11Backend();
#endif
        loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver")
            .LogWarning("D3D11 backend is not available in this build/runtime.");
        return null;
    }

    private static IGraphicsBackend? TryCreateVulkan(ILoggerFactory? loggerFactory)
    {
#if VIDERA_LINUX_BACKEND
        if (OperatingSystem.IsLinux())
            return new VulkanBackend();
#endif
        loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver")
            .LogWarning("Vulkan backend is not available in this build/runtime.");
        return null;
    }

    private static IGraphicsBackend? TryCreateMetal(ILoggerFactory? loggerFactory)
    {
#if VIDERA_MACOS_BACKEND
        if (OperatingSystem.IsMacOS())
            return new MetalBackend();
#endif
        loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver")
            .LogWarning("Metal backend is not available in this build/runtime.");
        return null;
    }
}
