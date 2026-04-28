using System.Globalization;

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
    string PrecisionProfile,
    string PaletteName,
    IReadOnlyList<string> PaletteColors,
    IReadOnlyList<string> Samples);

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

        var lines = new List<string>
        {
            $"PrecisionProfile: {evidence.PrecisionProfile}",
            $"PaletteName: {evidence.PaletteName}",
            $"PaletteColorCount: {evidence.PaletteColors.Count}",
            $"PaletteColors: {string.Join(", ", evidence.PaletteColors)}"
        };
        lines.AddRange(evidence.Samples.Select(static sample => $"Sample: {sample}"));
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
            "ChartPrecisionEvidence:",
            FormatChartEvidence(chartEvidence),
            $"DiagnosticsSnapshotStatus: {(hasDiagnostics ? "captured" : "empty")}",
            "DiagnosticsSnapshot:",
            diagnosticsSnapshot);
    }

    public static string FormatPaletteColor(uint argb)
    {
        return "#" + argb.ToString("X8", CultureInfo.InvariantCulture);
    }

    private static string FormatNullable(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? "unknown";
    }
}
