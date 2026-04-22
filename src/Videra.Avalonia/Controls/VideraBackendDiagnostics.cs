using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;
namespace Videra.Avalonia.Controls;

/// <summary>
/// Public diagnostics shell describing backend availability, fallback, and capability truth for a <see cref="VideraView" />.
/// </summary>
public sealed class VideraBackendDiagnostics
{
    internal const string CurrentTransparentFeatureStatus =
        "Alpha mask rendering is shipped for per-object carried alpha sources on the current runtime path; alpha blend ordering remains deferred.";

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
            LastFrameFeatureNames = Array.Empty<string>(),
            SupportedRenderFeatureNames = (
                RenderFeatureSet.Opaque |
                RenderFeatureSet.Overlay |
                RenderFeatureSet.Picking |
                RenderFeatureSet.Screenshot).ToFeatureNames(),
            TransparentFeatureStatus = CurrentTransparentFeatureStatus,
            SceneDocumentVersion = 0,
            PendingSceneUploads = 0,
            PendingSceneUploadBytes = 0,
            ResidentSceneObjects = 0,
            DirtySceneObjects = 0,
            FailedSceneUploads = 0,
            LastFrameUploadedObjects = 0,
            LastFrameUploadedBytes = 0,
            LastFrameUploadFailures = 0,
            LastFrameUploadDuration = TimeSpan.Zero,
            ResolvedUploadBudgetObjects = 0,
            ResolvedUploadBudgetBytes = 0,
            IsClippingActive = false,
            ActiveClippingPlaneCount = 0,
            MeasurementCount = 0,
            LastSnapshotExportPath = null,
            LastSnapshotExportStatus = null,
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

    /// <summary>
    /// Gets the resolved Linux display-server path when native host selection is relevant.
    /// `X11` means the direct supported X11 host path. `XWayland` means a Wayland session is
    /// running through the documented X11 compatibility bridge rather than compositor-native
    /// Wayland embedding.
    /// </summary>
    public string? ResolvedDisplayServer { get; init; }

    /// <summary>
    /// Gets a value indicating whether native host selection had to fall back from a preferred
    /// Linux display-server candidate to a compatibility path such as `XWayland`.
    /// </summary>
    public bool DisplayServerFallbackUsed { get; init; }

    /// <summary>
    /// Gets the reason a Linux display-server fallback or compatibility path was selected.
    /// </summary>
    public string? DisplayServerFallbackReason { get; init; }

    /// <summary>
    /// Gets the pipeline profile captured for the last rendered frame, if one is available.
    /// </summary>
    public string? RenderPipelineProfile { get; init; }

    /// <summary>
    /// Gets the stable stage names captured for the last rendered frame.
    /// </summary>
    public IReadOnlyList<string>? LastFrameStageNames { get; init; }

    /// <summary>
    /// Gets the active render-feature names observed on the last rendered frame.
    /// </summary>
    public IReadOnlyList<string>? LastFrameFeatureNames { get; init; }

    /// <summary>
    /// Gets the public render-feature contract names exposed through the viewer diagnostics surface.
    /// </summary>
    public IReadOnlyList<string>? SupportedRenderFeatureNames { get; init; }

    /// <summary>
    /// Gets the current transparency-contract status for the shipped viewer/runtime path.
    /// </summary>
    public string? TransparentFeatureStatus { get; init; }

    public bool UsesSoftwarePresentationCopy { get; init; }

    public int SceneDocumentVersion { get; init; }

    public int PendingSceneUploads { get; init; }

    public long PendingSceneUploadBytes { get; init; }

    public int ResidentSceneObjects { get; init; }

    public int DirtySceneObjects { get; init; }

    public int FailedSceneUploads { get; init; }

    public int LastFrameUploadedObjects { get; init; }

    public long LastFrameUploadedBytes { get; init; }

    public int LastFrameUploadFailures { get; init; }

    public TimeSpan LastFrameUploadDuration { get; init; }

    public int ResolvedUploadBudgetObjects { get; init; }

    public long ResolvedUploadBudgetBytes { get; init; }

    public bool IsClippingActive { get; init; }

    public int ActiveClippingPlaneCount { get; init; }

    public int MeasurementCount { get; init; }

    public string? LastSnapshotExportPath { get; init; }

    public string? LastSnapshotExportStatus { get; init; }

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
