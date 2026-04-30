using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsCiTruthTests
{
    [Fact]
    public void CiWorkflow_ShouldRunFocusedSurfaceChartsCookbookEvidence()
    {
        var workflow = ReadWorkflow();

        AssertContainsAll(workflow,
            "Run SurfaceCharts sample evidence",
            "SurfaceChartsDemoConfigurationTests",
            "SurfaceChartsDemoViewportBehaviorTests",
            "SurfaceChartsCookbookCoverageMatrixTests",
            "SurfaceChartsCookbookFirstSurfaceRecipeTests",
            "SurfaceChartsCookbookWaterfallLinkedRecipeTests",
            "SurfaceChartsCookbookScatterLiveRecipeTests",
            "SurfaceChartsCookbookBarContourSnapshotRecipeTests",
            "SurfaceChartsHighPerformancePathTests",
            "ScatterStreamingScenarioEvidenceTests",
            "SurfaceChartsPerformanceTruthTests");
    }

    [Fact]
    public void CiWorkflow_ShouldRunGeneratedRoadmapAndScopeEvidenceWithoutFakeGreen()
    {
        var workflow = ReadWorkflow();
        var surfaceChartsEvidenceStepIndex = workflow.IndexOf(
            "Run SurfaceCharts generated roadmap and scope evidence",
            StringComparison.Ordinal);

        surfaceChartsEvidenceStepIndex.Should().BeGreaterThanOrEqualTo(0);
        var surfaceChartsEvidenceStep = GetWorkflowStep(workflow, surfaceChartsEvidenceStepIndex);

        AssertContainsAll(surfaceChartsEvidenceStep,
            "scripts/Test-SnapshotExportScope.ps1",
            "BeadsPublicRoadmapTests");

        surfaceChartsEvidenceStep.Should().NotContain("continue-on-error: true");
        surfaceChartsEvidenceStep.Should().NotContain("|| true");
        surfaceChartsEvidenceStep.Should().NotContain("if: always()");
    }

    [Fact]
    public void CiWorkflow_ShouldRunSurfaceChartsRuntimeEvidenceWithoutFakeGreen()
    {
        var workflow = ReadWorkflow();
        var runtimeStepIndex = workflow.IndexOf(
            "Run SurfaceCharts runtime evidence",
            StringComparison.Ordinal);

        runtimeStepIndex.Should().BeGreaterThanOrEqualTo(0);
        var runtimeStep = GetWorkflowStep(workflow, runtimeStepIndex);

        AssertContainsAll(runtimeStep,
            "Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj",
            "VideraChartViewStateTests",
            "SurfaceChartInteractionTests",
            "VideraChartViewGpuFallbackTests",
            "VideraChartViewWaterfallIntegrationTests",
            "VideraChartViewPlotApiTests");

        runtimeStep.Should().NotContain("continue-on-error: true");
        runtimeStep.Should().NotContain("|| true");
        runtimeStep.Should().NotContain("if: always()");
    }

    [Fact]
    public void CiWorkflow_ShouldRunAndValidatePackagedSurfaceChartsSmokeWithoutFakeGreen()
    {
        var workflow = ReadWorkflow();
        var smokeStepIndex = workflow.IndexOf(
            "Run packaged SurfaceCharts consumer smoke with warnings as errors",
            StringComparison.Ordinal);
        var validationStepIndex = workflow.IndexOf(
            "Validate packaged SurfaceCharts consumer smoke artifacts",
            StringComparison.Ordinal);

        smokeStepIndex.Should().BeGreaterThanOrEqualTo(0);
        validationStepIndex.Should().BeGreaterThan(smokeStepIndex);

        var smokeStep = GetWorkflowStep(workflow, smokeStepIndex);
        AssertContainsAll(smokeStep,
            "scripts/Invoke-ConsumerSmoke.ps1",
            "-Scenario SurfaceCharts",
            "artifacts/surfacecharts-consumer-smoke-quality",
            "-TreatWarningsAsErrors");

        smokeStep.Should().NotContain("-BuildOnly");
        smokeStep.Should().NotContain("continue-on-error: true");
        smokeStep.Should().NotContain("|| true");
        smokeStep.Should().NotContain("if: always()");

        var validationStep = GetWorkflowStep(workflow, validationStepIndex);
        AssertContainsAll(validationStep,
            "scripts/Validate-Packages.ps1",
            "artifacts/surfacecharts-consumer-smoke-quality/packages",
            "consumer-smoke-version.outputs.package_version");

        validationStep.Should().NotContain("continue-on-error: true");
        validationStep.Should().NotContain("|| true");
        validationStep.Should().NotContain("if: always()");
    }

    private static string ReadWorkflow()
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "ci.yml"));
    }

    private static void AssertContainsAll(string text, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            text.Should().Contain(token);
        }
    }

    private static string GetWorkflowStep(string workflow, int stepNameIndex)
    {
        var nextStepIndex = workflow.IndexOf("\n      - name:", stepNameIndex + 1, StringComparison.Ordinal);
        return nextStepIndex < 0
            ? workflow[stepNameIndex..]
            : workflow[stepNameIndex..nextStepIndex];
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
