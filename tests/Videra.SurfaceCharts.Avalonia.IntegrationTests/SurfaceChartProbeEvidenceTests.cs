using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

public sealed class SurfaceChartProbeEvidenceTests
{
    [Fact]
    public void Create_WithHoveredAndPinnedProbes_ReportsStatusCountsReadoutsAndDelta()
    {
        var hoveredProbe = new SurfaceProbeInfo(
            sampleX: 1.234d,
            sampleY: 2.345d,
            axisX: 1234.567d,
            axisY: 98.765d,
            value: -9.876d,
            isApproximate: false);
        var pinnedProbe = new SurfaceProbeInfo(
            sampleX: 1d,
            sampleY: 2d,
            axisX: 1000d,
            axisY: 100d,
            value: -10d,
            isApproximate: true);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(
            hoveredProbe,
            [pinnedProbe],
            SurfaceChartNumericLabelPresets.Fixed(2));

        evidence.EvidenceKind.Should().Be("surface-chart-probe");
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.HoveredAndPinned);
        evidence.PinnedProbeCount.Should().Be(1);
        evidence.HoveredProbeReadout.Should().Be("X 1234.57 (sample 1.23), Y 98.77 (sample 2.35), Value -9.88 Exact");
        evidence.PinnedProbeReadouts.Should().ContainSingle()
            .Which.Should().Be("Pin 1 Approx\nX 1000.00 (sample 1.00)\nY 100.00 (sample 2.00)\nValue -10.00");
        evidence.DeltaVsFirstPinReadout.Should().Be("Delta vs Pin 1\nX +234.57\nY -1.23\nValue +0.12");
    }

    [Fact]
    public void Create_WithOnlyPinnedProbes_ReportsPinnedStatusWithoutHoveredDelta()
    {
        var pinnedProbe = new SurfaceProbeInfo(
            sampleX: 0.001234d,
            sampleY: 12.345d,
            axisX: 1234.567d,
            axisY: 987654d,
            value: 0.0001234d,
            isApproximate: false);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(
            hoveredProbe: null,
            [pinnedProbe],
            SurfaceChartNumericLabelPresets.Engineering(2));

        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.Pinned);
        evidence.HoveredProbeReadout.Should().BeNull();
        evidence.PinnedProbeCount.Should().Be(1);
        evidence.PinnedProbeReadouts[0].Should().Contain("X 1.23E+3 (sample 1.23E-3)");
        evidence.PinnedProbeReadouts[0].Should().Contain("Y 987.65E+3 (sample 12.35E+0)");
        evidence.PinnedProbeReadouts[0].Should().Contain("Value 123.40E-6");
        evidence.DeltaVsFirstPinReadout.Should().BeNull();
    }

    [Fact]
    public void Format_ProducesDeterministicEvidenceText()
    {
        var hoveredProbe = new SurfaceProbeInfo(
            sampleX: 1d,
            sampleY: 2d,
            axisX: 10d,
            axisY: 20d,
            value: 30d,
            isApproximate: false);
        var pinnedProbe = new SurfaceProbeInfo(
            sampleX: 0d,
            sampleY: 1d,
            axisX: 7d,
            axisY: 15d,
            value: 25d,
            isApproximate: false);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(
            hoveredProbe,
            [pinnedProbe],
            SurfaceChartNumericLabelPresets.Fixed(1));

        var formatted = SurfaceChartProbeEvidenceFormatter.Format(evidence);

        formatted.Should().Be(
            "EvidenceKind: surface-chart-probe\n" +
            "ProbeStatus: HoveredAndPinned\n" +
            "PinnedCount: 1\n" +
            "Hovered: X 10.0 (sample 1.0), Y 20.0 (sample 2.0), Value 30.0 Exact\n" +
            "Pinned: Pin 1 Exact\n" +
            "X 7.0 (sample 0.0)\n" +
            "Y 15.0 (sample 1.0)\n" +
            "Value 25.0\n" +
            "DeltaVsFirstPin: Delta vs Pin 1\n" +
            "X +3.0\n" +
            "Y +5.0\n" +
            "Value +5.0");
    }
}
