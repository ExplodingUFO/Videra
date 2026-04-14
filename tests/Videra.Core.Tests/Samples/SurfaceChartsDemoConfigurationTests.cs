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
        mainWindow.Should().Contain("Viewport selection");
        mainWindow.Should().Contain("Tag=\"overview\"");
        mainWindow.Should().Contain("Tag=\"detail\"");
        mainWindow.Should().Contain("Overview");
        mainWindow.Should().Contain("Zoomed detail");
        mainWindow.Should().NotContain("VideraView");
        mainWindow.Should().NotContain("Videra.Demo");

        var mainWindowCodeBehind = File.ReadAllText(mainWindowCodeBehindPath);
        mainWindowCodeBehind.Should().Contain("_viewportSelector.SelectionChanged += OnViewportSelectionChanged;");
        mainWindowCodeBehind.Should().Contain("CreateOverviewViewport");
        mainWindowCodeBehind.Should().Contain("CreateZoomedDetailViewport");
        mainWindowCodeBehind.Should().Contain("CacheManifestFileName");
        mainWindowCodeBehind.Should().Contain("CachePayloadSuffix");
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
