using Videra.Core.Graphics;
namespace Videra.Avalonia.Controls;

public sealed class VideraBackendDiagnostics
{
    public static VideraBackendDiagnostics CreateDefault(GraphicsBackendPreference requestedBackend = GraphicsBackendPreference.Auto)
    {
        return new VideraBackendDiagnostics
        {
            RequestedBackend = requestedBackend,
            ResolvedBackend = GraphicsBackendPreference.Auto,
            RenderLoopMode = VideraRenderLoopMode.Dispatcher,
            ResolvedDisplayServer = null,
            DisplayServerFallbackUsed = false,
            DisplayServerFallbackReason = null,
            LastFrameStageNames = Array.Empty<string>(),
            SupportsPassContributors = true,
            SupportsPassReplacement = true,
            SupportsFrameHooks = true,
            SupportsPipelineSnapshots = true
        };
    }

    public GraphicsBackendPreference RequestedBackend { get; init; }

    public GraphicsBackendPreference ResolvedBackend { get; init; }

    public bool IsReady { get; init; }

    public bool IsUsingSoftwareFallback { get; init; }

    public string? FallbackReason { get; init; }

    public bool NativeHostBound { get; init; }

    public VideraRenderLoopMode RenderLoopMode { get; init; } = VideraRenderLoopMode.Dispatcher;

    public bool EnvironmentOverrideApplied { get; init; }

    public string? LastInitializationError { get; init; }

    public string? ResolvedDisplayServer { get; init; }

    public bool DisplayServerFallbackUsed { get; init; }

    public string? DisplayServerFallbackReason { get; init; }

    public string? RenderPipelineProfile { get; init; }

    public IReadOnlyList<string>? LastFrameStageNames { get; init; }

    public bool UsesSoftwarePresentationCopy { get; init; }

    public bool SupportsPassContributors { get; init; }

    public bool SupportsPassReplacement { get; init; }

    public bool SupportsFrameHooks { get; init; }

    public bool SupportsPipelineSnapshots { get; init; }
}

public sealed class VideraBackendStatusChangedEventArgs : EventArgs
{
    public VideraBackendStatusChangedEventArgs(VideraBackendDiagnostics diagnostics)
    {
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public VideraBackendDiagnostics Diagnostics { get; }
}

public sealed class VideraBackendFailureEventArgs : EventArgs
{
    public VideraBackendFailureEventArgs(VideraBackendDiagnostics diagnostics, Exception exception)
    {
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
    }

    public VideraBackendDiagnostics Diagnostics { get; }

    public Exception Exception { get; }
}
