using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class MinimalAuthoringSampleContractTests
{
    private static readonly string[] RequiredProgramMarkers =
    {
        "SceneAuthoring.Create(\"minimal-authoring\")",
        "SceneAuthoringPlacement.Identity",
        "SceneAuthoringPlacement.At(",
        "SceneAuthoringPlacement.From(",
        "SceneMaterials.Metal(",
        "SceneMaterials.Emissive(",
        "SceneGeometry.BoxOutline(",
        ".AddAxisTriad(",
        ".AddScaleBar(",
        ".AddInstances(",
        ".TryBuild()",
        "SceneAuthoringDiagnostic"
    };

    private static readonly string[] ForbiddenProgramMarkers =
    {
        "Videra.Import",
        "Videra.Avalonia",
        "VideraView",
        "LoadModel",
        ".obj",
        ".gltf",
        "File.Open",
        "File.ReadAll",
        "Workbench",
        "Editor"
    };

    [Fact]
    public void MinimalAuthoringSampleProject_ShouldStayCoreOnlyWithoutImporterOrViewerReferences()
    {
        var project = File.ReadAllText(Path.Combine(SampleRoot, "Videra.MinimalAuthoringSample.csproj"));

        project.Should().Contain(@"..\..\src\Videra.Core\Videra.Core.csproj");
        project.Should().NotContain("Videra.Import");
        project.Should().NotContain("Videra.Avalonia");
        project.Should().NotContain("Videra.Demo");
    }

    [Fact]
    public void Program_ShouldDemonstrateConciseAuthoringApiWithoutImporterDrift()
    {
        var program = File.ReadAllText(Path.Combine(SampleRoot, "Program.cs"));

        program.Split('\n').Should().HaveCountLessThanOrEqualTo(90);

        foreach (var marker in RequiredProgramMarkers)
        {
            program.Should().Contain(marker);
        }

        foreach (var forbidden in ForbiddenProgramMarkers)
        {
            program.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void Readme_ShouldDocumentDiagnosticsAndSampleNonGoals()
    {
        var readme = File.ReadAllText(Path.Combine(SampleRoot, "README.md"));

        readme.Should().Contain("TryBuild()");
        readme.Should().Contain("SceneAuthoringDiagnostic");
        readme.Should().Contain("viewer");
        readme.Should().Contain("importer");
        readme.Should().Contain("asset file");
        readme.Should().Contain("editor/workbench");
        readme.Should().Contain("hidden fallback path");
        readme.Should().Contain("instead of silently repairing invalid geometry");
    }

    private static string SampleRoot => Path.Combine(GetRepositoryRoot(), "samples", "Videra.MinimalAuthoringSample");

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
