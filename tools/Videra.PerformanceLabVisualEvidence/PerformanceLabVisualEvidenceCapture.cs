using System.Globalization;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Videra.Demo.Services;
using Videra.SurfaceCharts.Demo.Services;

namespace Videra.PerformanceLabVisualEvidence;

public static class PerformanceLabVisualEvidenceCapture
{
    private const string EvidenceKind = "PerformanceLabVisualEvidence";
    private const int SchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static PerformanceLabVisualEvidenceResult Capture(
        PerformanceLabVisualEvidenceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.Width < 64 || options.Height < 64)
        {
            throw new ArgumentException("Visual evidence dimensions must be at least 64x64 pixels.", nameof(options));
        }

        Directory.CreateDirectory(options.OutputRoot);

        var entries = new List<PerformanceLabVisualEvidenceEntry>();
        foreach (var scenario in SelectViewerScenarios(options.ViewerScenarioIds))
        {
            cancellationToken.ThrowIfCancellationRequested();
            entries.Add(CaptureViewerScenario(options, scenario));
        }

        foreach (var scenario in SelectScatterScenarios(options.ScatterScenarioIds))
        {
            cancellationToken.ThrowIfCancellationRequested();
            entries.Add(CaptureScatterScenario(options, scenario));
        }

        var status = options.SimulateUnavailable ? "unavailable" : "produced";
        var summaryPath = Path.Combine(options.OutputRoot, "performance-lab-visual-evidence-summary.txt");
        WriteSummary(summaryPath, status, entries, options);

