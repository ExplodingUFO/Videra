using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;

namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

public sealed class RenderCapabilitySnapshot
{
    public bool IsInitialized { get; init; }

    public GraphicsBackendPreference? ActiveBackendPreference { get; init; }

    public bool SupportsPassContributors { get; init; }

    public bool SupportsPassReplacement { get; init; }

    public bool SupportsFrameHooks { get; init; }

    public bool SupportsPipelineSnapshots { get; init; }

    public bool SupportsShaderCreation { get; init; }

    public bool SupportsResourceSetCreation { get; init; }

    public bool SupportsResourceSetBinding { get; init; }

    public RenderFeatureSet SupportedFeatures { get; init; }

    public IReadOnlyList<string> SupportedFeatureNames { get; init; } = Array.Empty<string>();

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; init; }
}
