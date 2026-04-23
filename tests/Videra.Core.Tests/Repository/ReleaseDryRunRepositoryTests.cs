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
        script.Should().NotContain("dotnet nuget push");
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
