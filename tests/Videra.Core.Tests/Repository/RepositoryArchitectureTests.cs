using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryArchitectureTests
{
    [Fact]
    public void VideraEngine_ShouldSplitRenderingAndResourceOrchestrationAcrossDedicatedPartialFiles()
    {
        var repositoryRoot = GetRepositoryRoot();
        var graphicsRoot = Path.Combine(repositoryRoot, "src", "Videra.Core", "Graphics");

        var mainFile = Path.Combine(graphicsRoot, "VideraEngine.cs");
        var resourcesFile = Path.Combine(graphicsRoot, "VideraEngine.Resources.cs");
        var renderingFile = Path.Combine(graphicsRoot, "VideraEngine.Rendering.cs");

        File.Exists(mainFile).Should().BeTrue();
        File.Exists(resourcesFile).Should().BeTrue();
        File.Exists(renderingFile).Should().BeTrue();

        var resourcesSource = File.ReadAllText(resourcesFile);
        var renderingSource = File.ReadAllText(renderingFile);
        var mainSource = File.ReadAllText(mainFile);

        mainSource.Should().Contain("private enum EngineLifecycleState");
        resourcesSource.Should().Contain("private void CreateResourcesUnsafe()");
        resourcesSource.Should().Contain("private void ReleaseGraphicsResourcesUnsafe");
        renderingSource.Should().Contain("public void Draw()");
        renderingSource.Should().Contain("private void RenderSolidObjects");
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
