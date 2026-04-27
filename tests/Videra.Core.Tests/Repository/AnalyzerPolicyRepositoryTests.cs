using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class AnalyzerPolicyRepositoryTests
{
    [Fact]
    public void AnalyzerPolicy_ShouldDocumentAnalyzer10RuleDecisions()
    {
        var repositoryRoot = GetRepositoryRoot();
        var policyPath = Path.Combine(repositoryRoot, "docs", "analyzer-policy.md");

        File.Exists(policyPath).Should().BeTrue();

        var policy = File.ReadAllText(policyPath);

        policy.Should().Contain("Microsoft.CodeAnalysis.NetAnalyzers");
        policy.Should().Contain("9.0.0");
        policy.Should().Contain("analyzer 10");
        policy.Should().Contain("TreatWarningsAsErrors");

        foreach (var ruleId in new[] { "CA1051", "CA1720", "CA1822", "CA1859" })
        {
            policy.Should().Contain(ruleId);
        }

        policy.Should().Contain("Policy-gated suppression by contract area");
        policy.Should().Contain("Suppress for established vocabulary");
        policy.Should().Contain("Disable as an error by default");
        policy.Should().Contain("public API churn solely for style or micro-performance suggestions");
        policy.Should().Contain("broad Core, Import, Backend, Avalonia, or SurfaceCharts redesign");
        policy.Should().Contain("compatibility layers, fallback layers, or downgrade paths");
    }

    [Fact]
    public void AnalyzerPolicy_ShouldBeLinkedFromDocsIndex()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));

        docsIndex.Should().Contain("analyzer-policy.md");
        docsIndex.Should().Contain(".NET analyzer major-version triage");
    }

    [Fact]
    public void AnalyzerPackage_ShouldRemainOnPreAdoptionVersionDuringPolicyPhase()
    {
        var repositoryRoot = GetRepositoryRoot();
        var props = File.ReadAllText(Path.Combine(repositoryRoot, "Directory.Build.props"));

        props.Should().Contain("<EnableNETAnalyzers>true</EnableNETAnalyzers>");
        props.Should().Contain("<PackageReference Include=\"Microsoft.CodeAnalysis.NetAnalyzers\" Version=\"9.0.0\" PrivateAssets=\"all\" />");
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
