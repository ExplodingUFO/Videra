using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsCookbookFirstSurfaceRecipeTests
{
    [Theory]
    [MemberData(nameof(RecipeExpectations))]
    public void SurfaceChartsCookbookRecipes_ShouldUseNativeSurfaceChartContracts(
        string relativePath,
        string[] requiredTokens)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(requiredTokens);

        var repositoryRoot = GetRepositoryRoot();
        var recipePath = Path.Combine([repositoryRoot, .. relativePath.Split('/')]);

        File.Exists(recipePath).Should().BeTrue($"{relativePath} should exist.");

        var recipe = File.ReadAllText(recipePath);

        foreach (var token in requiredTokens)
        {
            recipe.Should().Contain(token, $"{relativePath} should pin the native cookbook contract.");
        }

        foreach (var forbiddenTerm in ForbiddenTerms)
        {
            recipe.Should().NotContain(forbiddenTerm, $"{relativePath} should stay inside Phase 410A scope.");
        }
    }

    public static TheoryData<string, string[]> RecipeExpectations()
    {
        return new TheoryData<string, string[]>
        {
            {
                "samples/Videra.SurfaceCharts.Demo/Recipes/first-chart.md",
                [
                    "VideraChartView",
                    "Plot.Add.Surface",
                    "SurfaceMatrix",
                    "SurfacePyramidBuilder",
                    "FitToData",
                    "ViewState",
                    "support evidence"
                ]
            },
            {
                "samples/Videra.SurfaceCharts.Demo/Recipes/surface-cache-backed.md",
                [
                    "VideraChartView",
                    "Plot.Add.Surface",
                    "SurfaceMatrix",
                    "SurfacePyramidBuilder",
                    "SurfaceCacheReader",
                    "SurfaceCacheTileSource",
                    "FitToData",
                    "ViewState",
                    "support evidence"
                ]
            }
        };
    }

    private static readonly string[] ForbiddenTerms =
    [
        "ScottPlot",
        "SurfaceChartView",
        "WaterfallChartView",
        "ScatterChartView",
        "Source API",
        "compatibility layer",
        "fallback",
        "downshift",
        "wrapper",
        "benchmark",
        "god-code"
    ];

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
