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

    private static readonly string[] ScenePipelineInternalSymbols =
    {
        "SceneDocumentStore",
        "SceneDeltaPlanner",
        "SceneResidencyRegistry",
        "SceneUploadQueue"
    };

    [Fact]
    public void Repository_ShouldIncludeViewerBenchmarkProjectForScenePipelineEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Videra.slnx"));
        var benchmarkProjectPath = Path.Combine(repositoryRoot, "benchmarks", "Videra.Viewer.Benchmarks", "Videra.Viewer.Benchmarks.csproj");
        var benchmarkSourcePath = Path.Combine(repositoryRoot, "benchmarks", "Videra.Viewer.Benchmarks", "ScenePipelineBenchmarks.cs");

        File.Exists(benchmarkProjectPath).Should().BeTrue();
        File.Exists(benchmarkSourcePath).Should().BeTrue();

        solution.Should().Contain("benchmarks/Videra.Viewer.Benchmarks/Videra.Viewer.Benchmarks.csproj");

        var benchmarkProject = File.ReadAllText(benchmarkProjectPath);
        benchmarkProject.Should().Contain("BenchmarkDotNet");
        benchmarkProject.Should().Contain(@"..\..\src\Videra.Core\Videra.Core.csproj");
        benchmarkProject.Should().Contain(@"..\..\src\Videra.Avalonia\Videra.Avalonia.csproj");

        var benchmarkSource = File.ReadAllText(benchmarkSourcePath);
        benchmarkSource.Should().Contain("ModelImporter_Import");
        benchmarkSource.Should().Contain("SceneResidencyRegistry_ApplyDelta");
        benchmarkSource.Should().Contain("SceneUploadQueue_Drain");
        benchmarkSource.Should().Contain("ScenePipeline_RehydrateAfterBackendReady");
    }

    [Fact]
    public void VideraViewRuntime_ShouldDelegateSceneOrchestrationToSceneRuntimeCoordinator()
    {
        var repositoryRoot = GetRepositoryRoot();
        var coordinatorPath = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Runtime", "Scene", "SceneRuntimeCoordinator.cs");
        var runtimeScenePath = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Runtime", "VideraViewRuntime.Scene.cs");

        File.Exists(coordinatorPath).Should().BeTrue();
        File.Exists(runtimeScenePath).Should().BeTrue();

        var coordinatorSource = File.ReadAllText(coordinatorPath);
        coordinatorSource.Should().Contain("SceneDocumentStore");
        coordinatorSource.Should().Contain("SceneDeltaPlanner");
        coordinatorSource.Should().Contain("SceneResidencyRegistry");
        coordinatorSource.Should().Contain("SceneUploadQueue");
        coordinatorSource.Should().Contain("PublishSceneDocument");

        var runtimeSceneSource = File.ReadAllText(runtimeScenePath);
        runtimeSceneSource.Should().Contain("_sceneCoordinator");
        runtimeSceneSource.Should().NotContain("_sceneDocumentStore");
        runtimeSceneSource.Should().NotContain("_sceneDeltaPlanner");
        runtimeSceneSource.Should().NotContain("_sceneResidencyRegistry");
        runtimeSceneSource.Should().NotContain("_sceneUploadQueue");
        runtimeSceneSource.Should().NotContain("_sceneEngineApplicator");
        runtimeSceneSource.Should().NotContain("private void PublishSceneDocument(");
    }

    [Fact]
    public void PublicViewerSceneAndCameraApis_ShouldCarryXmlDocs_AndSharedQuickStartVocabulary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var sceneApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraView.Scene.cs"));
        var cameraApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraView.Camera.cs"));
        var loadResultApi = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "ModelLoadResult.cs"));
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));
        var extensibilityDoc = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "extensibility.md"));

        sceneApi.Should().Contain("/// <summary>");
        sceneApi.Should().Contain("LoadModelAsync");
        sceneApi.Should().Contain("LoadModelsAsync");
        sceneApi.Should().Contain("AddObject");
        sceneApi.Should().Contain("ReplaceScene");
        sceneApi.Should().Contain("ClearScene");

        cameraApi.Should().Contain("/// <summary>");
        cameraApi.Should().Contain("ResetCamera");
        cameraApi.Should().Contain("FrameAll");
        cameraApi.Should().Contain("SetViewPreset");

        loadResultApi.Should().Contain("/// <summary>");
        loadResultApi.Should().Contain("ModelLoadResult");
        loadResultApi.Should().Contain("ModelLoadBatchResult");
        loadResultApi.Should().Contain("ModelLoadFailure");

        readme.Should().Contain("LoadModelAsync");
        readme.Should().Contain("BackendDiagnostics");
        readme.Should().Contain("FrameAll()");
        avaloniaReadme.Should().Contain("LoadModelsAsync(...)");
        avaloniaReadme.Should().Contain("BackendDiagnostics");
        avaloniaReadme.Should().Contain("Scene Pipeline Lab");
        extensibilityDoc.Should().Contain("LoadModelAsync(...)");
        extensibilityDoc.Should().Contain("FrameAll()");
        extensibilityDoc.Should().Contain("BackendDiagnostics");
    }

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
    public void BackendDocs_ShouldDescribeBuiltInMinimumContract_WithoutOpenGlPromise()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));
        var supportMatrix = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "support-matrix.md"));

        architecture.Should().Contain("Built-in backend minimum contract");
        architecture.Should().Contain("CreateShader(...)");
        architecture.Should().Contain("CreateResourceSet(...)");
        architecture.Should().Contain("SetResourceSet(...)");
        architecture.Should().Contain("OpenGL");

        coreReadme.Should().Contain("Built-in Backend Minimum Contract");
        coreReadme.Should().Contain("CreateShader(...)");
        coreReadme.Should().Contain("CreateResourceSet(...)");
        coreReadme.Should().Contain("SetResourceSet(...)");
        coreReadme.Should().Contain("OpenGL");

        supportMatrix.Should().Contain("built-in backend minimum contract");
        supportMatrix.Should().Contain("CreateShader(...)");
        supportMatrix.Should().Contain("CreateResourceSet(...)");
        supportMatrix.Should().Contain("SetResourceSet(...)");
        supportMatrix.Should().Contain("OpenGL");
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
    public void ArchitectureDocs_ShouldDescribeSceneDocumentTruth_AndDirectDeviceSurfacePath()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));
        var extensibilityDoc = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "extensibility.md"));

        architecture.Should().Contain("SceneDocument");
        architecture.Should().Contain("IGraphicsDevice");
        architecture.Should().Contain("IRenderSurface");
        architecture.Should().Contain("LegacyGraphicsBackendAdapter");
        architecture.Should().Contain("compatibility");
        foreach (var symbol in ScenePipelineInternalSymbols)
        {
            architecture.Should().Contain(symbol);
        }

        coreReadme.Should().Contain("IGraphicsDevice");
        coreReadme.Should().Contain("IRenderSurface");
        coreReadme.Should().Contain("SceneDocument");
        coreReadme.Should().Contain("ImportedSceneAsset.Metrics");
        avaloniaReadme.Should().Contain("SceneDocument");
        avaloniaReadme.Should().Contain("SceneUploadQueue");
        avaloniaReadme.Should().Contain("SceneResidencyRegistry");
        avaloniaReadme.Should().Contain("LoadModelsAsync(...)");
        avaloniaReadme.Should().Contain("active scene only when every requested file succeeds");
        extensibilityDoc.Should().Contain("SceneDocument");
        extensibilityDoc.Should().Contain("SceneDeltaPlanner");
        extensibilityDoc.Should().Contain("SceneResidencyRegistry");
        extensibilityDoc.Should().Contain("SceneUploadQueue");
        extensibilityDoc.Should().Contain("LoadModelsAsync(...)");
        extensibilityDoc.Should().Contain("active scene is replaced only when every requested file succeeds");
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
    public void EnglishInteractionDocs_ShouldAlignSampleAndHostOwnedContractVocabulary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md"));
        var sampleReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.InteractionSample", "README.md"));

        foreach (var symbol in InteractionContractDocumentationTerms.SharedApiSymbols)
        {
            readme.Should().Contain(symbol);
            avaloniaReadme.Should().Contain(symbol);
            sampleReadme.Should().Contain(symbol);
        }

        foreach (var marker in InteractionContractDocumentationTerms.SharedBehaviorMarkers)
        {
            readme.Should().Contain(marker);
            avaloniaReadme.Should().Contain(marker);
            sampleReadme.Should().Contain(marker);
        }

        readme.Should().Contain(InteractionContractDocumentationTerms.EnglishOwnershipSentence);
        avaloniaReadme.Should().Contain(InteractionContractDocumentationTerms.EnglishOwnershipSentence);

        foreach (var forbidden in InteractionContractDocumentationTerms.ForbiddenNodeAnchorPhrases)
        {
            readme.Should().NotContain(forbidden);
            avaloniaReadme.Should().NotContain(forbidden);
            sampleReadme.Should().NotContain(forbidden);
        }
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
        var runtimeFile = FindRepositoryFile(repositoryRoot, "VideraViewRuntime.cs");
        var videraViewFile = FindRepositoryFile(repositoryRoot, "VideraView.cs");

        architecture.Should().Contain("RenderSessionOrchestrator");
        architecture.Should().Contain("VideraViewRuntime");
        architecture.Should().Contain("VideraViewSessionBridge");

        var orchestratorSource = File.ReadAllText(orchestratorFile);
        foreach (var forbidden in ForbiddenRenderSessionOrchestratorSymbols)
        {
            orchestratorSource.Should().NotContain(forbidden);
        }

        var viewSource = File.ReadAllText(videraViewFile);
        viewSource.Should().Contain("VideraViewRuntime");
        viewSource.Should().NotContain("_renderSession.Attach(");
        viewSource.Should().NotContain("_renderSession.BindHandle(");
        viewSource.Should().NotContain("_renderSession.Resize(");

        var runtimeSource = File.ReadAllText(runtimeFile);
        runtimeSource.Should().Contain("VideraViewSessionBridge");

        orchestratorFile.Should().NotBeNullOrWhiteSpace();
        renderSessionInputsFile.Should().NotBeNullOrWhiteSpace();
        renderSessionSnapshotFile.Should().NotBeNullOrWhiteSpace();
        renderSessionBridgeFile.Should().NotBeNullOrWhiteSpace();
        runtimeFile.Should().NotBeNullOrWhiteSpace();
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
