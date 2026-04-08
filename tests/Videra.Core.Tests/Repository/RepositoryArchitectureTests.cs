using FluentAssertions;
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
    public void ArchitectureDocs_ShouldNotOverclaimPublicRenderPassExtensibility()
    {
        var repositoryRoot = GetRepositoryRoot();
        var architecture = File.ReadAllText(Path.Combine(repositoryRoot, "ARCHITECTURE.md"));
        var coreReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "README.md"));

        foreach (var forbiddenSymbol in new[] { "RegisterPass(", "FrameHook", "IRenderPassContributor" })
        {
            architecture.Should().NotContain(forbiddenSymbol);
            coreReadme.Should().NotContain(forbiddenSymbol);
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
