using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsRepositoryArchitectureTests
{
    [Fact]
    public void RootReadme_ShouldDescribeSurfaceChartsAsIndependentSiblingFamily()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsFamilyBoundarySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsDemoSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartViewSentence);
        readme.Should().Contain("src/Videra.SurfaceCharts.Core/README.md");
        readme.Should().Contain("src/Videra.SurfaceCharts.Avalonia/README.md");
        readme.Should().Contain("src/Videra.SurfaceCharts.Processing/README.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
    }

    [Fact]
    public void ChineseReadme_ShouldLinkSurfaceChartsModulePagesAndDemo()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "README.md"));

        readme.Should().Contain("modules/videra-surfacecharts-core.md");
        readme.Should().Contain("modules/videra-surfacecharts-avalonia.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
        readme.Should().Contain("SurfaceChartView");
        readme.Should().Contain("Videra.SurfaceCharts.Demo");
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFamilyBoundarySentence);
    }

    [Fact]
    public void VerifyScripts_ShouldBuildBothDemoApplications()
    {
        var repositoryRoot = GetRepositoryRoot();
        var shellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.sh"));
        var powerShellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));

        foreach (var verifyContent in new[] { shellVerify, powerShellVerify })
        {
            foreach (var target in SurfaceChartsDocumentationTerms.ExpectedVerifyTargets)
            {
                verifyContent.Should().Contain(target);
            }
        }
    }

    [Fact]
    public void SurfaceChartAxisProbeOverlay_ShouldStayOutOfVideraView()
    {
        var repositoryRoot = GetRepositoryRoot();
        var videraView = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraView.cs"));
        var videraViewOverlay = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraView.Overlay.cs"));

        foreach (var content in new[] { videraView, videraViewOverlay })
        {
            content.Should().NotContain("SurfaceAxisOverlayPresenter");
            content.Should().NotContain("SurfaceLegendOverlayPresenter");
            content.Should().NotContain("SurfaceProbeService");
            content.Should().NotContain("SurfaceProbeInfo");
            content.Should().NotContain("Videra.SurfaceCharts");
        }
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
