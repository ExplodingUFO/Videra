using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

/// <summary>
/// Avalonia proof control for the thin Waterfall chart path on top of the existing chart shell.
/// </summary>
public sealed class WaterfallChartView : SurfaceChartView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WaterfallChartView"/> class.
    /// </summary>
    public WaterfallChartView()
        : this(nativeHostFactory: null)
    {
    }

    internal WaterfallChartView(ISurfaceChartNativeHostFactory? nativeHostFactory)
        : base(CreateRenderHost(), nativeHostFactory)
    {
    }

    private static SurfaceChartRenderHost CreateRenderHost()
    {
        return new SurfaceChartRenderHost(
            new SurfaceChartRenderState(
                new WaterfallSurfaceRenderer()));
    }
}
