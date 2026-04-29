using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsDemoConfigurationTests
{
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
        var appXamlPath = Path.Combine(demoRoot, "App.axaml");
        var cacheAssetPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json");
        var cachePayloadPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json.bin");

        File.Exists(demoProjectPath).Should().BeTrue();
        File.Exists(rootReadmePath).Should().BeTrue();
        File.Exists(demoReadmePath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(mainWindowCodeBehindPath).Should().BeTrue();
        File.Exists(cacheAssetPath).Should().BeTrue();
        File.Exists(cachePayloadPath).Should().BeTrue();

        var project = File.ReadAllText(demoProjectPath);
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj");
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Processing\Videra.SurfaceCharts.Processing.csproj");

        var rootReadme = File.ReadAllText(rootReadmePath);
        rootReadme.Should().Contain("Minimal SurfaceCharts cookbook");
        rootReadme.Should().Contain("First surface");
        rootReadme.Should().Contain("chart.Plot.Axes.X.Label");
        rootReadme.Should().Contain("DataLogger3D");
        rootReadme.Should().Contain("Live scatter");
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
        readme.Should().Contain("Cookbook Recipes");
        readme.Should().Contain("First surface");
        readme.Should().Contain("chart.Plot.Axes.X.Label");
        readme.Should().Contain("First scatter");
        readme.Should().Contain("SavePngAsync");
        readme.Should().Contain("DataLogger3D");
        readme.Should().Contain("Live scatter");

        var mainWindow = File.ReadAllText(mainWindowXamlPath);
        mainWindow.Should().Contain("Start here: In-memory first chart");
        mainWindow.Should().Contain("Explore next: Cache-backed streaming");
        mainWindow.Should().Contain("Try next: Waterfall proof");
        mainWindow.Should().Contain("Try next: Analytics proof");
        mainWindow.Should().Contain("Try next: Scatter proof");
        mainWindow.Should().Contain("Scatter stream");
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
        mainWindowCodeBehind.Should().Contain("FitToDataButton");
        mainWindowCodeBehind.Should().Contain("ResetCameraButton");
        mainWindowCodeBehind.Should().Contain("ViewStateText");
        mainWindowCodeBehind.Should().Contain("InteractionQualityText");
        mainWindowCodeBehind.Should().Contain("InteractionQualityChanged");
        mainWindowCodeBehind.Should().Contain("OverlayOptionsText");
        mainWindowCodeBehind.Should().Contain("RenderingDiagnosticsText");
        mainWindowCodeBehind.Should().Contain("SupportSummaryText");
        mainWindowCodeBehind.Should().Contain("EvidenceKind: SurfaceChartsStreamingDatasetProof");
        mainWindowCodeBehind.Should().Contain("EvidenceOnly: true");
        mainWindowCodeBehind.Should().Contain("ChartControl:");
        mainWindowCodeBehind.Should().Contain("EnvironmentRuntime:");
        mainWindowCodeBehind.Should().Contain("AssemblyIdentity:");
        mainWindowCodeBehind.Should().Contain("BackendDisplayEnvironment:");
        mainWindowCodeBehind.Should().Contain("CacheLoadFailure:");
        mainWindowCodeBehind.Should().Contain("SeriesCount:");
        mainWindowCodeBehind.Should().Contain("ActiveSeries:");
        mainWindowCodeBehind.Should().Contain("ChartKind:");
        mainWindowCodeBehind.Should().Contain("ColorMap:");
        mainWindowCodeBehind.Should().Contain("PrecisionProfile:");
        mainWindowCodeBehind.Should().Contain("OutputEvidenceKind:");
        mainWindowCodeBehind.Should().Contain("OutputCapabilityDiagnostics:");
        mainWindowCodeBehind.Should().Contain("DatasetEvidenceKind:");
        mainWindowCodeBehind.Should().Contain("DatasetSeriesCount:");
        mainWindowCodeBehind.Should().Contain("DatasetActiveSeriesIndex:");
        mainWindowCodeBehind.Should().Contain("DatasetActiveSeriesMetadata:");
        mainWindowCodeBehind.Should().Contain("ScenarioId:");
        mainWindowCodeBehind.Should().Contain("CopySupportSummaryButton");
        mainWindowCodeBehind.Should().Contain("SupportSummaryStatusText");
        mainWindowCodeBehind.Should().Contain("BuiltInInteractionText");
        mainWindowCodeBehind.Should().Contain("Left drag orbit");
        mainWindowCodeBehind.Should().Contain("Right drag pan");
        mainWindowCodeBehind.Should().Contain("Wheel dolly");
        mainWindowCodeBehind.Should().Contain("Ctrl + Left drag");
        mainWindowCodeBehind.Should().Contain("chartView.InteractionQuality");
        mainWindowCodeBehind.Should().Contain("VideraChartView.Plot exposes `OverlayOptions`");
        mainWindowCodeBehind.Should().Contain("SurfaceChartOverlayOptions");
        mainWindowCodeBehind.Should().Contain(".Plot.OverlayOptions");
        mainWindowCodeBehind.Should().Contain("ShowMinorTicks");
        mainWindowCodeBehind.Should().Contain("SurfaceChartGridPlane");
        mainWindowCodeBehind.Should().Contain("CacheManifestFileName");
        mainWindowCodeBehind.Should().Contain("CachePayloadSuffix");
        mainWindowCodeBehind.Should().Contain("ScatterSourceIndex");
        mainWindowCodeBehind.Should().Contain("Plot.Add.Scatter");
        mainWindowCodeBehind.Should().Contain("Plot.SavePngAsync");
        mainWindowCodeBehind.Should().Contain("ScatterChartData");
        mainWindowCodeBehind.Should().Contain("ScatterStreamingScenarios");
        mainWindowCodeBehind.Should().Contain("ApplySelectedScatterScenario");
        mainWindowCodeBehind.Should().Contain("ScatterSeries");
        mainWindowCodeBehind.Should().Contain("ScatterPoint");
        mainWindowCodeBehind.Should().Contain("ScatterChartMetadata");
        mainWindowCodeBehind.Should().Contain("CreateScatterSource");
        mainWindowCodeBehind.Should().Contain("CreateAnalyticsProofSource");
        mainWindowCodeBehind.Should().Contain("CreateAnalyticsProofMatrix");
        mainWindowCodeBehind.Should().Contain("SurfaceExplicitGrid");
        mainWindowCodeBehind.Should().Contain("SurfaceScalarField");
        mainWindowCodeBehind.Should().Contain("ColorField");
        mainWindowCodeBehind.Should().Contain("CreateScatterCameraSummary");
        mainWindowCodeBehind.Should().Contain("Scatter proof");
        mainWindowCodeBehind.Should().Contain("RenderStatusChanged");
        mainWindowCodeBehind.Should().Contain("SurfaceChartRenderingStatus");
        mainWindowCodeBehind.Should().Contain("UpdateRenderingDiagnosticsText");
        mainWindowCodeBehind.Should().Contain("CreateSurfaceRenderingDiagnosticsSummary");
        mainWindowCodeBehind.Should().Contain("CreateScatterRenderingDiagnosticsSummary");
        mainWindowCodeBehind.Should().Contain("LastRefreshRevision");
        mainWindowCodeBehind.Should().Contain("UsesNativeSurface");
        mainWindowCodeBehind.Should().Contain("FallbackReason");
        mainWindowCodeBehind.Should().Contain("VideraChartView");
        mainWindowCodeBehind.Should().Contain(".FitToData(");
        mainWindowCodeBehind.Should().Contain(".ResetCamera(");
        mainWindowCodeBehind.Should().Contain("UpdateViewStateText");
        mainWindowCodeBehind.Should().Contain("UpdateInteractionQualityText");
        mainWindowCodeBehind.Should().Contain("UpdateSupportSummaryText");
        mainWindowCodeBehind.Should().Contain("CreateActiveSeriesSummary");
        mainWindowCodeBehind.Should().Contain("CreatePrecisionProfileSummary");
        mainWindowCodeBehind.Should().Contain("Current mode:");
        mainWindowCodeBehind.Should().Contain("Refine: full settled requests for the current view.");
        mainWindowCodeBehind.Should().Contain("Refine: settled plot is ready for");
        mainWindowCodeBehind.Should().Contain("OnCopySupportSummaryClicked");
        mainWindowCodeBehind.Should().Contain("Clipboard");
        mainWindowCodeBehind.Should().Contain("Start here: In-memory first chart");
        mainWindowCodeBehind.Should().Contain("Explore next: Cache-backed streaming");
        mainWindowCodeBehind.Should().NotContain("ViewportSelector");
        mainWindowCodeBehind.Should().NotContain("CreateOverviewViewport");
        mainWindowCodeBehind.Should().NotContain("CreateZoomedDetailViewport");
        mainWindowCodeBehind.Should().Contain("lazy");
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
