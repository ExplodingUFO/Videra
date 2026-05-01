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
    public void CreateUnsupportedExportDiagnostics_VectorExportIsSupported()
    {
        var diagnostics = Plot3DOutputCapabilityDiagnostic.CreateUnsupportedExportDiagnostics();
        var vectorExport = diagnostics.First(d => d.Capability == "VectorExport");

        vectorExport.IsSupported.Should().BeTrue();
    }

    // ── SVG export tests ─────────────────────────────────────────

    [Fact]
    public async Task CaptureSnapshotAsync_SvgFormat_EmptyPlot_ReturnsFailed()
    {
        var plot = CreateEmptyPlot();
        var request = new PlotSnapshotRequest(1920, 1080, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Svg);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeFalse();
        result.Failure.Should().NotBeNull();
        result.Failure!.DiagnosticCode.Should().Be("snapshot.chart.no-active-series");
    }

    [Fact]
    public async Task CaptureSnapshotAsync_SvgFormat_WithScatterData_Succeeds()
    {
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(800, 600, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Svg);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeTrue();
        result.Path.Should().EndWith(".svg");
        result.Manifest.Should().NotBeNull();
        result.Manifest!.Format.Should().Be(PlotSnapshotFormat.Svg);
    }

    [Fact]
    public async Task CaptureSnapshotAsync_SvgFormat_OutputContainsSvgMarkup()
    {
        var plot = CreatePlotWithSeries();
        var request = new PlotSnapshotRequest(800, 600, 1.0, PlotSnapshotBackground.Transparent, PlotSnapshotFormat.Svg);

        var result = await plot.CaptureSnapshotAsync(request);

        result.Succeeded.Should().BeTrue();
        var svgContent = await File.ReadAllTextAsync(result.Path!);
        svgContent.Should().Contain("<svg");
        svgContent.Should().Contain("xmlns=\"http://www.w3.org/2000/svg\"");
        svgContent.Should().Contain("</svg>");
    }

    [Fact]
    public async Task SaveSvgAsync_WritesToSpecifiedPath()
    {
        var plot = CreatePlotWithSeries();
        var tempPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid():N}.svg");

        try
        {
            var result = await plot.SaveSvgAsync(tempPath, 800, 600);

            result.Succeeded.Should().BeTrue();
            result.Path.Should().Be(tempPath);
            File.Exists(tempPath).Should().BeTrue();

            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().Contain("<svg");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void PlotSnapshotFormat_HasSvgValue()
    {
        Enum.GetValues<PlotSnapshotFormat>().Should().Contain(PlotSnapshotFormat.Svg);
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
