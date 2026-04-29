using System.Globalization;
using Videra.Avalonia.Controls;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.AvaloniaWorkbenchSample;

internal sealed record WorkbenchSceneEvidence(
    string Status,
    string SceneName,
    int? SceneVersion,
    int NodeCount,
    int PrimitiveCount,
    int InstanceBatchCount,
    int InstanceCount,
    Guid? SelectedMarkerId);

internal sealed record WorkbenchChartEvidence(
    SurfaceChartOutputEvidence OutputEvidence,
    SurfaceChartProbeEvidence ProbeEvidence);

internal sealed record WorkbenchInteractionEvidence(
    VideraInteractionEvidence InteractionEvidence);

internal sealed record WorkbenchSnapshotEvidence(
    string Status,
    string? Path,
    int? Width,
    int? Height,
    string? Format,
    string? Background,
    string? OutputEvidenceKind,
    string? DatasetEvidenceKind,
    string? ActiveSeriesIdentity,
    string? CreatedUtc);

internal static class WorkbenchSupportCapture
{
    public static string FormatSceneEvidence(WorkbenchSceneEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        return string.Join(
            Environment.NewLine,
            $"Status: {evidence.Status}",
            $"SceneName: {evidence.SceneName}",
            $"SceneVersion: {FormatNullable(evidence.SceneVersion)}",
            $"NodeCount: {evidence.NodeCount}",
            $"PrimitiveCount: {evidence.PrimitiveCount}",
            $"InstanceBatchCount: {evidence.InstanceBatchCount}",
            $"InstanceCount: {evidence.InstanceCount}",
            $"SelectedMarkerId: {evidence.SelectedMarkerId?.ToString() ?? "none"}");
    }

    public static string FormatChartEvidence(WorkbenchChartEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var output = evidence.OutputEvidence;
        var lines = new List<string>
        {
            $"OutputEvidenceKind: {output.EvidenceKind}",
            $"PrecisionProfile: {output.PrecisionProfile}",
            $"PaletteName: {output.PaletteName}",
            $"PaletteColorCount: {output.ColorStops.Count}",
            $"PaletteColors: {string.Join(", ", output.ColorStops)}"
        };
        lines.AddRange(output.SampleFormattedLabels.Select(static sample => $"Sample: {sample}"));
        lines.Add("ProbeEvidence:");
        lines.AddRange(SurfaceChartProbeEvidenceFormatter.Format(evidence.ProbeEvidence).Split(Environment.NewLine));
        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatInteractionEvidence(WorkbenchInteractionEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        return VideraInteractionEvidenceFormatter.Format(evidence.InteractionEvidence).TrimEnd();
    }

    public static string FormatSnapshotEvidence(WorkbenchSnapshotEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        return string.Join(
            Environment.NewLine,
            $"SnapshotStatus: {evidence.Status}",
            $"SnapshotPath: {evidence.Path ?? "none"}",
            $"SnapshotWidth: {evidence.Width?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"SnapshotHeight: {evidence.Height?.ToString(CultureInfo.InvariantCulture) ?? "none"}",
            $"SnapshotFormat: {evidence.Format ?? "none"}",
            $"SnapshotBackground: {evidence.Background ?? "none"}",
            $"SnapshotOutputEvidenceKind: {evidence.OutputEvidenceKind ?? "none"}",
            $"SnapshotDatasetEvidenceKind: {evidence.DatasetEvidenceKind ?? "none"}",
            $"SnapshotActiveSeriesIdentity: {evidence.ActiveSeriesIdentity ?? "none"}",
            $"SnapshotCreatedUtc: {evidence.CreatedUtc ?? "none"}");
    }

    public static string FormatSupportCapture(
        DateTimeOffset generatedUtc,
        WorkbenchSceneEvidence sceneEvidence,
        WorkbenchInteractionEvidence interactionEvidence,
        WorkbenchChartEvidence chartEvidence,
        string diagnosticsSnapshot,
        WorkbenchSnapshotEvidence? snapshotEvidence = null)
    {
        ArgumentNullException.ThrowIfNull(sceneEvidence);
        ArgumentNullException.ThrowIfNull(interactionEvidence);
        ArgumentNullException.ThrowIfNull(chartEvidence);

        var hasDiagnostics = !string.IsNullOrWhiteSpace(diagnosticsSnapshot);
        var lines = new List<string>
        {
            "Videra Avalonia workbench support capture",
            $"GeneratedUtc: {generatedUtc:O}",
            "SceneEvidence:",
            FormatSceneEvidence(sceneEvidence),
            "ViewerInteractionEvidence:",
            FormatInteractionEvidence(interactionEvidence),
            "ChartOutputEvidence:",
            FormatChartEvidence(chartEvidence),
            $"DiagnosticsSnapshotStatus: {(hasDiagnostics ? "captured" : "empty")}",
            "DiagnosticsSnapshot:",
            diagnosticsSnapshot
        };

        if (snapshotEvidence is not null)
        {
            lines.Add("SnapshotEvidence:");
            lines.Add(FormatSnapshotEvidence(snapshotEvidence));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatNullable(int? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unknown";
    }
}
