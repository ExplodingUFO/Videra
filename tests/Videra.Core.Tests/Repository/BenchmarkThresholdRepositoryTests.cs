using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class BenchmarkThresholdRepositoryTests
{
    [Fact]
    public void BenchmarkThresholdContract_ShouldDefineRepresentativeViewerAndSurfaceChartsSlices()
    {
        var repositoryRoot = GetRepositoryRoot();
        var thresholdPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-thresholds.json");

        File.Exists(thresholdPath).Should().BeTrue();

        using var thresholdDocument = JsonDocument.Parse(File.ReadAllText(thresholdPath));
        var root = thresholdDocument.RootElement;
        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);

        var suites = root.GetProperty("suites").EnumerateArray().ToDictionary(
            static suite => suite.GetProperty("name").GetString()!,
            static suite => suite);

        suites.Keys.Should().BeEquivalentTo("Viewer", "SurfaceCharts");

        AssertSuite(
            suites["Viewer"],
            2,
            new Dictionary<string, int>
            {
                ["Videra.Viewer.Benchmarks.ScenePipelineBenchmarks.ScenePipeline_RehydrateAfterBackendReady"] = 150,
                ["Videra.Viewer.Benchmarks.InspectionBenchmarks.SceneHitTest_MeshAccurateDistance"] = 150
            });

        AssertSuite(
            suites["SurfaceCharts"],
            2,
            new Dictionary<string, int>
            {
                ["Videra.SurfaceCharts.Benchmarks.SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement"] = 100,
                ["Videra.SurfaceCharts.Benchmarks.SurfaceChartsProbeBenchmarks.ProbeLatency"] = 150
            });
    }

    private static void AssertSuite(JsonElement suite, int expectedCount, IReadOnlyDictionary<string, int> expectedThresholds)
    {
        var thresholds = suite.GetProperty("thresholds").EnumerateArray().ToArray();
        thresholds.Should().HaveCount(expectedCount);

        var actualThresholds = thresholds.ToDictionary(
            static threshold => threshold.GetProperty("benchmark").GetString()!,
            static threshold => threshold);

        actualThresholds.Keys.Should().BeEquivalentTo(expectedThresholds.Keys);

        foreach (var (benchmark, maxRegressionPercent) in expectedThresholds)
        {
            var threshold = actualThresholds[benchmark];
            threshold.GetProperty("baselineMeanNs").GetDouble().Should().BeGreaterThan(0d);
            threshold.GetProperty("maxRegressionPercent").GetInt32().Should().Be(maxRegressionPercent);
        }
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
