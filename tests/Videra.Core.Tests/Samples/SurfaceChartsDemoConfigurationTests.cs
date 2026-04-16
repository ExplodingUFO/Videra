using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsDemoConfigurationTests
{
    [Fact]
    public void SurfaceChartsDemo_ShouldExistAsAnIndependentSurfaceChartEntryPoint()
    {
        var repositoryRoot = GetRepositoryRoot();
        var demoRoot = Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo");
        var demoProjectPath = Path.Combine(demoRoot, "Videra.SurfaceCharts.Demo.csproj");
        var demoReadmePath = Path.Combine(demoRoot, "README.md");
        var mainWindowXamlPath = Path.Combine(demoRoot, "Views", "MainWindow.axaml");
        var mainWindowCodeBehindPath = Path.Combine(demoRoot, "Views", "MainWindow.axaml.cs");
        var appXamlPath = Path.Combine(demoRoot, "App.axaml");
        var cacheAssetPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json");
        var cachePayloadPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json.bin");

        File.Exists(demoProjectPath).Should().BeTrue();
        File.Exists(demoReadmePath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(mainWindowCodeBehindPath).Should().BeTrue();
        File.Exists(cacheAssetPath).Should().BeTrue();
        File.Exists(cachePayloadPath).Should().BeTrue();

        var project = File.ReadAllText(demoProjectPath);
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj");
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Processing\Videra.SurfaceCharts.Processing.csproj");

        var readme = File.ReadAllText(demoReadmePath);
        readme.Should().Contain("in-memory");
        readme.Should().Contain("cache-backed");
        readme.Should().Contain("lazy");
        readme.Should().Contain("sample.surfacecache.json.bin");

        var mainWindow = File.ReadAllText(mainWindowXamlPath);
        mainWindow.Should().Contain("In-memory example");
        mainWindow.Should().Contain("Cache-backed example");
        mainWindow.Should().Contain("Built-in interaction");
        mainWindow.Should().Contain("Left drag orbit");
        mainWindow.Should().Contain("Right drag pan");
        mainWindow.Should().Contain("Wheel dolly");
        mainWindow.Should().Contain("Ctrl + Left drag");
        mainWindow.Should().Contain("Rendering path");
        mainWindow.Should().Contain("Interaction quality");
        mainWindow.Should().Contain("Probe workflow");
        mainWindow.Should().Contain("Shift + LeftClick");
        mainWindow.Should().Contain("Axes and legend");
        mainWindow.Should().Contain("Overlay options");
        mainWindow.Should().Contain("View-state contract");
        mainWindow.Should().Contain("Fit to data");
        mainWindow.Should().Contain("Reset camera");
        mainWindow.Should().Contain("ViewState");
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
        mainWindowCodeBehind.Should().Contain("SurfaceChartOverlayOptions");
        mainWindowCodeBehind.Should().Contain(".OverlayOptions");
        mainWindowCodeBehind.Should().Contain("ShowMinorTicks");
        mainWindowCodeBehind.Should().Contain("SurfaceChartGridPlane");
        mainWindowCodeBehind.Should().Contain("CacheManifestFileName");
        mainWindowCodeBehind.Should().Contain("CachePayloadSuffix");
        mainWindowCodeBehind.Should().Contain("RenderStatusChanged");
        mainWindowCodeBehind.Should().Contain("SurfaceChartInteractionQuality");
        mainWindowCodeBehind.Should().Contain("SurfaceChartRenderingStatus");
        mainWindowCodeBehind.Should().Contain(".FitToData(");
        mainWindowCodeBehind.Should().Contain(".ResetCamera(");
        mainWindowCodeBehind.Should().Contain("UpdateViewStateText");
        mainWindowCodeBehind.Should().Contain("UpdateInteractionQualityText");
        mainWindowCodeBehind.Should().NotContain("ViewportSelector");
        mainWindowCodeBehind.Should().NotContain("CreateOverviewViewport");
        mainWindowCodeBehind.Should().NotContain("CreateZoomedDetailViewport");
        mainWindowCodeBehind.Should().Contain("lazy");

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
