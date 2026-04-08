namespace Videra.Core.Platform.Linux;

public readonly record struct LinuxDisplayServerCandidate(
    LinuxDisplayServerKind DisplayServer,
    string SessionKind,
    bool AllowsXWaylandFallback);
