using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsRepositoryArchitectureTests
{
    private const string ChartRendererBoundarySentence =
        "`SurfaceChartView` works through a chart-local renderer seam";

    private const string GpuFallbackSentence =
        "The renderer is `GPU-first`, but `software fallback` remains a shipped path";

    private const string DemoGpuFallbackSentence =
        "the shipped `GPU-first` renderer path used by `SurfaceChartView`, with `software fallback` still available";

    private const string LinuxWaylandLimitSentence =
        "On Wayland sessions the chart host uses an `XWayland compatibility` path; compositor-native Wayland surface embedding is not available";

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
            content.Should().NotContain("SurfaceChartRenderHost");
            content.Should().NotContain("SurfaceChartGpuRenderBackend");
            content.Should().NotContain("Videra.SurfaceCharts.Rendering");
            content.Should().NotContain("Videra.SurfaceCharts");
        }
    }

    [Fact]
    public void SurfaceChartReadmes_ShouldDescribeRendererTruthWithoutCrossingSiblingBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));

        avaloniaReadme.Should().Contain(ChartRendererBoundarySentence);
        avaloniaReadme.Should().Contain(GpuFallbackSentence);
        avaloniaReadme.Should().Contain(LinuxWaylandLimitSentence);
        avaloniaReadme.Should().Contain("independent from `VideraView`");

        demoReadme.Should().Contain("not a `VideraView` mode");
        demoReadme.Should().Contain(DemoGpuFallbackSentence);
        demoReadme.Should().Contain("`XWayland compatibility` only, not compositor-native Wayland surface embedding");
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
