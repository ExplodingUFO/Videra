using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsCookbookWaterfallLinkedRecipeTests
{
    [Fact]
    public void WaterfallRecipe_ShouldUseNativeWaterfallAxesOverlayAndViewStateTokens()
    {
        var recipe = ReadRecipe("waterfall.md");

        AssertContainsAll(recipe,
        [
            "`VideraChartView`",
            "`Plot.Add.Waterfall`",
            "chart.Plot.Add.Waterfall",
            "chart.Plot.Axes.X.Label",
            "chart.Plot.Axes.Y.Label",
            "chart.Plot.Axes.Z.Label",
            "chart.Plot.Axes.X.SetBounds",
            "chart.Plot.Axes.Z.SetBounds",
            "chart.Plot.OverlayOptions = new SurfaceChartOverlayOptions",
            "SurfaceChartGridPlane.XZ",
            "SurfaceChartAxisSideMode.Auto",
            "SurfaceChartNumericLabelFormat.Engineering",
            "`VideraChartView.ViewState`",
            "chart.ViewState = savedViewState",
            "chart.FitToData();",
            "`RenderingStatus`",
            "`Plot.OverlayOptions`",
        ]);
    }

    [Fact]
    public void AxesAndLinkedViewsRecipe_ShouldPinDisposableLinkLifetimeAndFitToDataSemantics()
    {
        var recipe = ReadRecipe("axes-and-linked-views.md");

        AssertContainsAll(recipe,
        [
            "`LinkViewWith` returns an `IDisposable` lifetime",
            "using var linkedView = left.LinkViewWith(right);",
            "private IDisposable? _linkedView;",
            "_linkedView = left.LinkViewWith(right);",
            "_linkedView?.Dispose();",
            "left.Plot.Add.Waterfall",
            "right.Plot.Add.Waterfall",
            "chart.Plot.Axes.X.SetLimits",
            "chart.Plot.Axes.Y.SetBounds",
            "chart.Plot.Axes.AutoScale();",
            "chart.Plot.OverlayOptions = new SurfaceChartOverlayOptions",
            "XAxisFormatter",
            "YAxisFormatter",
            "ZAxisFormatter",
            "SurfaceChartLegendPosition.TopRight",
            "left.FitToData();",
            "right.FitToData();",
            "left.ViewState",
            "right.ViewState",
            "TryCreateProbeAnnotationAnchor",
            "TryCreateSelectionMeasurementReport",
            "SelectionReported",
            "SurfaceChartAnnotationAnchor",
            "SurfaceChartMeasurementReport",
            "SurfaceChartSelectionReport",
        ]);
    }

    [Fact]
    public void Recipes_ShouldAvoidScopeCreepTerms()
    {
        foreach (var recipeName in RecipeNames)
        {
            var recipe = ReadRecipe(recipeName);

            foreach (var forbiddenTerm in ForbiddenScopeCreepTerms)
            {
                recipe.Should().NotContain(forbiddenTerm, $"{recipeName} must stay within the native SurfaceCharts cookbook scope.");
            }
        }
    }

    private static readonly string[] RecipeNames =
    [
        "waterfall.md",
        "axes-and-linked-views.md",
    ];

    private static readonly string[] ForbiddenScopeCreepTerms =
    [
        "SurfaceChartView",
        "WaterfallChartView",
        "ScatterChartView",
        ".Source",
        "compatibility wrapper",
        "compatibility layer",
        "adapter layer",
        "migration shim",
        "PDF",
        "vector export",
        "OpenGL",
        "WebGL",
        "backend expansion",
        "downshift",
        "hidden fallback",
        "fallback proof",
        "generic plotting engine",
        "generic chart editor",
        "workbench",
        "benchmark guarantee",
    ];

    private static void AssertContainsAll(string actual, IReadOnlyList<string> expectedTokens)
    {
        foreach (var expectedToken in expectedTokens)
        {
            actual.Should().Contain(expectedToken);
        }
    }

    private static string ReadRecipe(string recipeName)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.SurfaceCharts.Demo", "Recipes", recipeName));
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
