using System.Xml.Linq;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class DependencyHygieneRepositoryTests
{
    private static readonly string[] SharedTestToolingPackageIds =
    [
        "Microsoft.NET.Test.Sdk",
        "xunit",
        "xunit.runner.visualstudio",
        "FluentAssertions",
        "coverlet.collector"
    ];

    [Fact]
    public void Dependabot_ShouldGroupSharedTestToolingUpdates()
    {
        var repositoryRoot = GetRepositoryRoot();
        var dependabot = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "dependabot.yml"));

        dependabot.Should().Contain("shared-test-tooling");
        dependabot.Should().Contain("analyzers");

        foreach (var packageId in SharedTestToolingPackageIds)
        {
            dependabot.Should().Contain(packageId);
        }
    }

    [Fact]
    public void SharedTestToolingDriftScript_ShouldClassifyRepoWideTestPackages()
    {
        var repositoryRoot = GetRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Test-SharedTestToolingPackages.ps1"));

        script.Should().Contain("tests");

        foreach (var packageId in SharedTestToolingPackageIds)
        {
            script.Should().Contain(packageId);
        }
    }

    [Fact]
    public void SharedTestToolingPackages_ShouldUseSingleVersionAcrossTestProjects()
    {
        var repositoryRoot = GetRepositoryRoot();
        var testProjects = Directory.GetFiles(Path.Combine(repositoryRoot, "tests"), "*.csproj", SearchOption.AllDirectories);

        testProjects.Should().NotBeEmpty();

        foreach (var packageId in SharedTestToolingPackageIds)
        {
            var versions = testProjects
                .Select(path => new { Path = path, Document = XDocument.Load(path) })
                .SelectMany(project => project.Document
                    .Descendants("PackageReference")
                    .Where(reference => (string?)reference.Attribute("Include") == packageId)
                    .Select(reference => new
                    {
                        project.Path,
                        Version = (string?)reference.Attribute("Version")
                    }))
                .ToArray();

            versions.Should().NotBeEmpty($"shared test package '{packageId}' should be used by test projects");
            versions.Select(reference => reference.Version).Distinct(StringComparer.Ordinal)
                .Should()
                .ContainSingle($"shared test package '{packageId}' should not drift across test projects");
        }
    }

    [Fact]
    public void DependencyUpdatePolicy_ShouldDocumentRobotPrTriage()
    {
        var repositoryRoot = GetRepositoryRoot();
        var policy = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "dependency-update-policy.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));

        policy.Should().Contain("Merge directly");
        policy.Should().Contain("Rebase and retest");
        policy.Should().Contain("Close and replace");
        policy.Should().Contain("Defer as a dedicated phase");
        policy.Should().Contain("coverlet.collector");
        policy.Should().Contain("10.0.0");
        policy.Should().Contain("central package management");

        docsIndex.Should().Contain("dependency-update-policy.md");
        docsIndex.Should().Contain("shared test-tooling drift checks");
    }

    [Fact]
    public void VerifyScript_ShouldRunSharedTestToolingDriftCheck()
    {
        var repositoryRoot = GetRepositoryRoot();
        var verifyScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));

        verifyScript.Should().Contain("Dependency Hygiene");
        verifyScript.Should().Contain("Test-SharedTestToolingPackages.ps1");
    }

    [Fact]
    public void MaintenanceQualityGateDocs_ShouldRecordAnalyzerAndDependencyEvidencePath()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "maintenance-quality-gates.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));

        docs.Should().Contain("pwsh -File ./scripts/verify.ps1 -Configuration Release");
        docs.Should().Contain("scripts/Test-SharedTestToolingPackages.ps1");
        docs.Should().Contain("dotnet build Videra.slnx -c Release -p:TreatWarningsAsErrors=true");
        docs.Should().Contain("quality-gate-evidence");
        docs.Should().Contain("Central package management remains deferred");
        docs.Should().Contain("Broader analyzer rule-family adoption remains deferred");

        docsIndex.Should().Contain("maintenance-quality-gates.md");
        docsIndex.Should().Contain("analyzer/dependency hygiene verification commands");
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
