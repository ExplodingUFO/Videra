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

    [Fact]
    public void SurfaceChartsReleaseCutoverDocs_ShouldDescribeConsumerSupportBoundary()
    {
        var repositoryRoot = GetRepositoryRoot();
        var cutover = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "surfacecharts-release-cutover.md"));
        var readme = File.ReadAllText(Path.Combine(repositoryRoot, "README.md"));
        var docsIndex = File.ReadAllText(Path.Combine(repositoryRoot, "docs", "index.md"));
        var avaloniaReadme = File.ReadAllText(Path.Combine(repositoryRoot, "src", "Videra.SurfaceCharts.Avalonia", "README.md"));
        var demoReadme = File.ReadAllText(Path.Combine(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md"));

        cutover.Should().Contain("SurfaceCharts v2.58 Release Cutover");
        cutover.Should().Contain("explicit maintainer approval");
        cutover.Should().Contain("Videra.SurfaceCharts.Avalonia");
        cutover.Should().Contain("Videra.SurfaceCharts.Processing");
        cutover.Should().Contain("Plot.Add.Surface");
        cutover.Should().Contain("Plot.Add.Waterfall");
        cutover.Should().Contain("Plot.Add.Scatter");
        cutover.Should().Contain("surfacecharts-support-summary.txt");
        cutover.Should().Contain("chart-snapshot.png");
        cutover.Should().Contain("public-release-notes.md");
        cutover.Should().Contain("ScottPlot 5's discoverable recipe ergonomics as inspiration only");
        cutover.Should().Contain("not a ScottPlot API compatibility, parity, adapter, or migration layer");
        cutover.Should().Contain("no hidden scenario/data-path fallback");
        cutover.Should().Contain("no PDF/vector export");
        cutover.Should().Contain("no OpenGL/WebGL/backend expansion");
        cutover.Should().Contain("repository-only");
        cutover.Should().Contain("support evidence, not benchmark truth");

        foreach (var document in new[] { readme, docsIndex, avaloniaReadme, demoReadme })
        {
            document.Should().Contain("SurfaceCharts v2.58 Release Cutover");
            document.Should().Contain("surfacecharts-release-cutover.md");
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
