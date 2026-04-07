using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryNativeValidationTests
{
    [Fact]
    public void NativeValidationWorkflow_ShouldProvideManualLinuxAndMacOSEntrypoints()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "native-validation.yml"));

        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("ubuntu-latest");
        workflow.Should().Contain("macos-latest");
        workflow.Should().Contain("xvfb-run -a");
        workflow.Should().Contain("scripts/run-native-validation.sh");
    }

    [Fact]
    public void NativeValidationScripts_ShouldWrapRepositoryVerifyEntrypoints()
    {
        var repositoryRoot = GetRepositoryRoot();
        var shellScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "run-native-validation.sh"));
        var powerShellScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "run-native-validation.ps1"));

        shellScript.Should().Contain("--include-native-linux");
        shellScript.Should().Contain("--include-native-macos");
        shellScript.Should().Contain("DISPLAY is not set");

        powerShellScript.Should().Contain("-IncludeNativeLinux");
        powerShellScript.Should().Contain("-IncludeNativeMacOS");
        powerShellScript.Should().Contain("DISPLAY is not set");
    }

    [Fact]
    public void NativeValidationDocs_ShouldExistAndBeLinkedFromPublicEntrypoints()
    {
        var repositoryRoot = GetRepositoryRoot();

        File.Exists(Path.Combine(repositoryRoot, "docs", "native-validation.md")).Should().BeTrue();
        File.Exists(Path.Combine(repositoryRoot, "docs", "zh-CN", "native-validation.md")).Should().BeTrue();

        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var troubleshooting = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "troubleshooting.md"));
        var chineseIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"));
        var chineseReadme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var chineseTroubleshooting = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md"));

        readme.Should().Contain("docs/native-validation.md");
        docsIndex.Should().Contain("native-validation.md");
        troubleshooting.Should().Contain("native-validation.md");
        chineseIndex.Should().Contain("native-validation.md");
        chineseReadme.Should().Contain("native-validation.md");
        chineseTroubleshooting.Should().Contain("native-validation.md");
    }

    [Fact]
    public void MacOSPlatformProject_ShouldExposeObjCRuntimeToAvaloniaHostAssembly()
    {
        var repositoryRoot = GetRepositoryRoot();
        var macOsProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "Videra.Platform.macOS.csproj"));
        var objcRuntime = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "ObjCRuntime.cs"));

        var hasInternalsVisibleTo = macOsProject.Contains(">Videra.Avalonia<", StringComparison.Ordinal);
        var isPublicRuntime = objcRuntime.Contains("public static class ObjCRuntime", StringComparison.Ordinal);

        (hasInternalsVisibleTo || isPublicRuntime)
            .Should()
            .BeTrue("macOS builds compile VideraMacOSNativeHost against ObjCRuntime, so the helper must be visible to the Videra.Avalonia assembly");
    }

    [Fact]
    public void LinuxPlatformProject_ShouldExposeX11RegistryToAvaloniaHostAssembly()
    {
        var repositoryRoot = GetRepositoryRoot();
        var linuxProject = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "Videra.Platform.Linux.csproj"));
        var registrySource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "X11NativeHandleRegistry.cs"));

        var hasInternalsVisibleTo = linuxProject.Contains(">Videra.Avalonia<", StringComparison.Ordinal);
        var isPublicRegistry = registrySource.Contains("public static class X11NativeHandleRegistry", StringComparison.Ordinal);

        (hasInternalsVisibleTo || isPublicRegistry)
            .Should()
            .BeTrue("Linux builds compile VideraLinuxNativeHost against X11NativeHandleRegistry, so the helper must be visible to the Videra.Avalonia assembly");
    }

    [Fact]
    public void VulkanBackend_Dispose_ShouldGuardPartialInitializationHandles()
    {
        var repositoryRoot = GetRepositoryRoot();
        var backendSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanBackend.cs"));

        backendSource.Should().Contain("if (_device.Handle != 0)");
        backendSource.Should().Contain("if (_imageAvailableSemaphore.Handle != 0)");
        backendSource.Should().Contain("if (_renderFinishedSemaphore.Handle != 0)");
        backendSource.Should().Contain("if (_inFlightFence.Handle != 0)");
    }

    [Fact]
    public void LinuxNativeTestFixture_ShouldMapAndFlushX11Windows()
    {
        var repositoryRoot = GetRepositoryRoot();
        var helperSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Tests.Common", "Platform", "NativeHostTestHelpers.cs"));

        helperSource.Should().Contain("XMapWindow(_display, _window);");
        helperSource.Should().Contain("XFlush(_display);");
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
