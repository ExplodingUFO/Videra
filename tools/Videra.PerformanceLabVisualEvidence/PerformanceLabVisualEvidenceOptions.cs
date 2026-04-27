namespace Videra.PerformanceLabVisualEvidence;

public sealed record PerformanceLabVisualEvidenceOptions(
    string OutputRoot,
    int Width = 1280,
    int Height = 720,
    IReadOnlyList<string>? ViewerScenarioIds = null,
    IReadOnlyList<string>? ScatterScenarioIds = null,
    bool SimulateUnavailable = false);
