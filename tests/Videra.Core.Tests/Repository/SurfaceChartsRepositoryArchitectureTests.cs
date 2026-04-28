using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsRepositoryArchitectureTests
{
    private const string LinuxWaylandLimitSentence =
        "On Wayland sessions the chart host uses an `XWayland compatibility` path; compositor-native Wayland surface embedding is not available";

    private static readonly Regex MarkdownLinkRegex = new(@"\[[^\]]+\]\(([^)]+)\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownReferenceUsageRegex = new(@"\[(?<text>[^\]]+)\]\[(?<label>[^\]]*)\]", RegexOptions.Compiled);
    private static readonly Regex MarkdownReferenceDefinitionRegex = new(@"^\s*\[(?<label>[^\]]+)\]:\s*(?<target>\S+)", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex MarkdownShortcutReferenceUsageRegex = new(@"\[(?<label>[^\[\]]+)\](?![\(\[:])", RegexOptions.Compiled);

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
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsFamilyBoundaryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsDemoEntryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.VideraChartViewEntryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsFirstChartTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsStartHereTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsRendererStatusTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsViewStateTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionQualityTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsOverlayOptionsTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsRenderingStatusFieldTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionDiagnosticsTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsOverlayBoundaryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsPlotEntryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsScatterProofTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsColumnarStreamingTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsStreamingBenchmarkTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsAnalyticsProofTokens);
        readme.Should().Contain("tighter interactive residency");
        readme.Should().Contain("lower probe-path churn");
        readme.Should().Contain("axis/legend overlays");
        readme.Should().Contain("src/Videra.SurfaceCharts.Core/README.md");
        readme.Should().Contain("src/Videra.SurfaceCharts.Avalonia/README.md");
        readme.Should().Contain("src/Videra.SurfaceCharts.Processing/README.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
        readme.Should().NotContain("current Avalonia controls");
        readme.Should().NotContain("controls remain the public chart entrypoints");
    }

    [Fact]
    public void ChineseReadme_ShouldLinkSurfaceChartsModulePagesAndDemo()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "README.md"));

        readme.Should().Contain("modules/videra-surfacecharts-core.md");
        readme.Should().Contain("modules/videra-surfacecharts-avalonia.md");
        readme.Should().Contain("modules/videra-surfacecharts-processing.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
        readme.Should().Contain("VideraChartView");
        readme.Should().Contain("Videra.SurfaceCharts.Demo");
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFamilyBoundaryTokens);
        readme.Should().Contain("交互驻留更紧");
        readme.Should().Contain("probe 路径抖动更低");
        readme.Should().Contain("RenderStatusChanged");
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFirstChartTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsStartHereTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsTruthTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsViewStateTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionQualityTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayOptionsTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionDiagnosticsTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayBoundaryTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsSourceFirstTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsColumnarStreamingTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.SurfaceChartsStreamingBenchmarkTokens);
        readme.Should().NotContain("三个 Avalonia 控件");
        readme.Should().NotContain("第二个控件证明");
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
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Core", "README.md"));
        var renderingReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Rendering", "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));

        AssertContainsAllTokens(coreReadme, SurfaceChartsDocumentationTerms.SurfaceChartsScalarCompatibilityTokens);
        coreReadme.Should().Contain("simplest source-first regular-grid entrypoint");
        coreReadme.Should().Contain("Advanced callers can keep the same chart shell");
        coreReadme.Should().Contain("independent `ColorField`");
        coreReadme.Should().Contain("first-class `SurfaceMask`");
        coreReadme.Should().Contain("tighter interactive residency");
        coreReadme.Should().Contain("lower probe-path churn");
        AssertContainsAllTokens(coreReadme, SurfaceChartsDocumentationTerms.SurfaceChartsColumnarStreamingTokens);

        renderingReadme.Should().Contain("tighter interactive residency");
        renderingReadme.Should().Contain("lower probe-path churn");

        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsRendererBoundaryTokens);
        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsGpuFallbackTokens);
        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsAvaloniaScalarCompatibilityTokens);
        avaloniaReadme.Should().Contain(LinuxWaylandLimitSentence);
        avaloniaReadme.Should().Contain("independent from `VideraView`");
        avaloniaReadme.Should().Contain("ViewState");
        avaloniaReadme.Should().Contain("OverlayOptions");
        avaloniaReadme.Should().Contain("hover/pinned probe");
        avaloniaReadme.Should().Contain("FitToData()");
        avaloniaReadme.Should().Contain("ResetCamera()");
        avaloniaReadme.Should().Contain("ZoomTo(...)");
        avaloniaReadme.Should().Contain("without widening `VideraChartView` itself");
        avaloniaReadme.Should().Contain("tighter under camera movement");
        avaloniaReadme.Should().Contain("probe work");
        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsAvaloniaReadmeContractTokens);
        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsPlotEntryTokens);
        AssertContainsAllTokens(avaloniaReadme, SurfaceChartsDocumentationTerms.SurfaceChartsColumnarStreamingTokens);

        demoReadme.Should().Contain("not a `VideraView` mode");
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsDemoGpuFallbackTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsDemoFirstChartTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsViewStateTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionQualityTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsInteractionDiagnosticsTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsOverlayBoundaryTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsRenderingStatusFieldTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsAnalyticsProofTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsColumnarStreamingTokens);
        demoReadme.Should().Contain("FitToData()");
        demoReadme.Should().Contain("ResetCamera()");
        demoReadme.Should().Contain("ZoomTo(...)");
        demoReadme.Should().Contain("SurfaceChartOverlayOptions");
        demoReadme.Should().Contain("tighter interactive residency");
        demoReadme.Should().Contain("lower probe-path churn");
        demoReadme.Should().Contain("`XWayland compatibility` only, not compositor-native Wayland surface embedding");
        demoReadme.Should().NotContain("second control proof");
        AssertMarkdownFileDoesNotContainSelfReferentialFileLinks(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));
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
    public void SurfaceChartResidency_ShouldNotCloneScalarFieldsIntoResidentCopies()
    {
        var repositoryRoot = GetRepositoryRoot();
        var renderState = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Rendering", "SurfaceChartRenderState.cs"));
        var residentTile = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Rendering", "SurfaceChartResidentTile.cs"));

        renderState.Should().NotContain("(sourceTile.ColorField?.Values ?? sourceTile.Values).ToArray()");
        residentTile.Should().NotContain("Array.AsReadOnly(values.ToArray())");
    }

    [Fact]
    public void SurfaceChartProcessingReadme_ShouldDescribeBenchmarkingAndOptionalNativeSeam()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Processing", "README.md"));

        readme.Should().Contain("BenchmarkDotNet");
        readme.Should().Contain("optional native seam");
        readme.Should().Contain("SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement");
        readme.Should().Contain("SurfaceChartsProbeBenchmarks.ProbeLatency");
        readme.Should().Contain("future escalation guidance");
        readme.Should().Contain("The live scheduler now consumes those ordered batch reads whenever a source implements `ISurfaceTileBatchSource`");
        readme.Should().Contain("per-tile fallback path");
    }

    [Fact]
    public void ChineseSurfaceChartPages_ShouldMirrorRendererAndProcessingTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var corePage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-core.md"));
        var avaloniaPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));
        var processingPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-processing.md"));

        AssertContainsAllTokens(corePage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsScalarCompatibilityTokens);
        corePage.Should().Contain("默认的 source-first regular-grid 入口");
        corePage.Should().Contain("独立的 `ColorField`");
        corePage.Should().Contain("一等 `SurfaceMask`");
        corePage.Should().Contain("交互驻留更紧");
        corePage.Should().Contain("probe 路径抖动更低");
        AssertContainsAllTokens(corePage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsColumnarStreamingTokens);

        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseAvaloniaRenderStatusTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseAvaloniaProbeTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsStartHereTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsViewStateTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionQualityTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayOptionsTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionDiagnosticsTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayBoundaryTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldTokens);
        avaloniaPage.Should().Contain("交互驻留更紧");
        avaloniaPage.Should().Contain("probe 路径抖动更低");
        avaloniaPage.Should().Contain("FitToData()");
        avaloniaPage.Should().Contain("ResetCamera()");
        avaloniaPage.Should().Contain("ZoomTo(...)");
        avaloniaPage.Should().Contain("SurfaceChartOverlayOptions");
        avaloniaPage.Should().Contain("XWayland compatibility");
        AssertContainsAllTokens(avaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsColumnarStreamingTokens);

        processingPage.Should().Contain(SurfaceChartsDocumentationTerms.ChineseProcessingStatisticsSentence);
        processingPage.Should().Contain("SurfaceChartsRenderStateBenchmarks.ApplyResidencyChurnUnderCameraMovement");
        processingPage.Should().Contain("SurfaceChartsProbeBenchmarks.ProbeLatency");
        processingPage.Should().Contain("future escalation guidance");
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
    public void SurfaceChartPlotRuntimeGuardrails_ShouldRejectDeletedSourceApiAndExamples()
    {
        var repositoryRoot = GetRepositoryRoot();
        var publicEntryFiles = new[]
        {
            "README.md",
            "docs/zh-CN/README.md",
            "docs/zh-CN/modules/videra-surfacecharts-avalonia.md",
            "src/Videra.SurfaceCharts.Avalonia/README.md",
            "samples/Videra.SurfaceCharts.Demo/README.md",
            "samples/Videra.SurfaceCharts.Demo/Views/MainWindow.axaml.cs",
            "smoke/Videra.SurfaceCharts.ConsumerSmoke/Views/MainWindow.axaml.cs"
        };
        var stalePublicTerms = new[]
        {
            "VideraChartView.Source",
            "SourceProperty",
            "Source path:",
            "Source details:",
            ".Source =",
            "new VideraChartView { Source"
        };

        foreach (var relativePath in publicEntryFiles)
        {
            var content = File.ReadAllText(Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

            foreach (var staleTerm in stalePublicTerms)
            {
                content.Should().NotContain(staleTerm, $"{relativePath} must stay on Plot.Add.* runtime loading instead of the deleted direct Source API");
            }

            if (relativePath.EndsWith("README.md", StringComparison.Ordinal))
            {
                content.Should().Contain("Plot.Add", $"{relativePath} should teach the Plot-owned chart path");
            }
        }

        var viewProperties = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Properties.cs"));
        viewProperties.Should().NotContain("SourceProperty");
        viewProperties.Should().NotContain("public ISurfaceTileSource? Source");

        var controlsRoot = Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls");
        var publicControlSources = Directory
            .GetFiles(controlsRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => (Path: path, Content: File.ReadAllText(path)))
            .Where(source => source.Content.Contains("public", StringComparison.Ordinal))
            .ToArray();

        foreach (var source in publicControlSources)
        {
            source.Content.Should().NotMatchRegex(
                @"public\s+(?:sealed\s+|partial\s+|abstract\s+)*class\s+(?:SurfaceChartView|WaterfallChartView|ScatterChartView)\b",
                $"{Path.GetRelativePath(repositoryRoot, source.Path)} must not restore old chart-specific public controls");

            if (Path.GetFileName(source.Path).StartsWith("VideraChartView", StringComparison.Ordinal))
            {
                source.Content.Should().NotContain("SourceProperty", "VideraChartView must stay Plot.Add-owned");
                source.Content.Should().NotMatchRegex(
                    @"public\s+.*\bSource\b",
                    "VideraChartView must not restore the deleted direct Source API");
            }
        }
    }

    [Fact]
    public void VideraChartStateAndCommandApis_ShouldStayOutOfVideraView()
    {
        var repositoryRoot = GetRepositoryRoot();
        var controlsRoot = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls");
        var publicVideraViewSurface = Directory.GetFiles(controlsRoot, "VideraView*.cs", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .Where(content => content.Contains("public partial class VideraView", StringComparison.Ordinal))
            .ToArray();

        publicVideraViewSurface.Should().NotBeEmpty();
        var videraView = string.Join(Environment.NewLine, publicVideraViewSurface);

        Regex.IsMatch(videraView, @"public\s+.*\bSurfaceViewState\b").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bSurfaceDataWindow\b").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bSurfaceDisplaySpace\b").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bFitToData\s*\(").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bZoomTo\s*\(").Should().BeFalse();
        Regex.IsMatch(videraView, @"public\s+.*\bSurfaceChart\w*\b").Should().BeFalse();
    }

    [Fact]
    public void VideraChartView_ShouldDelegateTileResidencyAndOverlayStateToInternalSeams()
    {
        var repositoryRoot = GetRepositoryRoot();
        var viewApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Core.cs"));
        var viewOverlay = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Overlay.cs"));
        var viewRendering = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Rendering.cs"));
        var runtime = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "Interaction", "SurfaceChartRuntime.cs"));
        var overlayCoordinator = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "Overlay", "SurfaceChartOverlayCoordinator.cs"));

        viewApi.Should().NotContain("private readonly SurfaceTileCache _tileCache");
        viewOverlay.Should().Contain("_overlayCoordinator");
        viewOverlay.Should().NotContain("_overlayState");
        viewOverlay.Should().NotContain("_axisOverlayState");
        viewOverlay.Should().NotContain("_legendOverlayState");
        viewRendering.Should().Contain("_runtime.GetLoadedTiles()");
        runtime.Should().Contain("private readonly SurfaceTileCache _tileCache = new();");
        runtime.Should().Contain("internal IReadOnlyList<SurfaceTile> GetLoadedTiles()");
        overlayCoordinator.Should().Contain("internal sealed class SurfaceChartOverlayCoordinator");
        overlayCoordinator.Should().Contain("public SurfaceProbeOverlayState ProbeState");
        overlayCoordinator.Should().Contain("public SurfaceAxisOverlayState AxisState");
        overlayCoordinator.Should().Contain("public SurfaceLegendOverlayState LegendState");
    }

    [Fact]
    public void GuardedSurfaceChartDocs_ShouldFreezeFirstChartContractLanguage()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));
        var chineseReadme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var chineseAvaloniaPage = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));

        AssertContainsAllTokens(rootReadme, SurfaceChartsDocumentationTerms.SurfaceChartsFirstChartTokens);
        AssertContainsAllTokens(rootReadme, SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(rootReadme, SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(rootReadme, SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstTokens);

        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsDemoFirstChartTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsSourceFirstTokens);
        AssertContainsAllTokens(demoReadme, SurfaceChartsDocumentationTerms.SurfaceChartsScatterProofTokens);
        AssertMarkdownFileDoesNotContainSelfReferentialFileLinks(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));

        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFirstChartTokens);
        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsSourceFirstTokens);
        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsScatterProofTokens);
        AssertContainsAllTokens(chineseReadme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsAnalyticsProofTokens);

        AssertContainsAllTokens(chineseAvaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(chineseAvaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(chineseAvaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldTokens);
        AssertContainsAllTokens(chineseAvaloniaPage, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsScatterProofTokens);
    }

    [Fact]
    public void VideraChartViewPublicApis_ShouldCarryContractXmlDocs()
    {
        var repositoryRoot = GetRepositoryRoot();
        var viewApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Core.cs"));
        var viewPropertiesApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "VideraChartView.Properties.cs"));
        var plotApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Controls", "Plot", "Plot3D.cs"));

        AssertContainsAllTokens(viewApi, SurfaceChartsDocumentationTerms.VideraChartViewTypeXmlDocTokens);
        AssertContainsAllTokens(viewApi, SurfaceChartsDocumentationTerms.SurfaceChartRenderingStatusXmlDocTokens);
        AssertContainsAllTokens(plotApi, SurfaceChartsDocumentationTerms.SurfaceChartOverlayOptionsXmlDocTokens);
        AssertContainsAllTokens(viewPropertiesApi, SurfaceChartsDocumentationTerms.SurfaceChartInteractionQualityXmlDocTokens);
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

    private static void AssertContainsAllTokens(string content, IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            content.Should().Contain(token);
        }
    }

    private static void AssertMarkdownFileDoesNotContainSelfReferentialFileLinks(string markdownPath)
    {
        var absoluteMarkdownPath = Path.GetFullPath(markdownPath);
        var markdownDirectory = Path.GetDirectoryName(absoluteMarkdownPath)
            ?? throw new DirectoryNotFoundException($"Could not determine directory for markdown file '{markdownPath}'.");
        var content = File.ReadAllText(absoluteMarkdownPath);
        var referenceTargets = MarkdownReferenceDefinitionRegex.Matches(content)
            .ToDictionary(
                static match => NormalizeMarkdownReferenceLabel(match.Groups["label"].Value),
                static match => match.Groups["target"].Value.Trim(),
                StringComparer.Ordinal);

        foreach (Match match in MarkdownLinkRegex.Matches(content))
        {
            var target = match.Groups[1].Value.Trim();
            AssertMarkdownTargetIsNotSelfReferential(markdownPath, absoluteMarkdownPath, markdownDirectory, target);
        }

        foreach (Match match in MarkdownReferenceUsageRegex.Matches(content))
        {
            var label = match.Groups["label"].Value;
            if (string.IsNullOrWhiteSpace(label))
            {
                label = match.Groups["text"].Value;
            }

            if (!referenceTargets.TryGetValue(NormalizeMarkdownReferenceLabel(label), out var target))
            {
                continue;
            }

            AssertMarkdownTargetIsNotSelfReferential(markdownPath, absoluteMarkdownPath, markdownDirectory, target);
        }

        foreach (Match match in MarkdownShortcutReferenceUsageRegex.Matches(content))
        {
            var label = match.Groups["label"].Value;
            if (!referenceTargets.TryGetValue(NormalizeMarkdownReferenceLabel(label), out var target))
            {
                continue;
            }

            AssertMarkdownTargetIsNotSelfReferential(markdownPath, absoluteMarkdownPath, markdownDirectory, target);
        }
    }

    private static void AssertMarkdownTargetIsNotSelfReferential(
        string markdownPath,
        string absoluteMarkdownPath,
        string markdownDirectory,
        string target)
    {
        if (string.IsNullOrWhiteSpace(target) || target.StartsWith('#'))
        {
            return;
        }

        var targetPath = target.Split('#')[0];
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            return;
        }

        if (Uri.TryCreate(targetPath, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
        {
            return;
        }

        var resolvedPath = Path.GetFullPath(
            Path.Combine(markdownDirectory, targetPath.Replace('/', Path.DirectorySeparatorChar)));

        resolvedPath.Should().NotBe(
            absoluteMarkdownPath,
            $"markdown file '{markdownPath}' should not link back to itself through '{target}'.");
    }

    private static string NormalizeMarkdownReferenceLabel(string label)
    {
        return Regex.Replace(label.Trim(), @"\s+", " ").ToLowerInvariant();
    }
}
