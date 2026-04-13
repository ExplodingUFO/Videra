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
        var appXamlPath = Path.Combine(demoRoot, "App.axaml");
        var cacheAssetPath = Path.Combine(demoRoot, "Assets", "sample-surface-cache", "sample.surfacecache.json");

        File.Exists(demoProjectPath).Should().BeTrue();
        File.Exists(demoReadmePath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(cacheAssetPath).Should().BeTrue();

        var project = File.ReadAllText(demoProjectPath);
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Avalonia\Videra.SurfaceCharts.Avalonia.csproj");
        project.Should().Contain(@"..\..\src\Videra.SurfaceCharts.Processing\Videra.SurfaceCharts.Processing.csproj");

        var readme = File.ReadAllText(demoReadmePath);
        readme.Should().Contain("in-memory");
        readme.Should().Contain("cache-backed");

        var mainWindow = File.ReadAllText(mainWindowXamlPath);
        mainWindow.Should().Contain("In-memory example");
        mainWindow.Should().Contain("Cache-backed example");
        mainWindow.Should().NotContain("VideraView");
        mainWindow.Should().NotContain("Videra.Demo");

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
