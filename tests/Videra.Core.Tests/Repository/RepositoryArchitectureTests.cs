using FluentAssertions;
using System.Linq;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryArchitectureTests
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

    private static readonly string[] ForbiddenRenderSessionOrchestratorSymbols =
    {
        "WriteableBitmap",
        "DispatcherTimer",
        "NativeControlHost"
    };

    private static readonly string[] PublicExtensibilitySymbols =
    {
        "IRenderPassContributor",
        "RegisterPassContributor",
        "ReplacePassContributor",
        "RegisterFrameHook",
        "RenderFrameHookPoint",
        "GetRenderCapabilities",
        "RenderCapabilities"
    };

    private static readonly string[] PublicExtensibilityFlowSymbols =
    {
        "VideraView.Engine",
        "RegisterPassContributor",
        "RegisterFrameHook",
        "RenderCapabilities",
        "BackendDiagnostics",
        "LoadModelAsync",
        "FrameAll()"
    };

    [Fact]
    public void VideraEngine_ShouldSplitRenderingAndResourceOrchestrationAcrossDedicatedPartialFiles()
    {
        var repositoryRoot = GetRepositoryRoot();
        var graphicsRoot = Path.Combine(repositoryRoot, "src", "Videra.Core", "Graphics");
        var renderPipelineRoot = Path.Combine(graphicsRoot, "RenderPipeline");

        var mainFile = Path.Combine(graphicsRoot, "VideraEngine.cs");
        var resourcesFile = Path.Combine(graphicsRoot, "VideraEngine.Resources.cs");
        var renderingFile = Path.Combine(graphicsRoot, "VideraEngine.Rendering.cs");
        var renderPipelineStageFile = Path.Combine(renderPipelineRoot, "RenderPipelineStage.cs");

        File.Exists(mainFile).Should().BeTrue();
        File.Exists(resourcesFile).Should().BeTrue();
        File.Exists(renderingFile).Should().BeTrue();
        File.Exists(renderPipelineStageFile).Should().BeTrue();

        var resourcesSource = File.ReadAllText(resourcesFile);
        var renderingSource = File.ReadAllText(renderingFile);
        var mainSource = File.ReadAllText(mainFile);
        var renderPipelineStageSource = File.ReadAllText(renderPipelineStageFile);

        mainSource.Should().Contain("private enum EngineLifecycleState");
        mainSource.Should().Contain("LastPipelineSnapshot");
        resourcesSource.Should().Contain("private void CreateResourcesUnsafe()");
        resourcesSource.Should().Contain("private void ReleaseGraphicsResourcesUnsafe");
        renderingSource.Should().Contain("public void Draw()");
        renderingSource.Should().Contain("private RenderFramePlan CreateFramePlan");
        renderingSource.Should().Contain("private void ExecuteFramePlan");
        renderingSource.Should().Contain("private void RenderSolidObjects");
        renderPipelineStageSource.Should().Contain("PrepareFrame");
        renderPipelineStageSource.Should().Contain("PresentFrame");
    }

    [Fact]
    public void ArchitectureDocs_ShouldMirrorPipelineVocabulary_AndDiagnosticsTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));

        foreach (var stageName in PipelineStageNames)
        {
            architecture.Should().Contain(stageName);
            coreReadme.Should().Contain(stageName);
        }

        architecture.Should().Contain("LastPipelineSnapshot");
        architecture.Should().Contain("RenderPipelineProfile");
        architecture.Should().Contain("LastFrameStageNames");

        coreReadme.Should().Contain("RenderPipelineProfile");
        coreReadme.Should().Contain("LastFrameStageNames");
        coreReadme.Should().Contain("UsesSoftwarePresentationCopy");
    }

    [Fact]
    public void ArchitectureDocs_ShouldDescribeShippedPublicRenderPassExtensibility()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));

        foreach (var expectedSymbol in PublicExtensibilitySymbols)
        {
            architecture.Should().Contain(expectedSymbol);
            coreReadme.Should().Contain(expectedSymbol);
        }

        avaloniaReadme.Should().Contain("VideraView.Engine");
        avaloniaReadme.Should().Contain("VideraView.RenderCapabilities");
        architecture.Should().Contain("internal orchestration seams");
        architecture.Should().Contain("does not add package discovery");
    }

    [Fact]
    public void EnglishExtensibilityDocs_ShouldAlignEntrypointsSampleAndBoundaryVocabulary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var contractDocPath = Path.Combine(repositoryRoot, "docs", "extensibility.md");

        File.Exists(contractDocPath).Should().BeTrue();
        var contractDoc = File.ReadAllText(contractDocPath);

        architecture.Should().Contain("docs/extensibility.md");
        architecture.Should().Contain("samples/Videra.ExtensibilitySample");
        architecture.Should().Contain("disposed");
        architecture.Should().Contain("FallbackReason");
        architecture.Should().Contain("package discovery");

        docsIndex.Should().Contain("Extensibility Contract");
        docsIndex.Should().Contain("Videra.ExtensibilitySample");
        docsIndex.Should().Contain("package discovery");
        docsIndex.Should().Contain("plugin loading");

        foreach (var symbol in PublicExtensibilityFlowSymbols)
        {
            contractDoc.Should().Contain(symbol);
        }

        contractDoc.Should().Contain("disposed");
        contractDoc.Should().Contain("no-op");
        contractDoc.Should().Contain("AllowSoftwareFallback");
        contractDoc.Should().Contain("FallbackReason");
        contractDoc.Should().Contain("package discovery");
        contractDoc.Should().Contain("plugin loading");
    }

    [Fact]
    public void Phase10OrchestrationBoundary_ShouldHaveFilePresence_DocsSignoff_AndOrchestratorContainment()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var orchestratorFile = FindRepositoryFile(repositoryRoot, "RenderSessionOrchestrator.cs");
        var renderSessionInputsFile = FindRepositoryFile(repositoryRoot, "RenderSessionInputs.cs");
        var renderSessionSnapshotFile = FindRepositoryFile(repositoryRoot, "RenderSessionSnapshot.cs");
        var renderSessionBridgeFile = FindRepositoryFile(repositoryRoot, "VideraViewSessionBridge.cs");
        var videraViewFile = FindRepositoryFile(repositoryRoot, "VideraView.cs");

        architecture.Should().Contain("RenderSessionOrchestrator");
        architecture.Should().Contain("VideraViewSessionBridge");

        var orchestratorSource = File.ReadAllText(orchestratorFile);
        foreach (var forbidden in ForbiddenRenderSessionOrchestratorSymbols)
        {
            orchestratorSource.Should().NotContain(forbidden);
        }

        var viewSource = File.ReadAllText(videraViewFile);
        viewSource.Should().Contain("VideraViewSessionBridge");
        viewSource.Should().NotContain("_renderSession.Attach(");
        viewSource.Should().NotContain("_renderSession.BindHandle(");
        viewSource.Should().NotContain("_renderSession.Resize(");

        orchestratorFile.Should().NotBeNullOrWhiteSpace();
        renderSessionInputsFile.Should().NotBeNullOrWhiteSpace();
        renderSessionSnapshotFile.Should().NotBeNullOrWhiteSpace();
        renderSessionBridgeFile.Should().NotBeNullOrWhiteSpace();
        videraViewFile.Should().NotBeNullOrWhiteSpace();
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

    private static string FindRepositoryFile(string repositoryRoot, string fileName)
    {
        var file = Directory.EnumerateFiles(repositoryRoot, fileName, SearchOption.AllDirectories).FirstOrDefault();
        file.Should().NotBeNullOrWhiteSpace($"expected {fileName} to exist in repository tree");
        return file!;
    }
}
