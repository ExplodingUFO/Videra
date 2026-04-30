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
