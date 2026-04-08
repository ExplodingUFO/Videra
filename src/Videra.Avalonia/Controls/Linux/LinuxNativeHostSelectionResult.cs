namespace Videra.Avalonia.Controls.Linux;

internal sealed record LinuxNativeHostSelectionResult(
    ILinuxPlatformNativeHost Host,
    string ResolvedDisplayServer,
    bool FallbackUsed,
    string? FallbackReason);
