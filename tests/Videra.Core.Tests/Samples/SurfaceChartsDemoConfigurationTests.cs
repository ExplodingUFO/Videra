using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsDemoConfigurationTests
{
    [Fact]
    public void SurfaceChartsCookbookDocs_ShouldStayAlignedWithVisibleDemoProofs()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));
        var cutover = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "surfacecharts-release-cutover.md"));
        var mainWindowCodeBehind = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Views", "MainWindow.axaml.cs"));
        var sampleData = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Services", "SampleDataFactory.cs"));
        var cookbookRecipeCatalog = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Services", "CookbookRecipeCatalog.cs"));
        var scenarioCatalog = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Services", "SurfaceDemoScenario.cs"));

        foreach (var document in new[] { rootReadme, demoReadme, cutover })
        {
            document.Should().Contain("Plot.Add.Bar");
            document.Should().Contain("Plot.Add.Contour");
        }

        foreach (var document in new[] { demoReadme, cutover, scenarioCatalog })
        {
            document.Should().Contain("Try next: Bar chart proof");
            document.Should().Contain("Try next: Contour plot proof");
        }

        demoReadme.Should().Contain("### Bar");
        demoReadme.Should().Contain("### Contour");
        cookbookRecipeCatalog.Should().Contain("\"Bar\"");
        cookbookRecipeCatalog.Should().Contain("\"Contour\"");
        mainWindowCodeBehind.Should().NotContain("new ScatterColumnarData(x, y, z)");
        sampleData.Should().Contain("new ScatterChartData");
    }

    [Fact]
    public void SurfaceChartsDemo_ShouldExistAsAnIndependentSurfaceChartEntryPoint()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadmePath = Path.Combine(repositoryRoot, "README.md");
        var demoRoot = Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo");
        var demoProjectPath = Path.Combine(demoRoot, "Videra.SurfaceCharts.Demo.csproj");
        var demoReadmePath = Path.Combine(demoRoot, "README.md");
        var mainWindowXamlPath = Path.Combine(demoRoot, "Views", "MainWindow.axaml");
        var mainWindowCodeBehindPath = Path.Combine(demoRoot, "Views", "MainWindow.axaml.cs");
        var mainWindowSampleDataPath = Path.Combine(demoRoot, "Services", "SampleDataFactory.cs");
        var cookbookRecipeCatalogPath = Path.Combine(demoRoot, "Services", "CookbookRecipeCatalog.cs");
        var scenarioCatalogPath = Path.Combine(demoRoot, "Services", "SurfaceDemoScenario.cs");
        var supportSummaryPath = Path.Combine(demoRoot, "Services", "SurfaceDemoSupportSummary.cs");
        var appXamlPath = Path.Combine(demoRoot, "App.axaml");
        var cacheAssetPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json");
        var cachePayloadPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json.bin");

        File.Exists(demoProjectPath).Should().BeTrue();
        File.Exists(rootReadmePath).Should().BeTrue();
        File.Exists(demoReadmePath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(mainWindowCodeBehindPath).Should().BeTrue();
        File.Exists(mainWindowSampleDataPath).Should().BeTrue();
        File.Exists(cookbookRecipeCatalogPath).Should().BeTrue();
        File.Exists(scenarioCatalogPath).Should().BeTrue();
        File.Exists(supportSummaryPath).Should().BeTrue();
        File.Exists(cacheAssetPath).Should().BeTrue();
        File.Exists(cachePayloadPath).Should().BeTrue();

        var project = File.ReadAllText(demoProjectPath);
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj");
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Processing\Videra.SurfaceCharts.Processing.csproj");

        var rootReadme = File.ReadAllText(rootReadmePath);
        rootReadme.Should().Contain("Minimal SurfaceCharts cookbook");
        rootReadme.Should().Contain("not a compatibility or parity layer");
        rootReadme.Should().Contain("recipe groups for first chart, styling, interactions, live data, linked axes, Bar, Contour, and export");
        rootReadme.Should().Contain("First surface");
        rootReadme.Should().Contain("chart.Plot.Axes.X.Label");
        rootReadme.Should().Contain("DataLogger3D");
        rootReadme.Should().Contain("Live scatter");
        rootReadme.Should().Contain("linked-axis");
        rootReadme.Should().Contain("Plot.Add.Bar");
        rootReadme.Should().Contain("Plot.Add.Contour");
        rootReadme.Should().Contain("SavePngAsync");

        var readme = File.ReadAllText(demoReadmePath);
        readme.Should().Contain("Start here");
        readme.Should().Contain("in-memory first chart");
        readme.Should().Contain("Explore next");
        readme.Should().Contain("in-memory");
        readme.Should().Contain("cache-backed");
        readme.Should().Contain("lazy");
        readme.Should().Contain("sample.surfacecache.json.bin");
        readme.Should().Contain("VideraChartView");
        readme.Should().Contain("ScatterStreamingScenarios");
        readme.Should().Contain("evidence-only support summary");
        readme.Should().Contain("active chart control type");
        readme.Should().Contain("runtime/assembly identity");
        readme.Should().Contain("backend/display environment variables");
        readme.Should().Contain("last cache-load failure");
        readme.Should().Contain("not stable benchmark guarantees");
        readme.Should().Contain("Try next: Analytics proof");
        readme.Should().Contain("ColorField");
        readme.Should().Contain("Try next: Bar chart proof");
        readme.Should().Contain("Try next: Contour plot proof");
        readme.Should().Contain("Cookbook Recipes");
        readme.Should().Contain("Cookbook gallery");
        readme.Should().Contain("not a compatibility or parity layer");
        readme.Should().Contain("### First Chart");
        readme.Should().Contain("### Styling");
        readme.Should().Contain("### Interactions");
        readme.Should().Contain("### Live Data");
        readme.Should().Contain("### Linked Axes");
        readme.Should().Contain("### Bar");
        readme.Should().Contain("### Contour");
        readme.Should().Contain("### Export");
        readme.Should().Contain("Recipes/first-chart.md");
        readme.Should().Contain("Recipes/surface-cache-backed.md");
        readme.Should().Contain("Recipes/waterfall.md");
        readme.Should().Contain("Recipes/axes-and-linked-views.md");
        readme.Should().Contain("Recipes/scatter-and-live-data.md");
        readme.Should().Contain("Recipes/bar.md");
        readme.Should().Contain("Recipes/contour.md");
        readme.Should().Contain("Recipes/support-evidence.md");
        readme.Should().Contain("Recipes/png-snapshot.md");
        readme.Should().Contain("First surface");
        readme.Should().Contain("chart.Plot.Axes.X.Label");
        readme.Should().Contain("SurfaceColorMapPresets.CreateProfessional");
        readme.Should().Contain("SurfaceChartInteractionProfile");
        readme.Should().Contain("TryExecuteChartCommand");
        readme.Should().Contain("TryResolveProbe");
        readme.Should().Contain("First scatter");
        readme.Should().Contain("SavePngAsync");
        readme.Should().Contain("DataLogger3D");
        readme.Should().Contain("UseLatestWindow");
        readme.Should().Contain("CreateLiveViewEvidence");
        readme.Should().Contain("LinkViewWith");
        readme.Should().Contain("Live scatter");
        readme.Should().Contain("BarChartData");
        readme.Should().Contain("BarSeries");
        readme.Should().Contain("SetSeriesColor");
        readme.Should().Contain("ContourChartData");
        readme.Should().Contain("SurfaceScalarField");
        readme.Should().Contain("explicitLevels");
        readme.Should().Contain("SelectionReported");

        rootReadme.Should().NotContain("ScottPlot");
        readme.Should().NotContain("ScottPlot");

        var mainWindow = File.ReadAllText(mainWindowXamlPath);
        var scenarioCatalog = File.ReadAllText(scenarioCatalogPath);
        mainWindow.Should().Contain("Cookbook gallery");
        mainWindow.Should().Contain("CookbookRecipeSelector");
        mainWindow.Should().Contain("CookbookRecipeStatusText");
        mainWindow.Should().Contain("Copy recipe snippet");
        mainWindow.Should().Contain("CookbookRecipeSnippetText");
        mainWindow.Should().Contain("SourceSelector");
        scenarioCatalog.Should().Contain("SurfaceDemoScenario");
        scenarioCatalog.Should().Contain("Start here: In-memory first chart");
        scenarioCatalog.Should().Contain("Explore next: Cache-backed streaming");
        scenarioCatalog.Should().Contain("Try next: Waterfall proof");
        scenarioCatalog.Should().Contain("Try next: Analytics proof");
        scenarioCatalog.Should().Contain("Try next: Scatter proof");
        scenarioCatalog.Should().Contain("Try next: Bar chart proof");
        scenarioCatalog.Should().Contain("Try next: Contour plot proof");
        mainWindow.Should().Contain("Scatter stream");
        mainWindow.Should().Contain("ScatterScenarioPanel");
        mainWindow.Should().Contain("ScatterScenarioSelector");
        mainWindow.Should().Contain("First-chart Plot path");
        mainWindow.Should().Contain("Start here status");
        mainWindow.Should().Contain("Built-in interaction");
        mainWindow.Should().Contain("BuiltInInteractionText");
        mainWindow.Should().Contain("Explore next: Rendering path");
        mainWindow.Should().Contain("Diagnostics: RenderingStatus");
        mainWindow.Should().Contain("RenderingDiagnosticsText");
        mainWindow.Should().Contain("Interaction quality");
        mainWindow.Should().Contain("Explore next: Probe workflow");
        mainWindow.Should().Contain("Shift + LeftClick");
        mainWindow.Should().Contain("Explore next: Axes and legend");
        mainWindow.Should().Contain("Explore next: Overlay options");
        mainWindow.Should().Contain("Advanced: cache asset");
        mainWindow.Should().Contain("Support summary");
        mainWindow.Should().Contain("Copy support summary");
        mainWindow.Should().Contain("Snapshot state: no PNG captured yet");
        mainWindow.Should().Contain("View-state contract");
        mainWindow.Should().Contain("Fit to data");
        mainWindow.Should().Contain("Reset camera");
        mainWindow.Should().Contain("ViewState");
        mainWindow.Should().Contain("VideraChartView");
        mainWindow.Should().NotContain("Viewport selection");
        mainWindow.Should().NotContain("Tag=\"overview\"");
        mainWindow.Should().NotContain("Tag=\"detail\"");
        mainWindow.Should().NotContain("VideraView");
        mainWindow.Should().NotContain("Videra.Demo");

        var mainWindowCodeBehind = File.ReadAllText(mainWindowCodeBehindPath);
        var sampleData = File.ReadAllText(mainWindowSampleDataPath);
        var cookbookRecipeCatalog = File.ReadAllText(cookbookRecipeCatalogPath);
        var supportSummary = File.ReadAllText(supportSummaryPath);
        var chartStatusFormatter = File.ReadAllText(Path.Combine(demoRoot, "Services", "ChartStatusFormatter.cs"));
        var supportSummaryService = File.ReadAllText(Path.Combine(demoRoot, "Services", "SupportSummaryService.cs"));
        var cacheSourceHandler = File.ReadAllText(Path.Combine(demoRoot, "Services", "CacheSourceHandler.cs"));
        mainWindowCodeBehind.Should().Contain("CookbookRecipes");
        mainWindowCodeBehind.Should().Contain("CookbookRecipe");
        mainWindowCodeBehind.Should().Contain("SurfaceDemoScenarios");
        mainWindowCodeBehind.Should().NotContain("private sealed record CookbookRecipe");
        mainWindowCodeBehind.Should().NotContain("private static class CookbookRecipes");
        cookbookRecipeCatalog.Should().Contain("ScenarioId");
        cookbookRecipeCatalog.Should().NotContain("SourceIndex");
        cookbookRecipeCatalog.Should().Contain("First chart");
        cookbookRecipeCatalog.Should().Contain("Styling");
        cookbookRecipeCatalog.Should().Contain("Interactions");
        cookbookRecipeCatalog.Should().Contain("Live data");
        cookbookRecipeCatalog.Should().Contain("Linked axes");
        cookbookRecipeCatalog.Should().Contain("Bar");
        cookbookRecipeCatalog.Should().Contain("Contour");
        cookbookRecipeCatalog.Should().Contain("Export");
        mainWindowCodeBehind.Should().Contain("OnCopyRecipeSnippetClicked");
        cookbookRecipeCatalog.Should().Contain("SurfaceChartInteractionProfile");
        cookbookRecipeCatalog.Should().Contain("TryExecuteChartCommand");
        cookbookRecipeCatalog.Should().Contain("UseLatestWindow");
        cookbookRecipeCatalog.Should().Contain("CreateLiveViewEvidence");
        cookbookRecipeCatalog.Should().Contain("LinkViewWith");
        mainWindowCodeBehind.Should().Contain("FitToDataButton");
        mainWindowCodeBehind.Should().Contain("ResetCameraButton");
        mainWindowCodeBehind.Should().Contain("ViewStateText");
        mainWindowCodeBehind.Should().Contain("InteractionQualityText");
        mainWindowCodeBehind.Should().Contain("InteractionQualityChanged");
        mainWindowCodeBehind.Should().Contain("OverlayOptionsText");
        mainWindowCodeBehind.Should().Contain("RenderingDiagnosticsText");
        mainWindowCodeBehind.Should().Contain("SupportSummaryText");
        supportSummary.Should().Contain("EvidenceKind: SurfaceChartsStreamingDatasetProof");
        supportSummary.Should().Contain("EvidenceOnly: true");
        supportSummary.Should().Contain("ChartControl:");
        supportSummary.Should().Contain("EnvironmentRuntime:");
        supportSummary.Should().Contain("AssemblyIdentity:");
        supportSummary.Should().Contain("BackendDisplayEnvironment:");
        supportSummary.Should().Contain("CacheLoadFailure:");
        supportSummary.Should().Contain("SeriesCount:");
        supportSummary.Should().Contain("ActiveSeries:");
        supportSummary.Should().Contain("ChartKind:");
        supportSummary.Should().Contain("ColorMap:");
        supportSummary.Should().Contain("PrecisionProfile:");
        supportSummary.Should().Contain("OutputEvidenceKind:");
        supportSummary.Should().Contain("OutputCapabilityDiagnostics:");
        supportSummary.Should().Contain("SnapshotStatus:");
        supportSummary.Should().Contain("SnapshotPath:");
        supportSummary.Should().Contain("SnapshotWidth:");
        supportSummary.Should().Contain("SnapshotHeight:");
        supportSummary.Should().Contain("SnapshotFormat:");
        supportSummary.Should().Contain("SnapshotBackground:");
        supportSummary.Should().Contain("SnapshotOutputEvidenceKind:");
        supportSummary.Should().Contain("SnapshotDatasetEvidenceKind:");
        supportSummary.Should().Contain("SnapshotActiveSeriesIdentity:");
        supportSummary.Should().Contain("SnapshotCreatedUtc:");
        supportSummary.Should().Contain("DatasetEvidenceKind:");
        supportSummary.Should().Contain("DatasetSeriesCount:");
        supportSummary.Should().Contain("DatasetActiveSeriesIndex:");
        supportSummary.Should().Contain("DatasetActiveSeriesMetadata:");
        supportSummary.Should().Contain("ScenarioId:");
        mainWindowCodeBehind.Should().Contain("CopySupportSummaryButton");
        mainWindowCodeBehind.Should().Contain("SupportSummaryStatusText");
        mainWindowCodeBehind.Should().Contain("BuiltInInteractionText");
        chartStatusFormatter.Should().Contain("Left drag orbit");
        chartStatusFormatter.Should().Contain("Right drag pan");
        chartStatusFormatter.Should().Contain("Wheel dolly");
        chartStatusFormatter.Should().Contain("Ctrl + Left drag");
        chartStatusFormatter.Should().Contain("VideraChartView.Plot exposes `OverlayOptions`");
        mainWindowCodeBehind.Should().Contain("SurfaceChartOverlayOptions");
        mainWindowCodeBehind.Should().Contain(".Plot.OverlayOptions");
        mainWindowCodeBehind.Should().Contain("ShowMinorTicks");
        mainWindowCodeBehind.Should().Contain("SurfaceChartGridPlane");
        cacheSourceHandler.Should().Contain("_cachePath");
        cacheSourceHandler.Should().Contain("_cachePayloadPath");
        mainWindowCodeBehind.Should().Contain("SurfaceDemoScenarios.ScatterId");
        cookbookRecipeCatalog.Should().Contain("SurfaceDemoScenarios.BarId");
        cookbookRecipeCatalog.Should().Contain("SurfaceDemoScenarios.ContourId");
        mainWindowCodeBehind.Should().Contain("Plot.Add.Scatter");
        cookbookRecipeCatalog.Should().Contain("Plot.Add.Bar");
        cookbookRecipeCatalog.Should().Contain("Plot.Add.Contour");
        cookbookRecipeCatalog.Should().Contain("Plot.SavePngAsync");
        sampleData.Should().Contain("ScatterChartData");
        mainWindowCodeBehind.Should().Contain("ScatterStreamingScenarios");
        mainWindowCodeBehind.Should().Contain("ApplySelectedScatterScenario");
        supportSummaryService.Should().Contain("UpdateScatterScenarioSelectorState");
        supportSummaryService.Should().Contain("_scatterScenarioSelector.IsEnabled");
        sampleData.Should().Contain("ScatterSeries");
        sampleData.Should().Contain("ScatterPoint");
        sampleData.Should().Contain("ScatterChartMetadata");
        sampleData.Should().Contain("CreateScatterSource");
        sampleData.Should().Contain("CreateSampleBarData");
        sampleData.Should().Contain("CreateSampleContourField");
        sampleData.Should().Contain("CreateAnalyticsProofSource");
        sampleData.Should().Contain("CreateAnalyticsProofMatrix");
        sampleData.Should().Contain("SurfaceExplicitGrid");
        sampleData.Should().Contain("SurfaceScalarField");
        cookbookRecipeCatalog.Should().Contain("ColorField");
        chartStatusFormatter.Should().Contain("CreateScatterCameraSummary");
        mainWindowCodeBehind.Should().Contain("Scatter proof");
        mainWindowCodeBehind.Should().Contain("RenderStatusChanged");
        supportSummary.Should().Contain("SurfaceChartRenderingStatus");
        chartStatusFormatter.Should().Contain("CreateSurfaceRenderingDiagnosticsSummary");
        chartStatusFormatter.Should().Contain("CreateScatterRenderingDiagnosticsSummary");
        supportSummary.Should().Contain("LastRefreshRevision");
        supportSummary.Should().Contain("UsesNativeSurface");
        supportSummary.Should().Contain("FallbackReason");
        mainWindowCodeBehind.Should().Contain("VideraChartView");
        mainWindowCodeBehind.Should().Contain(".FitToData(");
        mainWindowCodeBehind.Should().Contain(".ResetCamera(");
        mainWindowCodeBehind.Should().Contain("UpdateSupportSummaryText");
        supportSummary.Should().Contain("CreateActiveSeriesSummary");
        supportSummary.Should().Contain("CreatePrecisionProfileSummary");
        chartStatusFormatter.Should().Contain("Current mode:");
        chartStatusFormatter.Should().Contain("Refine: full settled requests for the current view.");
        chartStatusFormatter.Should().Contain("Refine: settled plot is ready for");
        mainWindowCodeBehind.Should().Contain("OnCopySupportSummaryClicked");
        mainWindowCodeBehind.Should().Contain("Clipboard");
        scenarioCatalog.Should().Contain("Start here: In-memory first chart");
        scenarioCatalog.Should().Contain("Explore next: Cache-backed streaming");
        mainWindowCodeBehind.Should().NotContain("ViewportSelector");
        mainWindowCodeBehind.Should().NotContain("CreateOverviewViewport");
        mainWindowCodeBehind.Should().NotContain("CreateZoomedDetailViewport");
        cacheSourceHandler.Should().Contain("lazy");
        readme.Should().Contain("no GPU-driven culling");
        readme.Should().Contain("no hard performance guarantee");

        var cacheAsset = File.ReadAllText(cacheAssetPath);
        cacheAsset.Should().Contain(@"""version"": 2");
        cacheAsset.Should().Contain("payloadOffset");
        cacheAsset.Should().Contain("payloadLength");
        cacheAsset.Should().NotContain(@"""values""");

        var appXaml = File.ReadAllText(appXamlPath);
        appXaml.Should().NotContain("VideraView");
        appXaml.Should().NotContain("Videra.Demo");
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
