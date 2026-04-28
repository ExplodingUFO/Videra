using System.Diagnostics;
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
        after.Should().Contain("## Backlog");
        after.Should().Contain("## Recently Closed");
        after.Should().Contain("Videra-21d");
        after.Should().Contain("v2.43 Visual Workbench and Authoring Polish");
        after.Should().Contain("Videra-bja");
        after.Should().Contain("Phase 314: Scene authoring polish and MinimalAuthoringSample");
        after.Should().Contain("Videra-dux");
        after.Should().Contain("Phase 315: SurfaceCharts numeric precision policy");
        after.Should().Contain("Videra-9m7");
        after.Should().Contain("Phase 317: Beads public roadmap generation");

        docsIndex.Should().Contain("ROADMAP.generated.md");
        beadsCoordination.Should().Contain("scripts/Export-BeadsRoadmap.ps1");
        beadsCoordination.Should().Contain("docs/ROADMAP.generated.md");
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
