using Silk.NET.Vulkan;

namespace Videra.Platform.Linux;

/// <summary>
/// Strategy interface for creating Vulkan surfaces on different display servers.
/// Implementations handle platform-specific surface creation (X11, Wayland, etc.).
/// </summary>
internal interface ISurfaceCreator
{
    /// <summary>
    /// Get the Vulkan instance extension name required by this surface creator.
    /// </summary>
    string RequiredExtensionName { get; }

    /// <summary>
    /// Create a Vulkan surface for the given instance and window handle.
    /// </summary>
    /// <param name="vk">The Vulkan API.</param>
    /// <param name="instance">The Vulkan instance.</param>
    /// <param name="windowHandle">Platform-specific window handle (e.g., X11 Window).</param>
    /// <returns>The created surface.</returns>
    SurfaceKHR CreateSurface(Vk vk, Instance instance, IntPtr windowHandle);

    /// <summary>
    /// Clean up platform-specific resources (e.g., close X11 display connections).
    /// </summary>
    void Cleanup();
}
