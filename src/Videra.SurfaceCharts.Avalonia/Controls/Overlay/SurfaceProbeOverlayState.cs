using Avalonia;
using Videra.SurfaceCharts.Avalonia.Controls;
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
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes,
        SurfaceChartOverlayOptions? overlayOptions = null)
    {
        ArgumentNullException.ThrowIfNull(pinnedProbes);

        HasNoData = hasNoData;
        NoDataText = noDataText;
        HoveredProbeScreenPosition = hoveredProbeScreenPosition;
        HoveredProbe = hoveredProbe;
        PinnedProbes = pinnedProbes;
        OverlayOptions = overlayOptions ?? SurfaceChartOverlayOptions.Default;
    }

    public bool HasNoData { get; }

    public string? NoDataText { get; }

    public Point? HoveredProbeScreenPosition { get; }

    public SurfaceProbeInfo? HoveredProbe { get; }

    public IReadOnlyList<SurfaceProbeInfo> PinnedProbes { get; }

    internal SurfaceChartOverlayOptions OverlayOptions { get; }

    // Kept as a convenience projection for existing integration coverage.
    public string? ReadoutText => HoveredProbe is SurfaceProbeInfo hoveredProbe
        ? CreateReadoutText(hoveredProbe, PinnedProbes, OverlayOptions)
        : null;

    // Kept as a convenience projection for existing integration coverage.
    public Point? ProbeScreenPosition => HoveredProbeScreenPosition;

    // Kept as a convenience projection for existing integration coverage.
    public SurfaceProbeResult? ProbeResult => HoveredProbe is SurfaceProbeInfo hoveredProbe
        ? new SurfaceProbeResult(hoveredProbe.SampleX, hoveredProbe.SampleY, hoveredProbe.Value)
        : null;

    private static string CreateReadoutText(
        SurfaceProbeInfo probe,
        IReadOnlyList<SurfaceProbeInfo> pinnedProbes,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var readout = $"X {overlayOptions.FormatProbeAxisX(probe.AxisX)} (sample {overlayOptions.FormatProbeAxisX(probe.SampleX)}), " +
            $"Y {overlayOptions.FormatProbeAxisY(probe.AxisY)} (sample {overlayOptions.FormatProbeAxisY(probe.SampleY)}), " +
            $"Value {overlayOptions.FormatProbeValue(probe.Value)} {(probe.IsApproximate ? "Approx" : "Exact")}";

        if (pinnedProbes.Count == 0)
        {
            return readout;
        }

        var referenceProbe = pinnedProbes[0];
        var deltaX = probe.AxisX - referenceProbe.AxisX;
        var deltaY = probe.AxisY - referenceProbe.AxisY;
        var deltaValue = probe.Value - referenceProbe.Value;
        return $"{readout}\nDelta vs Pin 1\nX {overlayOptions.FormatProbeDelta("X", deltaX)}\nY {overlayOptions.FormatProbeDelta("Z", deltaY)}\nValue {overlayOptions.FormatProbeDelta("Y", deltaValue)}";
    }
}
