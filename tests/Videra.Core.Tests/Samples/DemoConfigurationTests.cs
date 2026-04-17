using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class DemoConfigurationTests
{
    [Fact]
    public void MainWindow_DefaultsToAutoBackend()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().Contain("PreferredBackend=\"Auto\"");
    }

    [Fact]
    public void Program_ShouldNotForceBackendThroughEnvironmentVariable()
    {
        var programPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Program.cs");
        var program = File.ReadAllText(programPath);

        program.Should().NotContain("SetEnvironmentVariable(\"VIDERA_BACKEND\"");
    }

    [Fact]
    public void MainWindow_ShouldNotCreateFallbackViewModelInstance()
    {
        var mainWindowCodeBehindPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml.cs");
        var codeBehind = File.ReadAllText(mainWindowCodeBehindPath);

        codeBehind.Should().NotContain("new MainWindowViewModel(null!)");
    }

    [Fact]
    public void MacOSNativeHost_ShouldUseSharedObjCRuntimeInterop()
    {
        var nativeHostPath = Path.Combine(GetRepositoryRoot(), "src", "Videra.Avalonia", "Controls", "VideraMacOSNativeHost.cs");
        var nativeHost = File.ReadAllText(nativeHostPath);

        nativeHost.Should().NotContain("[DllImport(\"/usr/lib/libobjc.dylib\"");
        nativeHost.Should().NotContain("private struct CGRect");
    }

    [Fact]
    public void Documentation_ShouldNotContainInternalAgentExecutionPlaybooks()
    {
        var docsDirectory = Path.Combine(GetRepositoryRoot(), "docs");
        var markdownFiles = Directory.GetFiles(docsDirectory, "*.md", SearchOption.AllDirectories);

        foreach (var markdownFile in markdownFiles)
        {
            var markdown = File.ReadAllText(markdownFile);
            markdown.Should().NotContain("For Claude", $"public docs should not embed agent instructions: {markdownFile}");
            markdown.Should().NotContain("For agentic workers", $"public docs should not embed agent instructions: {markdownFile}");
            markdown.Should().NotContain("superpowers:", $"public docs should not embed internal skill routing: {markdownFile}");
        }
    }

    [Fact]
    public void Readme_ShouldUseHighLevelViewerApiExamples()
    {
        var readmePath = Path.Combine(GetRepositoryRoot(), "README.md");
        var readme = File.ReadAllText(readmePath);

        readme.Should().Contain("LoadModelAsync");
        readme.Should().Contain("FrameAll()");
        readme.Should().Contain("BackendDiagnostics");
        readme.Should().NotContain("view.Engine.AddObject");
    }

    [Fact]
    public void RootReadme_ShouldDescribeDemoDiagnosticsAndStatusFeedback()
    {
        var readmePath = Path.Combine(GetRepositoryRoot(), "README.md");
        var readme = File.ReadAllText(readmePath);

        readme.Should().Contain("backend diagnostics");
        readme.Should().Contain("default demo cube");
        readme.Should().Contain("import feedback");
    }

    [Fact]
    public void DemoInteractionButtons_ShouldNotBindRawIsEnabledToIsBackendReady()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().NotContain("IsEnabled=\"{Binding IsBackendReady}\"");
    }

    [Fact]
    public void DemoStatusPanel_ShouldExposeBackendDetails()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().Contain("Text=\"{Binding BackendDetails}\"");
    }

    [Fact]
    public void Demo_ShouldExposeCameraUtilityCommands()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().Contain("Content=\"Frame All\"");
        xaml.Should().Contain("Command=\"{Binding FrameAllCommand}\"");
        xaml.Should().Contain("Content=\"Reset Camera\"");
        xaml.Should().Contain("Command=\"{Binding ResetCameraCommand}\"");
    }

    [Fact]
    public void DemoReadme_ShouldDescribeDegradedDefaultScenePath_AndImportSummaryVisibility()
    {
        var demoReadmePath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "README.md");
        var demoReadme = File.ReadAllText(demoReadmePath);

        demoReadme.Should().Contain("default demo cube");
        demoReadme.Should().Contain("status area");
        demoReadme.Should().Contain("Model import remains available");
        demoReadme.Should().Contain("summarized in the status area");
        demoReadme.Should().Contain("replaced only when every requested file succeeds");
        demoReadme.Should().Contain("document version");
        demoReadme.Should().Contain("resident");
        demoReadme.Should().Contain("failed");
    }

    [Fact]
    public void Demo_ShouldExposeScenePipelineLabTruth()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().Contain("SCENE PIPELINE LAB");
        xaml.Should().Contain("Text=\"{Binding ScenePipelineMetrics}\"");
        xaml.Should().Contain("bounded parallel import");
        xaml.Should().Contain("backend-neutral");
        xaml.Should().Contain("scene upload queue");
        xaml.Should().Contain("residency counts");
        xaml.Should().Contain("backend rebind");
    }

    [Fact]
    public void DemoImporter_ShouldFrameOnlyAfterFullySuccessfulBatchReplace()
    {
        var importerPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Services", "AvaloniaModelImporter.cs");
        var importer = File.ReadAllText(importerPath);

        importer.Should().Contain("result.Succeeded && result.LoadedObjects.Count > 0");
        importer.Should().NotContain("if (result.LoadedObjects.Count > 0)");
    }

    [Fact]
    public void Demo_ShouldNotExposeLegacyTestWireframeButton()
    {
        var mainWindowPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml");
        var xaml = File.ReadAllText(mainWindowPath);

        xaml.Should().NotContain("Content=\"Test Wireframe\"");
    }

    [Fact]
    public void DemoSceneBootstrapper_ShouldUseSingleExplicitStatusContract()
    {
        var bootstrapperPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Services", "DemoSceneBootstrapper.cs");
        var bootstrapper = File.ReadAllText(bootstrapperPath);

        bootstrapper.Should().Contain("SetWaitingForBackend(");
        bootstrapper.Should().Contain("SetBackendReadyWithDefaultScene(");
        bootstrapper.Should().Contain("SetBackendReadyWithoutDefaultScene(");
        bootstrapper.Should().Contain("SetBackendReadyWithDefaultSceneFailure(");
        bootstrapper.Should().NotContain("SetStatusMessage(\"Default demo model creation failed:");
    }

    [Fact]
    public void MainWindowCodeBehind_ShouldUseExplicitBackendFailureContract()
    {
        var codeBehindPath = Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Views", "MainWindow.axaml.cs");
        var codeBehind = File.ReadAllText(codeBehindPath);

        codeBehind.Should().Contain("SetBackendInitializationFailed(");
        codeBehind.Should().NotContain("SetStatusMessage($\"Backend initialization failed:");
    }

    [Fact]
    public void AvaloniaPackage_ShouldUseRuntimeBackendDiscoveryInsteadOfHostOsCompileSwitches()
    {
        var repositoryRoot = GetRepositoryRoot();
        var avaloniaProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Videra.Avalonia.csproj"));
        var resolverSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Composition", "AvaloniaGraphicsBackendResolver.cs"));
        var linuxHostSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraLinuxNativeHost.cs"));
        var macHostSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraMacOSNativeHost.cs"));

        avaloniaProject.Should().NotContain("VIDERA_WINDOWS_BACKEND");
        avaloniaProject.Should().NotContain("VIDERA_LINUX_BACKEND");
        avaloniaProject.Should().NotContain("VIDERA_MACOS_BACKEND");
        avaloniaProject.Should().NotContain(@"..\Videra.Platform.Windows\Videra.Platform.Windows.csproj");
        avaloniaProject.Should().NotContain(@"..\Videra.Platform.Linux\Videra.Platform.Linux.csproj");
        avaloniaProject.Should().NotContain(@"..\Videra.Platform.macOS\Videra.Platform.macOS.csproj");
        resolverSource.Should().NotContain("#if VIDERA_");
        linuxHostSource.Should().NotContain("#if VIDERA_");
        macHostSource.Should().NotContain("#if VIDERA_");
    }

    [Fact]
    public void DemoProject_ShouldDeclarePlatformBackendDependenciesExplicitly()
    {
        var demoProject = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "samples", "Videra.Demo", "Videra.Demo.csproj"));

        demoProject.Should().Contain(@"..\..\src\Videra.Avalonia\Videra.Avalonia.csproj");
        demoProject.Should().Contain(@"..\..\src\Videra.Platform.Windows\Videra.Platform.Windows.csproj");
        demoProject.Should().Contain(@"..\..\src\Videra.Platform.Linux\Videra.Platform.Linux.csproj");
        demoProject.Should().Contain(@"..\..\src\Videra.Platform.macOS\Videra.Platform.macOS.csproj");
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
