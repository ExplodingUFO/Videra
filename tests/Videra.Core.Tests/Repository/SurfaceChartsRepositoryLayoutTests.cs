using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsRepositoryLayoutTests
{
    [Fact]
    public void SurfaceChartsModuleFamily_ShouldExistAsStandaloneProjectsAndDocs()
    {
        var repositoryRoot = GetRepositoryRoot();

        var expectedProjectFiles =
            new[]
            {
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Core", "Videra.SurfaceCharts.Core.csproj"),
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "Videra.SurfaceCharts.Avalonia.csproj"),
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Processing", "Videra.SurfaceCharts.Processing.csproj"),
                Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Videra.SurfaceCharts.Demo.csproj"),
                Path.Combine(repositoryRoot, "tests", "Videra.SurfaceCharts.Core.Tests", "Videra.SurfaceCharts.Core.Tests.csproj"),
                Path.Combine(repositoryRoot, "tests", "Videra.SurfaceCharts.Processing.Tests", "Videra.SurfaceCharts.Processing.Tests.csproj"),
                Path.Combine(repositoryRoot, "tests", "Videra.SurfaceCharts.Avalonia.IntegrationTests", "Videra.SurfaceCharts.Avalonia.IntegrationTests.csproj")
            };

        foreach (var projectFile in expectedProjectFiles)
        {
            File.Exists(projectFile).Should().BeTrue($"expected project file {projectFile} to exist");
        }

        var expectedReadmeFiles =
            new[]
            {
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Core", "README.md"),
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "README.md"),
                Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Processing", "README.md"),
                Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md")
            };

        foreach (var readmeFile in expectedReadmeFiles)
        {
            File.Exists(readmeFile).Should().BeTrue($"expected README file {readmeFile} to exist");
        }

        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Videra.slnx"));
        foreach (var projectFile in expectedProjectFiles)
        {
            var relativePath = Path.GetRelativePath(repositoryRoot, projectFile).Replace('\\', '/');
            solution.Should().Contain(relativePath);
        }
    }

    [Fact]
    public void SurfaceChartsCanonicalEntryDocs_ShouldExist()
    {
        var repositoryRoot = GetRepositoryRoot();

        foreach (var relativePath in SurfaceChartsDocumentationTerms.GuardedSurfaceChartsEntryPointPaths)
        {
            var absolutePath = Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            File.Exists(absolutePath).Should().BeTrue($"expected guarded SurfaceCharts entry doc {relativePath} to exist");
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
