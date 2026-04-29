using Videra.SurfaceCharts.Avalonia.Controls;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Represents a single series entry within a multi-series tooltip, pairing the series identity
/// with its resolved probe information at the hovered position.
/// </summary>
internal readonly record struct SurfaceTooltipSeriesEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTooltipSeriesEntry"/> struct.
    /// </summary>
    /// <param name="seriesName">The display name of the series, or a generated default if unnamed.</param>
    /// <param name="seriesKind">The chart family for this series.</param>
    /// <param name="probeInfo">The resolved probe information at the hovered position.</param>
    public SurfaceTooltipSeriesEntry(
        string seriesName,
        Plot3DSeriesKind seriesKind,
        SurfaceProbeInfo probeInfo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seriesName);

        SeriesName = seriesName;
        SeriesKind = seriesKind;
        ProbeInfo = probeInfo;
    }

    /// <summary>
    /// Gets the display name of the series.
    /// </summary>
    public string SeriesName { get; }

    /// <summary>
    /// Gets the chart family for this series.
    /// </summary>
    public Plot3DSeriesKind SeriesKind { get; }

    /// <summary>
    /// Gets the resolved probe information at the hovered position.
    /// </summary>
    public SurfaceProbeInfo ProbeInfo { get; }
}
