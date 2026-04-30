using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsCookbookScatterLiveRecipeTests
{
    [Fact]
    public void ScatterLiveRecipe_ShouldPinVideraNativeApiAndEvidenceTokens()
    {
        var recipe = ReadRecipe();

        foreach (var token in RequiredTokens)
        {
            recipe.Should().Contain(token);
        }
    }

    [Fact]
    public void ScatterLiveRecipe_ShouldAvoidPerformancePromisesAndScopeExpansion()
    {
        var recipe = ReadRecipe();

        foreach (var token in ForbiddenTokens)
        {
            recipe.Should().NotContain(token);
        }
    }

    private static readonly IReadOnlyList<string> RequiredTokens =
    [
        "Plot.Add.Scatter",
        "DataLogger3D",
        "ScatterColumnarData",
        "ScatterColumnarSeries",
        "ScatterChartData",
        "Pickable=false",
        "fifoCapacity",
        "UseLatestWindow",
        "CreateLiveViewEvidence",
        "DataLogger3DLiveViewMode.FullData",
        "DataLogger3DLiveViewMode.LatestWindow",
        "DataLogger3DAutoscaleDecision.FullData",
        "DataLogger3DAutoscaleDecision.LatestWindow",
        "AppendedPointCount",
        "DroppedPointCount",
        "RetainedPointCount",
        "VisibleStartIndex",
        "VisiblePointCount",
        "ColumnarPointCount",
        "PickablePointCount",
        "StreamingAppendBatchCount",
        "StreamingReplaceBatchCount",
        "StreamingDroppedPointCount",
        "LastStreamingDroppedPointCount",
        "ConfiguredFifoCapacity",
        "InteractionQuality",
        "support summary",
        "no wrapper chart",
        "no fallback path",
        "no downshift",
    ];

    private static readonly IReadOnlyList<string> ForbiddenTokens =
    [
        "performance guarantee",
        "performance guarantees",
        "benchmark guarantee",
        "benchmark guarantees",
        "guaranteed FPS",
        "guaranteed frame",
        "guaranteed latency",
        "always faster",
        "zero-copy",
        "GPU-driven culling",
        "compatibility wrapper",
        "fallback renderer",
        "automatic downshift",
        "generic plotting engine",
        "general-purpose workbench",
    ];

    private static string ReadRecipe()
    {
        var repositoryRoot = GetRepositoryRoot();
        return File.ReadAllText(Path.Combine(
            repositoryRoot,
            "samples",
            "Videra.SurfaceCharts.Demo",
            "Recipes",
            "scatter-and-live-data.md"));
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
