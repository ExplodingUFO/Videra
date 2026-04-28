using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class AlphaConsumerIntegrationTests
{
    [Fact]
    public void ConsumerSmoke_ShouldUsePublicPackagesAndDedicatedWorkflow()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeProjectPath = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Videra.ConsumerSmoke.csproj");
        var surfaceChartsSmokeProjectPath = Path.Combine(repositoryRoot, "smoke", "Videra.SurfaceCharts.ConsumerSmoke", "Videra.SurfaceCharts.ConsumerSmoke.csproj");
        var smokeWorkflowPath = Path.Combine(repositoryRoot, ".github", "workflows", "consumer-smoke.yml");
        var smokeScriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1");

        File.Exists(smokeProjectPath).Should().BeTrue();
        File.Exists(surfaceChartsSmokeProjectPath).Should().BeTrue();
        File.Exists(smokeWorkflowPath).Should().BeTrue();
        File.Exists(smokeScriptPath).Should().BeTrue();

        var smokeProject = File.ReadAllText(smokeProjectPath);
        var surfaceChartsSmokeProject = File.ReadAllText(surfaceChartsSmokeProjectPath);
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Avalonia\"");
        smokeProject.Should().Contain("<VideraConsumerSmokeModelFormat Condition=\"'$(VideraConsumerSmokeModelFormat)' == ''\">Obj</VideraConsumerSmokeModelFormat>");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Import.Obj\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Import.Gltf\"");
        smokeProject.Should().Contain("VIDERA_CONSUMER_SMOKE_OBJ");
        smokeProject.Should().Contain("VIDERA_CONSUMER_SMOKE_GLTF");
        smokeProject.Should().Contain("'$(VideraConsumerSmokeModelFormat)' == 'Obj'");
        smokeProject.Should().Contain("'$(VideraConsumerSmokeModelFormat)' == 'Gltf'");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.Windows\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.Linux\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.macOS\"");
        smokeProject.Should().NotContain("<ProjectReference");
        surfaceChartsSmokeProject.Should().Contain("<PackageReference Include=\"Videra.SurfaceCharts.Avalonia\"");
        surfaceChartsSmokeProject.Should().Contain("<PackageReference Include=\"Videra.SurfaceCharts.Processing\"");
        surfaceChartsSmokeProject.Should().NotContain("<ProjectReference");

        var smokeWindowCodeBehind = File.ReadAllText(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Views", "MainWindow.axaml.cs"));
        smokeWindowCodeBehind.Should().Contain("using Videra.Import.Obj;");
        smokeWindowCodeBehind.Should().Contain("using Videra.Import.Gltf;");
        smokeWindowCodeBehind.Should().Contain("UseModelImporter(ObjModelImporter.Create())");
        smokeWindowCodeBehind.Should().Contain("UseModelImporter(GltfModelImporter.Create())");
        smokeWindowCodeBehind.Should().Contain("private const string ModelPath = \"Assets/reference-triangle.gltf\";");
        smokeWindowCodeBehind.Should().Contain("Assets/reference-triangle.gltf");
        smokeWindowCodeBehind.Should().Contain("Viewer-only smoke path selected");

        var smokeWorkflow = File.ReadAllText(smokeWorkflowPath);
        smokeWorkflow.Should().Contain("pull_request:");
        smokeWorkflow.Should().Contain("push:");
        smokeWorkflow.Should().Contain("github.event_name != 'workflow_dispatch'");
        smokeWorkflow.Should().Contain("workflow_dispatch:");
        smokeWorkflow.Should().Contain("windows");
        smokeWorkflow.Should().Contain("windows-surfacecharts");
        smokeWorkflow.Should().Contain("linux-x11");
        smokeWorkflow.Should().Contain("linux-xwayland");
        smokeWorkflow.Should().Contain("macos");
        smokeWorkflow.Should().Contain("Invoke-ConsumerSmoke.ps1");
        smokeWorkflow.Should().Contain("-LightingProofHoldSeconds 10");
        smokeWorkflow.Should().Contain("-Scenario SurfaceCharts");
        smokeWorkflow.Should().Contain("actions/upload-artifact@v4");
        smokeWorkflow.Should().Contain("consumer-smoke-windows");
        smokeWorkflow.Should().Contain("consumer-smoke-windows-surfacecharts");
        smokeWorkflow.Should().Contain("consumer-smoke-linux-x11");
        smokeWorkflow.Should().Contain("consumer-smoke-linux-xwayland");
        smokeWorkflow.Should().Contain("consumer-smoke-macos");
        smokeWorkflow.Should().NotContain("Videra.WpfSmoke");

        var smokeScript = File.ReadAllText(smokeScriptPath);
        smokeScript.Should().Contain("Pack Public Consumer Packages");
        smokeScript.Should().Contain("[ValidateSet(\"ViewerOnly\", \"ViewerObj\", \"ViewerGltf\", \"SurfaceCharts\")]");
        smokeScript.Should().Contain("[string]$Scenario = \"ViewerObj\"");
        smokeScript.Should().Contain("\"ViewerOnly\" { @(\"Videra.Avalonia\", (Get-CurrentPlatformPackageId)) }");
        smokeScript.Should().Contain("\"ViewerObj\" { @(\"Videra.Avalonia\", \"Videra.Import.Obj\", (Get-CurrentPlatformPackageId)) }");
        smokeScript.Should().Contain("\"ViewerGltf\" { @(\"Videra.Avalonia\", \"Videra.Import.Gltf\", (Get-CurrentPlatformPackageId)) }");
        smokeScript.Should().Contain("-p:VideraConsumerSmokeModelFormat=$modelFormat");
        smokeScript.Should().Contain("VIDERA_CONSUMER_SMOKE_SCENARIO");
        smokeScript.Should().Contain("VIDERA_CONSUMER_SMOKE_PACKAGE_IDS");
        smokeScript.Should().Contain("smoke/Videra.SurfaceCharts.ConsumerSmoke/Videra.SurfaceCharts.ConsumerSmoke.csproj");
        smokeScript.Should().Contain("src/Videra.Import.Gltf/Videra.Import.Gltf.csproj");
        smokeScript.Should().Contain("src/Videra.Import.Obj/Videra.Import.Obj.csproj");
        smokeScript.Should().Contain("dotnet pack");
        smokeScript.Should().Contain("VIDERA_CONSUMER_SMOKE_OUTPUT");
        smokeScript.Should().Contain("VideraConsumerPackageVersion");
        smokeScript.Should().Contain("NuGet.Config");
        smokeScript.Should().Contain("dotnet restore");
        smokeScript.Should().Contain("--configfile");
        smokeScript.Should().Contain("https://api.nuget.org/v3/index.json");
        smokeScript.Should().Contain("dotnet build");
        smokeScript.Should().Contain("Start-Process");
        smokeScript.Should().Contain("-FilePath \"dotnet\"");
        smokeScript.Should().Contain("\"run\",");
        smokeScript.Should().Contain("FrameAllReturned");
        smokeScript.Should().Contain("FirstChartRendered");
        smokeScript.Should().Contain("ResolvedBackend");
        smokeScript.Should().Contain("ActiveBackend");
        smokeScript.Should().Contain("ResolvedDisplayServer");
        smokeScript.Should().Contain("DisplayServerCompatibility");
        smokeScript.Should().Contain("diagnostics-snapshot.txt");
        smokeScript.Should().Contain("inspection-bundle");
        smokeScript.Should().Contain("surfacecharts-support-summary.txt");
        smokeScript.Should().NotContain("Videra.WpfSmoke");
    }

    [Fact]
    public void ConsumerSmokeScenarioMatrix_ShouldUseCleanPackageReferences()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeProject = File.ReadAllText(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Videra.ConsumerSmoke.csproj"));
        var smokeScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1"));
        var smokeCode = File.ReadAllText(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Views", "MainWindow.axaml.cs"));

        File.Exists(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Assets", "reference-triangle.gltf")).Should().BeTrue();

        smokeProject.Should().NotContain("<ProjectReference");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Avalonia\"");
        smokeProject.Should().Contain("<ItemGroup Condition=\"'$(VideraConsumerSmokeModelFormat)' == 'Obj'\">");
        smokeProject.Should().Contain("<ItemGroup Condition=\"'$(VideraConsumerSmokeModelFormat)' == 'Gltf'\">");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Import.Obj\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Import.Gltf\"");

        smokeScript.Should().Contain("[ValidateSet(\"ViewerOnly\", \"ViewerObj\", \"ViewerGltf\", \"SurfaceCharts\")]");
        smokeScript.Should().Contain("\"ViewerOnly\" { \"None\" }");
        smokeScript.Should().Contain("\"ViewerObj\" { \"Obj\" }");
        smokeScript.Should().Contain("\"ViewerGltf\" { \"Gltf\" }");
        smokeScript.Should().Contain("\"SurfaceCharts\" { @(\"Videra.SurfaceCharts.Avalonia\", \"Videra.SurfaceCharts.Processing\") }");
        smokeScript.Should().Contain("PackageIds = @($script:scenarioPackageIds)");
        smokeScript.Should().Contain("ModelFormat = $script:modelFormat");

        smokeCode.Should().Contain("string[] PackageIds");
        smokeCode.Should().Contain("string ConsumerModelFormat");
        smokeCode.Should().Contain("string? ImportedModelPath");
    }

    [Fact]
    public void LinuxXWaylandConsumerSmokeWorkflow_ShouldCaptureWaylandSessionEnvironment()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "consumer-smoke.yml"));
        var publishWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-public.yml"));

        foreach (var workflow in new[] { smokeWorkflow, publishWorkflow })
        {
            workflow.Should().Contain("linux-xwayland-consumer-smoke");
            workflow.Should().Contain("bash -lc");
            workflow.Should().Contain("env | sort | grep -E \"^(DISPLAY|WAYLAND_DISPLAY|XDG_RUNTIME_DIR|XDG_SESSION_TYPE)=\" || true");
            workflow.Should().Contain("WAYLAND_DISPLAY");
            workflow.Should().Contain("XDG_SESSION_TYPE=\"${XDG_SESSION_TYPE:-wayland}\"");
            workflow.Should().Contain("-LightingProofHoldSeconds 10");
            workflow.Should().Contain("Invoke-ConsumerSmoke.ps1 -Configuration Release -LightingProofHoldSeconds 10 -OutputRoot artifacts/consumer-smoke/linux-xwayland");
        }
    }

    [Fact]
    public void ConsumerSmokeScript_ShouldPersistFallbackFailureArtifactsAndSessionContext()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1"));

        smokeScript.Should().Contain("consumer-smoke-environment.txt");
        smokeScript.Should().Contain("ProcessExitCode");
        smokeScript.Should().Contain("Scenario");
        smokeScript.Should().Contain("WAYLAND_DISPLAY");
        smokeScript.Should().Contain("XDG_SESSION_TYPE");
        smokeScript.Should().Contain("Write-FallbackConsumerSmokeArtifacts");
        smokeScript.Should().Contain("surfacecharts-support-summary.txt");
    }

    [Fact]
    public void ConsumerSmokeScript_ShouldValidateSurfaceChartsSupportSummaryContractNearProducer()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1"));

        smokeScript.Should().Contain("Test-SurfaceChartsSupportSummaryContract");
        smokeScript.Should().Contain("SurfaceCharts support summary '$summaryPath' is missing required field(s):");
        smokeScript.Should().Contain("GeneratedUtc:");
        smokeScript.Should().Contain("EvidenceKind:");
        smokeScript.Should().Contain("EvidenceOnly:");
        smokeScript.Should().Contain("ChartControl:");
        smokeScript.Should().Contain("EnvironmentRuntime:");
        smokeScript.Should().Contain("AssemblyIdentity:");
        smokeScript.Should().Contain("BackendDisplayEnvironment:");
        smokeScript.Should().Contain("RenderingStatus");
    }

    [Fact]
    public void ConsumerSmokeWindow_ShouldAvoidLayoutDrivenViewportResizesDuringStatusUpdates()
    {
        var repositoryRoot = GetRepositoryRoot();
        var smokeWindowXaml = File.ReadAllText(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Views", "MainWindow.axaml"));
        var smokeWindowCodeBehind = File.ReadAllText(Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Views", "MainWindow.axaml.cs"));

        smokeWindowXaml.Should().Contain("TextWrapping=\"NoWrap\"");
        smokeWindowXaml.Should().Contain("TextTrimming=\"CharacterEllipsis\"");
        smokeWindowXaml.Should().NotContain("TextWrapping=\"Wrap\"");
        smokeWindowCodeBehind.Should().Contain("_executionStarted");
        smokeWindowCodeBehind.Should().Contain("ResolvedDisplayServer=");
        smokeWindowCodeBehind.Should().NotContain("DisplayServerCompatibility=");
    }

    [Fact]
    public void ConsumerSmokeAsset_ShouldBeTrackedByGit()
    {
        var repositoryRoot = GetRepositoryRoot();
        var relativeAssetPath = "smoke/Videra.ConsumerSmoke/Assets/reference-cube.obj";
        var assetPath = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Assets", "reference-cube.obj");
        var relativeGltfAssetPath = "smoke/Videra.ConsumerSmoke/Assets/reference-triangle.gltf";
        var gltfAssetPath = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Assets", "reference-triangle.gltf");

        File.Exists(assetPath).Should().BeTrue();
        File.Exists(gltfAssetPath).Should().BeTrue();

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

        process.ExitCode.Should().Be(0, $"the consumer smoke asset must be committed so release checkouts can build.{Environment.NewLine}{standardOutput}{standardError}");
        standardOutput.Should().Contain(relativeAssetPath);

        startInfo.Arguments = $"ls-files --error-unmatch -- \"{relativeGltfAssetPath}\"";
        using var gltfProcess = System.Diagnostics.Process.Start(startInfo);
        gltfProcess.Should().NotBeNull();
        gltfProcess!.WaitForExit();

        var gltfStandardOutput = gltfProcess.StandardOutput.ReadToEnd();
        var gltfStandardError = gltfProcess.StandardError.ReadToEnd();

        gltfProcess.ExitCode.Should().Be(0, $"the glTF consumer smoke asset must be committed so release checkouts can build.{Environment.NewLine}{gltfStandardOutput}{gltfStandardError}");
        gltfStandardOutput.Should().Contain(relativeGltfAssetPath);
    }

    [Fact]
    public void BenchmarkGate_ShouldHaveWorkflowScriptAndDocsTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflowPath = Path.Combine(repositoryRoot, ".github", "workflows", "benchmark-gates.yml");
        var runScriptPath = Path.Combine(repositoryRoot, "scripts", "Run-Benchmarks.ps1");
        var thresholdScriptPath = Path.Combine(repositoryRoot, "scripts", "Test-BenchmarkThresholds.ps1");
        var contractPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-contract.json");
        var thresholdPath = Path.Combine(repositoryRoot, "benchmarks", "benchmark-thresholds.json");
        var docsPath = Path.Combine(repositoryRoot, "docs", "benchmark-gates.md");
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));

        File.Exists(workflowPath).Should().BeTrue();
        File.Exists(runScriptPath).Should().BeTrue();
        File.Exists(thresholdScriptPath).Should().BeTrue();
        File.Exists(contractPath).Should().BeTrue();
        File.Exists(thresholdPath).Should().BeTrue();
        File.Exists(docsPath).Should().BeTrue();

        var workflow = File.ReadAllText(workflowPath);
        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("pull_request:");
        workflow.Should().Contain("opened");
        workflow.Should().Contain("Run-Benchmarks.ps1 -Suite Viewer");
        workflow.Should().Contain("Run-Benchmarks.ps1 -Suite SurfaceCharts");
        workflow.Should().Contain("Test-BenchmarkThresholds.ps1 -Suite Viewer");
        workflow.Should().Contain("Test-BenchmarkThresholds.ps1 -Suite SurfaceCharts");
        workflow.Should().NotContain("run-benchmarks");
        workflow.Should().Contain("benchmarks-viewer");
        workflow.Should().Contain("benchmarks-surfacecharts");

        var runScript = File.ReadAllText(runScriptPath);
        runScript.Should().Contain("benchmark-contract.json");
        runScript.Should().Contain("$requestedExporters = @(\"json\", \"csv\", \"markdown\")");
        runScript.Should().Contain("--exporters $requestedExporters");
        runScript.Should().Contain("benchmark-manifest.json");
        runScript.Should().Contain("SUMMARY.txt");

        var thresholdScript = File.ReadAllText(thresholdScriptPath);
        thresholdScript.Should().Contain("benchmark-thresholds.json");
        thresholdScript.Should().Contain("benchmark-threshold-evaluation.json");
        thresholdScript.Should().Contain("benchmark-threshold-summary.txt");

        var contract = File.ReadAllText(contractPath);
        contract.Should().Contain("Videra.Viewer.Benchmarks");
        contract.Should().Contain("Videra.SurfaceCharts.Benchmarks");

        var thresholds = File.ReadAllText(thresholdPath);
        thresholds.Should().Contain("ScenePipeline_RehydrateAfterBackendReady");
        thresholds.Should().Contain("SceneHitTest_MeshAccurateDistance");
        thresholds.Should().Contain("ApplyResidencyChurnUnderCameraMovement");
        thresholds.Should().Contain("ProbeLatency");

        var docs = File.ReadAllText(docsPath);
        docs.Should().Contain("Run workflow");
        docs.Should().Contain("Run-Benchmarks.ps1");
        docs.Should().Contain("benchmark-contract.json");
        docs.Should().Contain("benchmark-thresholds.json");
        docs.Should().Contain("benchmark-manifest.json");
        docs.Should().Contain("hard numeric blocker");
        docs.Should().Contain("viewer");
        docs.Should().Contain("surfacecharts");
        docs.ToLowerInvariant().Should().Contain("compare runs over time");
        docs.Should().Contain("trend");

        docsIndex.Should().Contain("benchmark-gates.md");
        docsIndex.Should().Contain("automatic pull-request runtime thresholds");
        rootReadme.Should().Contain("docs/benchmark-gates.md");
    }

    [Fact]
    public void AlphaFeedbackSurface_ShouldCaptureInstallPathDiagnosticsAndSupportBoundaries()
    {
        var repositoryRoot = GetRepositoryRoot();
        var feedbackDocPath = Path.Combine(repositoryRoot, "docs", "alpha-feedback.md");
        var bugFormPath = Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "bug_report.yml");
        var featureFormPath = Path.Combine(repositoryRoot, ".github", "ISSUE_TEMPLATE", "feature_request.yml");
        var contributingPath = Path.Combine(repositoryRoot, "CONTRIBUTING.md");
        var troubleshootingPath = Path.Combine(repositoryRoot, "docs", "troubleshooting.md");
        var supportMatrixPath = Path.Combine(repositoryRoot, "docs", "support-matrix.md");
        var avaloniaReadmePath = Path.Combine(repositoryRoot, "src", "Videra.Avalonia", "README.md");

        File.Exists(feedbackDocPath).Should().BeTrue();

        var feedbackDoc = File.ReadAllText(feedbackDocPath);
        feedbackDoc.Should().Contain("Videra.MinimalSample");
        feedbackDoc.Should().Contain("consumer smoke");
        feedbackDoc.Should().Contain("smoke/Videra.SurfaceCharts.ConsumerSmoke");
        feedbackDoc.Should().Contain("diagnostics snapshot");
        feedbackDoc.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        feedbackDoc.Should().Contain("VideraInspectionBundleService");
        feedbackDoc.Should().Contain("CanReplayScene");
        feedbackDoc.Should().Contain("ReplayLimitation");
        feedbackDoc.Should().Contain("LastSnapshotExportPath");
        feedbackDoc.Should().Contain("LastSnapshotExportStatus");
        feedbackDoc.Should().Contain("ResolvedDisplayServer");
        feedbackDoc.Should().Contain("DisplayServerFallbackUsed");
        feedbackDoc.Should().Contain("DisplayServerFallbackReason");
        feedbackDoc.Should().Contain("DisplayServerCompatibility");
        feedbackDoc.Should().Contain("XWayland");
        feedbackDoc.Should().Contain("compositor-native Wayland");
        feedbackDoc.Should().Contain("Videra.SurfaceCharts.*");
        feedbackDoc.Should().Contain("Videra.SurfaceCharts.Demo");
        feedbackDoc.Should().Contain("Start here: In-memory first chart");
        feedbackDoc.Should().Contain("Explore next: Cache-backed streaming");
        feedbackDoc.Should().Contain("Try next: Scatter proof");
        feedbackDoc.Should().Contain("Support summary");
        feedbackDoc.Should().Contain("surfacecharts-support-summary.txt");
        feedbackDoc.Should().Contain("Copy support summary");
        feedbackDoc.Should().Contain("InteractionQuality");
        feedbackDoc.Should().Contain("RenderingStatus");
        feedbackDoc.Should().Contain("ViewState");
        feedbackDoc.Should().Contain("OverlayOptions");

        var bugForm = File.ReadAllText(bugFormPath);
        bugForm.Should().Contain("install_path");
        bugForm.Should().Contain("version");
        bugForm.Should().Contain("host_environment");
        bugForm.Should().Contain("diagnostics snapshot");
        bugForm.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        bugForm.Should().Contain("Videra.MinimalSample");
        bugForm.Should().Contain("consumer smoke");
        bugForm.Should().Contain("surfacecharts-support-summary.txt");
        bugForm.Should().Contain("surfacecharts_support_summary");
        bugForm.Should().Contain("SurfaceCharts support summary");
        bugForm.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        bugForm.Should().Contain("Copy support summary");

        var featureForm = File.ReadAllText(featureFormPath);
        featureForm.Should().Contain("consumer_path");
        featureForm.Should().Contain("happy path");
        featureForm.Should().Contain("advanced integration path");
        featureForm.Should().Contain("evidence");

        var contributing = File.ReadAllText(contributingPath);
        contributing.Should().Contain("docs/alpha-feedback.md");
        contributing.Should().Contain("Consumer Smoke");

        var troubleshooting = File.ReadAllText(troubleshootingPath);
        troubleshooting.Should().Contain("Videra.MinimalSample");
        troubleshooting.Should().Contain("consumer smoke");
        troubleshooting.Should().Contain("smoke/Videra.SurfaceCharts.ConsumerSmoke");
        troubleshooting.Should().Contain("diagnostics snapshot");
        troubleshooting.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        troubleshooting.Should().Contain("VideraInspectionBundleService");
        troubleshooting.Should().Contain("CanReplayScene");
        troubleshooting.Should().Contain("ReplayLimitation");
        troubleshooting.Should().Contain("LastSnapshotExportPath");
        troubleshooting.Should().Contain("LastSnapshotExportStatus");
        troubleshooting.Should().Contain("DisplayServerCompatibility");
        troubleshooting.Should().Contain("Videra.SurfaceCharts.Demo");
        troubleshooting.Should().Contain("Support summary");
        troubleshooting.Should().Contain("Copy support summary");
        troubleshooting.Should().Contain("Try next: Scatter proof");
        troubleshooting.Should().Contain("RenderingStatus");
        troubleshooting.Should().Contain("InteractionQuality");
        troubleshooting.Should().Contain("ViewState");
        troubleshooting.Should().Contain("OverlayOptions");

        var supportMatrix = File.ReadAllText(supportMatrixPath);
        supportMatrix.Should().Contain("alpha-feedback.md");

        var avaloniaReadme = File.ReadAllText(avaloniaReadmePath);
        avaloniaReadme.Should().Contain("VideraDiagnosticsSnapshotFormatter");
    }

    [Fact]
    public void SampleContracts_ShouldHaveExplicitPullRequestEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        var ciWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "ci.yml"));
        var releasing = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "releasing.md"));
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));

        ciWorkflow.Should().Contain("sample-contract-evidence:");
        ciWorkflow.Should().Contain("Run sample configuration evidence");
        ciWorkflow.Should().Contain("Run sample runtime evidence");
        ciWorkflow.Should().Contain("ExtensibilitySampleConfigurationTests");
        ciWorkflow.Should().Contain("InteractionSampleConfigurationTests");
        ciWorkflow.Should().Contain("Run SurfaceCharts sample evidence");
        ciWorkflow.Should().Contain("SurfaceChartsDemoConfigurationTests");
        ciWorkflow.Should().Contain("SurfaceChartsDemoViewportBehaviorTests");
        ciWorkflow.Should().Contain("VideraViewExtensibilityIntegrationTests");
        ciWorkflow.Should().Contain("VideraViewInteractionIntegrationTests");
        ciWorkflow.Should().Contain("VideraViewInspectionIntegrationTests");
        ciWorkflow.Should().Contain("VideraInspectionBundleIntegrationTests");
        ciWorkflow.Should().Contain("Run SurfaceCharts runtime evidence");
        ciWorkflow.Should().Contain("SurfaceChartViewViewStateTests");
        ciWorkflow.Should().Contain("SurfaceChartInteractionTests");
        ciWorkflow.Should().Contain("SurfaceChartViewGpuFallbackTests");
        ciWorkflow.Should().Contain("WaterfallChartViewIntegrationTests");
        ciWorkflow.Should().Contain("ScatterChartViewIntegrationTests");

        releasing.Should().Contain("sample-contract-evidence");
        releasing.Should().Contain("Videra.ExtensibilitySample");
        releasing.Should().Contain("Videra.InteractionSample");
        readme.Should().Contain("sample-contract evidence");
        ciWorkflow.Should().Contain("quality-gate-evidence:");
        ciWorkflow.Should().Contain("Run packaged consumer smoke with warnings as errors");
        ciWorkflow.Should().Contain("Run packaged SurfaceCharts consumer smoke with warnings as errors");
        ciWorkflow.Should().Contain("Invoke-ConsumerSmoke.ps1 -Configuration Release");
        ciWorkflow.Should().Contain("-Scenario SurfaceCharts");
        ciWorkflow.Should().NotContain("-BuildOnly");
        ciWorkflow.Should().Contain("-TreatWarningsAsErrors");
        releasing.Should().Contain("package-size budgets");
        releasing.Should().Contain("packaged viewer consumer smoke");
        releasing.Should().Contain("packaged SurfaceCharts consumer smoke");
        readme.Should().Contain("package-size budgets");
        readme.Should().Contain("packaged viewer consumer smoke");
        readme.Should().Contain("packaged SurfaceCharts");
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
