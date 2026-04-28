using System.Numerics;
using Videra.Core.Inspection;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Report-only evidence for the viewer interaction state attached to an inspection session.
/// </summary>
public sealed class VideraInteractionEvidence
{
    public string EvidenceKind { get; init; } = "ViewerInteraction";

    public int SelectedObjectCount { get; init; }

    public Guid? PrimarySelectedObjectId { get; init; }

    public int AnnotationCount { get; init; }

    public IReadOnlyList<string> AnnotationKinds { get; init; } = Array.Empty<string>();

    public int MeasurementCount { get; init; }

    public IReadOnlyList<string> MeasurementLabels { get; init; } = Array.Empty<string>();

    public int ClippingPlaneCount { get; init; }

    public VideraMeasurementSnapMode MeasurementSnapMode { get; init; }

    public Vector3 CameraTarget { get; init; }

    public float CameraRadius { get; init; }

    public float CameraYaw { get; init; }

    public float CameraPitch { get; init; }

    public bool? SupportsControlledSelection { get; init; }

    public bool? SupportsControlledAnnotations { get; init; }

    public bool? SupportsIntentEvents { get; init; }
}
