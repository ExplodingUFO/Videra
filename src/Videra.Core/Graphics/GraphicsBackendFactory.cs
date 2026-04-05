using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Graphics.Software;

namespace Videra.Core.Graphics;

/// <summary>
/// 平台后端工厂 - 根据运行时平台创建对应的图形后端
/// </summary>
public static partial class GraphicsBackendFactory
{
    private static IGraphicsBackendResolver? _resolver;

    /// <summary>
    /// Configures the backend resolver used by the current composition layer.
    /// Passing <c>null</c> clears the resolver and leaves only software fallback available.
    /// </summary>
    public static void ConfigureResolver(IGraphicsBackendResolver? resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// 创建当前平台对应的图形后端
    /// </summary>
    public static IGraphicsBackend CreateBackend(GraphicsBackendPreference preference = GraphicsBackendPreference.Auto, ILoggerFactory? loggerFactory = null)
    {
        var logger = loggerFactory?.CreateLogger("GraphicsBackendFactory")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("GraphicsBackendFactory");

        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        Log.PreferenceResolved(logger, preference, backendMode ?? "<null>");
        if (preference == GraphicsBackendPreference.Auto && !string.IsNullOrWhiteSpace(backendMode))
            preference = ParsePreference(backendMode);

        if (preference == GraphicsBackendPreference.Software)
            return new SoftwareBackend();

        var resolvedBackend = _resolver?.CreateBackend(preference, loggerFactory);
        if (resolvedBackend != null)
            return resolvedBackend;

        Log.FallingBackToSoftware(logger, preference);
        return new SoftwareBackend();
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

    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[GraphicsBackendFactory] Preference={Preference}, Env={EnvVar}")]
        public static partial void PreferenceResolved(ILogger logger, GraphicsBackendPreference preference, string envVar);

        [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "[GraphicsBackendFactory] No resolver configured for native backend preference {Preference}; falling back to software.")]
        public static partial void FallingBackToSoftware(ILogger logger, GraphicsBackendPreference preference);
    }
}
