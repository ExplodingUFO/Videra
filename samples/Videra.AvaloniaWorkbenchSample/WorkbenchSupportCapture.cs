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
    SurfaceChartOutputEvidence OutputEvidence);

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
        return string.Join(Environment.NewLine, lines);
    }

    public static string FormatSupportCapture(
        DateTimeOffset generatedUtc,
        WorkbenchSceneEvidence sceneEvidence,
        WorkbenchChartEvidence chartEvidence,
        string diagnosticsSnapshot)
    {
        ArgumentNullException.ThrowIfNull(sceneEvidence);
        ArgumentNullException.ThrowIfNull(chartEvidence);

        var hasDiagnostics = !string.IsNullOrWhiteSpace(diagnosticsSnapshot);
        return string.Join(
            Environment.NewLine,
            "Videra Avalonia workbench support capture",
            $"GeneratedUtc: {generatedUtc:O}",
            "SceneEvidence:",
            FormatSceneEvidence(sceneEvidence),
            "ChartOutputEvidence:",
            FormatChartEvidence(chartEvidence),
            $"DiagnosticsSnapshotStatus: {(hasDiagnostics ? "captured" : "empty")}",
            "DiagnosticsSnapshot:",
            diagnosticsSnapshot);
    }

    private static string FormatNullable(int? value)
    {
        return value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unknown";
    }
}
