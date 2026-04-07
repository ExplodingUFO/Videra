using Microsoft.Extensions.Logging;
using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Avalonia.Composition;

internal static class AvaloniaRuntimeBackendDiscovery
{
    private sealed record BackendRegistration(
        GraphicsBackendPreference Preference,
        string AssemblyName,
        string TypeName,
        Func<bool> PlatformPredicate,
        string PlatformLabel);

    private static readonly BackendRegistration[] Registrations =
    [
        new(GraphicsBackendPreference.D3D11, "Videra.Platform.Windows", "Videra.Platform.Windows.D3D11Backend", OperatingSystem.IsWindows, "Windows"),
        new(GraphicsBackendPreference.Vulkan, "Videra.Platform.Linux", "Videra.Platform.Linux.VulkanBackend", OperatingSystem.IsLinux, "Linux"),
        new(GraphicsBackendPreference.Metal, "Videra.Platform.macOS", "Videra.Platform.macOS.MetalBackend", OperatingSystem.IsMacOS, "macOS")
    ];

    public static GraphicsBackendResolverResult ResolveBackend(GraphicsBackendPreference preference, ILoggerFactory? loggerFactory = null)
    {
        _ = loggerFactory;

        var registration = ResolveRegistration(preference);
        if (registration is null)
        {
            return new GraphicsBackendResolverResult(
                null,
                $"No native backend mapping exists for preference {preference} on platform {GetCurrentPlatformLabel()}.");
        }

        if (!registration.PlatformPredicate())
        {
            return new GraphicsBackendResolverResult(
                null,
                $"{registration.Preference} backend targets {registration.PlatformLabel} and is not available on platform {GetCurrentPlatformLabel()}.");
        }

        var backendType = Type.GetType($"{registration.TypeName}, {registration.AssemblyName}", throwOnError: false);
        if (backendType is null)
        {
            return new GraphicsBackendResolverResult(
                null,
                $"Backend package '{registration.AssemblyName}' is not installed or could not be loaded.");
        }

        if (!typeof(IGraphicsBackend).IsAssignableFrom(backendType))
        {
            return new GraphicsBackendResolverResult(
                null,
                $"Resolved backend type '{registration.TypeName}' does not implement IGraphicsBackend.");
        }

        try
        {
            if (Activator.CreateInstance(backendType) is IGraphicsBackend backend)
            {
                return new GraphicsBackendResolverResult(backend);
            }

            return new GraphicsBackendResolverResult(
                null,
                $"Resolved backend type '{registration.TypeName}' could not be instantiated.");
        }
        catch (Exception ex)
        {
            return new GraphicsBackendResolverResult(
                null,
                $"Backend type '{registration.TypeName}' failed to initialize: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static BackendRegistration? ResolveRegistration(GraphicsBackendPreference preference)
    {
        return preference switch
        {
            GraphicsBackendPreference.Auto => Registrations.FirstOrDefault(static registration => registration.PlatformPredicate()),
            GraphicsBackendPreference.Software => null,
            _ => Registrations.FirstOrDefault(registration => registration.Preference == preference)
        };
    }

    private static string GetCurrentPlatformLabel()
    {
        if (OperatingSystem.IsWindows())
        {
            return "Windows";
        }

        if (OperatingSystem.IsLinux())
        {
            return "Linux";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "macOS";
        }

        return "Unknown";
    }
}
