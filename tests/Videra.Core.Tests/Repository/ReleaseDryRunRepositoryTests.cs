using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class ReleaseDryRunRepositoryTests
{
    [Fact]
    public void ReleaseDryRunScript_ShouldPackCanonicalPackagesAndReuseValidator()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-ReleaseDryRun.ps1");

        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("eng/public-api-contract.json");
        script.Should().Contain("dotnet pack");
        script.Should().Contain("/p:PackageVersion=$Version");
        script.Should().Contain("scripts/Validate-Packages.ps1");
        script.Should().Contain("release-dry-run-summary.json");
        script.Should().Contain("release-dry-run-summary.txt");
        script.Should().Contain("status = \"pass\"");
        script.Should().Contain("artifactPaths");
        script.Should().Contain("steps");
        script.Should().Contain("New-ReleaseCandidateEvidenceIndex.ps1");
        script.Should().Contain("package-size-evaluation.json");
        script.Should().Contain("package-size-summary.txt");
        script.Should().NotContain("dotnet nuget push");
    }

    [Fact]
    public void ReleaseCandidateVersionScript_ShouldValidateSimulatedTagAndDryRunSummaryTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Test-ReleaseCandidateVersion.ps1");

        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("CandidateTag");
        script.Should().Contain("Directory.Build.props");
        script.Should().Contain("eng/public-api-contract.json");
        script.Should().Contain("ReleaseDryRunSummaryPath");
        script.Should().Contain("expectedVersion");
        script.Should().Contain("packageContractPath");
        script.Should().NotContain("dotnet nuget push");
        script.Should().NotContain("git tag");
        script.Should().NotContain("NUGET_API_KEY");
        script.Should().NotContain("GITHUB_TOKEN");
    }

    [Fact]
    public void ReleaseDryRunScript_ShouldRunCandidateVersionSimulationBeforeAndAfterSummary()
    {
        var script = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "scripts", "Invoke-ReleaseDryRun.ps1"));

        script.Should().Contain("scripts/Test-ReleaseCandidateVersion.ps1");
        script.Should().Contain("-CandidateTag \"v$ExpectedVersion\"");
        script.Should().Contain("-ReleaseDryRunSummaryPath $summaryPath");
    }

    [Fact]
    public void ReleaseCandidateEvidenceContract_ShouldNameRequiredReviewSignals()
    {
        var repositoryRoot = GetRepositoryRoot();
        var contractPath = Path.Combine(repositoryRoot, "eng", "release-candidate-evidence.json");

        File.Exists(contractPath).Should().BeTrue();

        var contract = File.ReadAllText(contractPath);
        contract.Should().Contain("\"schemaVersion\": 1");
        contract.Should().Contain("release-dry-run");
        contract.Should().Contain("benchmark-gates");
        contract.Should().Contain("native-validation");
        contract.Should().Contain("consumer-smoke");
        contract.Should().Contain("release-candidate-evidence-index.json");
        contract.Should().Contain("release-candidate-evidence-index.txt");
        contract.Should().Contain("docs/releasing.md");
        contract.Should().NotContain("dotnet nuget push");
        contract.Should().NotContain("NUGET_API_KEY");
    }

    [Fact]
    public void ReleaseCandidateEvidenceIndexScript_ShouldReadDryRunSummaryAndContract()
    {
        var scriptPath = Path.Combine(GetRepositoryRoot(), "scripts", "New-ReleaseCandidateEvidenceIndex.ps1");

        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("eng/release-candidate-evidence.json");
        script.Should().Contain("ReleaseDryRunSummaryPath");
        script.Should().Contain("release-candidate-evidence-index.json");
        script.Should().Contain("release-candidate-evidence-index.txt");
        script.Should().Contain("requiredChecks");
        script.Should().Contain("requiredArtifacts");
        script.Should().Contain("supportDocs");
        script.Should().Contain("dryRunStatus");
        script.Should().Contain("dryRunArtifacts");
        script.Should().Contain("validationSteps");
        script.Should().Contain("Assert-ExistingFile");
        script.Should().Contain("Package size evaluation artifact");
        script.Should().NotContain("dotnet nuget push");
    }

    [Fact]
    public void ReleaseCandidateEvidenceIndexScript_ShouldFailClosedWhenValidationArtifactsAreMissing()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "New-ReleaseCandidateEvidenceIndex.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraReleaseDryRunTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputRoot);

        var missingPackageSizeEvaluation = Path.Combine(outputRoot, "missing-package-size-evaluation.json");
        var missingPackageSizeSummary = Path.Combine(outputRoot, "missing-package-size-summary.txt");
        var summaryPath = Path.Combine(outputRoot, "release-dry-run-summary.json");
        var summary = new
        {
            schemaVersion = 1,
            status = "pass",
            expectedVersion = "0.1.0-alpha.7",
            packageContractPath = "eng/public-api-contract.json",
            packageValidationScript = "scripts/Validate-Packages.ps1",
            validationArtifacts = new
            {
                packageSizeEvaluation = missingPackageSizeEvaluation,
                packageSizeSummary = missingPackageSizeSummary
            },
            steps = new[]
            {
                new { id = "version-simulation-prepack", status = "pass" },
                new { id = "package-build", status = "pass" },
                new { id = "package-validation", status = "pass" },
                new { id = "version-simulation-summary", status = "pass" },
                new { id = "evidence-index", status = "pass" }
            }
        };

        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));

        var result = RunPowerShell(
            scriptPath,
            repositoryRoot,
            "-ExpectedVersion",
            "0.1.0-alpha.7",
            "-ReleaseDryRunSummaryPath",
            summaryPath,
            "-OutputRoot",
            outputRoot);

        result.ExitCode.Should().NotBe(0);
        $"{result.Stdout}{result.Stderr}".Should().Contain("Package size evaluation artifact");
    }

    [Fact]
    public void ReleaseDryRunWorkflow_ShouldBeReadOnlyAndNonPublishing()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "release-dry-run.yml"));

        workflow.Should().Contain("workflow_dispatch:");
        workflow.Should().Contain("pull_request:");
        workflow.Should().Contain("permissions:");
        workflow.Should().Contain("contents: read");
        workflow.Should().Contain("Invoke-ReleaseDryRun.ps1");
        workflow.Should().Contain("0.0.0-ci-dryrun");
        workflow.Should().Contain("actions/upload-artifact@v4");
        workflow.Should().Contain("release-dry-run-evidence");
        workflow.Should().NotContain("packages: write");
        workflow.Should().NotContain("dotnet nuget push");
        workflow.Should().NotContain("NUGET_API_KEY");
        workflow.Should().NotContain("GITHUB_TOKEN");
        workflow.Should().NotContain("softprops/action-gh-release");
        workflow.Should().NotContain("api.nuget.org");
        workflow.Should().NotContain("nuget.pkg.github.com");
    }

    [Fact]
    public void PublishWorkflows_ShouldKeepFeedPushesOutsideDryRunPath()
    {
        var repositoryRoot = GetRepositoryRoot();
        var publicWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-public.yml"));
        var previewWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-github-packages.yml"));
        var dryRunWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "release-dry-run.yml"));

        publicWorkflow.Should().Contain("dotnet nuget push");
        publicWorkflow.Should().Contain("scripts/Validate-Packages.ps1");
        publicWorkflow.Should().Contain("NUGET_API_KEY");

        previewWorkflow.Should().Contain("dotnet nuget push");
        previewWorkflow.Should().Contain("scripts/Validate-Packages.ps1");
        previewWorkflow.Should().Contain("GITHUB_TOKEN");

        dryRunWorkflow.Should().Contain("Invoke-ReleaseDryRun.ps1");
        dryRunWorkflow.Should().NotContain("dotnet nuget push");
    }

    [Fact]
    public void PublicReleasePreflightScript_ShouldValidateEvidenceAndRemainNonMutating()
    {
        var scriptPath = Path.Combine(GetRepositoryRoot(), "scripts", "Invoke-PublicReleasePreflight.ps1");

        File.Exists(scriptPath).Should().BeTrue();

        var script = File.ReadAllText(scriptPath);
        script.Should().Contain("ExpectedVersion");
        script.Should().Contain("ExpectedCommit");
        script.Should().Contain("ReleaseDryRunRoot");
        script.Should().Contain("release-dry-run-summary.json");
        script.Should().Contain("release-candidate-evidence-index.json");
        script.Should().Contain("package-size-evaluation.json");
        script.Should().Contain("benchmark-threshold-evaluation.json");
        script.Should().Contain("consumer-smoke-result.json");
        script.Should().Contain("surfacecharts-support-summary.txt");
        script.Should().Contain("native-validation");
        script.Should().Contain("git status --porcelain");
        script.Should().Contain("public-release-preflight-summary.json");
        script.Should().Contain("public-release-preflight-summary.txt");
        script.Should().NotContain("dotnet nuget push");
        script.Should().NotContain("git tag");
        script.Should().NotContain("NUGET_API_KEY");
        script.Should().NotContain("GITHUB_TOKEN");
    }

    [Fact]
    public void PublicReleasePreflightScript_ShouldFailClosedWhenEvidenceIsMissing()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Invoke-PublicReleasePreflight.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraPublicReleasePreflightTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputRoot);

        var result = RunPowerShell(
            scriptPath,
            repositoryRoot,
            "-ExpectedVersion",
            "0.1.0-alpha.7",
            "-EvidenceRoot",
            outputRoot,
            "-OutputRoot",
            outputRoot,
            "-SkipRepositoryStateCheck");

        result.ExitCode.Should().NotBe(0);
        $"{result.Stdout}{result.Stderr}".Should().Contain("release-dry-run-summary.json");
        File.Exists(Path.Combine(outputRoot, "public-release-preflight-summary.json")).Should().BeTrue();
        File.Exists(Path.Combine(outputRoot, "public-release-preflight-summary.txt")).Should().BeTrue();
    }

    [Fact]
    public void PublicReleaseNotesScript_ShouldGenerateNotesFromPublishEvidence()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "New-PublicReleaseNotes.ps1");
        var outputRoot = Path.Combine(Path.GetTempPath(), "VideraPublicReleaseNotesTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputRoot);

        var evidencePath = Path.Combine(outputRoot, "public-publish-after-summary.json");
        var outputPath = Path.Combine(outputRoot, "public-release-notes.md");
        var evidence = new
        {
            stage = "after-publish",
            releaseTag = "v0.1.0-alpha.7",
            expectedVersion = "0.1.0-alpha.7",
            sourceCommit = "abc123",
            publishTarget = "https://api.nuget.org/v3/index.json",
            packages = new[]
            {
                new { id = "Videra.Avalonia", version = "0.1.0-alpha.7" },
                new { id = "Videra.Platform.Windows", version = "0.1.0-alpha.7" },
                new { id = "Videra.Import.Gltf", version = "0.1.0-alpha.7" },
                new { id = "Videra.SurfaceCharts.Avalonia", version = "0.1.0-alpha.7" },
                new { id = "Videra.SurfaceCharts.Processing", version = "0.1.0-alpha.7" }
            }
        };
        File.WriteAllText(evidencePath, JsonSerializer.Serialize(evidence));

        var result = RunPowerShell(
            scriptPath,
            repositoryRoot,
            "-EvidenceSummaryPath",
            evidencePath,
            "-OutputPath",
            outputPath);

        result.ExitCode.Should().Be(0, result.Stderr);
        var notes = File.ReadAllText(outputPath);
        notes.Should().Contain("Videra 0.1.0-alpha.7");
        notes.Should().Contain("dotnet add package Videra.Avalonia --version 0.1.0-alpha.7");
        notes.Should().Contain("dotnet add package Videra.Platform.Windows --version 0.1.0-alpha.7");
        notes.Should().Contain("dotnet add package Videra.SurfaceCharts.Avalonia --version 0.1.0-alpha.7");
        notes.Should().Contain("Package matrix");
        notes.Should().Contain("Known alpha limitations");
        notes.Should().Contain("public-publish-after-summary.json");
        notes.Should().Contain("nuget.org");
        notes.Should().Contain("GitHub Packages is preview/internal");
        notes.Should().Contain("Videra.Demo remains repository-only");
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

    private static PowerShellResult RunPowerShell(string scriptPath, string workingDirectory, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        process.StartInfo.ArgumentList.Add("-NoProfile");
        process.StartInfo.ArgumentList.Add("-ExecutionPolicy");
        process.StartInfo.ArgumentList.Add("Bypass");
        process.StartInfo.ArgumentList.Add("-File");
        process.StartInfo.ArgumentList.Add(scriptPath);
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start().Should().BeTrue();
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(60_000).Should().BeTrue();

        return new PowerShellResult(process.ExitCode, stdout, stderr);
    }

    private sealed record PowerShellResult(int ExitCode, string Stdout, string Stderr);
}
