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
