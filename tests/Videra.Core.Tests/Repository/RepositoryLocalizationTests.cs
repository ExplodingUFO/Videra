using FluentAssertions;
using System.Linq;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryLocalizationTests
{
    private static readonly string[] PipelineStageNames =
    {
        "PrepareFrame",
        "BindSharedFrameState",
        "GridPass",
        "SolidGeometryPass",
        "WireframePass",
        "AxisPass",
        "PresentFrame"
    };

    private static readonly string[] PublicExtensibilitySymbols =
    {
        "IRenderPassContributor",
        "RegisterPassContributor",
        "ReplacePassContributor",
        "RegisterFrameHook",
        "RenderFrameHookPoint",
        "GetRenderCapabilities",
        "RenderCapabilities",
        "SupportsShaderCreation",
        "SupportsResourceSetCreation",
        "SupportsResourceSetBinding"
    };

    private static readonly string[] ExtensibilityContractSymbols =
    {
        "Videra.ExtensibilitySample",
        "RegisterPassContributor",
        "RegisterFrameHook",
        "RenderCapabilities",
        "BackendDiagnostics"
    };

    private static readonly string[] ExtensibilityBoundaryVocabulary =
    {
        "disposed",
        "no-op",
        "FallbackReason",
        "package discovery",
        "plugin loading"
    };

    [Fact]
    public void Readme_ShouldBeEnglishPrimaryWithChineseSwitch()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain("English");
        readme.Should().Contain("中文");
        readme.Should().Contain("docs/zh-CN/README.md");
        readme.Should().Contain("Cross-platform");
    }

    [Fact]
    public void EnglishEntryDocs_ShouldExposeChineseNavigation()
    {
        var docsIndex = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "index.md"));
        var architecture = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "ARCHITECTURE.md"));
        var contributing = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "CONTRIBUTING.md"));
        var troubleshooting = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "troubleshooting.md"));

        docsIndex.Should().Contain("zh-CN/index.md");
        architecture.Should().Contain("docs/zh-CN/ARCHITECTURE.md");
        contributing.Should().Contain("docs/zh-CN/CONTRIBUTING.md");
        troubleshooting.Should().Contain("zh-CN/troubleshooting.md");
    }

    [Fact]
    public void ChineseEntryDocs_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var expectedFiles = new[]
        {
            Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "ARCHITECTURE.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "CONTRIBUTING.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md")
        };

        foreach (var file in expectedFiles)
        {
            File.Exists(file).Should().BeTrue($"expected Chinese documentation file {file} to exist");
        }
    }

    [Fact]
    public void ChinesePackageReadmeMirrors_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();
        var expectedFiles = new[]
        {
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-core.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-core.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-windows.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-linux.md"),
            Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-macos.md")
        };

        foreach (var file in expectedFiles)
        {
            File.Exists(file).Should().BeTrue($"expected Chinese package mirror {file} to exist");
        }
    }

    [Fact]
    public void ChineseSurfaceChartsModulePages_ShouldExistAndUseVideraChartViewVocabulary()
    {
        var repositoryRoot = GetRepositoryRoot();

        foreach (var relativePath in SurfaceChartsDocumentationTerms.ExpectedChineseModulePages)
        {
            File.Exists(Path.Combine(repositoryRoot, relativePath)).Should().BeTrue($"expected localized surface-chart module page {relativePath} to exist");
        }

        var coreModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-core.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));

        foreach (var module in new[] { coreModule, avaloniaModule })
        {
            module.Should().Contain("VideraChartView");
            module.Should().Contain("VideraView");
            module.Should().Contain("Videra.SurfaceCharts.Demo");
        }

        coreModule.Should().Contain("SurfacePyramidBuilder");
        coreModule.Should().Contain("SurfaceTileSource");
        avaloniaModule.Should().Contain("SurfaceProbeOverlayPresenter");
        avaloniaModule.Should().Contain("独立于 `VideraView`");
    }

    [Fact]
    public void ChineseDemoDoc_ShouldMentionHighLevelViewerApi()
    {
        var demoDoc = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "modules", "demo.md"));

        demoDoc.Should().Contain("LoadModelAsync");
        demoDoc.Should().Contain("FrameAll");
    }

    [Fact]
    public void ChineseDemoDoc_ShouldDescribeDegradedPath_AndAvoidRawIsBackendReadyNarrative()
    {
        var demoDoc = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "modules", "demo.md"));

        demoDoc.Should().Contain("默认演示立方体");
        demoDoc.Should().Contain("状态区域");
        demoDoc.Should().Contain("仍可继续导入模型");
        demoDoc.Should().NotContain("`IsBackendReady` 为 `true` 后才可用");
    }

    [Fact]
    public void ChineseDistributionDocs_ShouldMirrorInstallGuidance()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var index = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"));
        var troubleshooting = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"));
        var coreModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-core.md"));
        var windowsModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-windows.md"));
        var linuxModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-linux.md"));
        var macosModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "platform-macos.md"));

        readme.Should().Contain("dotnet add package Videra.Avalonia");
        readme.Should().Contain("Videra.Platform.Windows");
        readme.Should().Contain("Videra.Platform.Linux");
        readme.Should().Contain("Videra.Platform.macOS");
        readme.Should().Contain("VIDERA_BACKEND");
        readme.Should().Contain("nuget.org");
        readme.Should().Contain("GitHub Packages");
        readme.Should().Contain("preview");
        readme.Should().Contain("package-matrix.md");
        readme.Should().Contain("support-matrix.md");
        readme.Should().Contain("release-policy.md");
        readme.Should().Contain("英文版为准");

        index.Should().Contain("安装");
        index.Should().Contain("故障排查");
        index.Should().Contain("package-matrix.md");
        index.Should().Contain("support-matrix.md");
        index.Should().Contain("release-policy.md");

        troubleshooting.Should().Contain("VIDERA_BACKEND");
        troubleshooting.Should().Contain("不会安装缺失的平台包");
        troubleshooting.Should().Contain("matching-host");
        troubleshooting.Should().Contain("XWayland");
        troubleshooting.Should().Contain("LastFrameObjectCount");
        troubleshooting.Should().Contain("LastFrameOpaqueObjectCount");
        troubleshooting.Should().Contain("LastFrameTransparentObjectCount");
        troubleshooting.Should().Contain("LastSnapshotExportPath");
        troubleshooting.Should().Contain("LastSnapshotExportStatus");
        troubleshooting.Should().Contain("CanReplayScene");
        troubleshooting.Should().Contain("ReplayLimitation");

        foreach (var module in new[] { avaloniaModule, coreModule, windowsModule, linuxModule, macosModule })
        {
            module.Should().Contain("nuget.org");
            module.Should().Contain("GitHub Packages");
            module.Should().Contain("preview");
            module.Should().Contain("英文版为准");
        }

        avaloniaModule.Should().Contain("PreferredBackend");
        avaloniaModule.Should().Contain("Videra.Platform.Windows");
        avaloniaModule.Should().Contain("Videra.Platform.Linux");
        avaloniaModule.Should().Contain("Videra.Platform.macOS");
        avaloniaModule.Should().Contain("LastFrameObjectCount");
        avaloniaModule.Should().Contain("LastFrameOpaqueObjectCount");
        avaloniaModule.Should().Contain("LastFrameTransparentObjectCount");
        avaloniaModule.Should().Contain("LastSnapshotExportPath");
        avaloniaModule.Should().Contain("LastSnapshotExportStatus");
        avaloniaModule.Should().Contain("CanReplayScene");
        avaloniaModule.Should().Contain("ReplayLimitation");
        linuxModule.Should().Contain("X11");
        linuxModule.Should().Contain("XWayland");
        macosModule.Should().Contain("CAMetalLayer");
    }

    [Fact]
    public void ChineseStaticGltfPbrDocs_ShouldDescribeRendererConsumptionBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var index = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"));
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "ARCHITECTURE.md"));
        var troubleshooting = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md"));
        var coreModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-core.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"));
        var demoModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "demo.md"));

        foreach (var document in new[] { readme, index, architecture, troubleshooting, coreModule, avaloniaModule, demoModule })
        {
            document.Should().Contain("baseColor");
            document.Should().Contain("occlusion texture binding/strength");
            document.Should().Contain("KHR_texture_transform");
            document.Should().Contain("emissive");
            document.Should().Contain("normal-map-ready");
            document.Should().Contain("renderer-consumption seam");
            document.Should().NotContain("不宣称 renderer/shader/backend 消费这些 metadata");
            document.Should().NotContain("并不宣称 renderer/shader/backend 会消费这些 metadata");
        }
    }

    [Fact]
    public void ChineseCoreModule_ShouldMirrorPipelineVocabulary_AndDiagnosticsTruth()
    {
        var coreModule = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "modules", "videra-core.md"));

        foreach (var stageName in PipelineStageNames)
        {
            coreModule.Should().Contain(stageName);
        }

        coreModule.Should().Contain("RenderPipelineProfile");
        coreModule.Should().Contain("LastFrameStageNames");
        coreModule.Should().Contain("LastFrameObjectCount");
        coreModule.Should().Contain("LastFrameOpaqueObjectCount");
        coreModule.Should().Contain("LastFrameTransparentObjectCount");
        coreModule.Should().Contain("UsesSoftwarePresentationCopy");
    }

    [Fact]
    public void ChineseDocs_ShouldMirrorShippedPublicRenderPassExtensibility()
    {
        var repositoryRoot = GetRepositoryRoot();
        var coreModule = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "modules", "videra-core.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"));
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "ARCHITECTURE.md"));

        foreach (var expectedSymbol in PublicExtensibilitySymbols)
        {
            coreModule.Should().Contain(expectedSymbol);
        }

        avaloniaModule.Should().Contain("VideraView.Engine");
        avaloniaModule.Should().Contain("VideraView.RenderCapabilities");
        avaloniaModule.Should().Contain("LastFrameObjectCount");
        avaloniaModule.Should().Contain("LastFrameOpaqueObjectCount");
        avaloniaModule.Should().Contain("LastFrameTransparentObjectCount");
        architecture.Should().Contain("public extensibility root");
        architecture.Should().Contain("package discovery");
        architecture.Should().Contain("LastFrameObjectCount");
        architecture.Should().Contain("LastFrameOpaqueObjectCount");
        architecture.Should().Contain("LastFrameTransparentObjectCount");
    }

    [Fact]
    public void ChineseLocalizedEntrypoints_ShouldLinkToExtensibilityContract_AndSampleFlow()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var index = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"));
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "ARCHITECTURE.md"));
        var coreModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-core.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"));

        foreach (var localizedDoc in new[] { readme, index, architecture, coreModule, avaloniaModule })
        {
            localizedDoc.Should().Contain("extensibility.md");
        }

        foreach (var localizedDoc in new[] { readme, index, coreModule, avaloniaModule })
        {
            localizedDoc.Should().Contain("Videra.ExtensibilitySample");
        }
    }

    [Fact]
    public void ChineseSurfaceChartsDocumentation_ShouldMirrorIndependentModuleFamilyBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var coreModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-core.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-surfacecharts-avalonia.md"));

        readme.Should().Contain("modules/videra-surfacecharts-core.md");
        readme.Should().Contain("modules/videra-surfacecharts-avalonia.md");
        readme.Should().Contain("samples/Videra.SurfaceCharts.Demo/README.md");
        readme.Should().Contain(SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFamilyBoundarySentence);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsFirstChartTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsStartHereTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldTokens);
        AssertContainsAllTokens(readme, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsSourceFirstTokens);

        foreach (var module in new[] { coreModule, avaloniaModule })
        {
            module.Should().Contain("VideraChartView");
            module.Should().Contain("Videra.SurfaceCharts.Demo");
            module.Should().Contain("独立于 `VideraView`");
        }

        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOwnershipTokens);
        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartControlOwnershipTokens);
        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsStartHereTokens);
        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsInteractionDiagnosticsTokens);
        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsOverlayBoundaryTokens);
        AssertContainsAllTokens(avaloniaModule, SurfaceChartsDocumentationTerms.ChineseSurfaceChartsRenderingStatusFieldTokens);
    }

    [Fact]
    public void ChineseExtensibilityContract_ShouldMirrorEnglishLifecycleAndBoundaryVocabulary()
    {
        var extensibilityPath = Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "extensibility.md");

        File.Exists(extensibilityPath).Should().BeTrue("expected localized extensibility contract page to exist");

        var extensibility = File.ReadAllText(extensibilityPath);

        foreach (var symbol in ExtensibilityContractSymbols)
        {
            extensibility.Should().Contain(symbol);
        }

        foreach (var vocabulary in ExtensibilityBoundaryVocabulary)
        {
            extensibility.Should().Contain(vocabulary);
        }
    }

    [Fact]
    public void ChineseInteractionDocs_ShouldMirrorInteractionSampleAndHostOwnedContract()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var avaloniaModule = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "modules", "videra-avalonia.md"));
        var sampleReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.InteractionSample", "README.md"));

        foreach (var symbol in InteractionContractDocumentationTerms.SharedApiSymbols)
        {
            readme.Should().Contain(symbol);
            avaloniaModule.Should().Contain(symbol);
            sampleReadme.Should().Contain(symbol);
        }

        foreach (var marker in InteractionContractDocumentationTerms.SharedBehaviorMarkers)
        {
            readme.Should().Contain(marker);
            avaloniaModule.Should().Contain(marker);
            sampleReadme.Should().Contain(marker);
        }

        readme.Should().Contain(InteractionContractDocumentationTerms.ChineseOwnershipSentence);
        avaloniaModule.Should().Contain(InteractionContractDocumentationTerms.ChineseOwnershipSentence);
        avaloniaModule.Should().Contain("LastSnapshotExportPath");
        avaloniaModule.Should().Contain("LastSnapshotExportStatus");
        avaloniaModule.Should().Contain("CanReplayScene");
        avaloniaModule.Should().Contain("ReplayLimitation");

        foreach (var forbidden in InteractionContractDocumentationTerms.ForbiddenNodeAnchorPhrases)
        {
            readme.Should().NotContain(forbidden);
            avaloniaModule.Should().NotContain(forbidden);
            sampleReadme.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void ChineseArchitectureDocs_ShouldReferencePhase10OrchestrationBoundary()
    {
        var architecture = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "zh-CN", "ARCHITECTURE.md"));

        architecture.Should().Contain("RenderSessionOrchestrator");
        architecture.Should().Contain("VideraViewSessionBridge");
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

    private static void AssertContainsAllTokens(string content, IEnumerable<string> tokens)
    {
        foreach (var token in tokens)
        {
            content.Should().Contain(token);
        }
    }
}
