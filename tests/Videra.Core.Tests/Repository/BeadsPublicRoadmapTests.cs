using System.Diagnostics;
using System.Text.Json.Nodes;
using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class BeadsPublicRoadmapTests
{
    [Fact]
    public void PublicRoadmap_ShouldBeGeneratedFromExportedBeadsSnapshot()
    {
        var repositoryRoot = GetRepositoryRoot();
        var scriptPath = Path.Combine(repositoryRoot, "scripts", "Export-BeadsRoadmap.ps1");
        var issuesPath = Path.Combine(repositoryRoot, ".beads", "issues.jsonl");
        var roadmapPath = Path.Combine(repositoryRoot, "docs", "ROADMAP.generated.md");
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var beadsCoordination = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "beads-coordination.md"));

        File.Exists(scriptPath).Should().BeTrue();
        File.Exists(issuesPath).Should().BeTrue();
        File.Exists(roadmapPath).Should().BeTrue();

        var before = File.ReadAllText(roadmapPath);
        RunPowerShell(repositoryRoot, scriptPath);
        var after = File.ReadAllText(roadmapPath);

        after.Should().Be(before);
        after.Should().Contain("# Videra Public Roadmap");
        after.Should().Contain("Generated from `.beads/issues.jsonl`.");
        after.Should().Contain("Beads remains the single authoritative task tracker.");
        after.Should().Contain("## Active");
        after.Should().Contain("## Ready");
        after.Should().Contain("## Blocked");
        after.Should().Contain("## Backlog");
        after.Should().Contain("## Recently Closed");
        var issues = ReadIssues(issuesPath);
        var issuesById = issues.ToDictionary(static issue => GetString(issue, "id"), StringComparer.Ordinal);
        var openIssues = issues.Where(static issue => GetString(issue, "status") != "closed").ToArray();
        var readyIssues = openIssues
            .Where(issue => GetString(issue, "status") == "open")
            .Where(static issue => GetInt(issue, "priority") <= 2)
            .Where(issue => !HasOpenBlockingDependency(issue, issuesById))
            .ToArray();
        var blockedIssues = openIssues
            .Where(issue => GetString(issue, "status") == "open")
            .Where(static issue => GetInt(issue, "priority") <= 2)
            .Where(issue => HasOpenBlockingDependency(issue, issuesById))
            .ToArray();
        var recentlyClosed = issues
            .Where(static issue => GetString(issue, "status") == "closed")
            .OrderByDescending(static issue => GetString(issue, "closed_at"))
            .ThenBy(static issue => GetString(issue, "id"), StringComparer.Ordinal)
            .Take(10)
            .ToArray();

        if (readyIssues.Length == 0)
        {
            after.Should().Contain("## Ready");
            after.Should().Contain("_No matching beads in the exported snapshot._");
        }

        foreach (var issue in readyIssues.Concat(blockedIssues).Concat(recentlyClosed))
        {
            after.Should().Contain(GetString(issue, "id"));
            after.Should().Contain(GetString(issue, "title"));
        }

        docsIndex.Should().Contain("ROADMAP.generated.md");
        beadsCoordination.Should().Contain("scripts/Export-BeadsRoadmap.ps1");
        beadsCoordination.Should().Contain("docs/ROADMAP.generated.md");
    }

    private static JsonObject[] ReadIssues(string issuesPath)
    {
        return File.ReadLines(issuesPath)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .Select(static line => JsonNode.Parse(line)!.AsObject())
            .Where(static issue => GetString(issue, "_type") == "issue")
            .ToArray();
    }

    private static bool HasOpenBlockingDependency(JsonObject issue, IReadOnlyDictionary<string, JsonObject> issuesById)
    {
        if (issue["dependencies"] is not JsonArray dependencies)
        {
            return false;
        }

        foreach (var dependency in dependencies.OfType<JsonObject>())
        {
            if (GetString(dependency, "type") != "blocks")
            {
                continue;
            }

            var dependencyId = GetString(dependency, "depends_on_id");
            if (string.IsNullOrWhiteSpace(dependencyId))
            {
                continue;
            }

            if (!issuesById.TryGetValue(dependencyId, out var dependencyIssue))
            {
                return true;
            }

            if (GetString(dependencyIssue, "status") != "closed")
            {
                return true;
            }
        }

        return false;
    }

    private static string GetString(JsonObject issue, string propertyName)
    {
        return issue[propertyName]?.GetValue<string>() ?? "";
    }

    private static int GetInt(JsonObject issue, string propertyName)
    {
        return issue[propertyName]?.GetValue<int>() ?? 0;
    }

    private static void RunPowerShell(string workingDirectory, string scriptPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-File");
        startInfo.ArgumentList.Add(scriptPath);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start pwsh.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        process.ExitCode.Should().Be(0, output + error);
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
