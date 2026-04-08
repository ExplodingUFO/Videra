namespace Videra.Platform.Linux;

internal readonly record struct LinuxDisplayServerCandidate(
    LinuxDisplayServerKind DisplayServer,
    string SessionKind,
    bool AllowsXWaylandFallback);
