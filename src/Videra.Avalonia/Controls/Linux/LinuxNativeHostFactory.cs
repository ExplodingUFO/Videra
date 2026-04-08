using Videra.Core.Exceptions;
using Videra.Core.Platform.Linux;

namespace Videra.Avalonia.Controls.Linux;

internal sealed class LinuxNativeHostFactory
{
    private const string XWaylandCompatibilityReason =
        "Avalonia Linux native hosting currently embeds through X11 handles; using an XWayland compatibility path for this Wayland session.";

    private const string WaylandOnlyUnsupportedReason =
        "Wayland was detected, but the current Avalonia Linux native-host stack cannot embed directly into a compositor-native Wayland surface. Enable XWayland for this session or validate on an X11 host.";

    private readonly LinuxDisplayServerDetector _detector;
    private readonly Func<ILinuxPlatformNativeHost> _x11HostFactory;

    public LinuxNativeHostFactory(
        LinuxDisplayServerDetector? detector = null,
        Func<ILinuxPlatformNativeHost>? x11HostFactory = null)
    {
        _detector = detector ?? new LinuxDisplayServerDetector();
        _x11HostFactory = x11HostFactory ?? (() => new X11NativeHost());
    }

    public LinuxNativeHostSelectionResult CreateHost()
    {
        var candidates = _detector.DetectCandidates(
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"),
            Environment.GetEnvironmentVariable("DISPLAY"),
            Environment.GetEnvironmentVariable("XDG_SESSION_TYPE"));

        string? fallbackReason = null;

        foreach (var candidate in candidates)
        {
            switch (candidate.DisplayServer)
            {
                case LinuxDisplayServerKind.Wayland:
                    fallbackReason = candidate.AllowsXWaylandFallback
                        ? XWaylandCompatibilityReason
                        : WaylandOnlyUnsupportedReason;
                    continue;

                case LinuxDisplayServerKind.XWayland:
                    return new LinuxNativeHostSelectionResult(
                        _x11HostFactory(),
                        ResolvedDisplayServer: LinuxDisplayServerKind.XWayland.ToString(),
                        FallbackUsed: true,
                        FallbackReason: fallbackReason ?? XWaylandCompatibilityReason);

                case LinuxDisplayServerKind.X11:
                    return new LinuxNativeHostSelectionResult(
                        _x11HostFactory(),
                        ResolvedDisplayServer: LinuxDisplayServerKind.X11.ToString(),
                        FallbackUsed: false,
                        FallbackReason: null);
            }
        }

        throw new PlatformDependencyException(
            fallbackReason ?? "No supported Linux display server is available for native rendering.",
            "CreateLinuxNativeHost",
            "Linux");
    }
}
