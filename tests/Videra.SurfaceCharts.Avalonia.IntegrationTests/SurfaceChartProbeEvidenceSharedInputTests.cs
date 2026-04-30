using FluentAssertions;
using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;
using Xunit;

namespace Videra.SurfaceCharts.Avalonia.IntegrationTests;

/// <summary>
/// Verifies that <see cref="SurfaceChartProbeEvidenceFormatter"/> accepts the shared
/// <see cref="SurfaceProbeInfo"/> shape used by surface, scatter, bar, and contour probes.
/// </summary>
public sealed class SurfaceChartProbeEvidenceSharedInputTests
{
    [Fact]
    public void SurfaceProbeInfo_CreatesValidEvidence()
    {
        var probe = CreateSurfaceProbe(sampleX: 32, sampleY: 24, axisX: 90, axisY: 12, value: 5.5);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.Should().NotBeNull();
        evidence.EvidenceKind.Should().Be("surface-chart-probe");
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.Hovered);
        evidence.HoveredProbeReadout.Should().Contain("90");
        evidence.HoveredProbeReadout.Should().Contain("12");
        evidence.HoveredProbeReadout.Should().Contain("5.5");
        evidence.PinnedProbeCount.Should().Be(0);
    }

    [Fact]
    public void ScatterProbeInfo_CreatesValidEvidence()
    {
        var probe = CreateSurfaceProbe(sampleX: 100, sampleY: 200, axisX: 15.5, axisY: 25.5, value: 42.0);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.Should().NotBeNull();
        evidence.EvidenceKind.Should().Be("surface-chart-probe");
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.Hovered);
        evidence.HoveredProbeReadout.Should().Contain("15.5");
        evidence.HoveredProbeReadout.Should().Contain("25.5");
        evidence.HoveredProbeReadout.Should().Contain("42");
    }

    [Fact]
    public void BarProbeInfo_CreatesValidEvidence()
    {
        var probe = CreateSurfaceProbe(sampleX: 50, sampleY: 50, axisX: 2, axisY: 0, value: 25.0);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.Should().NotBeNull();
        evidence.EvidenceKind.Should().Be("surface-chart-probe");
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.Hovered);
        evidence.HoveredProbeReadout.Should().Contain("2");
        evidence.HoveredProbeReadout.Should().Contain("25");
    }

    [Fact]
    public void ContourProbeInfo_CreatesValidEvidence()
    {
        var probe = CreateSurfaceProbe(sampleX: 80, sampleY: 60, axisX: 5.0, axisY: 3.0, value: 10.0);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.Should().NotBeNull();
        evidence.EvidenceKind.Should().Be("surface-chart-probe");
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.Hovered);
        evidence.HoveredProbeReadout.Should().Contain("5");
        evidence.HoveredProbeReadout.Should().Contain("3");
        evidence.HoveredProbeReadout.Should().Contain("10");
    }

    [Fact]
    public void MixedProbeTypes_HoveredAndPinned_CreatesValidEvidence()
    {
        var hoveredProbe = CreateSurfaceProbe(sampleX: 32, sampleY: 24, axisX: 90, axisY: 12, value: 5.5);
        var pinnedProbe1 = CreateSurfaceProbe(sampleX: 10, sampleY: 10, axisX: 30, axisY: 5, value: 2.0);
        var pinnedProbe2 = CreateSurfaceProbe(sampleX: 60, sampleY: 40, axisX: 150, axisY: 20, value: 8.0);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(hoveredProbe, [pinnedProbe1, pinnedProbe2]);

        evidence.Should().NotBeNull();
        evidence.ProbeStatus.Should().Be(SurfaceChartProbeEvidenceStatus.HoveredAndPinned);
        evidence.PinnedProbeCount.Should().Be(2);
        evidence.HoveredProbeReadout.Should().NotBeNullOrEmpty();
        evidence.PinnedProbeReadouts.Should().HaveCount(2);
        evidence.DeltaVsFirstPinReadout.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProbeEvidence_FormatOutput_IsDeterministic()
    {
        var probe = CreateSurfaceProbe(sampleX: 32, sampleY: 24, axisX: 90, axisY: 12, value: 5.5);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);
        var formatted1 = SurfaceChartProbeEvidenceFormatter.Format(evidence);
        var formatted2 = SurfaceChartProbeEvidenceFormatter.Format(evidence);

        formatted1.Should().Be(formatted2);
        formatted1.Should().Contain("EvidenceKind: surface-chart-probe");
        formatted1.Should().Contain("ProbeStatus: Hovered");
    }

    [Fact]
    public void ApproximateProbe_IncludesApproxIndicator()
    {
        var probe = CreateSurfaceProbe(sampleX: 32, sampleY: 24, axisX: 90, axisY: 12, value: 5.5, isApproximate: true);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.HoveredProbeReadout.Should().Contain("Approx");
    }

    [Fact]
    public void ExactProbe_IncludesExactIndicator()
    {
        var probe = CreateSurfaceProbe(sampleX: 32, sampleY: 24, axisX: 90, axisY: 12, value: 5.5, isApproximate: false);

        var evidence = SurfaceChartProbeEvidenceFormatter.Create(probe, []);

        evidence.HoveredProbeReadout.Should().Contain("Exact");
    }

    private static SurfaceProbeInfo CreateSurfaceProbe(
        double sampleX,
        double sampleY,
        double axisX,
        double axisY,
        double value,
        bool isApproximate = false)
    {
        return new SurfaceProbeInfo(
            sampleX,
            sampleY,
            axisX,
            axisY,
            value,
            isApproximate);
    }
}
