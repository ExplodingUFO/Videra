using System.Collections.ObjectModel;
using Videra.Core.Graphics.Wireframe;

namespace Videra.Core.Graphics.RenderPipeline;

public sealed class RenderPipelineSnapshot
{
    public RenderPipelineSnapshot(
        RenderPipelineProfile profile,
        RenderFeatureSet activeFeatures,
        WireframeMode effectiveWireframeMode,
        bool renderGrid,
        bool renderSolidGeometry,
        bool renderWireframe,
        bool renderAxis,
        int frameObjectCount,
        int opaqueObjectCount,
        int transparentObjectCount,
        IEnumerable<RenderPipelineStage> executedStages)
    {
        Profile = profile;
        ActiveFeatures = activeFeatures;
        EffectiveWireframeMode = effectiveWireframeMode;
        RenderGrid = renderGrid;
        RenderSolidGeometry = renderSolidGeometry;
        RenderWireframe = renderWireframe;
        RenderAxis = renderAxis;
        FrameObjectCount = frameObjectCount;
        OpaqueObjectCount = opaqueObjectCount;
        TransparentObjectCount = transparentObjectCount;
        Stages = Array.AsReadOnly(executedStages.ToArray());
        StageNames = Array.AsReadOnly(Stages.Select(static stage => stage.ToString()).ToArray());
        FeatureNames = Array.AsReadOnly(activeFeatures.ToFeatureNames().ToArray());
    }

    public RenderPipelineProfile Profile { get; }

    public RenderFeatureSet ActiveFeatures { get; }

    public WireframeMode EffectiveWireframeMode { get; }

    public bool RenderGrid { get; }

    public bool RenderSolidGeometry { get; }

    public bool RenderWireframe { get; }

    public bool RenderAxis { get; }

    public int FrameObjectCount { get; }

    public int OpaqueObjectCount { get; }

    public int TransparentObjectCount { get; }

    public ReadOnlyCollection<RenderPipelineStage> Stages { get; }

    public ReadOnlyCollection<string> StageNames { get; }

    public ReadOnlyCollection<string> FeatureNames { get; }
}
