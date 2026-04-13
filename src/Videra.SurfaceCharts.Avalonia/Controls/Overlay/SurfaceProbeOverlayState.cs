using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceProbeOverlayState
{
    public static SurfaceProbeOverlayState Empty { get; } = new(
        hasNoData: false,
        noDataText: null,
        readoutText: null,
        probeScreenPosition: null,
        probeResult: null);

    public SurfaceProbeOverlayState(
        bool hasNoData,
        string? noDataText,
        string? readoutText,
        Point? probeScreenPosition,
        SurfaceProbeResult? probeResult)
    {
        HasNoData = hasNoData;
        NoDataText = noDataText;
        ReadoutText = readoutText;
        ProbeScreenPosition = probeScreenPosition;
        ProbeResult = probeResult;
    }

    public bool HasNoData { get; }

    public string? NoDataText { get; }

    public string? ReadoutText { get; }

    public Point? ProbeScreenPosition { get; }

    public SurfaceProbeResult? ProbeResult { get; }
}
