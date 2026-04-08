using System.Collections.ObjectModel;
using Videra.Core.Graphics.Wireframe;

namespace Videra.Core.Graphics.RenderPipeline;

public sealed class RenderPipelineSnapshot
{
    public RenderPipelineSnapshot(
        RenderPipelineProfile profile,
        WireframeMode effectiveWireframeMode,
        bool renderGrid,
        bool renderSolidGeometry,
        bool renderWireframe,
        bool renderAxis,
        IEnumerable<RenderPipelineStage> executedStages)
    {
        Profile = profile;
        EffectiveWireframeMode = effectiveWireframeMode;
        RenderGrid = renderGrid;
        RenderSolidGeometry = renderSolidGeometry;
        RenderWireframe = renderWireframe;
        RenderAxis = renderAxis;
        Stages = Array.AsReadOnly(executedStages.ToArray());
        StageNames = Array.AsReadOnly(Stages.Select(static stage => stage.ToString()).ToArray());
    }

    public RenderPipelineProfile Profile { get; }

    public WireframeMode EffectiveWireframeMode { get; }

    public bool RenderGrid { get; }

    public bool RenderSolidGeometry { get; }

    public bool RenderWireframe { get; }

    public bool RenderAxis { get; }

    public ReadOnlyCollection<RenderPipelineStage> Stages { get; }

    public ReadOnlyCollection<string> StageNames { get; }
}
