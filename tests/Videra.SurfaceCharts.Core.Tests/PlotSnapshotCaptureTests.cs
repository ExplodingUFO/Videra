using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Core.Tests;

public class PlotSnapshotCaptureTests
{
    // ── Validation tests (no render bridge needed) ─────────────────

    [Fact]
    public async Task CaptureSnapshotAsync_EmptyPlot_ReturnsFailedWithDiagnostic()
    {
        var plot = CreateEmptyPlot();
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.chart.no-active-series");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_UnsupportedFormat_ReturnsFailedWithDiagnostic()
    {
        // PlotSnapshotFormat currently only has Png, so this test uses (PlotSnapshotFormat)(-1)
        // to simulate an unsupported enum value
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, (PlotSnapshotFormat)(-1));

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.format.unsupported");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_NoRenderHost_ReturnsFailedWithDiagnostic()
    {
        var plot = CreatePlotWithSeries();
        // Don't set render bridge — simulates Plot3D without VideraChartView
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.render.no-host");
    }

    [Fact]
    public void Request_RejectsZeroWidth()
    {
        var act = () => new PlotSnapshotRequest(
            0, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Request_RejectsNegativeHeight()
    {
        var act = () => new PlotSnapshotRequest(
            1920, -1, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Png);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Manifest determinism tests ─────────────────────────────────

    [Fact]
    public void Manifest_OutputEvidenceKind_IsPlot3DOutput()
    {
        var manifest = CreateManifest();

        manifest.OutputEvidenceKind.Should().Be("plot-3d-output");
    }

    [Fact]
    public void Manifest_DatasetEvidenceKind_IsPlot3DDatasetEvidence()
    {
        var manifest = CreateManifest();

        manifest.DatasetEvidenceKind.Should().Be("Plot3DDatasetEvidence");
    }

    [Fact]
    public void Manifest_ActiveSeriesIdentity_FollowsConvention()
    {
        var manifest = CreateManifest();

        manifest.ActiveSeriesIdentity.Should().MatchRegex(@"^.+:\S+:\d+$");
    }

    // ── Capability diagnostic tests ────────────────────────────────

    [Fact]
    public void CreateUnsupportedExportDiagnostics_ImageExportIsSupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var imageExport = diagnostics.First(d => d.Capability == "ImageExport");

        imageExport.IsSupported.Should().BeTrue();
    }

    [Fact]
    public void CreateUnsupportedExportDiagnostics_PdfExportRemainsUnsupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var pdfExport = diagnostics.First(d => d.Capability == "PdfExport");

        pdfExport.IsSupported.Should().BeFalse();
    }

    [Fact]
    public void CreateUnsupportedExportDiagnostics_VectorExportRemainsUnsupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var vectorExport = diagnostics.First(d => d.Capability == "VectorExport");

        vectorExport.IsSupported.Should().BeFalse();
    }

    // ── Helpers ────────────────────────────────────────────────────

    private static Plot3D CreateEmptyPlot()
    {
        return new Plot3D(() => { });
    }

    private static Plot3D CreatePlotWithSeries()
    {
        var plot = new Plot3D(() => { });
        var metadata = new ScatterChartMetadata(
            new SurfaceAxisDescriptor("X", null, 0, 10),
            new SurfaceAxisDescriptor("Z", null, 0, 10),
            new SurfaceValueRange(0, 10));
        var series = new ScatterSeries([new ScatterPoint(1, 1, 1)], 0xFF0000);
        var scatterData = new ScatterChartData(metadata, [series]);
        plot.Add.Scatter(scatterData, "test-series");
        return plot;
    }

    private static PlotSnapshotManifest CreateManifest()
    {
        return new PlotSnapshotManifest(
            1920, 1080, "plot-3d-output", "Plot3DDatasetEvidence",
            "Scatter:test-series:0", PlotSnapshotFormat.Png,
            PlotSnapshotBackground.Transparent, DateTime.UtcNow);
    }
}
