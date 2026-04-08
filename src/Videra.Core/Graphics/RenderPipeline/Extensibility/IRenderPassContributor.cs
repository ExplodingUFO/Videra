namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

public enum RenderPassSlot
{
    Grid = 0,
    SolidGeometry,
    Wireframe,
    Axis
}

public interface IRenderPassContributor
{
    void Contribute(RenderPassContributionContext context);
}
