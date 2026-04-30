using FluentAssertions;
using Xunit;

namespace Videra.Core.Tests.Samples;

public sealed class SurfaceChartsCookbookCoverageMatrixTests
{
    [Fact]
    public void SurfaceChartsCookbookCoverageMatrix_ShouldPinCurrentConsumerHandoff()
    {
        var repositoryRoot = GetRepositoryRoot();
        var rootReadme = Read(repositoryRoot, "README.md");
        var docsIndex = Read(repositoryRoot, "docs", "index.md");
        var demoReadme = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "README.md");
        var cutover = Read(repositoryRoot, "docs", "surfacecharts-release-cutover.md");
        var mainWindowXaml = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Views", "MainWindow.axaml");
        var mainWindowCodeBehind = Read(repositoryRoot, "samples", "Videra.SurfaceCharts.Demo", "Views", "MainWindow.axaml.cs");

        foreach (var document in new[] { rootReadme, docsIndex, demoReadme })
        {
            document.Should().Contain("SurfaceCharts Current Consumer Handoff");
            document.Should().Contain("surfacecharts-release-cutover.md");
        }

        cutover.Should().StartWith("# SurfaceCharts Current Consumer Handoff");
        cutover.Should().Contain("This page is documentation only.");
        cutover.Should().Contain("Public package publishing, public tags, and GitHub Release publication still require explicit maintainer approval");

        foreach (var row in CookbookCoverageRows)
        {
            rootReadme.Should().Contain(row.RootReadmeToken);
            demoReadme.Should().Contain(row.DemoReadmeHeading);
            cutover.Should().Contain(row.CutoverEntry);
            mainWindowCodeBehind.Should().Contain($@"""{row.CodeBehindGroup}""");
            mainWindowCodeBehind.Should().Contain(row.SnippetToken);

            if (row.VisibleProofLabel is not null)
            {
                demoReadme.Should().Contain(row.VisibleProofLabel);
                cutover.Should().Contain(row.VisibleProofLabel);
                mainWindowXaml.Should().Contain(row.VisibleProofLabel);
            }
        }

        demoReadme.Should().Contain("copyable evidence-only support summary");
        mainWindowXaml.Should().Contain("Copy support summary");
        cutover.Should().Contain("For repository-only repros, use `Videra.SurfaceCharts.Demo`.");
        cutover.Should().Contain("For packaged SurfaceCharts issues, first run or attach evidence from `smoke/Videra.SurfaceCharts.ConsumerSmoke`");
        cutover.Should().Contain("not a ScottPlot API compatibility, parity, adapter, or migration layer");
        cutover.Should().Contain("no direct public `VideraChartView.Source` data-loading API");
        cutover.Should().Contain("no hidden scenario/data-path fallback");
        cutover.Should().Contain("no PDF/vector export and no image export beyond bounded PNG chart snapshots");
        cutover.Should().Contain("no OpenGL/WebGL/backend expansion");
        cutover.Should().NotContain("SurfaceChartView` is the shipped");
        cutover.Should().NotContain("WaterfallChartView` is the shipped");
        cutover.Should().NotContain("ScatterChartView` is the shipped");
    }

    private static readonly IReadOnlyList<CookbookCoverageRow> CookbookCoverageRows =
    [
        new(
            CodeBehindGroup: "First chart",
            DemoReadmeHeading: "### First Chart",
            RootReadmeToken: "First surface",
            CutoverEntry: "`First Chart` for the smallest `VideraChartView` plus `Plot.Add.Surface` setup.",
            VisibleProofLabel: "Start here: In-memory first chart",
            SnippetToken: "Plot.Add.Surface"),
        new(
            CodeBehindGroup: "Styling",
            DemoReadmeHeading: "### Styling",
            RootReadmeToken: "chart.Plot.Axes.X.Label",
            CutoverEntry: "`Styling` for axes, color maps, precision, and `Plot.OverlayOptions`.",
            VisibleProofLabel: "Try next: Analytics proof",
            SnippetToken: "SurfaceColorMapPresets.CreateProfessional"),
        new(
            CodeBehindGroup: "Interactions",
            DemoReadmeHeading: "### Interactions",
            RootReadmeToken: "chart-local interaction/profile APIs",
            CutoverEntry: "`Interactions` for built-in commands and probe resolution.",
            VisibleProofLabel: null,
            SnippetToken: "TryExecuteChartCommand"),
        new(
            CodeBehindGroup: "Live data",
            DemoReadmeHeading: "### Live Data",
            RootReadmeToken: "DataLogger3D",
            CutoverEntry: "`Live Data` for `Plot.Add.Scatter`, `DataLogger3D`, FIFO windows, and live-view evidence.",
            VisibleProofLabel: "Try next: Scatter proof",
            SnippetToken: "CreateLiveViewEvidence"),
        new(
            CodeBehindGroup: "Linked axes",
            DemoReadmeHeading: "### Linked Axes",
            RootReadmeToken: "linked-axis",
            CutoverEntry: "`Linked Axes` for explicit two-chart view linking with a disposable lifetime.",
            VisibleProofLabel: "Try next: Waterfall proof",
            SnippetToken: "LinkViewWith"),
        new(
            CodeBehindGroup: "Bar",
            DemoReadmeHeading: "### Bar",
            RootReadmeToken: "Plot.Add.Bar",
            CutoverEntry: "`Bar` for the bounded grouped-bar proof path in the demo gallery.",
            VisibleProofLabel: "Try next: Bar chart proof",
            SnippetToken: "BarChartData"),
        new(
            CodeBehindGroup: "Contour",
            DemoReadmeHeading: "### Contour",
            RootReadmeToken: "Plot.Add.Contour",
            CutoverEntry: "`Contour` for the bounded radial scalar-field contour proof path in the demo gallery.",
            VisibleProofLabel: "Try next: Contour plot proof",
            SnippetToken: "ContourChartData"),
        new(
            CodeBehindGroup: "Export",
            DemoReadmeHeading: "### Export",
            RootReadmeToken: "SavePngAsync",
            CutoverEntry: "`Export` for PNG-only chart snapshots.",
            VisibleProofLabel: null,
            SnippetToken: "Plot.SavePngAsync"),
    ];

    private static string Read(string repositoryRoot, params string[] pathParts)
    {
        return File.ReadAllText(Path.Combine([repositoryRoot, .. pathParts]));
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

    private sealed record CookbookCoverageRow(
        string CodeBehindGroup,
        string DemoReadmeHeading,
        string RootReadmeToken,
        string CutoverEntry,
        string? VisibleProofLabel,
        string SnippetToken);
}
