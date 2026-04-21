using Videra.Core.Graphics;
using Videra.Core.Graphics.RenderPipeline;

namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

public sealed class RenderFrameHookContext
{
    public required RenderFrameHookPoint HookPoint { get; init; }

    public required RenderFramePlan FramePlan { get; init; }

    public required RenderFeatureSet ActiveFeatures { get; init; }

    public required uint Width { get; init; }

    public required uint Height { get; init; }

    public required float RenderScale { get; init; }

    public required bool IsInitialized { get; init; }

    public GraphicsBackendPreference? ActiveBackendPreference { get; init; }

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; init; }
}