        var manifest = new PerformanceLabVisualEvidenceManifest(
            SchemaVersion,
            EvidenceKind,
            true,
            status,
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            options.Width,
            options.Height,
            NormalizePath(options.OutputRoot),
            NormalizePath(summaryPath),
            entries,
            [
                "Evidence-only artifact for PR review and support reports.",
                "Produced PNGs are bounded visual evidence, not pixel-perfect regression baselines.",
                "Unavailable entries are explicit host/capture-state evidence, not product rendering failures.",
                "This does not claim real GPU instancing, renderer parity, or stable benchmark guarantees."
            ]);
        var manifestPath = Path.Combine(options.OutputRoot, "performance-lab-visual-evidence-manifest.json");
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), Encoding.UTF8);

        return new PerformanceLabVisualEvidenceResult(manifest.Status, manifestPath, entries);
    }

    private static PerformanceLabVisualEvidenceEntry CaptureViewerScenario(
        PerformanceLabVisualEvidenceOptions options,
        PerformanceLabViewerScenario scenario)
    {
        var fileName = $"{scenario.Id}.png";
        var diagnosticsName = $"{scenario.Id}-diagnostics.txt";
        var pngPath = Path.Combine(options.OutputRoot, fileName);
        var diagnosticsPath = Path.Combine(options.OutputRoot, diagnosticsName);

        File.WriteAllText(
            diagnosticsPath,
            string.Join(
                Environment.NewLine,
                "EvidenceKind: PerformanceLabVisualEvidence",
                "EvidenceOnly: true",
                $"ScenarioType: viewer",
                $"ScenarioId: {scenario.Id}",
                $"DisplayName: {scenario.DisplayName}",
                $"ObjectCount: {scenario.ObjectCount.ToString(CultureInfo.InvariantCulture)}",
                $"Pickable: {scenario.Pickable}",
                "StableBenchmarkGuarantee: false"),
            Encoding.UTF8);

        if (!options.SimulateUnavailable)
        {
            using var image = PerformanceLabVisualEvidenceRenderer.RenderViewerScenario(scenario, options.Width, options.Height);
            image.SaveAsPng(pngPath, new PngEncoder());
        }

        return new PerformanceLabVisualEvidenceEntry(
            scenario.Id,
            "viewer",
            scenario.DisplayName,
            options.SimulateUnavailable ? "unavailable" : "produced",
            options.SimulateUnavailable ? null : NormalizePath(pngPath),
            NormalizePath(diagnosticsPath),
            options.SimulateUnavailable
                ? [NormalizePath(diagnosticsPath)]
                : [NormalizePath(pngPath), NormalizePath(diagnosticsPath)],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["mode"] = "InstanceBatch",
                ["objectCount"] = scenario.ObjectCount.ToString(CultureInfo.InvariantCulture),
                ["size"] = scenario.Size.ToString(),
                ["pickable"] = scenario.Pickable.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static PerformanceLabVisualEvidenceEntry CaptureScatterScenario(
        PerformanceLabVisualEvidenceOptions options,
        ScatterStreamingScenario scenario)
    {
        var fileName = $"{scenario.Id}.png";
        var diagnosticsName = $"{scenario.Id}-diagnostics.txt";
        var pngPath = Path.Combine(options.OutputRoot, fileName);
        var diagnosticsPath = Path.Combine(options.OutputRoot, diagnosticsName);
        var series = ScatterStreamingScenarios.CreateSeries(scenario);

        File.WriteAllText(
            diagnosticsPath,
            string.Join(
                Environment.NewLine,
                "EvidenceKind: PerformanceLabVisualEvidence",
                "EvidenceOnly: true",
                $"ScenarioType: surfacecharts-scatter",
                $"ScenarioId: {scenario.Id}",
                $"DisplayName: {scenario.DisplayName}",
                $"InitialPointCount: {scenario.InitialPointCount.ToString(CultureInfo.InvariantCulture)}",
                $"UpdatePointCount: {scenario.UpdatePointCount.ToString(CultureInfo.InvariantCulture)}",
                $"RenderedPointCount: {series.Count.ToString(CultureInfo.InvariantCulture)}",
                $"TotalDroppedPointCount: {series.TotalDroppedPointCount.ToString(CultureInfo.InvariantCulture)}",
                "StableBenchmarkGuarantee: false"),
            Encoding.UTF8);

        if (!options.SimulateUnavailable)
        {
            using var image = PerformanceLabVisualEvidenceRenderer.RenderScatterScenario(scenario, series, options.Width, options.Height);
            image.SaveAsPng(pngPath, new PngEncoder());
        }

        return new PerformanceLabVisualEvidenceEntry(
            scenario.Id,
            "surfacecharts-scatter",
            scenario.DisplayName,
            options.SimulateUnavailable ? "unavailable" : "produced",
            options.SimulateUnavailable ? null : NormalizePath(pngPath),
            NormalizePath(diagnosticsPath),
            options.SimulateUnavailable
                ? [NormalizePath(diagnosticsPath)]
                : [NormalizePath(pngPath), NormalizePath(diagnosticsPath)],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["updateMode"] = scenario.UpdateMode.ToString(),
                ["initialPointCount"] = scenario.InitialPointCount.ToString(CultureInfo.InvariantCulture),
                ["updatePointCount"] = scenario.UpdatePointCount.ToString(CultureInfo.InvariantCulture),
                ["fifoCapacity"] = scenario.FifoCapacity?.ToString(CultureInfo.InvariantCulture) ?? "",
                ["pickable"] = scenario.Pickable.ToString(CultureInfo.InvariantCulture)
            });
    }

    private static IEnumerable<PerformanceLabViewerScenario> SelectViewerScenarios(IReadOnlyList<string>? ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return PerformanceLabViewerScenarios.All;
        }

        return ids.Select(PerformanceLabViewerScenarios.Get);
    }

    private static IEnumerable<ScatterStreamingScenario> SelectScatterScenarios(IReadOnlyList<string>? ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return ScatterStreamingScenarios.All;
        }

        return ids.Select(ScatterStreamingScenarios.Get);
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static void WriteSummary(
        string summaryPath,
        string status,
        IReadOnlyList<PerformanceLabVisualEvidenceEntry> entries,
        PerformanceLabVisualEvidenceOptions options)
    {
        var lines = new List<string>
        {
            "Performance Lab Visual Evidence",
            $"EvidenceKind: {EvidenceKind}",
            $"SchemaVersion: {SchemaVersion.ToString(CultureInfo.InvariantCulture)}",
            "EvidenceOnly: true",
            $"Status: {status}",
            $"Dimensions: {options.Width.ToString(CultureInfo.InvariantCulture)}x{options.Height.ToString(CultureInfo.InvariantCulture)}",
            $"OutputRoot: {NormalizePath(options.OutputRoot)}",
            "StableBenchmarkGuarantee: false",
            "PixelPerfectRegressionGate: false",
            "RealGpuInstancingClaim: false",
            "",
            "Entries:"
        };

        lines.AddRange(entries.Select(entry =>
            $"- {entry.Id} [{entry.ScenarioType}] status={entry.Status} png={entry.PngPath ?? "(unavailable)"} diagnostics={entry.DiagnosticsPath}"));

        lines.Add("");
        lines.Add("Attach this bundle to PR reviews or support reports when visual evidence helps explain the selected Performance Lab scenario.");
        File.WriteAllLines(summaryPath, lines, Encoding.UTF8);
    }
}
