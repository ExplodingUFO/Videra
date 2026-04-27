using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class BenchmarkThresholdRepositoryTests
{
    [Fact]
    public void BenchmarkThresholdContract_ShouldDefineCommittedViewerAndSurfaceChartsHardGateSlices()
    {
        var repositoryRoot = GetRepositoryRoot();
        var benchmarkContractPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-contract.json");
        var thresholdPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-thresholds.json");

        File.Exists(benchmarkContractPath).Should().BeTrue();
        File.Exists(thresholdPath).Should().BeTrue();

        var benchmarkContractNames = ReadBenchmarkContractNames(benchmarkContractPath);
        using var thresholdDocument = JsonDocument.Parse(File.ReadAllText(thresholdPath));
        var root = thresholdDocument.RootElement;
        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);

        var suites = root.GetProperty("suites").EnumerateArray().ToDictionary(
            static suite => suite.GetProperty("name").GetString()!,
            static suite => suite);

        suites.Keys.Should().BeEquivalentTo("Viewer", "SurfaceCharts");

        AssertSuite(
            suites["Viewer"],
            5,
            new Dictionary<string, int>
            {
                ["Videra.Viewer.Benchmarks.ScenePipelineBenchmarks.SceneResidencyRegistry_ApplyDelta"] = 150,
                ["Videra.Viewer.Benchmarks.ScenePipelineBenchmarks.SceneUploadQueue_Drain"] = 150,
                ["Videra.Viewer.Benchmarks.ScenePipelineBenchmarks.ScenePipeline_RehydrateAfterBackendReady"] = 50,
                ["Videra.Viewer.Benchmarks.InspectionBenchmarks.SceneHitTest_MeshAccurateDistance"] = 155,
                ["Videra.Viewer.Benchmarks.InspectionBenchmarks.SnapshotExport_LiveReadbackFastPath"] = 150
            },
            benchmarkContractNames);

        AssertSuite(
            suites["SurfaceCharts"],
            2,
            new Dictionary<string, int>
            {
                ["Videra.SurfaceCharts.Benchmarks.SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement"] = 150,
                ["Videra.SurfaceCharts.Benchmarks.SurfaceChartsProbeBenchmarks.ProbeLatency"] = 151
            },
            benchmarkContractNames);
    }

    private static void AssertSuite(
        JsonElement suite,
        int expectedCount,
        IReadOnlyDictionary<string, int> expectedThresholds,
        IReadOnlySet<string> benchmarkContractNames)
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
            benchmarkContractNames.Should().Contain(ToBenchmarkContractName(benchmark));
        }
    }

    private static HashSet<string> ReadBenchmarkContractNames(string benchmarkContractPath)
    {
        using var benchmarkDocument = JsonDocument.Parse(File.ReadAllText(benchmarkContractPath));

        return benchmarkDocument.RootElement.GetProperty("suites")
            .EnumerateArray()
            .SelectMany(static suite => suite.GetProperty("families").EnumerateArray())
            .SelectMany(static family => family.GetProperty("benchmarks").EnumerateArray())
            .Select(static benchmark => benchmark.GetString()!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string ToBenchmarkContractName(string benchmarkName)
    {
        const string BenchmarksSegment = ".Benchmarks.";

        var index = benchmarkName.IndexOf(BenchmarksSegment, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException($"Benchmark threshold name '{benchmarkName}' does not include '{BenchmarksSegment}'.");
        }

        return benchmarkName[(index + BenchmarksSegment.Length)..];
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
