using FluentAssertions;
using System.Text.RegularExpressions;
using Xunit;

namespace Videra.Core.Tests.Repository;

public sealed class SurfaceChartsReleaseTruthRepositoryTests
{
    [Fact]
    public void PublishPublicWorkflow_ShouldRequireSurfaceChartsSmokeBeforeReleasePush()
    {
        var repositoryRoot = GetRepositoryRoot();
        var publishWorkflow = File.ReadAllText(Path.Combine(repositoryRoot, ".github", "workflows", "publish-public.yml"));
        var publishNeedsBlock = Regex.Match(
            publishWorkflow,
            @"publish:\s*\r?\n\s*needs:\s*\r?\n(?<needs>(?:\s+- .+\r?\n)+)",
            RegexOptions.Multiline);

        publishNeedsBlock.Success.Should().BeTrue();
        publishNeedsBlock.Groups["needs"].Value.Should().Contain("windows-surfacecharts-consumer-smoke");
        publishWorkflow.Should().Contain("Push public packages to nuget.org");
        publishWorkflow.Should().Contain("Create GitHub release with package assets");
    }

    [Fact]
    public void SurfaceChartsDocs_ShouldPreserveRepositoryOnlyAndPackagedChartProofSplit()
    {
        var repositoryRoot = GetRepositoryRoot();
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "zh-CN", "README.md"));
        var hostingBoundary = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "hosting-boundary.md"));

        readme.Should().Contain("Videra.SurfaceCharts.Avalonia");
        readme.Should().Contain("Videra.SurfaceCharts.Processing");
        readme.Should().Contain("smoke/Videra.SurfaceCharts.ConsumerSmoke");
        readme.Should().Contain("surface/cache-backed proof");
        readme.Should().Contain("Start here: In-memory first chart");
        readme.Should().Contain("Explore next: Cache-backed streaming");
        readme.Should().Contain("保持 repository-only，只用于参考和 support repro");
        readme.Should().Contain("作为 repository-only 的 support-ready chart 参考应用");
        readme.Should().Contain("Copy support summary");
        readme.Should().Contain("Videra.SurfaceCharts.Demo");
        readme.Should().Contain("Try next: Analytics proof");
        readme.Should().NotContain("单独发布为 `Videra.SurfaceCharts.Demo`");

        hostingBoundary.Should().Contain("shipped public chart package family", Exactly.Once());
        hostingBoundary.Should().NotContain("Source-first analytics sibling");
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
