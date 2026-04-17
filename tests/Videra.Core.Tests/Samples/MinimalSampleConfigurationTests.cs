using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class MinimalSampleConfigurationTests
{
    private static readonly string[] RequiredReadmeMarkers =
    {
        "VideraViewOptions",
        "LoadModelAsync(\"Assets/reference-cube.obj\")",
        "FrameAll()",
        "ResetCamera()",
        "BackendDiagnostics"
    };

    private static readonly string[] ForbiddenAdvancedSeams =
    {
        "VideraView.Engine",
        "RegisterPassContributor",
        "RegisterFrameHook",
        "RenderCapabilities",
        "Videra.Demo"
    };

    [Fact]
    public void SampleFiles_ShouldExistAtDedicatedMinimalSamplePath_AndBeWiredIntoSolution()
    {
        var repositoryRoot = GetRepositoryRoot();
        var sampleRoot = Path.Combine(repositoryRoot, "samples", "Videra.MinimalSample");
        var solution = File.ReadAllText(Path.Combine(repositoryRoot, "Videra.slnx"));

        Directory.Exists(sampleRoot).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "README.md")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Videra.MinimalSample.csproj")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Program.cs")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "App.axaml")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "App.axaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Views", "MainWindow.axaml")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Views", "MainWindow.axaml.cs")).Should().BeTrue();
        File.Exists(Path.Combine(sampleRoot, "Assets", "reference-cube.obj")).Should().BeTrue();

        solution.Should().Contain("samples/Videra.MinimalSample/Videra.MinimalSample.csproj");
    }

    [Fact]
    public void SampleReadme_ShouldDescribeTheHappyPathOnly()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.MinimalSample", "README.md"));

        foreach (var marker in RequiredReadmeMarkers)
        {
            readme.Should().Contain(marker);
        }

        readme.Should().Contain("does not call `VideraView.Engine`");
        readme.Should().Contain("does not register frame hooks");
    }

    [Fact]
    public void MainWindowCodeBehind_ShouldUseHappyPathApisWithoutEngineExtensibility()
    {
        var codeBehind = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.MinimalSample", "Views", "MainWindow.axaml.cs"));

        codeBehind.Should().Contain("new VideraViewOptions");
        codeBehind.Should().Contain("AllowSoftwareFallback = true");
        codeBehind.Should().Contain("LoadModelAsync(\"Assets/reference-cube.obj\")");
        codeBehind.Should().Contain("FrameAll()");
        codeBehind.Should().Contain("ResetCamera()");
        codeBehind.Should().Contain("BackendDiagnostics");

        foreach (var forbidden in ForbiddenAdvancedSeams)
        {
            codeBehind.Should().NotContain(forbidden);
        }
    }

    [Fact]
    public void SampleProject_ShouldReferenceAvaloniaEntryPointWithoutDemoCoupling()
    {
        var project = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.MinimalSample", "Videra.MinimalSample.csproj"));

        project.Should().Contain(@"..\..\src\Videra.Avalonia\Videra.Avalonia.csproj");
        project.Should().NotContain(@"..\..\samples\Videra.Demo\");
        project.Should().NotContain("Videra.Demo");
    }

    [Fact]
    public void MinimalSampleAsset_ShouldBeTrackedByGit()
    {
        var repositoryRoot = GetRepositoryRoot();
        var relativeAssetPath = "samples/Videra.MinimalSample/Assets/reference-cube.obj";
        var assetPath = Path.Combine(repositoryRoot, "samples", "Videra.MinimalSample", "Assets", "reference-cube.obj");

        File.Exists(assetPath).Should().BeTrue();

        var startInfo = new System.Diagnostics.ProcessStartInfo("git", $"ls-files --error-unmatch -- \"{relativeAssetPath}\"")
        {
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        process.Should().NotBeNull();
        process!.WaitForExit();

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.ExitCode.Should().Be(0, $"the minimal sample asset must be committed so cross-platform checkouts can build.{Environment.NewLine}{standardOutput}{standardError}");
        standardOutput.Should().Contain(relativeAssetPath);
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
