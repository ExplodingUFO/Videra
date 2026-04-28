using Avalonia.Controls;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Single Avalonia view entry point for 3D chart plotting.
/// </summary>
public sealed class VideraChartView : Control
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VideraChartView"/> class.
    /// </summary>
    public VideraChartView()
    {
        Plot = new Plot3D(OnPlotChanged);
        ClipToBounds = true;
        Focusable = true;
    }

    /// <summary>
    /// Gets the chart plot model used to add surface, waterfall, and scatter series.
    /// </summary>
    public Plot3D Plot { get; }

    /// <summary>
    /// Gets the latest plot revision rendered or requested through <see cref="Refresh"/>.
    /// </summary>
    public int LastRefreshRevision { get; private set; }

    /// <summary>
    /// Requests a visual refresh for the current plot model.
    /// </summary>
    public void Refresh()
    {
        LastRefreshRevision = Plot.Revision;
        InvalidateVisual();
    }

    private void OnPlotChanged()
    {
        Refresh();
    }
}
