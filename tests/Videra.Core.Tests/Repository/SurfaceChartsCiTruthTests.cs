using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsCiTruthTests
{
    [Fact]
    public void CiWorkflow_ShouldRunFocusedSurfaceChartsSampleEvidenceWithoutFakeGreen()
    {
        var workflow = ReadWorkflow();
        var sampleEvidenceStepIndex = workflow.IndexOf(
            "Run SurfaceCharts sample evidence",
            StringComparison.Ordinal);

        sampleEvidenceStepIndex.Should().BeGreaterThanOrEqualTo(0);
        var sampleEvidenceStep = GetWorkflowStep(workflow, sampleEvidenceStepIndex);

        AssertContainsAll(sampleEvidenceStep,
            "SurfaceChartsDemoConfigurationTests",
            "SurfaceChartsDemoViewportBehaviorTests",
            "SurfaceChartsCookbookCoverageMatrixTests",
            "SurfaceChartsCookbookFirstSurfaceRecipeTests",
            "SurfaceChartsCookbookWaterfallLinkedRecipeTests",
            "SurfaceChartsCookbookScatterLiveRecipeTests",
            "SurfaceChartsCookbookBarContourSnapshotRecipeTests",
            "SurfaceChartsHighPerformancePathTests",
            "ScatterStreamingScenarioEvidenceTests",
            "SurfaceChartsPerformanceTruthTests",
            "SurfaceChartWorkspaceTests",
            "SurfaceChartLinkGroupTests",
            "SurfaceChartInteractionPropagatorTests",
            "SurfaceChartStreamingEvidenceTests");

        AssertDoesNotMaskValidationFailure(sampleEvidenceStep);
    }

    [Fact]
    public void CiWorkflow_ShouldReserveAlwaysConditionForArtifactUploads()
    {
        var workflow = ReadWorkflow();
        var searchIndex = 0;

        while (true)
        {
            var alwaysIndex = workflow.IndexOf("if: always()", searchIndex, StringComparison.Ordinal);
            if (alwaysIndex < 0)
            {
                break;
            }

            var stepStartIndex = workflow.LastIndexOf("\n      - ", alwaysIndex, StringComparison.Ordinal);
            stepStartIndex.Should().BeGreaterThanOrEqualTo(0);

            var step = GetWorkflowStep(workflow, stepStartIndex);
            step.Should().Contain("uses: actions/upload-artifact");

            searchIndex = alwaysIndex + "if: always()".Length;
        }
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

        AssertDoesNotMaskValidationFailure(surfaceChartsEvidenceStep);
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
            "VideraChartViewPlotApiTests",
            "SurfaceChartWorkspaceTests",
            "SurfaceChartLinkGroupTests");

        AssertDoesNotMaskValidationFailure(runtimeStep);
    }

    [Fact]
    public void CiWorkflow_ShouldRunAndValidatePackagedSurfaceChartsSmokeWithoutFakeGreen()
    {
        var workflow = ReadWorkflow();
        var smokeStepIndex = workflow.IndexOf(
            "Run packaged SurfaceCharts consumer smoke",
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
            "artifacts/surfacecharts-consumer-smoke-quality");

        smokeStep.Should().NotContain("-BuildOnly");
        AssertDoesNotMaskValidationFailure(smokeStep);

        var validationStep = GetWorkflowStep(workflow, validationStepIndex);
        AssertContainsAll(validationStep,
            "scripts/Validate-Packages.ps1",
            "artifacts/surfacecharts-consumer-smoke-quality/packages",
            "consumer-smoke-version.outputs.package_version");

        AssertDoesNotMaskValidationFailure(validationStep);
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

    private static void AssertDoesNotMaskValidationFailure(string workflowStep)
    {
        workflowStep.Should().NotContain("continue-on-error");
        workflowStep.Should().NotContain("|| true");
        workflowStep.Should().NotContain("if: always()");
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
