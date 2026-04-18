using System.Numerics;
using Videra.Core.Inspection;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Captures the viewer-first inspection state needed to restore what the host was looking at.
/// </summary>
public sealed class VideraInspectionState
{
    public required Vector3 CameraTarget { get; init; }

    public required float CameraRadius { get; init; }

    public required float CameraYaw { get; init; }

    public required float CameraPitch { get; init; }

    public IReadOnlyList<Guid> SelectedObjectIds { get; init; } = Array.Empty<Guid>();

    public Guid? PrimarySelectedObjectId { get; init; }

    public IReadOnlyList<VideraClipPlane> ClippingPlanes { get; init; } = Array.Empty<VideraClipPlane>();

    public IReadOnlyList<VideraMeasurement> Measurements { get; init; } = Array.Empty<VideraMeasurement>();
}
