using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsPerformanceTruthTests
{
    [Fact]
    public void SurfaceChartsDemoDocs_ShouldKeepSupportAndSnapshotEvidenceOutOfBenchmarkTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = Read(repositoryRoot, "README.md");
        var demoReadme = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md");
        var cutover = Read(repositoryRoot, "docs", "surfacecharts-release-cutover.md");
        var benchmarkGates = Read(repositoryRoot, "docs", "benchmark-gates.md");
        var mainWindowCodeBehind = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Views", "MainWindow.axaml.cs");
        var supportSummary = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Services", "SurfaceDemoSupportSummary.cs");

        demoReadme.Should().Contain("support evidence, not stable benchmark guarantees");
        demoReadme.Should().Contain("evidence-only, not a pixel-perfect visual-regression gate");
        demoReadme.Should().Contain("not a stable benchmark guarantee");
        demoReadme.Should().Contain("no hard performance guarantee from the demo scenarios");
        demoReadme.Should().Contain("benchmark thresholds remain in the dedicated benchmark gate files");
        demoReadme.Should().Contain("no GPU-driven culling");
        demoReadme.Should().Contain("no scenario/data-path fallback");
        demoReadme.Should().Contain("not a compatibility or parity layer");
        demoReadme.Should().Contain("image/PDF/vector export");

        cutover.Should().Contain("not benchmark truth");
        cutover.Should().Contain("GPU performance guarantee");
        cutover.Should().Contain("backend fallback proof");
        cutover.Should().Contain("PDF/vector export");
        cutover.Should().Contain("not an API compatibility, parity, adapter, or migration layer");

        rootReadme.Should().Contain("evidence-only results");
        rootReadme.Should().Contain("intentionally absent from `benchmark-thresholds.json` until CI history supports hard thresholds");
        rootReadme.Should().Contain("PNG-only chart snapshots");

        benchmarkGates.Should().Contain("Benchmark names listed in `benchmark-contract.json` but not in `benchmark-thresholds.json` remain evidence-only");
        benchmarkGates.Should().Contain("The SurfaceCharts streaming benchmarks are also evidence-only");
        benchmarkGates.Should().Contain("not hard gates");
        benchmarkGates.Should().Contain("hard numeric blocker for the committed threshold slice");

        supportSummary.Should().Contain("EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.");
        mainWindowCodeBehind.Should().Contain("there was no scenario/data-path fallback");
        supportSummary.Should().Contain("no fallback active");
    }

    [Fact]
    public void SurfaceChartsDemoDocs_ShouldNotInventAdHocPerformanceMetrics()
    {
        var repositoryRoot = GetRepositoryRoot();
        var checkedDocuments = new[]
        {
            ("README.md", Read(repositoryRoot, "README.md")),
            ("samples/Videra.SurfaceCharts.Demo/README.md", Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md")),
            ("docs/surfacecharts-release-cutover.md", Read(repositoryRoot, "docs", "surfacecharts-release-cutover.md")),
            ("samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs", Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Views", "MainWindow.axaml.cs")),
        };

        foreach (var (path, text) in checkedDocuments)
        {
            text.Should().NotContain("frames per second", because: $"{path} must not turn demo evidence into an FPS claim");
            text.Should().NotContain("FPS", because: $"{path} must not turn demo evidence into an FPS claim");
            text.Should().NotContain("throughput guarantee", because: $"{path} must not invent throughput guarantees");
            text.Should().NotContain("latency guarantee", because: $"{path} must not invent latency guarantees");
            text.Should().NotContain("guaranteed performance", because: $"{path} must not invent performance guarantees");
        }
    }

    private static string Read(string repositoryRoot, params string[] pathParts)
    {
        return File.ReadAllText(Path.Combine([repositoryRoot, .. pathParts]));
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Videra.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing Videra.slnx.");
    }
}
