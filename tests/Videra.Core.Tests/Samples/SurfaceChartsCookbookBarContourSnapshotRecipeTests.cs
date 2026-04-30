using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsCookbookBarContourSnapshotRecipeTests
{
    [Fact]
    public void BarRecipe_ShouldDocumentBoundedVideraNativePlotAddBarPath()
    {
        var recipe = ReadRecipe("bar.md");

        AssertContainsAll(recipe,
            "Plot.Add.Bar",
            "VideraChartView",
            "BarChartData",
            "BarSeries",
            "BarChartLayout.Grouped",
            "Try next: Bar chart proof",
            "VideraChartView.Plot.Add.Bar",
            "EvidenceKind: SurfaceChartsDatasetProof",
            "Plot path: Try next: Bar chart proof",
            "OutputCapabilityDiagnostics",
            "not stable benchmark");

        recipe.Should().Contain("SurfaceChartView");
        recipe.Should().Contain("VideraChartView.Source");
        AssertContainsNone(recipe, ForbiddenImplementationTokens);
    }

    [Fact]
    public void ContourRecipe_ShouldDocumentBoundedVideraNativePlotAddContourPath()
    {
        var recipe = ReadRecipe("contour.md");

        AssertContainsAll(recipe,
            "Plot.Add.Contour",
            "VideraChartView",
            "ContourChartData",
            "SurfaceScalarField",
            "SurfaceValueRange",
            "Try next: Contour plot proof",
            "VideraChartView.Plot.Add.Contour",
            "EvidenceKind: SurfaceChartsContourDatasetProof",
            "ContourRenderingStatus",
            "DatasetEvidenceKind",
            "OutputCapabilityDiagnostics");

        AssertContainsNone(recipe, ForbiddenImplementationTokens);
    }

    [Fact]
    public void SupportEvidenceRecipe_ShouldPinCopySupportSummaryFieldsAndEvidenceBoundaries()
    {
        var recipe = ReadRecipe("support-evidence.md");

        AssertContainsAll(recipe,
            "Copy support summary",
            "surfacecharts-support-summary.txt",
            "SurfaceCharts support summary",
            "EvidenceOnly: true - values are support evidence, not stable benchmark guarantees.",
            "ChartControl",
            "EnvironmentRuntime",
            "AssemblyIdentity",
            "BackendDisplayEnvironment",
            "CacheLoadFailure",
            "OutputCapabilityDiagnostics",
            "SnapshotStatus",
            "SnapshotOutputEvidenceKind",
            "SnapshotDatasetEvidenceKind",
            "SnapshotActiveSeriesIdentity",
            "SnapshotCreatedUtc",
            "DatasetActiveSeriesMetadata",
            "ScenarioId",
            "ContourRenderingStatus",
            "consumer-smoke-result.json",
            "chart-snapshot.png");

        recipe.Should().Contain("does not claim stable");
        recipe.Should().Contain("performance numbers");
        recipe.Should().Contain("does not silently switch scenarios");
        AssertContainsNone(recipe, ForbiddenImplementationTokens);
    }

    [Fact]
    public void PngSnapshotRecipe_ShouldDocumentManifestTruthAndPngOnlyBitmapBoundary()
    {
        var recipe = ReadRecipe("png-snapshot.md");

        AssertContainsAll(recipe,
            "Plot.SavePngAsync",
            "CaptureSnapshotAsync",
            "PlotSnapshotRequest",
            "PlotSnapshotResult",
            "PlotSnapshotManifest",
            "PlotSnapshotFormat.Png",
            "PlotSnapshotBackground.Transparent",
            "Manifest.OutputEvidenceKind",
            "Manifest.DatasetEvidenceKind",
            "Manifest.ActiveSeriesIdentity",
            "SnapshotStatus",
            "SnapshotFormat",
            "snapshot.chart.no-active-series",
            "snapshot.format.unsupported",
            "snapshot.render.no-host",
            "ImageExport",
            "PdfExport",
            "VectorExport",
            "PNG-only bitmap export",
            "RenderTargetBitmap");

        AssertContainsNone(recipe, ForbiddenImplementationTokens);
    }

    [Fact]
    public void Phase410DSummary_ShouldListDeliverablesVerificationAndScopeGuardrails()
    {
        var summary = Read(Path.Combine(
            ".planning",
            "phases",
            "410-detailed-3d-cookbook-demo-recipes",
            "410D-SUMMARY.md"));

        AssertContainsAll(summary,
            "Videra-d29",
            "Plot.Add.Bar",
            "Plot.Add.Contour",
            "Copy support summary",
            "Plot.SavePngAsync",
            "CaptureSnapshotAsync",
            "PNG-only bitmap",
            "SurfaceChartsCookbookBarContourSnapshotRecipeTests",
            "git diff --check",
            "PlotSnapshotContractTests",
            "PlotSnapshotCaptureTests");

        AssertContainsNone(summary, ForbiddenImplementationTokens);
    }

    private static readonly string[] ForbiddenImplementationTokens =
    [
        "SavePdfAsync",
        "SaveSvgAsync",
        "ExportPdfAsync",
        "ExportVectorAsync",
        "ExportSvgAsync",
        "new SurfaceChartView",
        "new WaterfallChartView",
        "new ScatterChartView",
        "OpenGLBackend",
        "WebGLBackend",
        "ICompatibilityWrapper",
        "ScottPlotAdapter",
        "AutomaticDownshiftPolicy",
        "silent fallback",
        "FakePerformanceEvidence",
    ];

    private static void AssertContainsAll(string text, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            text.Should().Contain(token);
        }
    }

    private static void AssertContainsNone(string text, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            text.Should().NotContain(token);
        }
    }

    private static string ReadRecipe(string fileName)
    {
        return Read(Path.Combine("samples", "Videra.SurfaceCharts.Demo", "Recipes", fileName));
    }

    private static string Read(string relativePath)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));
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
