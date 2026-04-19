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
        var smokeWorkflowPath = Path.Combine(repositoryRoot, ".github", "workflows", "consumer-smoke.yml");
        var smokeScriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-ConsumerSmoke.ps1");

        File.Exists(smokeProjectPath).Should().BeTrue();
        File.Exists(smokeWorkflowPath).Should().BeTrue();
        File.Exists(smokeScriptPath).Should().BeTrue();

        var smokeProject = File.ReadAllText(smokeProjectPath);
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Avalonia\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.Windows\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.Linux\"");
        smokeProject.Should().Contain("<PackageReference Include=\"Videra.Platform.macOS\"");
        smokeProject.Should().NotContain("<ProjectReference");

        var smokeWorkflow = File.ReadAllText(smokeWorkflowPath);
        smokeWorkflow.Should().Contain("pull_request:");
        smokeWorkflow.Should().Contain("push:");
        smokeWorkflow.Should().Contain("github.event_name != 'workflow_dispatch'");
        smokeWorkflow.Should().Contain("workflow_dispatch:");
        smokeWorkflow.Should().Contain("windows");
        smokeWorkflow.Should().Contain("linux-x11");
        smokeWorkflow.Should().Contain("linux-xwayland");
        smokeWorkflow.Should().Contain("macos");
        smokeWorkflow.Should().Contain("Invoke-ConsumerSmoke.ps1");
        smokeWorkflow.Should().Contain("actions/upload-artifact@v4");
        smokeWorkflow.Should().Contain("consumer-smoke-windows");
        smokeWorkflow.Should().Contain("consumer-smoke-linux-x11");
        smokeWorkflow.Should().Contain("consumer-smoke-linux-xwayland");
        smokeWorkflow.Should().Contain("consumer-smoke-macos");

        var smokeScript = File.ReadAllText(smokeScriptPath);
        smokeScript.Should().Contain("Pack Public Consumer Packages");
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
        smokeScript.Should().Contain("ResolvedBackend");
        smokeScript.Should().Contain("ResolvedDisplayServer");
        smokeScript.Should().Contain("DisplayServerCompatibility");
        smokeScript.Should().Contain("diagnostics-snapshot.txt");
        smokeScript.Should().Contain("inspection-bundle");
    }

    [Fact]
    public void ConsumerSmokeAsset_ShouldBeTrackedByGit()
    {
        var repositoryRoot = GetRepositoryRoot();
        var relativeAssetPath = "smoke/Videra.ConsumerSmoke/Assets/reference-cube.obj";
        var assetPath = Path.Combine(repositoryRoot, "smoke", "Videra.ConsumerSmoke", "Assets", "reference-cube.obj");

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

        process.ExitCode.Should().Be(0, $"the consumer smoke asset must be committed so release checkouts can build.{Environment.NewLine}{standardOutput}{standardError}");
        standardOutput.Should().Contain(relativeAssetPath);
    }

    [Fact]
    public void BenchmarkGate_ShouldHaveWorkflowScriptAndDocsTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflowPath = Path.Combine(repositoryRoot, ".github", "workflows", "benchmark-gates.yml");
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Run-Benchmarks.ps1");
        var docsPath = Path.Combine(repositoryRoot, "docs", "benchmark-gates.md");
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var rootReadme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));

        File.Exists(workflowPath).Should().BeTrue();
        File.Exists(scriptPath).Should().BeTrue();
        File.Exists(docsPath).Should().BeTrue();

        var workflow = File.ReadAllText(workflowPath);
        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("pull_request:");
        workflow.Should().Contain("run-benchmarks");
        workflow.Should().Contain("Run-Benchmarks.ps1 -Suite Viewer");
        workflow.Should().Contain("Run-Benchmarks.ps1 -Suite SurfaceCharts");
        workflow.Should().Contain("benchmarks-viewer");
        workflow.Should().Contain("benchmarks-surfacecharts");

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("Videra.Viewer.Benchmarks");
        script.Should().Contain("Videra.SurfaceCharts.Benchmarks");
        script.Should().Contain("--exporters json csv markdown");
        script.Should().Contain("SUMMARY.txt");

        var docs = File.ReadAllText(docsPath);
        docs.Should().Contain("Run workflow");
        docs.Should().Contain("run-benchmarks");
        docs.Should().Contain("Run-Benchmarks.ps1");
        docs.Should().Contain("viewer");
        docs.Should().Contain("surfacecharts");
        docs.Should().Contain("compare runs over time");
        docs.Should().Contain("trend");

        docsIndex.Should().Contain("benchmark-gates.md");
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
        feedbackDoc.Should().Contain("diagnostics snapshot");
        feedbackDoc.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        feedbackDoc.Should().Contain("VideraInspectionBundleService");
        feedbackDoc.Should().Contain("ResolvedDisplayServer");
        feedbackDoc.Should().Contain("DisplayServerFallbackUsed");
        feedbackDoc.Should().Contain("DisplayServerFallbackReason");
        feedbackDoc.Should().Contain("DisplayServerCompatibility");
        feedbackDoc.Should().Contain("XWayland");
        feedbackDoc.Should().Contain("compositor-native Wayland");
        feedbackDoc.Should().Contain("Videra.SurfaceCharts.*");

        var bugForm = File.ReadAllText(bugFormPath);
        bugForm.Should().Contain("install_path");
        bugForm.Should().Contain("version");
        bugForm.Should().Contain("host_environment");
        bugForm.Should().Contain("diagnostics snapshot");
        bugForm.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        bugForm.Should().Contain("Videra.MinimalSample");
        bugForm.Should().Contain("consumer smoke");

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
        troubleshooting.Should().Contain("diagnostics snapshot");
        troubleshooting.Should().Contain("VideraDiagnosticsSnapshotFormatter");
        troubleshooting.Should().Contain("VideraInspectionBundleService");
        troubleshooting.Should().Contain("DisplayServerCompatibility");

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
        ciWorkflow.Should().Contain("VideraViewExtensibilityIntegrationTests");
        ciWorkflow.Should().Contain("VideraViewInteractionIntegrationTests");
        ciWorkflow.Should().Contain("VideraViewInspectionIntegrationTests");
        ciWorkflow.Should().Contain("VideraInspectionBundleIntegrationTests");

        releasing.Should().Contain("sample-contract-evidence");
        releasing.Should().Contain("Videra.ExtensibilitySample");
        releasing.Should().Contain("Videra.InteractionSample");
        readme.Should().Contain("sample-contract evidence");
        ciWorkflow.Should().Contain("quality-gate-evidence:");
        ciWorkflow.Should().Contain("Build packaged consumer smoke with warnings as errors");
        ciWorkflow.Should().Contain("Invoke-ConsumerSmoke.ps1 -Configuration Release");
        ciWorkflow.Should().Contain("-BuildOnly");
        ciWorkflow.Should().Contain("-TreatWarningsAsErrors");
        releasing.Should().Contain("packaged consumer path");
        readme.Should().Contain("packaged consumer path");
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
