using FluentAssertions;
using System.Text.RegularExpressions;
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

    private static readonly string[] NativeInteropTokens =
    [
        "DllImport",
        "LibraryImport",
        "NativeLibraryHelper"
    ];

    [Fact]
    public void RootReadme_ShouldDescribeSurfaceChartsAsIndependentSiblingFamily()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOnboardingHeading);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsFamilyBoundarySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsFirstChartSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsDemoSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartViewSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsRendererStatusSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsRenderingStatusFieldsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsViewStateSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionQualitySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionDiagnosticsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOverlayOptionsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOverlayBoundarySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstSentence);
        readme.Should().Contain("axis/legend overlays");
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
        readme.Should().Contain("modules/videra-surfacecharts-processing.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
        readme.Should().Contain("SurfaceChartView");
        readme.Should().Contain("Videra.SurfaceCharts.Demo");
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFamilyBoundarySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFirstChartSentence);
        readme.Should().Contain("RenderStatusChanged");
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsTruthSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsViewStateSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionQualitySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionDiagnosticsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayOptionsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayBoundarySentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldsSentence);
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsSourceFirstSentence);
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
        avaloniaReadme.Should().Contain("ViewState");
        avaloniaReadme.Should().Contain("OverlayOptions");
        avaloniaReadme.Should().Contain("hover/pinned probe");
        avaloniaReadme.Should().Contain("FitToData()");
        avaloniaReadme.Should().Contain("ResetCamera()");
        avaloniaReadme.Should().Contain("ZoomTo(...)");

        demoReadme.Should().Contain("not a `VideraView` mode");
        demoReadme.Should().Contain(DemoGpuFallbackSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsDemoFirstChartSentence);
        demoReadme.Should().NotContain("[Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md)");
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsViewStateSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionQualitySentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsInteractionDiagnosticsSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOverlayBoundarySentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsRenderingStatusFieldsSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstSentence);
        demoReadme.Should().Contain("FitToData()");
        demoReadme.Should().Contain("ResetCamera()");
        demoReadme.Should().Contain("ZoomTo(...)");
        demoReadme.Should().Contain("SurfaceChartOverlayOptions");
        demoReadme.Should().Contain("`XWayland compatibility` only, not compositor-native Wayland surface embedding");
    }

    [Fact]
    public void SurfaceChartInteractionAndRenderingLayers_ShouldStayFreeOfNativeInteropHelpers()
    {
        var repositoryRoot = GetRepositoryRoot();
        var interactionDirectory = Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "Interaction");
        var renderingDirectory = Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Rendering");

        AssertDirectoryDoesNotContainNativeInteropTokens(interactionDirectory);
        AssertDirectoryDoesNotContainNativeInteropTokens(renderingDirectory);
    }

    [Fact]
    public void SurfaceChartProcessingReadme_ShouldDescribeBenchmarkingAndOptionalNativeSeam()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Processing", "README.md"));

        readme.Should().Contain("BenchmarkDotNet");
        readme.Should().Contain("optional native seam");
        readme.Should().Contain("The live scheduler now consumes those ordered batch reads whenever a source implements `ISurfaceTileBatchSource`");
        readme.Should().Contain("per-tile fallback path");
    }

    [Fact]
    public void ChineseSurfaceChartPages_ShouldMirrorRendererAndProcessingTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var avaloniaPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));
        var processingPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-processing.md"));

        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseAvaloniaRenderStatusSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseAvaloniaProbeSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsViewStateSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionQualitySentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionDiagnosticsSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayOptionsSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayBoundarySentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipSentence);
        avaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldsSentence);
        avaloniaPage.Should().Contain("FitToData()");
        avaloniaPage.Should().Contain("ResetCamera()");
        avaloniaPage.Should().Contain("ZoomTo(...)");
        avaloniaPage.Should().Contain("SurfaceChartOverlayOptions");
        avaloniaPage.Should().Contain("XWayland compatibility");

        processingPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseProcessingStatisticsSentence);
        processingPage.Should().Contain("optional native seam");
        processingPage.Should().Contain("XWayland");
    }

    [Fact]
    public void SurfaceChartEntryPoints_ShouldNotContainStaleLimitationLanguage()
    {
        var repositoryRoot = GetRepositoryRoot();
        var guardedFiles = SurfaceChartsDocumentationTerms.GuardedSurfaceChartsEntryPointPaths
            .Select(relativePath => Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)))
            .ToArray();

        var contents = guardedFiles
            .Select(path => File.ReadAllText(path))
            .ToArray();

        foreach (var staleTerm in SurfaceChartsDocumentationTerms.StaleSurfaceChartsTerms)
        {
            contents.Should().OnlyContain(
                content => !content.Contains(staleTerm, StringComparison.Ordinal),
                $"stale chart limitation wording '{staleTerm}' should stay out of the guarded entrypoints");
        }
    }

    [Fact]
    public void SurfaceChartViewStateAndCommandApis_ShouldStayOutOfVideraView()
    {
        var repositoryRoot = GetRepositoryRoot();
        var videraView = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraView.cs"));

        Regex.IsMatch(videraView, @"public\s+.*\bViewState\b").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bFitToData\s*\(").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bResetCamera\s*\(").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bZoomTo\s*\(").Should().BeFalse();
    }

    [Fact]
    public void GuardedSurfaceChartDocs_ShouldFreezeFirstChartContractLanguage()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));
        var chineseReadme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var chineseAvaloniaPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));

        rootReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsFirstChartSentence);
        rootReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipSentence);
        rootReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipSentence);
        rootReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstSentence);

        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsDemoFirstChartSentence);
        demoReadme.Should().NotContain("[Videra.SurfaceCharts.Demo](samples/Videra.SurfaceCharts.Demo/README.md)");
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipSentence);
        demoReadme.Should().Contain(SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstSentence);

        chineseReadme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFirstChartSentence);
        chineseReadme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipSentence);
        chineseReadme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipSentence);
        chineseReadme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsSourceFirstSentence);

        chineseAvaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipSentence);
        chineseAvaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipSentence);
        chineseAvaloniaPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldsSentence);
    }

    [Fact]
    public void RecoveredSurfaceChartSummaries_ShouldDeclareRequirementsCompletedMetadata()
    {
        var repositoryRoot = GetRepositoryRoot();
        if (!HasRecoveredSurfaceChartSummaryArtifacts(repositoryRoot))
        {
            return;
        }

        foreach (var (relativePath, requirements) in SurfaceChartsDocumentationTerms.RecoveredSummaryRequirementMetadata)
        {
            var content = File.ReadAllText(Path.Combine(repositoryRoot, relativePath));

            content.Should().Contain("requirements-completed:");
            foreach (var requirement in requirements)
            {
                content.Should().Contain(requirement);
            }
        }
    }

    [Fact]
    public void SurfaceChartPlanningArtifacts_ShouldStayAlignedWithRecoveredMilestoneTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        if (!HasRecoveredSurfaceChartVerificationArtifacts(repositoryRoot))
        {
            return;
        }

        var phase13Verification = File.ReadAllText(Path.Combine(repositoryRoot, ".planning", "phases", "13-surfacechart-runtime-and-view-state-contract", "13-VERIFICATION.md"));
        var phase14Verification = File.ReadAllText(Path.Combine(repositoryRoot, ".planning", "phases", "14-built-in-interaction-and-camera-workflow", "14-VERIFICATION.md"));
        var phase18Verification = File.ReadAllText(Path.Combine(repositoryRoot, ".planning", "phases", "18-demo-docs-and-repository-truth-for-professional-charts", "18-VERIFICATION.md"));
        var phase19Verification = File.ReadAllText(Path.Combine(repositoryRoot, ".planning", "phases", "19-surfacechart-runtime-and-view-state-recovery", "19-VERIFICATION.md"));

        phase13Verification.Should().Contain(SurfaceChartsDocumentationTerms.Phase13HistoricalRecoverySentence);
        phase14Verification.Should().Contain(SurfaceChartsDocumentationTerms.Phase14HistoricalRecoverySentence);
        phase18Verification.Should().Contain(SurfaceChartsDocumentationTerms.Phase18HistoricalRecoverySentence);
        phase19Verification.Should().Contain(SurfaceChartsDocumentationTerms.Phase19HistoricalRecoverySentence);
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

    private static bool HasRecoveredSurfaceChartSummaryArtifacts(string repositoryRoot)
    {
        return SurfaceChartsDocumentationTerms.RecoveredSummaryRequirementMetadata
            .Select(metadata => Path.Combine(repositoryRoot, metadata.Path))
            .All(File.Exists);
    }

    private static bool HasRecoveredSurfaceChartVerificationArtifacts(string repositoryRoot)
    {
        return new[]
        {
            Path.Combine(repositoryRoot, ".planning", "phases", "13-surfacechart-runtime-and-view-state-contract", "13-VERIFICATION.md"),
            Path.Combine(repositoryRoot, ".planning", "phases", "14-built-in-interaction-and-camera-workflow", "14-VERIFICATION.md"),
            Path.Combine(repositoryRoot, ".planning", "phases", "18-demo-docs-and-repository-truth-for-professional-charts", "18-VERIFICATION.md"),
            Path.Combine(repositoryRoot, ".planning", "phases", "19-surfacechart-runtime-and-view-state-recovery", "19-VERIFICATION.md")
        }.All(File.Exists);
    }

    private static void AssertDirectoryDoesNotContainNativeInteropTokens(string directoryPath)
    {
        var fileContents = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Select(path => new { Path = path, Content = File.ReadAllText(path) })
            .ToArray();

        fileContents.Should().NotBeEmpty();

        foreach (var token in NativeInteropTokens)
        {
            fileContents.Should().OnlyContain(
                file => !file.Content.Contains(token, StringComparison.Ordinal),
                $"native interop token '{token}' should stay out of {directoryPath}");
        }
    }
}
