using Videra.Core.Graphics.RenderPipeline;

namespace Videra.Core.Graphics.RenderPipeline.Extensibility;

internal static class RenderPassSlotFeatureMap
{
    public static RenderFeatureSet Resolve(RenderPassSlot slot)
    {
        return slot switch
        {
            RenderPassSlot.Grid => RenderFeatureSet.Overlay,
            RenderPassSlot.SolidGeometry => RenderFeatureSet.Opaque,
            RenderPassSlot.Wireframe => RenderFeatureSet.Overlay,
            RenderPassSlot.Axis => RenderFeatureSet.Overlay,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown render pass slot.")
        };
    }
}
