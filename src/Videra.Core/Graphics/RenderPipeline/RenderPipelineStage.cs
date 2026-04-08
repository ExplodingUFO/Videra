namespace Videra.Core.Graphics.RenderPipeline;

public enum RenderPipelineStage
{
    PrepareFrame = 0,
    BindSharedFrameState,
    GridPass,
    SolidGeometryPass,
    WireframePass,
    AxisPass,
    PresentFrame
}
