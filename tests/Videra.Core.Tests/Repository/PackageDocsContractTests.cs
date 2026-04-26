using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class PackageDocsContractTests
{
    [Fact]
    public void ImportPackageReadmes_ShouldRequireExplicitAvaloniaInstallForImporterBackedLoading()
    {
        var repositoryRoot = GetRepositoryRoot();
        var gltfReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Import.Gltf", "README.md"));
        var objReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Import.Obj", "README.md"));

        foreach (var readme in new[] { gltfReadme, objReadme })
        {
            readme.Should().Contain("explicitly");
            readme.Should().Contain("VideraViewOptions.UseModelImporter");
            readme.Should().Contain("LoadModelAsync(...)");
            readme.Should().Contain("LoadModelsAsync(...)");
            readme.Should().NotContain("already brings this package transitively");
            readme.Should().NotContain("do not need to add it manually");
        }
    }

    [Fact]
    public void RootReadme_ShouldExposeScenarioBasedInstallMatrix()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));

        readme.Should().Contain("Install by scenario");
        readme.Should().Contain("| Scenario | Install packages | Notes |");
        readme.Should().Contain("Viewer only");
        readme.Should().Contain("Viewer + OBJ");
        readme.Should().Contain("Viewer + glTF");
        readme.Should().Contain("SurfaceCharts");
        readme.Should().Contain("Core-only");
    }

    [Fact]
    public void PackageDocs_ShouldMatchAvaloniaImporterDependencyBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var avaloniaProject = XDocument.Load(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Videra.Avalonia.csproj"));
        var avaloniaReferences = avaloniaProject
            .Descendants()
            .Where(element => element.Name.LocalName is "ProjectReference" or "PackageReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var packageMatrix = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "package-matrix.md"));

        avaloniaReferences.Should().Contain(reference => reference.Contains("Videra.Core", StringComparison.Ordinal));
        avaloniaReferences.Should().NotContain(reference => reference.Contains("Videra.Import.Gltf", StringComparison.Ordinal));
        avaloniaReferences.Should().NotContain(reference => reference.Contains("Videra.Import.Obj", StringComparison.Ordinal));

        foreach (var document in new[] { readme, packageMatrix })
        {
            document.Should().Contain("Videra.Avalonia");
            document.Should().Contain("Videra.Import.Gltf");
            document.Should().Contain("Videra.Import.Obj");
            document.Should().Contain("explicit");
            document.Should().Contain("VideraViewOptions.UseModelImporter");
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
