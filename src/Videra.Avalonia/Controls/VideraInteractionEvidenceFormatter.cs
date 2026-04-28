using System.Globalization;
using System.Numerics;
using System.Text;
using Videra.Avalonia.Controls.Interaction;

namespace Videra.Avalonia.Controls;

/// <summary>
/// Creates and formats report-only viewer interaction evidence from public inspection contracts.
/// </summary>
public static class VideraInteractionEvidenceFormatter
{
    /// <summary>
    /// Creates deterministic interaction evidence from an inspection state snapshot.
    /// </summary>
    /// <param name="state">The inspection state snapshot to summarize.</param>
    /// <param name="diagnostics">Optional interaction capability diagnostics.</param>
    /// <returns>A report-only interaction evidence snapshot.</returns>
    public static VideraInteractionEvidence Create(
        VideraInspectionState state,
        VideraInteractionDiagnostics? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(state);

        return new VideraInteractionEvidence
        {
            SelectedObjectCount = state.SelectedObjectIds.Count,
            PrimarySelectedObjectId = state.PrimarySelectedObjectId,
            AnnotationCount = state.Annotations.Count,
            AnnotationKinds = state.Annotations
                .Select(static annotation => annotation.GetType().Name)
                .ToArray(),
            MeasurementCount = state.Measurements.Count,
            MeasurementLabels = state.Measurements
                .Select(static measurement => string.IsNullOrWhiteSpace(measurement.Label) ? "Unlabeled" : measurement.Label)
                .ToArray(),
            ClippingPlaneCount = state.ClippingPlanes.Count,
            MeasurementSnapMode = state.MeasurementSnapMode,
            CameraTarget = state.CameraTarget,
            CameraRadius = state.CameraRadius,
            CameraYaw = state.CameraYaw,
            CameraPitch = state.CameraPitch,
            SupportsControlledSelection = diagnostics?.SupportsControlledSelection,
            SupportsControlledAnnotations = diagnostics?.SupportsControlledAnnotations,
            SupportsIntentEvents = diagnostics?.SupportsIntentEvents
        };
    }

    /// <summary>
    /// Formats inspection state interaction evidence as stable plain text.
    /// </summary>
    /// <param name="state">The inspection state snapshot to summarize.</param>
    /// <param name="diagnostics">Optional interaction capability diagnostics.</param>
    /// <returns>A multi-line interaction evidence report.</returns>
    public static string Format(
        VideraInspectionState state,
        VideraInteractionDiagnostics? diagnostics = null)
    {
        return Format(Create(state, diagnostics));
    }

    /// <summary>
    /// Formats interaction evidence as stable plain text.
    /// </summary>
    /// <param name="evidence">The interaction evidence snapshot to format.</param>
    /// <returns>A multi-line interaction evidence report.</returns>
    public static string Format(VideraInteractionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var builder = new StringBuilder();
        builder.AppendLine("Videra interaction evidence");
        builder.AppendLine($"EvidenceKind: {evidence.EvidenceKind}");
        builder.AppendLine($"SelectedObjectCount: {evidence.SelectedObjectCount}");
        builder.AppendLine($"PrimarySelectedObjectId: {FormatNullable(evidence.PrimarySelectedObjectId)}");
        builder.AppendLine($"AnnotationCount: {evidence.AnnotationCount}");
        builder.AppendLine($"AnnotationKinds: {FormatList(evidence.AnnotationKinds)}");
        builder.AppendLine($"MeasurementCount: {evidence.MeasurementCount}");
        builder.AppendLine($"MeasurementLabels: {FormatList(evidence.MeasurementLabels)}");
        builder.AppendLine($"ClippingPlaneCount: {evidence.ClippingPlaneCount}");
        builder.AppendLine($"MeasurementSnapMode: {evidence.MeasurementSnapMode}");
        builder.AppendLine($"CameraTarget: {FormatVector(evidence.CameraTarget)}");
        builder.AppendLine($"CameraRadius: {FormatFloat(evidence.CameraRadius)}");
        builder.AppendLine($"CameraYaw: {FormatFloat(evidence.CameraYaw)}");
        builder.AppendLine($"CameraPitch: {FormatFloat(evidence.CameraPitch)}");
        builder.AppendLine($"SupportsControlledSelection: {FormatNullable(evidence.SupportsControlledSelection)}");
        builder.AppendLine($"SupportsControlledAnnotations: {FormatNullable(evidence.SupportsControlledAnnotations)}");
        builder.AppendLine($"SupportsIntentEvents: {FormatNullable(evidence.SupportsIntentEvents)}");
        return builder.ToString();
    }

    private static string FormatNullable(Guid? value) =>
        value?.ToString() ?? "Unavailable";

    private static string FormatNullable(bool? value) =>
        value?.ToString() ?? "Unavailable";

    private static string FormatList(IReadOnlyList<string>? values) =>
        values is { Count: > 0 } ? string.Join(", ", values) : "Unavailable";

    private static string FormatVector(Vector3 value) =>
        $"{FormatFloat(value.X)}, {FormatFloat(value.Y)}, {FormatFloat(value.Z)}";

    private static string FormatFloat(float value) =>
        value.ToString("G9", CultureInfo.InvariantCulture);
}
