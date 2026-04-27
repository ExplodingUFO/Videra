namespace Videra.PerformanceLabVisualEvidence;

public sealed record PerformanceLabVisualEvidenceManifest(
    int SchemaVersion,
    string EvidenceKind,
    bool EvidenceOnly,
    string Status,
    string GeneratedUtc,
    int Width,
    int Height,
    string OutputRoot,
    string SummaryPath,
    IReadOnlyList<PerformanceLabVisualEvidenceEntry> Entries,
    IReadOnlyList<string> Notes);

public sealed record PerformanceLabVisualEvidenceEntry(
    string Id,
    string ScenarioType,
    string DisplayName,
    string Status,
    string? PngPath,
    string DiagnosticsPath,
    IReadOnlyList<string> Artifacts,
    IReadOnlyDictionary<string, string> Settings);

public sealed record PerformanceLabVisualEvidenceResult(
    string Status,
    string ManifestPath,
    IReadOnlyList<PerformanceLabVisualEvidenceEntry> Entries);
