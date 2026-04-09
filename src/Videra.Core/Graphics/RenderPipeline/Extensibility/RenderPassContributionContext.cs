using Videra.Core.Graphics;
using Videra.Core.Graphics.Abstractions;
using Videra.Core.Selection.Rendering;

namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

public sealed class RenderPassContributionContext
{
    public required RenderPassSlot Slot { get; init; }

    public required RenderFramePlan FramePlan { get; init; }

    public required ICommandExecutor CommandExecutor { get; init; }

    public required IResourceFactory ResourceFactory { get; init; }

    public required IPipeline MeshPipeline { get; init; }

    public required IReadOnlyList<Object3D> SceneObjects { get; init; }

    public required uint Width { get; init; }

    public required uint Height { get; init; }

    public required float RenderScale { get; init; }

    public required bool ShouldLog { get; init; }

    public required bool IsInitialized { get; init; }

    public GraphicsBackendPreference? ActiveBackendPreference { get; init; }

    public bool IsUsingSoftwareBackend => ActiveBackendPreference == GraphicsBackendPreference.Software;

    public RenderPipelineSnapshot? LastPipelineSnapshot { get; init; }

    public required SelectionOverlayRenderState SelectionOverlay { get; init; }

    public required AnnotationOverlayRenderState AnnotationOverlay { get; init; }
}
