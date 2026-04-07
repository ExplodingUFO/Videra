using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryNativeValidationTests
{
    [Fact]
    public void NativeValidationWorkflow_ShouldProvideManualLinuxMacOSAndWindowsEntrypoints()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "native-validation.yml"));

        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("ubuntu-latest");
        workflow.Should().Contain("macos-latest");
        workflow.Should().Contain("windows-latest");
        workflow.Should().Contain("windows");
        workflow.Should().Contain("xvfb-run -a");
        workflow.Should().Contain("scripts/run-native-validation.sh");
        workflow.Should().Contain("scripts/run-native-validation.ps1");
    }

    [Fact]
    public void NativeValidationWorkflow_ShouldInstallLinuxShadercRuntime()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "native-validation.yml"));

        workflow.Should().Contain("libshaderc1");
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
        powerShellScript.Should().Contain("-IncludeNativeWindows");
        powerShellScript.Should().Contain("\"Windows\"");
        powerShellScript.Should().Contain("DISPLAY is not set");
    }

    [Fact]
    public void NativeValidationVerifyScripts_ShouldUseDetailedNativeTestLogging()
    {
        var repositoryRoot = GetRepositoryRoot();
        var shellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "verify.sh"));
        var powerShellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "verify.ps1"));

        shellVerify.Should().Contain("console;verbosity=detailed");
        powerShellVerify.Should().Contain("console;verbosity=detailed");
    }

    [Fact]
    public void NativeValidationDocs_ShouldExistAndBeLinkedFromPublicEntrypoints()
    {
        var repositoryRoot = GetRepositoryRoot();
        var englishRunbook = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "native-validation.md"));
        var chineseRunbook = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "native-validation.md"));

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
        englishRunbook.Should().Contain("Windows native validation");
        englishRunbook.Should().Contain("windows");
        chineseRunbook.Should().Contain("Windows 原生验证");
        chineseRunbook.Should().Contain("windows");
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

    [Fact]
    public void MacOSNativeHost_ShouldBootstrapAppKitBeforeCreatingNSView()
    {
        var repositoryRoot = GetRepositoryRoot();
        var hostSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraMacOSNativeHost.cs"));
        var objcRuntimeSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "ObjCRuntime.cs"));

        hostSource.Should().Contain("EnsureAppKitReady");
        objcRuntimeSource.Should().Contain("/System/Library/Frameworks/AppKit.framework/AppKit");
        objcRuntimeSource.Should().Contain("sharedApplication");
    }

    [Fact]
    public void LinuxVulkanLifecycleDrawTest_ShouldBindRequiredUniformBuffers()
    {
        var repositoryRoot = GetRepositoryRoot();
        var lifecycleTestSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Videra.Platform.Linux.Tests", "Backend", "VulkanBackendLifecycleTests.cs"));

        lifecycleTestSource.Should().Contain("CreateUniformBuffer(128)");
        lifecycleTestSource.Should().Contain("CreateUniformBuffer(64)");
        lifecycleTestSource.Should().Contain("RenderBindingSlots.Camera");
        lifecycleTestSource.Should().Contain("RenderBindingSlots.World");
    }

    [Fact]
    public void MacOSNativeHostIntegrationTest_ShouldValidateHandleWithoutConcreteClassAssertion()
    {
        var repositoryRoot = GetRepositoryRoot();
        var integrationTest = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Videra.Core.IntegrationTests", "Rendering", "VideraMacOSNativeHostIntegrationTests.cs"));

        integrationTest.Should().Contain("nsView.Should().NotBe(IntPtr.Zero);");
        integrationTest.Contains("object_getClassName", StringComparison.Ordinal).Should().BeFalse();
        integrationTest.Contains(".Be(\"NSView\")", StringComparison.Ordinal).Should().BeFalse();
    }

    [Fact]
    public void MacOSNativeHost_ShouldInitializeAppKitBeforeCreatingNSView()
    {
        var repositoryRoot = GetRepositoryRoot();
        var nativeHost = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraMacOSNativeHost.cs"));
        var objcRuntime = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "ObjCRuntime.cs"));

        nativeHost.Should().Contain("ObjCRuntime.EnsureAppKitReady();");
        objcRuntime.Should().Contain("public static void EnsureAppKitReady()");
        objcRuntime.Should().Contain("sharedApplication");
    }

    [Fact]
    public void LinuxVulkanLifecycleTest_ShouldBindCameraAndWorldUniformBuffersBeforeDraw()
    {
        var repositoryRoot = GetRepositoryRoot();
        var lifecycleTest = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Videra.Platform.Linux.Tests", "Backend", "VulkanBackendLifecycleTests.cs"));

        lifecycleTest.Should().Contain("CreateHiddenX11Window(128, 96)");
        lifecycleTest.Should().Contain("CreateUniformBuffer(128)");
        lifecycleTest.Should().Contain("CreateUniformBuffer(64)");
        lifecycleTest.Should().Contain("cameraBuffer.SetData(Matrix4x4.Identity, 0);");
        lifecycleTest.Should().Contain("cameraBuffer.SetData(Matrix4x4.Identity, 64);");
        lifecycleTest.Should().Contain("worldBuffer.SetData(Matrix4x4.Identity, 0);");
        lifecycleTest.Should().Contain("executor.SetVertexBuffer(cameraBuffer, RenderBindingSlots.Camera);");
        lifecycleTest.Should().Contain("executor.SetVertexBuffer(worldBuffer, RenderBindingSlots.World);");
    }

    [Fact]
    public void VulkanShaderCompilation_ShouldResolveUbuntuShadercSonameFallback()
    {
        var repositoryRoot = GetRepositoryRoot();
        var factorySource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanResourceFactory.cs"));

        factorySource.Should().Contain("new DefaultNativeContext");
        factorySource.Should().Contain("libshaderc_shared.so");
        factorySource.Should().Contain("/usr/lib/x86_64-linux-gnu/libshaderc.so.1");
        factorySource.Should().Contain("libshaderc.so.1");
    }

    [Fact]
    public void VulkanCommandExecutor_ShouldRebindDescriptorSetAfterUniformUpdates()
    {
        var repositoryRoot = GetRepositoryRoot();
        var executorSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanCommandExecutor.cs"));

        var updateCallIndex = executorSource.IndexOf("_vk.UpdateDescriptorSets(_device, 2, writes, 0, null);", StringComparison.Ordinal);
        updateCallIndex.Should().BeGreaterThanOrEqualTo(0);

        var reboundIndex = executorSource.IndexOf("BindDescriptorSet();", updateCallIndex, StringComparison.Ordinal);
        reboundIndex.Should().BeGreaterThan(
            updateCallIndex,
            "descriptor updates must be rebound immediately after uniform writes on stricter Vulkan drivers");
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
