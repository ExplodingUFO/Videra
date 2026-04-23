using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryNativeValidationTests
{
    [Fact]
    public void NativeValidationWorkflow_ShouldRunAutomaticallyForPullRequestsPushesAndManualTargets()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "native-validation.yml"));

        workflow.Should().Contain("pull_request:");
        workflow.Should().Contain("push:");
        workflow.Should().Contain("branches:");
        workflow.Should().Contain("- master");
        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("ubuntu-latest");
        workflow.Should().Contain("macos-latest");
        workflow.Should().Contain("windows-latest");
        workflow.Should().Contain("linux-x11-native:");
        workflow.Should().Contain("linux-wayland-xwayland-native:");
        workflow.Should().Contain("linux-x11");
        workflow.Should().Contain("linux-wayland-xwayland");
        workflow.Should().Contain("windows");
        workflow.Should().Contain("github.event_name != 'workflow_dispatch'");
        workflow.Should().Contain("xvfb-run -a");
        workflow.Should().Contain("xwfb-run --");
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
        shellScript.Should().Contain("--include-native-linux-xwayland");
        shellScript.Should().Contain("--linux-display-server");
        shellScript.Should().Contain("--include-native-macos");
        shellScript.Should().Contain("DISPLAY is not set");
        shellScript.Should().Contain("WAYLAND_DISPLAY is not set");

        powerShellScript.Should().Contain("-IncludeNativeLinux");
        powerShellScript.Should().Contain("-IncludeNativeLinuxXWayland");
        powerShellScript.Should().Contain("LinuxDisplayServer");
        powerShellScript.Should().Contain("-IncludeNativeMacOS");
        powerShellScript.Should().Contain("-IncludeNativeWindows");
        powerShellScript.Should().Contain("\"Windows\"");
        powerShellScript.Should().Contain("DISPLAY is not set");
        powerShellScript.Should().Contain("WAYLAND_DISPLAY is not set");
        powerShellScript.Should().Contain("scripts/verify.ps1");
    }

    [Fact]
    public void NativeValidationVerifyScripts_ShouldUseDetailedNativeTestLogging()
    {
        var repositoryRoot = GetRepositoryRoot();
        var shellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.sh"));
        var powerShellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));

        shellVerify.Should().Contain("console;verbosity=detailed");
        powerShellVerify.Should().Contain("console;verbosity=detailed");
    }

    [Fact]
    public void WindowsNativeValidation_ShouldRunRepositoryOnlyWpfSmokeProof()
    {
        var repositoryRoot = GetRepositoryRoot();
        var powerShellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));
        var invokeWpfSmoke = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-WpfSmoke.ps1"));

        powerShellVerify.Should().Contain("Windows WPF Smoke");
        powerShellVerify.Should().Contain("Invoke-WpfSmoke.ps1");
        powerShellVerify.Should().Contain("artifacts/test-results/verify/wpf-smoke");

        invokeWpfSmoke.Should().Contain("VIDERA_WPF_SMOKE_OUTPUT");
        invokeWpfSmoke.Should().Contain("wpf-smoke-diagnostics.txt");
        invokeWpfSmoke.Should().Contain("NativeHostBound: True");
    }

    [Fact]
    public void NativeValidationVerifyScripts_ShouldPreserveDistinctTrxFilesForSolutionRuns()
    {
        var powerShellVerify = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "scripts", "verify.ps1"));

        powerShellVerify.Should().Contain("trx;LogFilePrefix=verify");
        powerShellVerify.Should().NotContain("trx;LogFileName=verify.trx");
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
        englishRunbook.Should().Contain("XWayland");
        englishRunbook.Should().Contain("linux-wayland-xwayland");
        englishRunbook.Should().Contain("pull requests");
        englishRunbook.Should().Contain("workflow_dispatch");
        englishRunbook.Should().Contain("local matching-host path");
        englishRunbook.Should().Contain("windows");
        englishRunbook.Should().Contain("pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeWindows");
        englishRunbook.Should().Contain("Invoke-WpfSmoke.ps1");
        englishRunbook.Should().Contain("wpf-smoke-diagnostics.txt");
        englishRunbook.Should().Contain("repository-only validation/support evidence");
        englishRunbook.Should().Contain("not a public package or release artifact");
        readme.Should().Contain("GitHub Actions");
        readme.Should().Contain("XWayland");
        readme.Should().Contain("pull requests");
        readme.Should().Contain("Run workflow");
        chineseRunbook.Should().Contain("Windows 原生验证");
        chineseRunbook.Should().Contain("XWayland");
        chineseRunbook.Should().Contain("windows");
        chineseRunbook.Should().Contain("pwsh -File ./scripts/verify.ps1 -Configuration Release -IncludeNativeWindows");
        chineseRunbook.Should().Contain("Invoke-WpfSmoke.ps1");
        chineseRunbook.Should().Contain("wpf-smoke-diagnostics.txt");
        chineseRunbook.Should().Contain("repository-only");
        chineseRunbook.Should().Contain("公开包或发布产物");
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
    public void LinuxAvaloniaHost_ShouldUseFactoryDrivenDisplayServerSelection()
    {
        var repositoryRoot = GetRepositoryRoot();
        var defaultFactorySource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "DefaultNativeHostFactory.cs"));
        var coordinatorSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "VideraLinuxNativeHost.cs"));
        var linuxFactoryPath = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "Linux", "LinuxNativeHostFactory.cs");
        var x11HostPath = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "Controls", "Linux", "X11NativeHost.cs");

        File.Exists(linuxFactoryPath).Should().BeTrue();
        File.Exists(x11HostPath).Should().BeTrue();

        var linuxFactorySource = File.ReadAllText(linuxFactoryPath);
        var x11HostSource = File.ReadAllText(x11HostPath);

        defaultFactorySource.Should().Contain("new VideraLinuxNativeHost()");
        coordinatorSource.Should().Contain("LinuxNativeHostFactory");
        coordinatorSource.Should().NotContain("XCreateSimpleWindow(");
        coordinatorSource.Should().NotContain("XOpenDisplay(");
        linuxFactorySource.Should().Contain("LinuxDisplayServerDetector");
        x11HostSource.Should().Contain("XCreateSimpleWindow(");
        x11HostSource.Should().Contain("PlatformHandle");
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
    public void LinuxVulkanStaticSceneLightingContract_ShouldBindStyleUniform()
    {
        var repositoryRoot = GetRepositoryRoot();
        var lightingSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "Styles", "Parameters", "LightingParameters.cs"));
        var uniformSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "Styles", "Parameters", "StyleUniformData.cs"));
        var renderStyleSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Core", "Styles", "Parameters", "RenderStyleParameters.cs"));
        var factorySource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanResourceFactory.cs"));
        var executorSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanCommandExecutor.cs"));
        var d3d11FactorySource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Windows", "D3D11ResourceFactory.cs"));
        var metalSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "Shaders.metal"));

        lightingSource.Should().Contain("public float FillIntensity { get; set; } = 0f;");
        uniformSource.Should().Contain("[FieldOffset(28)] public float FillIntensity;");
        renderStyleSource.Should().Contain("FillIntensity = Lighting.FillIntensity");
        d3d11FactorySource.Should().Contain("float fillIntensity;");
        d3d11FactorySource.Should().Contain("float diffuse = max((dot(normal, lightDir) + fill) / (1.0f + fill), 0.0f) * diffuseIntensity;");
        factorySource.Should().Contain("layout(set = 0, binding = 3) uniform StyleBuffer");
        factorySource.Should().Contain("float fillIntensity;");
        factorySource.Should().Contain("float diffuse = max((dot(normal, lightDir) + fill) / (1.0 + fill), 0.0) * style.diffuseIntensity;");
        metalSource.Should().Contain("float fillIntensity;");
        metalSource.Should().Contain("float diffuse = max((dot(normal, lightDir) + fill) / (1.0f + fill), 0.0) * style.diffuseIntensity;");
        factorySource.Should().Contain("fragColor = style.useVertexColor != 0 ? inColor : style.overrideColor;");
        factorySource.Should().Contain("layout(location = 3) out vec3 fragWorldPos;");
        factorySource.Should().Contain("layout(location = 4) out vec3 fragNormal;");
        executorSource.Should().Contain("RenderBindingSlots.Style");
        executorSource.Should().Contain("_styleBuffer");
    }

    [Fact]
    public void LinuxVulkanLifecycleDrawTest_ShouldExerciseSurfaceChartScalarBindings()
    {
        var repositoryRoot = GetRepositoryRoot();
        var lifecycleTestSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Videra.Platform.Linux.Tests", "Backend", "VulkanBackendLifecycleTests.cs"));

        lifecycleTestSource.Should().Contain("SurfaceChartScalarResourceCreation_AndDrawPath_Succeeds");
        lifecycleTestSource.Should().Contain("CreateUniformBuffer(4112)");
        lifecycleTestSource.Should().Contain("CreateUniformBuffer(65536)");
        lifecycleTestSource.Should().Contain("GetProperty(\"UsesSurfaceChartScalarBindings\")");
        lifecycleTestSource.Should().Contain("usesScalarBindings.Should().Be(true);");
        lifecycleTestSource.Should().Contain("executor.SetVertexBuffer(colorMapBuffer, RenderBindingSlots.SurfaceColorMap);");
        lifecycleTestSource.Should().Contain("executor.SetVertexBuffer(scalarBuffer, RenderBindingSlots.SurfaceTileScalars);");
        lifecycleTestSource.Should().Contain("factory.CreatePipeline(CreateSurfaceChartPipelineDescription())");
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

        var updateCallIndex = executorSource.IndexOf("_vk.UpdateDescriptorSets(_device, descriptorWriteCount, writes, 0, null);", StringComparison.Ordinal);
        updateCallIndex.Should().BeGreaterThanOrEqualTo(0);

        var reboundIndex = executorSource.IndexOf("BindDescriptorSet();", updateCallIndex, StringComparison.Ordinal);
        reboundIndex.Should().BeGreaterThan(
            updateCallIndex,
            "descriptor updates must be rebound immediately after uniform writes on stricter Vulkan drivers");
    }

    [Fact]
    public void VulkanCommandExecutor_ShouldClearReleasedScalarBufferState()
    {
        var repositoryRoot = GetRepositoryRoot();
        var executorSource = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanCommandExecutor.cs"));

        executorSource.Should().Contain("if (ReferenceEquals(_surfaceTileScalarBuffer, vkBuffer))");
        executorSource.Should().Contain("_surfaceTileScalarBuffer = null;");
    }

    [Fact]
    public void NativeSurfaceChartScalarPipeline_ShouldPinPaletteAndScalarBindingsAcrossBackends()
    {
        var repositoryRoot = GetRepositoryRoot();
        var d3d11Factory = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Windows", "D3D11ResourceFactory.cs"));
        var metalFactory = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.macOS", "MetalResourceFactory.cs"));
        var vulkanFactory = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanResourceFactory.cs"));
        var vulkanExecutor = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.Platform.Linux", "VulkanCommandExecutor.cs"));

        d3d11Factory.Should().Contain("cbuffer SurfaceColorMapBuffer : register(b4)");
        d3d11Factory.Should().Contain("cbuffer SurfaceTileScalarBuffer : register(b5)");
        metalFactory.Should().Contain("constant SurfaceColorMapUniforms& colorMap [[buffer(4)]]");
        metalFactory.Should().Contain("constant float4* tileScalars [[buffer(5)]]");

        vulkanFactory.Should().Contain("var bindingCount = 4u;");
        vulkanFactory.Should().Contain("bindings[2] = new DescriptorSetLayoutBinding");
        vulkanFactory.Should().Contain("bindings[3] = new DescriptorSetLayoutBinding");
        vulkanFactory.Should().Contain("layout(set = 0, binding = 2) uniform AlphaMaskBuffer");
        vulkanFactory.Should().Contain("BindingCount = bindingCount");
        vulkanFactory.Should().Contain("SurfaceChartScalarDescriptorSetCapacity = 256;");
        vulkanFactory.Should().Contain("SurfaceChartScalarDescriptorPoolSetCount = SurfaceChartScalarDescriptorSetCapacity + 1u;");
        vulkanFactory.Should().Contain("descriptorSetCapacity = usesSurfaceChartScalarBindings ? SurfaceChartScalarDescriptorPoolSetCount : 1u;");
        vulkanFactory.Should().Contain("DescriptorPoolSize(DescriptorType.UniformBuffer, bindingCount * descriptorSetCapacity)");
        vulkanFactory.Should().Contain("MaxSets = descriptorSetCapacity");
        vulkanFactory.Should().Contain("MaxUniformBufferRange");
        vulkanFactory.Should().Contain("usesSurfaceChartScalarBindings: true");
        vulkanFactory.Should().Contain("layout(set = 0, binding = 2) uniform SurfaceColorMapBuffer");
        vulkanFactory.Should().Contain("layout(set = 0, binding = 3) uniform SurfaceTileScalarBuffer");
        vulkanExecutor.Should().Contain("RenderBindingSlots.SurfaceColorMap");
        vulkanExecutor.Should().Contain("RenderBindingSlots.SurfaceTileScalars");
        vulkanExecutor.Should().Contain("RenderBindingSlots.AlphaMask");
        vulkanExecutor.Should().Contain("_surfaceScalarDescriptorSets");
        vulkanExecutor.Should().Contain("GetOrCreateSurfaceScalarDescriptorSet");
        vulkanExecutor.Should().Contain("ReferenceEquals(_surfaceTileScalarBuffer, vkBuffer)");
        vulkanExecutor.Should().Contain("_surfaceTileScalarBuffer = null;");
        vulkanExecutor.Should().Contain("ReleaseCachedDescriptorSets(_pipeline)");
        vulkanExecutor.Should().Contain("_vk.FreeDescriptorSets");
        vulkanExecutor.Should().Contain("DstBinding = 2");
        vulkanExecutor.Should().Contain("DstBinding = 3");
        vulkanExecutor.Should().Contain("_vk.UpdateDescriptorSets(_device, descriptorWriteCount, writes, 0, null);");
    }

    [Fact]
    public void LinuxNativeLifecycleTests_ShouldRequireExplicitNativeValidationMode()
    {
        var repositoryRoot = GetRepositoryRoot();
        var supportedOsSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Tests.Common", "Platform", "SupportedOSFactAttribute.cs"));
        var helperSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Tests.Common", "Platform", "NativeHostTestHelpers.cs"));
        var lifecycleSource = File.ReadAllText(Path.Combine(repositoryRoot, "tests", "Videra.Platform.Linux.Tests", "Backend", "VulkanBackendLifecycleTests.cs"));
        var shellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.sh"));
        var powerShellVerify = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));

        supportedOsSource.Should().Contain("public sealed class LinuxNativeFactAttribute");
        supportedOsSource.Should().Contain("VIDERA_RUN_LINUX_NATIVE_TESTS");
        helperSource.Should().Contain("public static bool CanOpenX11Display()");
        lifecycleSource.Should().Contain("[LinuxNativeFact]");
        shellVerify.Should().Contain("VIDERA_RUN_LINUX_NATIVE_TESTS=true");
        powerShellVerify.Should().Contain("VIDERA_RUN_LINUX_NATIVE_TESTS");
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
