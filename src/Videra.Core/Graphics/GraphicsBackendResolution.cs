using Microsoft.Extensions.Logging;
using Videra.Core.Graphics.Abstractions;

namespace Videra.Core.Graphics;

public enum BackendEnvironmentOverrideMode
{
    Disabled,
    AllowOverrides,
    PreferOverrides
}

public readonly record struct GraphicsBackendRequest(
    GraphicsBackendPreference RequestedPreference,
    BackendEnvironmentOverrideMode EnvironmentOverrideMode = BackendEnvironmentOverrideMode.PreferOverrides,
    bool AllowSoftwareFallback = false,
    ILoggerFactory? LoggerFactory = null);

public sealed class GraphicsBackendResolution
{
    public GraphicsBackendResolution(
        IGraphicsBackend backend,
        GraphicsBackendPreference requestedPreference,
        GraphicsBackendPreference resolvedPreference,
        bool environmentOverrideApplied = false,
        bool isUsingSoftwareFallback = false,
        string? fallbackReason = null)
    {
        Backend = backend ?? throw new ArgumentNullException(nameof(backend));
        RequestedPreference = requestedPreference;
        ResolvedPreference = resolvedPreference;
        EnvironmentOverrideApplied = environmentOverrideApplied;
        IsUsingSoftwareFallback = isUsingSoftwareFallback;
        FallbackReason = fallbackReason;
    }

    public IGraphicsBackend Backend { get; }

    public GraphicsBackendPreference RequestedPreference { get; }

    public GraphicsBackendPreference ResolvedPreference { get; }

    public bool EnvironmentOverrideApplied { get; }

    public bool IsUsingSoftwareFallback { get; }

    public string? FallbackReason { get; }
}
