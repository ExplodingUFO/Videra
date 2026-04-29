using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

/// <summary>
/// Aggregated tooltip content for multi-series awareness. Holds the shared world coordinates
/// and a list of per-series probe entries resolved at the same X/Z position.
/// </summary>
internal sealed class SurfaceTooltipContent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceTooltipContent"/> class.
    /// </summary>
    /// <param name="worldX">The shared horizontal axis coordinate for all series entries.</param>
    /// <param name="worldZ">The shared depth axis coordinate for all series entries.</param>
    /// <param name="isApproximate">Whether any series probe came from a coarse tile.</param>
    /// <param name="entries">The per-series tooltip entries.</param>
    public SurfaceTooltipContent(
        double worldX,
        double worldZ,
        bool isApproximate,
        IReadOnlyList<SurfaceTooltipSeriesEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        WorldX = worldX;
        WorldZ = worldZ;
        IsApproximate = isApproximate;
        Entries = entries;
    }

    /// <summary>
    /// Gets the shared horizontal axis coordinate for all series entries.
    /// </summary>
    public double WorldX { get; }

    /// <summary>
    /// Gets the shared depth axis coordinate for all series entries.
    /// </summary>
    public double WorldZ { get; }

    /// <summary>
    /// Gets a value indicating whether any series probe came from a coarse tile approximation.
    /// </summary>
    public bool IsApproximate { get; }

    /// <summary>
    /// Gets the per-series tooltip entries resolved at the hovered position.
    /// </summary>
    public IReadOnlyList<SurfaceTooltipSeriesEntry> Entries { get; }

    /// <summary>
    /// Creates a <see cref="SurfaceTooltipContent"/> from a list of resolved series probes.
    /// </summary>
    /// <param name="entries">The per-series tooltip entries.</param>
    /// <returns>A new tooltip content instance, or null if entries is empty.</returns>
    public static SurfaceTooltipContent? FromSeriesProbes(IReadOnlyList<SurfaceTooltipSeriesEntry> entries)
    {
        if (entries is null || entries.Count == 0)
        {
            return null;
        }

        var first = entries[0].ProbeInfo;
        var isApproximate = false;
        foreach (var entry in entries)
        {
            if (entry.ProbeInfo.IsApproximate)
            {
                isApproximate = true;
                break;
            }
        }

        return new SurfaceTooltipContent(
            first.AxisX,
            first.AxisY,
            isApproximate,
            entries);
    }
}
