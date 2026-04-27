namespace Videra.PerformanceLabVisualEvidence;

public sealed record PerformanceLabVisualEvidenceManifest(
    string EvidenceKind,
    bool EvidenceOnly,
    string Status,
    string GeneratedUtc,
    int Width,
    int Height,
    string OutputRoot,
    IReadOnlyList<PerformanceLabVisualEvidenceEntry> Entries,
    IReadOnlyList<string> Notes);

public sealed record PerformanceLabVisualEvidenceEntry(
    string Id,
    string ScenarioType,
    string DisplayName,
    string Status,
    string? PngPath,
    string DiagnosticsPath,
    IReadOnlyDictionary<string, string> Settings);

public sealed record PerformanceLabVisualEvidenceResult(
    string Status,
    string ManifestPath,
    IReadOnlyList<PerformanceLabVisualEvidenceEntry> Entries);
