namespace Videra.Avalonia.Runtime;

internal sealed record InspectionWorkflowDiagnostics(
    bool IsClippingActive,
    int ActiveClippingPlaneCount,
    int MeasurementCount,
    string? LastSnapshotExportPath,
    string? LastSnapshotExportStatus)
{
    public static InspectionWorkflowDiagnostics Empty { get; } = new(
        false,
        0,
        0,
        null,
        null);
}
