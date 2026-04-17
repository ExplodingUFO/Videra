using Videra.Core.Selection;

namespace Videra.Core.Overlay;

public sealed record SelectionOutlineProjection(
    Guid ObjectId,
    ProjectedScreenRect ScreenBounds,
    bool IsPrimary);
