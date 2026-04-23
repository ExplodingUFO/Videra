using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class ExtensibilitySampleConfigurationTests
{
    private static readonly string[] RequiredReadmeMarkers =
    {
        "VideraView.Engine",
        "RegisterPassContributor(RenderPassSlot.SolidGeometry, ...)",
        "RegisterFrameHook(RenderFrameHookPoint.FrameEnd, ...)",
        "RenderCapabilities",
        "RenderCapabilities.SupportedFeatureNames",
        "BackendDiagnostics",
        "BackendDiagnostics.LastFrameFeatureNames",
        "BackendDiagnostics.SupportedRenderFeatureNames",
        "BackendDiagnostics.LastFrameObjectCount",
        "BackendDiagnostics.LastFrameOpaqueObjectCount",
        "BackendDiagnostics.LastFrameTransparentObjectCount",
        "LoadModelAsync(\"Assets/reference-cube.obj\")",
        "FrameAll()",
        "Opaque",
        "Transparent",
        "Overlay",
        "Picking",
        "Screenshot",
        "package discovery",
        "plugin loading"
    };

    private static readonly string[] ForbiddenLibraryAndDemoSeams =
    {
        "SoftwareBackend",
        "RenderSession",
        "RenderSessionOrchestrator",
        "VideraViewSessionBridge",
        "DemoMeshFactory",
        "Videra.Demo"
    };

    [Fact]
    public void SampleFiles_ShouldExistAtDedicatedNarrowSamplePath()
    {
        var sampleRoot = Path.Combine(GetRepositoryRoot(), "samples", "Videra.ExtensibilitySample");

        Directory.Exists(sampleRoot).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "README.md")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Videra.ExtensibilitySample.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Views", "MainWindow.axaml.cs")).Should().BeTrue();
    }

    [Fact]
    public void SampleReadme_ShouldDescribeThePublicExtensibilityFlow()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.ExtensibilitySample", "README.md"));

        foreach (var marker in RequiredReadmeMarkers)
        {
            readme.Should().Contain(marker);
        }
    }

    [Fact]
    public void MainWindowCodeBehind_ShouldUsePublicViewerApisOnly()
    {
        var codeBehind = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.ExtensibilitySample", "Views", "MainWindow.axaml.cs"));

        codeBehind.Should().Contain("View3D.Engine.RegisterPassContributor(RenderPassSlot.SolidGeometry, _recordingContributor);");
        codeBehind.Should().Contain("View3D.Engine.RegisterFrameHook(RenderFrameHookPoint.FrameEnd, OnFrameEnd);");
        codeBehind.Should().Contain("View3D.LoadModelAsync(\"Assets/reference-cube.obj\")");
        codeBehind.Should().Contain("View3D.FrameAll()");
        codeBehind.Should().Contain("View3D.RenderCapabilities");
        codeBehind.Should().Contain("View3D.BackendDiagnostics");
        codeBehind.Should().Contain("capabilities.SupportedFeatureNames");
        codeBehind.Should().Contain("snapshot?.FeatureNames");
        codeBehind.Should().Contain("diagnostics.LastFrameFeatureNames");
        codeBehind.Should().Contain("diagnostics.SupportedRenderFeatureNames");
        codeBehind.Should().Contain("diagnostics.LastFrameObjectCount");
        codeBehind.Should().Contain("diagnostics.LastFrameOpaqueObjectCount");
        codeBehind.Should().Contain("diagnostics.LastFrameTransparentObjectCount");

        foreach (var forbidden in ForbiddenLibraryAndDemoSeams)
        {
            codeBehind.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void SampleProject_ShouldReferenceAvaloniaEntryPointWithoutDemoCoupling()
    {
        var project = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.ExtensibilitySample", "Videra.ExtensibilitySample.csproj"));

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
