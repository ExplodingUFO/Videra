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

        File.Exists(demoProjectPath).Should().BeTrue();
        File.Exists(demoReadmePath).Should().BeTrue();
        File.Exists(appXamlPath).Should().BeTrue();
        File.Exists(mainWindowXamlPath).Should().BeTrue();
        File.Exists(mainWindowCodeBehindPath).Should().BeTrue();
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
        mainWindow.Should().Contain("Viewport selection");
        mainWindow.Should().Contain("SelectionChanged=\"OnViewportSelectionChanged\"");
        mainWindow.Should().Contain("Overview");
        mainWindow.Should().Contain("Zoomed detail");
        mainWindow.Should().NotContain("VideraView");
        mainWindow.Should().NotContain("Videra.Demo");

        var mainWindowCodeBehind = File.ReadAllText(mainWindowCodeBehindPath);
        mainWindowCodeBehind.Should().Contain("_viewportSelector.SelectionChanged += OnViewportSelectionChanged;");
        mainWindowCodeBehind.Should().Contain("_chartView.Viewport = mode == ViewportMode.Overview");
        mainWindowCodeBehind.Should().Contain("CreateOverviewViewport");
        mainWindowCodeBehind.Should().Contain("CreateZoomedDetailViewport");

        var applySourceBody = ExtractMethodBody(mainWindowCodeBehind, "private void ApplySource(");
        applySourceBody.Should().Contain("ConfigureViewportPresets(source.Metadata);");
        applySourceBody.Should().Contain("ApplyViewportMode(GetSelectedViewportMode());");

        var appXaml = File.ReadAllText(appXamlPath);
        appXaml.Should().NotContain("VideraView");
        appXaml.Should().NotContain("Videra.Demo");
    }

    private static string ExtractMethodBody(string source, string methodSignature)
    {
        var signatureIndex = source.IndexOf(methodSignature, StringComparison.Ordinal);
        signatureIndex.Should().BeGreaterOrEqualTo(0, $"Expected to find '{methodSignature}' in the source file.");

        var bodyStart = source.IndexOf('{', signatureIndex);
        bodyStart.Should().BeGreaterOrEqualTo(0, $"Expected '{methodSignature}' to have an opening brace.");

        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            var character = source[index];
            if (character == '{')
            {
                depth++;
            }
            else if (character == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[(bodyStart + 1)..index];
                }
            }
        }

        throw new InvalidOperationException($"Could not find the end of '{methodSignature}' in the source file.");
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
