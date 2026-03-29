namespace Videra.Core.Graphics;

/// <summary>
/// Specifies the preferred graphics backend for rendering.
/// Used at startup to select which rendering API the application should use.
/// </summary>
public enum GraphicsBackendPreference
{
    /// <summary>
    /// Automatically select the best available backend for the current platform.
    /// On Windows this prefers D3D11, on Linux Vulkan, and on macOS Metal.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Use a software-based rendering backend. Intended for testing and environments
    /// without GPU access.
    /// </summary>
    Software,

    /// <summary>
    /// Use the Direct3D 11 backend. Available on Windows only.
    /// </summary>
    D3D11,

    /// <summary>
    /// Use the Vulkan backend. Available on Windows and Linux.
    /// </summary>
    Vulkan,

    /// <summary>
    /// Use the Metal backend. Available on macOS only.
    /// </summary>
    Metal
}
