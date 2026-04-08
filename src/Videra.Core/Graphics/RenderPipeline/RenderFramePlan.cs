using Videra.Core.Graphics.Wireframe;

namespace Videra.Core.Graphics.RenderPipeline;

public sealed record RenderFramePlan(
    RenderPipelineProfile Profile,
    WireframeMode EffectiveWireframeMode,
    bool RenderGrid,
    bool RenderSolidGeometry,
    bool RenderWireframe,
    bool RenderAxis,
    IReadOnlyList<RenderPipelineStage> PlannedStages);
