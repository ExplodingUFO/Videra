using System;
using System.Text;
using Videra.Avalonia.Controls;

namespace Videra.Demo.Services;

public static class PerformanceLabEvidenceSnapshotBuilder
{
    public static string Build(
        PerformanceLabViewerScenario scenario,
        string mode,
        int objectCount,
        bool pickable,
        string diagnosticsText,
        VideraBackendDiagnostics diagnostics,
        DateTimeOffset generatedUtc)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentException.ThrowIfNullOrWhiteSpace(mode);
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnosticsText);
        ArgumentNullException.ThrowIfNull(diagnostics);

        var builder = new StringBuilder();
        builder.AppendLine("Videra Performance Lab snapshot");
        builder.AppendLine($"GeneratedUtc: {generatedUtc:O}");
        builder.AppendLine("EvidenceKind: PerformanceLabDatasetProof");
        builder.AppendLine("EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.");
        builder.AppendLine();
        builder.AppendLine("Scenario");
        builder.AppendLine($"ScenarioId: {scenario.Id}");
        builder.AppendLine($"ScenarioName: {scenario.DisplayName}");
        builder.AppendLine($"ScenarioSize: {scenario.Size}");
        builder.AppendLine($"Mode: {mode}");
        builder.AppendLine($"ObjectCount: {objectCount}");
        builder.AppendLine($"Pickable: {pickable}");
        builder.AppendLine();
        builder.AppendLine("Runtime status");
        builder.AppendLine($"BackendReady: {diagnostics.IsReady}");
        builder.AppendLine($"ResolvedBackend: {diagnostics.ResolvedBackend}");
        builder.AppendLine($"SoftwareFallback: {diagnostics.IsUsingSoftwareFallback}");
        builder.AppendLine();
        builder.AppendLine("Scenario diagnostics");
        builder.AppendLine(diagnosticsText);
        builder.AppendLine();
        builder.AppendLine("Backend diagnostics");
        builder.AppendLine(VideraDiagnosticsSnapshotFormatter.Format(diagnostics));
        return builder.ToString().TrimEnd();
    }
}
