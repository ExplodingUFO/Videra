namespace Videra.Platform.Linux;

internal readonly record struct LinuxDisplayServerResolution(
    LinuxDisplayServerKind ResolvedDisplayServer,
    bool FallbackUsed,
    string? FailureReason);
