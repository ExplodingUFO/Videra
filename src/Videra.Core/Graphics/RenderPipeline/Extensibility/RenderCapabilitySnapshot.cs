using Videra.Core.Graphics;

namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

public sealed class RenderCapabilitySnapshot
{
    public bool IsInitialized { get; init; }

    public GraphicsBackendPreference? ActiveBackendPreference { get; init; }

    public bool SupportsPassContributors { get; init; }

    public bool SupportsPassReplacement { get; init; }

    public bool SupportsFrameHooks { get; init; }

    public bool SupportsPipelineSnapshots { get; init; }

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; init; }
}
