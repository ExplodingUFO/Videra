using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;

namespace Videra.Avalonia.Rendering;

internal sealed record RenderSessionSnapshot
{
    public RenderSessionState State { get; init; } = RenderSessionState.Detached;

    public RenderSessionInputs Inputs { get; init; } = new();

    public RenderSessionHandle HandleState { get; init; } = RenderSessionHandle.Unbound;

    public GraphicsBackendResolution? LastBackendResolution { get; init; }

    public Exception? LastInitializationError { get; init; }

    public string? ResolvedDisplayServer { get; init; }

    public bool DisplayServerFallbackUsed { get; init; }

    public string? DisplayServerFallbackReason { get; init; }

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; init; }

    public bool UsesSoftwarePresentationCopy { get; init; }
}

internal enum RenderSessionState
{
    Detached = 0,
    WaitingForSize,
    WaitingForHandle,
    Ready,
    Faulted,
    Disposed
}
