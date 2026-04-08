namespace Videra.Core.Platform.Linux;

public sealed class LinuxDisplayServerDetector
{
    public IReadOnlyList<LinuxDisplayServerCandidate> DetectCandidates(
        string? waylandDisplay,
        string? x11Display,
        string? sessionType)
    {
        var normalizedSessionType = NormalizeSessionType(sessionType);
        var hasWaylandDisplay = !string.IsNullOrWhiteSpace(waylandDisplay);
        var hasX11Display = !string.IsNullOrWhiteSpace(x11Display);

        if (!hasWaylandDisplay && !hasX11Display)
        {
            return Array.Empty<LinuxDisplayServerCandidate>();
        }

        if (normalizedSessionType == "wayland" || (normalizedSessionType == "unknown" && hasWaylandDisplay))
        {
            return CreateWaylandFirstCandidates(normalizedSessionType, hasWaylandDisplay, hasX11Display);
        }

        if (normalizedSessionType == "x11")
        {
            return hasX11Display
                ? new[]
                {
                    new LinuxDisplayServerCandidate(LinuxDisplayServerKind.X11, "x11", AllowsXWaylandFallback: false)
                }
                : Array.Empty<LinuxDisplayServerCandidate>();
        }

        if (hasWaylandDisplay && hasX11Display)
        {
            return CreateWaylandFirstCandidates("unknown", hasWaylandDisplay, hasX11Display);
        }

        if (hasWaylandDisplay)
        {
            return new[]
            {
                new LinuxDisplayServerCandidate(LinuxDisplayServerKind.Wayland, normalizedSessionType, AllowsXWaylandFallback: false)
            };
        }

        return new[]
        {
            new LinuxDisplayServerCandidate(LinuxDisplayServerKind.X11, normalizedSessionType, AllowsXWaylandFallback: false)
        };
    }

    private static IReadOnlyList<LinuxDisplayServerCandidate> CreateWaylandFirstCandidates(
        string sessionKind,
        bool hasWaylandDisplay,
        bool hasX11Display)
    {
        if (!hasWaylandDisplay)
        {
            return hasX11Display
                ? new[]
                {
                    new LinuxDisplayServerCandidate(LinuxDisplayServerKind.X11, sessionKind, AllowsXWaylandFallback: false)
                }
                : Array.Empty<LinuxDisplayServerCandidate>();
        }

        if (hasX11Display)
        {
            return new[]
            {
                new LinuxDisplayServerCandidate(LinuxDisplayServerKind.Wayland, sessionKind, AllowsXWaylandFallback: true),
                new LinuxDisplayServerCandidate(LinuxDisplayServerKind.XWayland, sessionKind, AllowsXWaylandFallback: false)
            };
        }

        return new[]
        {
            new LinuxDisplayServerCandidate(LinuxDisplayServerKind.Wayland, sessionKind, AllowsXWaylandFallback: false)
        };
    }

    private static string NormalizeSessionType(string? sessionType)
    {
        if (string.IsNullOrWhiteSpace(sessionType))
        {
            return "unknown";
        }

        return sessionType.Trim().ToLowerInvariant();
    }
}
