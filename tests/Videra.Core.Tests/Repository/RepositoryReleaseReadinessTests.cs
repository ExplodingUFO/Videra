using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class RepositoryReleaseReadinessTests
{
    [Fact]
    public void Readme_ShouldDocumentGitHubPackagesInstallation()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));

        readme.Should().Contain("https://nuget.pkg.github.com/ExplodingUFO/index.json");
        readme.Should().Contain("dotnet nuget add source");
        readme.Should().Contain("dotnet add package Videra.Avalonia");
    }

    [Fact]
    public void PublishWorkflow_ShouldUseTaggedReleasesAndValidatePackages()
    {
        var workflow = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "workflows", "publish-nuget.yml"));

        workflow.Should().NotContain("workflow_dispatch:");
        workflow.Should().Contain("runs-on: windows-latest");
        workflow.Should().Contain("pwsh -File ./verify.ps1 -Configuration Release");
        workflow.Should().Contain("Expand-Archive");
    }

    [Fact]
    public void Changelog_ShouldContainInitialAlphaReleaseEntry()
    {
        var changelog = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "CHANGELOG.md"));

        changelog.Should().Contain("## [0.1.0-alpha.1] - 2026-04-06");
    }

    [Fact]
    public void PullRequestTemplate_ShouldRequestIssueAndBreakingChangeContext()
    {
        var template = File.ReadAllText(Path.Combine(GetRepositoryRoot(), ".github", "pull_request_template.md"));

        template.Should().Contain("Related issue");
        template.Should().Contain("Breaking change?");
        template.Should().Contain("Platform validation");
    }

    [Fact]
    public void SecurityPolicy_ShouldDescribePrivateReportingAndResponseWindow()
    {
        var policy = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "SECURITY.md"));

        policy.Should().Contain("GitHub private vulnerability reporting");
        policy.Should().Contain("within 5 business days");
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
