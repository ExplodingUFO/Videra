using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class PlotSnapshotContractTests
{
    // ── Request construction ──────────────────────────────────────────

    [Fact]
    public void Request_ConstructsWithValidParameters()
    {
        var request = new PlotSnapshotRequest(
            1920, 1080, 2.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        request.Width.Should().Be(1920);
        request.Height.Should().Be(1080);
        request.Scale.Should().Be(2.0);
        request.Background.Should().Be(PlotSnapshotBackground.Transparent);
        request.Format.Should().Be(PlotSnapshotFormat.Png);
    }

    [Fact]
    public void Request_RejectsZeroWidth()
    {
        var act = () => new PlotSnapshotRequest(
            0, 1080, 2.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Request_RejectsZeroHeight()
    {
        var act = () => new PlotSnapshotRequest(
            1920, 0, 2.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Request_RejectsZeroScale()
    {
        var act = () => new PlotSnapshotRequest(
            1920, 1080, 0.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Request_RejectsNegativeWidth()
    {
        var act = () => new PlotSnapshotRequest(
            -1, 1080, 2.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Result construction ───────────────────────────────────────────

    [Fact]
    public void Result_Success_FactoryCreatesSuccessResult()
    {
        var manifest = CreateManifest();
        var result = PlotSnapshotResult.Success("/tmp/snap.png", manifest, TimeSpan.FromMilliseconds(120));

        result.Succeeded.Should().BeTrue();
        result.Path.Should().Be("/tmp/snap.png");
        result.Manifest.Should().BeSameAs(manifest);
        result.Failure.Should().BeNull();
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(120));
    }

    [Fact]
    public void Result_Failed_FactoryCreatesFailedResult()
    {
        var diagnostic = PlotSnapshotDiagnostic.Create("snap.format.unsupported", "Format not supported.");
        var result = PlotSnapshotResult.Failed(diagnostic, TimeSpan.FromMilliseconds(5));

        result.Succeeded.Should().BeFalse();
        result.Path.Should().BeNull();
        result.Manifest.Should().BeNull();
        result.Failure.Should().BeSameAs(diagnostic);
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(5));
    }

    [Fact]
    public void Result_Success_RequiresPath()
    {
        var manifest = CreateManifest();
        var act = () => PlotSnapshotResult.Success(null!, manifest, TimeSpan.Zero);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Result_Success_RequiresManifest()
    {
        var act = () => PlotSnapshotResult.Success("/tmp/snap.png", null!, TimeSpan.Zero);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Result_Failed_RequiresDiagnostic()
    {
        var act = () => PlotSnapshotResult.Failed(null!, TimeSpan.Zero);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Manifest construction ─────────────────────────────────────────

    [Fact]
    public void Manifest_ConstructsWithValidParameters()
    {
        var createdUtc = new DateTime(2026, 4, 29, 12, 0, 0, DateTimeKind.Utc);
        var manifest = new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "Surface:demo:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Opaque, createdUtc);

        manifest.Width.Should().Be(1920);
        manifest.Height.Should().Be(1080);
        manifest.OutputEvidenceKind.Should().Be("plot-3d-output");
        manifest.DatasetEvidenceKind.Should().Be("Plot3DDatasetEvidence");
        manifest.ActiveSeriesIdentity.Should().Be("Surface:demo:0");
        manifest.Format.Should().Be(PlotSnapshotFormat.Png);
        manifest.Background.Should().Be(PlotSnapshotBackground.Opaque);
        manifest.CreatedUtc.Should().Be(createdUtc);
    }

    [Fact]
    public void Manifest_RejectsZeroWidth()
    {
        var act = () => new PlotSnapshotManifest(
            0, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "Surface:demo:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Opaque, DateTime.UtcNow);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Manifest_RejectsWhitespaceOutputEvidenceKind()
    {
        var act = () => new PlotSnapshotManifest(
            1920, 1080, "  ", "Plot3DDatasetEvidence",
            "Surface:demo:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Opaque, DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Manifest_RejectsWhitespaceDatasetEvidenceKind()
    {
        var act = () => new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "  ",
            "Surface:demo:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Opaque, DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Manifest_RejectsWhitespaceActiveSeriesIdentity()
    {
        var act = () => new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "  ", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Opaque, DateTime.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    // ── Diagnostic construction ───────────────────────────────────────

    [Fact]
    public void Diagnostic_ConstructsWithValidParameters()
    {
        var diagnostic = PlotSnapshotDiagnostic.Create("snap.error.test", "Test message.");

        diagnostic.DiagnosticCode.Should().Be("snap.error.test");
        diagnostic.Message.Should().Be("Test message.");
    }

    [Fact]
    public void Diagnostic_RejectsWhitespaceCode()
    {
        var act = () => PlotSnapshotDiagnostic.Create("  ", "Test message.");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Diagnostic_RejectsWhitespaceMessage()
    {
        var act = () => PlotSnapshotDiagnostic.Create("snap.error.test", "  ");

        act.Should().Throw<ArgumentException>();
    }

    // ── Evidence kind linkage ─────────────────────────────────────────

    [Fact]
    public void Manifest_OutputEvidenceKind_MatchesPlot3DOutputEvidenceConvention()
    {
        var manifest = CreateManifest();

        manifest.OutputEvidenceKind.Should().Be("plot-3d-output");
    }

    [Fact]
    public void Manifest_DatasetEvidenceKind_MatchesPlot3DDatasetEvidenceConvention()
    {
        var manifest = CreateManifest();

        manifest.DatasetEvidenceKind.Should().Be("Plot3DDatasetEvidence");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static PlotSnapshotManifest CreateManifest()
    {
        return new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "Surface:demo:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Transparent, DateTime.UtcNow);
    }
}
