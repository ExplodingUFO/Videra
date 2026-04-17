using Videra.Core.Graphics;
namespace Videra.Avalonia.Controls;

/// <summary>
/// Public diagnostics shell describing backend availability, fallback, and capability truth for a <see cref="VideraView" />.
/// </summary>
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
            SceneDocumentVersion = 0,
            PendingSceneUploads = 0,
            ResidentSceneObjects = 0,
            DirtySceneObjects = 0,
            FailedSceneUploads = 0,
            SupportsPassContributors = true,
            SupportsPassReplacement = true,
            SupportsFrameHooks = true,
            SupportsPipelineSnapshots = true
        };
    }

    /// <summary>
    /// Gets the backend preference the view asked to initialize.
    /// </summary>
    public GraphicsBackendPreference RequestedBackend { get; init; }

    /// <summary>
    /// Gets the backend preference actually resolved for the current diagnostics snapshot.
    /// </summary>
    public GraphicsBackendPreference ResolvedBackend { get; init; }

    /// <summary>
    /// Gets a value indicating whether the render session is currently ready to present frames.
    /// </summary>
    public bool IsReady { get; init; }

    /// <summary>
    /// Gets a value indicating whether native backend resolution fell back to software.
    /// </summary>
    public bool IsUsingSoftwareFallback { get; init; }

    /// <summary>
    /// Gets the reason a native backend was unavailable when the view resolved to software fallback.
    /// </summary>
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

    public int SceneDocumentVersion { get; init; }

    public int PendingSceneUploads { get; init; }

    public int ResidentSceneObjects { get; init; }

    public int DirtySceneObjects { get; init; }

    public int FailedSceneUploads { get; init; }

    /// <summary>
    /// Gets a value indicating whether pass contributor registration is supported on the public surface.
    /// </summary>
    public bool SupportsPassContributors { get; init; }

    /// <summary>
    /// Gets a value indicating whether pass replacement is supported on the public surface.
    /// </summary>
    public bool SupportsPassReplacement { get; init; }

    /// <summary>
    /// Gets a value indicating whether frame hook registration is supported on the public surface.
    /// </summary>
    public bool SupportsFrameHooks { get; init; }

    /// <summary>
    /// Gets a value indicating whether pipeline snapshots are exposed on the public surface.
    /// </summary>
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
