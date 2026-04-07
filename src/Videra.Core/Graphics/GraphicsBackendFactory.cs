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
        return ResolveBackend(new GraphicsBackendRequest(
            preference,
            BackendEnvironmentOverrideMode.PreferOverrides,
            AllowSoftwareFallback: true,
            LoggerFactory: loggerFactory)).Backend;
    }

    public static GraphicsBackendResolution ResolveBackend(GraphicsBackendRequest request)
    {
        var logger = request.LoggerFactory?.CreateLogger("GraphicsBackendFactory")
            ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("GraphicsBackendFactory");

        var backendMode = Environment.GetEnvironmentVariable("VIDERA_BACKEND");
        var preference = ResolveRequestedPreference(request, backendMode);
        var environmentOverrideApplied = preference != request.RequestedPreference;

        Log.PreferenceResolved(logger, request.RequestedPreference, backendMode ?? "<null>");

        if (preference == GraphicsBackendPreference.Software)
        {
            return new GraphicsBackendResolution(
                new SoftwareBackend(),
                request.RequestedPreference,
                GraphicsBackendPreference.Software,
                environmentOverrideApplied: environmentOverrideApplied);
        }

        var resolvedBackend = _resolver?.CreateBackend(preference, request.LoggerFactory);
        if (resolvedBackend != null)
        {
            return new GraphicsBackendResolution(
                resolvedBackend,
                request.RequestedPreference,
                preference,
                environmentOverrideApplied: environmentOverrideApplied);
        }

        var fallbackReason = $"No native resolver configured for backend preference {preference}.";
        if (!request.AllowSoftwareFallback)
        {
            throw new InvalidOperationException(fallbackReason);
        }

        Log.FallingBackToSoftware(logger, preference);
        return new GraphicsBackendResolution(
            new SoftwareBackend(),
            request.RequestedPreference,
            GraphicsBackendPreference.Software,
            environmentOverrideApplied: environmentOverrideApplied,
            isUsingSoftwareFallback: true,
            fallbackReason: fallbackReason);
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

    public static GraphicsBackendPreference ResolveRequestedPreference(GraphicsBackendRequest request)
    {
        return ResolveRequestedPreference(request, Environment.GetEnvironmentVariable("VIDERA_BACKEND"));
    }

    public static GraphicsBackendPreference ParsePreference(string value)
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

    private static GraphicsBackendPreference ResolveRequestedPreference(GraphicsBackendRequest request, string? backendMode)
    {
        if (string.IsNullOrWhiteSpace(backendMode))
        {
            return request.RequestedPreference;
        }

        var envPreference = ParsePreference(backendMode);
        if (envPreference == GraphicsBackendPreference.Auto &&
            !string.Equals(backendMode.Trim(), "auto", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(backendMode.Trim(), "native", StringComparison.OrdinalIgnoreCase))
        {
            return request.RequestedPreference;
        }

        if (!ShouldApplyEnvironmentOverride(request.RequestedPreference, envPreference, request.EnvironmentOverrideMode))
        {
            return request.RequestedPreference;
        }

        return envPreference;
    }

    private static bool ShouldApplyEnvironmentOverride(
        GraphicsBackendPreference requestedPreference,
        GraphicsBackendPreference envPreference,
        BackendEnvironmentOverrideMode overrideMode)
    {
        return overrideMode switch
        {
            BackendEnvironmentOverrideMode.Disabled => false,
            BackendEnvironmentOverrideMode.AllowOverrides => requestedPreference == GraphicsBackendPreference.Auto,
            BackendEnvironmentOverrideMode.PreferOverrides => envPreference != requestedPreference,
            _ => false
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
