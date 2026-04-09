using Videra.Core.Geometry;

namespace Videra.Core.Selection.Rendering;

public sealed class SelectionOverlayRenderState
{
    public static SelectionOverlayRenderState Empty { get; } = new();

    public SelectionOverlayRenderState(
        IReadOnlyList<Guid>? selectedObjectIds = null,
        Guid? hoverObjectId = null,
        RgbaFloat? selectedLineColor = null,
        RgbaFloat? hoverLineColor = null)
    {
        SelectedObjectIds = selectedObjectIds?.ToArray() ?? Array.Empty<Guid>();
        HoverObjectId = hoverObjectId;
        SelectedLineColor = selectedLineColor ?? RgbaFloat.Black;
        HoverLineColor = hoverLineColor ?? RgbaFloat.Green;
    }

    public IReadOnlyList<Guid> SelectedObjectIds { get; }

    public Guid? HoverObjectId { get; }

    public RgbaFloat SelectedLineColor { get; }

    public RgbaFloat HoverLineColor { get; }

    public bool HasOverlay => SelectedObjectIds.Count > 0 || HoverObjectId.HasValue;

    public SelectionOverlayRenderState Snapshot()
    {
        return new(
            SelectedObjectIds,
            HoverObjectId,
            SelectedLineColor,
            HoverLineColor);
    }
}
