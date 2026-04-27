using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class AnalyzerPolicyRepositoryTests
{
    private static readonly string[] PolicyDisabledRuleIds =
    [
        "CA1051",
        "CA1305",
        "CA1707",
        "CA1720",
        "CA1822",
        "CA1848",
        "CA1859",
        "CA1861",
        "CA1873",
        "CA2201",
        "CA2263"
    ];

    [Fact]
    public void AnalyzerPolicy_ShouldDocumentAnalyzer10RuleDecisions()
    {
        var repositoryRoot = GetRepositoryRoot();
        var policyPath = Path.Combine(repositoryRoot, "docs", "analyzer-policy.md");

        File.Exists(policyPath).Should().BeTrue();

        var policy = File.ReadAllText(policyPath);

        policy.Should().Contain("Microsoft.CodeAnalysis.NetAnalyzers");
        policy.Should().Contain("10.0.203");
        policy.Should().Contain("analyzer 10");
        policy.Should().Contain("TreatWarningsAsErrors");

        foreach (var ruleId in PolicyDisabledRuleIds)
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
    public void AnalyzerPackage_ShouldUseAnalyzer10WithDocumentedPolicySuppressions()
    {
        var repositoryRoot = GetRepositoryRoot();
        var props = File.ReadAllText(Path.Combine(repositoryRoot, "Directory.Build.props"));
        var editorConfig = File.ReadAllText(Path.Combine(repositoryRoot, ".editorconfig"));

        props.Should().Contain("<EnableNETAnalyzers>true</EnableNETAnalyzers>");
        props.Should().Contain("<PackageReference Include=\"Microsoft.CodeAnalysis.NetAnalyzers\" Version=\"10.0.203\" PrivateAssets=\"all\" />");

        foreach (var ruleId in PolicyDisabledRuleIds)
        {
            editorConfig.Should().Contain($"dotnet_diagnostic.{ruleId}.severity = none");
        }
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
