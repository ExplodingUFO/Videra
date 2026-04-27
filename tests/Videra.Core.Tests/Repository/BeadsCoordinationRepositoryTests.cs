using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class BeadsCoordinationRepositoryTests
{
    [Fact]
    public void BeadsCoordinationDocs_ShouldDefineServiceAndAgentContract()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "beads-coordination.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var agents = File.ReadAllText(Path.Combine(repositoryRoot, "AGENTS.md"));
        var claude = File.ReadAllText(Path.Combine(repositoryRoot, "CLAUDE.md"));

        docs.Should().Contain("127.0.0.1");
        docs.Should().Contain("3306");
        docs.Should().Contain("Videra");
        docs.Should().Contain("cf27bb80-40f6-4ba7-95f7-bc455a484d7b");
        docs.Should().Contain("AgentDialog");
        docs.Should().Contain("bd ready --json");
        docs.Should().Contain("bd update <id> --claim --json");
        docs.Should().Contain("discovered-from");
        docs.Should().Contain("bd close <id> --reason");
        docs.Should().Contain("bd export -o .beads/issues.jsonl");
        docs.Should().Contain("does not replace GSD requirements");

        docsIndex.Should().Contain("beads-coordination.md");
        agents.Should().Contain("docs/beads-coordination.md");
        claude.Should().Contain("docs/beads-coordination.md");
    }

    [Fact]
    public void BeadsCoordinationDocs_ShouldDefineWorktreeLifecycleAndValidationBoundaries()
    {
        var repositoryRoot = GetRepositoryRoot();
        var docs = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "beads-coordination.md"));

        docs.Should().Contain("bd worktree list");
        docs.Should().Contain("redirect -> Videra");
        docs.Should().Contain("is_redirected: true");
        docs.Should().Contain("Branch/worktree ownership is still Git-local");
        docs.Should().Contain("eng/beads-lifecycle-proof.json");
        docs.Should().Contain("Videra-mnx");
        docs.Should().Contain("Videra-4yl");
        docs.Should().Contain("Git stores source");
        docs.Should().Contain("Dolt stores live Beads issue state");
        docs.Should().Contain("Normal product builds, package validation, release dry runs, and CI workflows do not start or require the Beads Docker service");
    }

    [Fact]
    public void BeadsCoordinationValidationScript_ShouldBeExplicitAndNonProduct()
    {
        var repositoryRoot = GetRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "Test-BeadsCoordination.ps1"));
        var verifyScript = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify.ps1"));
        var workflows = Directory.GetFiles(Path.Combine(repositoryRoot, ".github", "workflows"), "*.yml", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();

        script.Should().Contain("bd");
        script.Should().Contain("context");
        script.Should().Contain("--json");
        script.Should().Contain("doctor");
        script.Should().Contain("worktree");
        script.Should().Contain("list");
        script.Should().Contain("docker");
        script.Should().Contain("dolt-sql-server");
        script.Should().Contain("metadata");
        script.Should().Contain("cf27bb80-40f6-4ba7-95f7-bc455a484d7b");

        script.Should().NotContain("docker run");
        script.Should().NotContain("docker start");
        script.Should().NotContain("dotnet nuget push");
        script.Should().NotContain("git push");
        script.Should().NotContain("git tag");

        verifyScript.Should().NotContain("Test-BeadsCoordination.ps1");
        workflows.Should().OnlyContain(workflow => !workflow.Contains("Test-BeadsCoordination.ps1", StringComparison.Ordinal));
    }

    [Fact]
    public void BeadsLifecycleProof_ShouldRecordDiscoveredFromRelationship()
    {
        var repositoryRoot = GetRepositoryRoot();
        var proofPath = Path.Combine(repositoryRoot, "eng", "beads-lifecycle-proof.json");
        using var document = JsonDocument.Parse(File.ReadAllText(proofPath));
        var root = document.RootElement;

        root.GetProperty("schemaVersion").GetInt32().Should().Be(1);
        root.GetProperty("database").GetProperty("name").GetString().Should().Be("Videra");
        root.GetProperty("database").GetProperty("projectId").GetString().Should().Be("cf27bb80-40f6-4ba7-95f7-bc455a484d7b");
        root.GetProperty("phaseIssue").GetProperty("id").GetString().Should().Be("Videra-mnx");
        root.GetProperty("discoveredFollowUp").GetProperty("id").GetString().Should().Be("Videra-4yl");
        root.GetProperty("discoveredFollowUp").GetProperty("dependencyType").GetString().Should().Be("discovered-from");
        root.GetProperty("discoveredFollowUp").GetProperty("dependsOn").GetString().Should().Be("Videra-mnx");
        root.GetProperty("discoveredFollowUp").GetProperty("observedStatus").GetString().Should().Be("closed");
    }

    [Fact]
    public void BeadsRuntimeFiles_ShouldStayIgnored()
    {
        var repositoryRoot = GetRepositoryRoot();
        var beadsIgnore = File.ReadAllText(Path.Combine(repositoryRoot, ".beads", ".gitignore"));

        beadsIgnore.Should().Contain("export-state.json");
        beadsIgnore.Should().Contain("embeddeddolt/");
        beadsIgnore.Should().Contain(".beads-credential-key");
        beadsIgnore.Should().Contain("redirect");
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
