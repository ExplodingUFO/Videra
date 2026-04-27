using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class BenchmarkContractRepositoryTests
{
    [Fact]
    public void BenchmarkContract_ShouldDefineStableViewerAndSurfaceChartsSuites()
    {
        var repositoryRoot = GetRepositoryRoot();
        var contractPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-contract.json");

        File.Exists(contractPath).Should().BeTrue();

        using var contract = JsonDocument.Parse(File.ReadAllText(contractPath));
        var root = contract.RootElement;
        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);

        var suites = root.GetProperty("suites").EnumerateArray().ToDictionary(
            static suite => suite.GetProperty("name").GetString()!,
            static suite => suite);

        suites.Keys.Should().BeEquivalentTo("Viewer", "SurfaceCharts");

        AssertSuite(
            suites["Viewer"],
            "viewer",
            "benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj",
            [
                "ScenePipelineBenchmarks.ModelImporter_Import",
                "ScenePipelineBenchmarks.SceneImportService_ImportBatchAsync",
                "ScenePipelineBenchmarks.SceneResidencyRegistry_ApplyDelta",
                "ScenePipelineBenchmarks.SceneUploadQueue_Drain",
                "ScenePipelineBenchmarks.ScenePipeline_RehydrateAfterBackendReady",
                "InspectionBenchmarks.SceneHitTest_MeshAccurateDistance",
                "InspectionBenchmarks.ClipPayload_CachedPlaneSignature",
                "InspectionBenchmarks.SnapshotExport_LiveReadbackFastPath",
                "InstanceBatchBenchmarks.NormalObjects_SceneDocumentPopulation",
                "InstanceBatchBenchmarks.InstanceBatch_SceneDocumentPopulation",
                "InstanceBatchBenchmarks.NormalObjects_HitTestPickLatency",
                "InstanceBatchBenchmarks.InstanceBatch_HitTestPickLatency",
                "InstanceBatchBenchmarks.InstanceBatch_DiagnosticsSnapshotEvidence",
                "DemoDiagnosticsBenchmarks.FormatImportReport",
                "DemoDiagnosticsBenchmarks.BuildDiagnosticsBundle"
            ]);

        AssertSuite(
            suites["SurfaceCharts"],
            "surfacecharts",
            "benchmarks/Videra.SurfaceCharts.Benchmarks/Videra.SurfaceCharts.Benchmarks.csproj",
            [
                "SurfaceChartsSelectionBenchmarks.SelectViewportTiles",
                "SurfaceChartsSelectionBenchmarks.SelectCameraAwareViewportTiles",
                "SurfaceChartsSelectionBenchmarks.BuildPyramidWithStatistics",
                "SurfaceChartsRenderStateBenchmarks.BuildResidentRenderState",
                "SurfaceChartsRenderStateBenchmarks.ApplyColorMapChangeToResidentRenderState",
                "SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement",
                "SurfaceChartsCacheBenchmarks.ReadBatchFromCache",
                "SurfaceChartsCacheBenchmarks.LookupCacheMissBurst",
                "SurfaceChartsProbeBenchmarks.ProbeLatency",
                "SurfaceChartsRenderHostContractBenchmarks.RecolorResidentTilesGpuContractPath",
                "SurfaceChartsRenderHostContractBenchmarks.OrbitInteractiveFrameGpuContractPath",
                "SurfaceChartsRenderHostContractBenchmarks.ResizeAndRebindHandleGpuContractPath",
                "SurfaceChartsDiagnosticsBenchmarks.RenderingStatus_FromSnapshot",
                "SurfaceChartsDiagnosticsBenchmarks.SupportSummaryFormatting",
                "SurfaceChartsStreamingBenchmarks.AppendColumnarBatch",
                "SurfaceChartsStreamingBenchmarks.AppendColumnarBatch_WithFifoTrim",
                "SurfaceChartsStreamingBenchmarks.StreamingDiagnosticsAggregation"
            ]);
    }

    [Fact]
    public void BenchmarkDocs_ShouldKeepSurfaceChartsStreamingBenchmarksEvidenceOnly()
    {
        var repositoryRoot = GetRepositoryRoot();
        var runbook = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "benchmark-gates.md"));
        var thresholds = File.ReadAllText(Path.Combine(repositoryRoot, "benchmarks", "benchmark-thresholds.json"));

        runbook.Should().Contain("SurfaceChartsStreamingBenchmarks.cs");
        runbook.Should().Contain("SurfaceChartsStreamingBenchmarks.AppendColumnarBatch");
        runbook.Should().Contain("AppendColumnarBatch_WithFifoTrim");
        runbook.Should().Contain("StreamingDiagnosticsAggregation");
        runbook.Should().Contain("evidence-only");
        runbook.Should().Contain("Mean` / `Allocated");

        thresholds.Should().NotContain("SurfaceChartsStreamingBenchmarks");
    }

    private static void AssertSuite(JsonElement suite, string artifactDirectory, string projectPath, string[] benchmarks)
    {
        suite.GetProperty("artifactDirectory").GetString().Should().Be(artifactDirectory);
        suite.GetProperty("project").GetString().Should().Be(projectPath);

        var definedBenchmarks = suite.GetProperty("families")
            .EnumerateArray()
            .SelectMany(static family => family.GetProperty("benchmarks").EnumerateArray())
            .Select(static benchmark => benchmark.GetString())
            .ToArray();

        definedBenchmarks.Should().BeEquivalentTo(benchmarks);
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
