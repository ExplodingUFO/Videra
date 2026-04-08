namespace Videra.Core.Platform.Linux;

public readonly record struct LinuxDisplayServerResolution(
    LinuxDisplayServerKind ResolvedDisplayServer,
    bool FallbackUsed,
    string? FailureReason);
