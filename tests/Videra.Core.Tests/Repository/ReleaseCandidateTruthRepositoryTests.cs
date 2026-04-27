using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class ReleaseCandidateTruthRepositoryTests
{
    [Fact]
    public void CandidateDocs_ShouldShareDryRunEvidenceTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = new Dictionary<string, string>
        {
            ["README.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "README.md")),
            ["docs/capability-matrix.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "capability-matrix.md")),
            ["docs/package-matrix.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "package-matrix.md")),
            ["docs/support-matrix.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "support-matrix.md")),
            ["docs/release-policy.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "release-policy.md")),
            ["docs/releasing.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "releasing.md")),
            ["CHANGELOG.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "CHANGELOG.md")),
            ["docs/zh-CN/README.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md")),
            ["docs/zh-CN/index.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "index.md"))
        };

        foreach (var (name, content) in docs)
        {
            content.Should().Contain("Release Dry Run", name);
            content.Should().Contain("release-dry-run-evidence", name);
        }

        docs["README.md"].Should().Contain(".github/workflows/release-dry-run.yml");
        docs["README.md"].Should().Contain("scripts/Invoke-ReleaseDryRun.ps1");
        docs["README.md"].Should().Contain("eng/public-api-contract.json");
        docs["README.md"].Should().Contain("scripts/Validate-Packages.ps1");

        docs["docs/release-policy.md"].Should().Contain("does not push assets to `nuget.org` or GitHub Packages");
        docs["docs/release-policy.md"].Should().Contain("candidate version/tag simulation");
        docs["docs/releasing.md"].Should().Contain("release-candidate-evidence-index.json");
        docs["docs/releasing.md"].Should().Contain("release-candidate-evidence-index.txt");
        docs["docs/releasing.md"].Should().Contain("Avoid `dotnet nuget push`");
        docs["docs/releasing.md"].Should().Contain("NUGET_API_KEY");
        docs["docs/releasing.md"].Should().Contain("Dry-run evidence should be linked from release-candidate review notes");
    }

    [Fact]
    public void DryRunWorkflowAndScript_ShouldStayNonPublishingAndContractDriven()
    {
        var repositoryRoot = GetRepositoryRoot();
        var workflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "release-dry-run.yml"));
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Invoke-ReleaseDryRun.ps1"));

        workflow.Should().Contain("permissions:");
        workflow.Should().Contain("contents: read");
        workflow.Should().Contain("scripts/Invoke-ReleaseDryRun.ps1");
        workflow.Should().Contain("release-dry-run-evidence");
        workflow.Should().NotContain("dotnet nuget push");
        workflow.Should().NotContain("NUGET_API_KEY");
        workflow.Should().NotContain("softprops/action-gh-release");

        script.Should().Contain("eng/public-api-contract.json");
        script.Should().Contain("scripts/Validate-Packages.ps1");
        script.Should().Contain("release-dry-run-summary.json");
        script.Should().Contain("release-dry-run-summary.txt");
        script.Should().NotContain("dotnet nuget push");
    }

    [Fact]
    public void CutoverRunbook_ShouldPreserveAbortCriteriaAndHumanApprovalBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var runbook = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "release-candidate-cutover.md"));
        var releasing = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "releasing.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var evidenceContract = File.ReadAllText(Path.Combine(repositoryRoot, "eng", "release-candidate-evidence.json"));
        var dryRunWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "release-dry-run.yml"));

        runbook.Should().Contain("Abort Criteria");
        runbook.Should().Contain("Finding Classification");
        runbook.Should().Contain("Release blocker");
        runbook.Should().Contain("Environment residual");
        runbook.Should().Contain("Deferred enhancement");
        runbook.Should().Contain("do not fold it into closeout");
        runbook.Should().Contain("Do not create a release tag");
        runbook.Should().Contain("Do not publish packages");
        runbook.Should().Contain("Human Cutover Preconditions");
        runbook.Should().Contain("release-candidate-evidence-index.txt");
        runbook.Should().Contain("v<package-version>");
        runbook.Should().Contain("No fallback publishing path");

        releasing.Should().Contain("release-candidate-cutover.md");
        releasing.Should().Contain("Abort criteria");
        docsIndex.Should().Contain("release-candidate-cutover.md");
        evidenceContract.Should().Contain("docs/release-candidate-cutover.md");

        dryRunWorkflow.Should().NotContain("dotnet nuget push");
        dryRunWorkflow.Should().NotContain("softprops/action-gh-release");
        dryRunWorkflow.Should().NotContain("NUGET_API_KEY");
    }

    [Fact]
    public void CandidateDocs_ShouldRouteVisualEvidenceThroughEvidenceIndexTruth()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = new Dictionary<string, string>
        {
            ["docs/videra-doctor.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "videra-doctor.md")),
            ["docs/alpha-feedback.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "alpha-feedback.md")),
            ["docs/releasing.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "releasing.md")),
            ["docs/release-candidate-cutover.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "release-candidate-cutover.md")),
            ["docs/zh-CN/README.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md")),
            ["docs/zh-CN/troubleshooting.md"] = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "troubleshooting.md"))
        };

        foreach (var (name, content) in docs)
        {
            content.Should().Contain("release-candidate-evidence-index", name);
            content.ToLowerInvariant().Should().Contain("visual evidence", name);
        }

        docs["docs/videra-doctor.md"].Should().Contain("visual evidence status as optional evidence-only context");
        docs["docs/alpha-feedback.md"].Should().Contain("visualEvidence.performanceLabVisualEvidence");
        docs["docs/alpha-feedback.md"].Should().Contain("visualEvidence.doctorVisualEvidence");
        docs["docs/release-candidate-cutover.md"].Should().Contain("not a release blocker");
        docs["docs/zh-CN/README.md"].Should().Contain("optional evidence-only context");
        docs["docs/zh-CN/troubleshooting.md"].Should().Contain("不会自动成为 publish blocker");
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
