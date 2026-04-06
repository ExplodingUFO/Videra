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
public sealed partial class AvaloniaGraphicsBackendResolver : IGraphicsBackendResolver
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
        var logger = loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver");
        if (logger != null)
            Log.D3D11Unavailable(logger);
        return null;
    }

    private static IGraphicsBackend? TryCreateVulkan(ILoggerFactory? loggerFactory)
    {
#if VIDERA_LINUX_BACKEND
        if (OperatingSystem.IsLinux())
            return new VulkanBackend();
#endif
        var logger = loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver");
        if (logger != null)
            Log.VulkanUnavailable(logger);
        return null;
    }

    private static IGraphicsBackend? TryCreateMetal(ILoggerFactory? loggerFactory)
    {
#if VIDERA_MACOS_BACKEND
        if (OperatingSystem.IsMacOS())
            return new MetalBackend();
#endif
        var logger = loggerFactory?.CreateLogger("AvaloniaGraphicsBackendResolver");
        if (logger != null)
            Log.MetalUnavailable(logger);
        return null;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "D3D11 backend is not available in this build/runtime.")]
        public static partial void D3D11Unavailable(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Vulkan backend is not available in this build/runtime.")]
        public static partial void VulkanUnavailable(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Metal backend is not available in this build/runtime.")]
        public static partial void MetalUnavailable(ILogger logger);
    }
}
