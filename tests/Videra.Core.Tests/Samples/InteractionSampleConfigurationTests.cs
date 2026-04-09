using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class InteractionSampleConfigurationTests
{
    private static readonly string[] RequiredReadmeMarkers =
    {
        "SelectionState",
        "host owns",
        "annotation state",
        "Navigate",
        "Select",
        "Annotate",
        "object-level",
        "VideraNodeAnnotation",
        "VideraWorldPointAnnotation",
        "SelectionRequested",
        "AnnotationRequested",
        "3D highlight/render state",
        "2D label/feedback rendering"
    };

    private static readonly string[] ForbiddenInternalSeams =
    {
        "SelectionOverlayRenderState",
        "AnnotationOverlayRenderState",
        "VideraViewSessionBridge",
        "VideraInteractionController",
        "Videra.Demo"
    };

    [Fact]
    public void SampleFiles_ShouldExistAtDedicatedInteractionSamplePath_AndBeWiredIntoSolution()
    {
        var repositoryRoot = GetRepositoryRoot();
        var sampleRoot = Path.Combine(repositoryRoot, "samples", "Videra.InteractionSample");
        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Videra.slnx"));

        Directory.Exists(sampleRoot).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "README.md")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Videra.InteractionSample.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Program.cs")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "App.axaml")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "App.axaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Views", "MainWindow.axaml")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Views", "MainWindow.axaml.cs")).Should().BeTrue();

        solution.Should().Contain("samples/Videra.InteractionSample/Videra.InteractionSample.csproj");
    }

    [Fact]
    public void SampleReadme_ShouldDescribeThePublicInteractionContract()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.InteractionSample", "README.md"));

        foreach (var marker in RequiredReadmeMarkers)
        {
            readme.Should().Contain(marker);
        }
    }

    [Fact]
    public void MainWindowCodeBehind_ShouldDemonstrateHostOwnedPublicInteractionFlowOnly()
    {
        var codeBehind = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.InteractionSample", "Views", "MainWindow.axaml.cs"));

        codeBehind.Should().Contain("new VideraSelectionState");
        codeBehind.Should().Contain("IReadOnlyList<VideraAnnotation>");
        codeBehind.Should().Contain("View3D.SelectionState = _selectionState;");
        codeBehind.Should().Contain("View3D.Annotations = _annotations;");
        codeBehind.Should().Contain("View3D.SelectionRequested +=");
        codeBehind.Should().Contain("View3D.AnnotationRequested +=");
        codeBehind.Should().Contain("View3D.InteractionMode = VideraInteractionMode.Navigate;");
        codeBehind.Should().Contain("VideraInteractionMode.Select");
        codeBehind.Should().Contain("VideraInteractionMode.Annotate");
        codeBehind.Should().Contain("new VideraNodeAnnotation");
        codeBehind.Should().Contain("new VideraWorldPointAnnotation");

        foreach (var forbidden in ForbiddenInternalSeams)
        {
            codeBehind.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void SampleProject_ShouldReferenceAvaloniaEntryPointWithoutDemoCoupling()
    {
        var project = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.InteractionSample", "Videra.InteractionSample.csproj"));

        project.Should().Contain(@"..\..\src\Videra.Avalonia\Videra.Avalonia.csproj");
        project.Should().NotContain(@"..\..\samples\Videra.Demo\");
        project.Should().NotContain("Videra.Demo");
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
