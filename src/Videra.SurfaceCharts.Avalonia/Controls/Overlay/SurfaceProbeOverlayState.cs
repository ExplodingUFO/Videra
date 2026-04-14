using System.Globalization;
using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceProbeOverlayState
{
    public static SurfaceProbeOverlayState Empty { get; } = new(
        hasNoData: false,
        noDataText: null,
        hoveredProbeScreenPosition: null,
        hoveredProbe: null,
        pinnedProbes: []);

    public SurfaceProbeOverlayState(
        bool hasNoData,
        string? noDataText,
        Point? hoveredProbeScreenPosition,
        SurfaceProbeInfo? hoveredProbe,
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes)
    {
        ArgumentNullException.ThrowIfNull(pinnedProbes);

        HasNoData = hasNoData;
        NoDataText = noDataText;
        HoveredProbeScreenPosition = hoveredProbeScreenPosition;
        HoveredProbe = hoveredProbe;
        PinnedProbes = pinnedProbes;
    }

    public bool HasNoData { get; }

    public string? NoDataText { get; }

    public Point? HoveredProbeScreenPosition { get; }

    public SurfaceProbeInfo? HoveredProbe { get; }

    public IReadOnlyList<SurfaceProbeInfo> PinnedProbes { get; }

    // Kept as a convenience projection for existing integration coverage.
    public string? ReadoutText => HoveredProbe is SurfaceProbeInfo hoveredProbe
        ? CreateLegacyReadoutText(hoveredProbe)
        : null;

    // Kept as a convenience projection for existing integration coverage.
    public Point? ProbeScreenPosition => HoveredProbeScreenPosition;

    // Kept as a convenience projection for existing integration coverage.
    public SurfaceProbeResult? ProbeResult => HoveredProbe is SurfaceProbeInfo hoveredProbe
        ? new SurfaceProbeResult(hoveredProbe.SampleX, hoveredProbe.SampleY, hoveredProbe.Value)
        : null;

    private static string CreateLegacyReadoutText(SurfaceProbeInfo probe)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"X {probe.AxisX:0.###} (sample {probe.SampleX:0.###}), Y {probe.AxisY:0.###} (sample {probe.SampleY:0.###}), Value {probe.Value:0.###} {(probe.IsApproximate ? "Approx" : "Exact")}");
    }
}
