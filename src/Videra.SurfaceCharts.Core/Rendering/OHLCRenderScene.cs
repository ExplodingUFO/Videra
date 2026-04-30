using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Represents a render-ready OHLC/Candlestick snapshot.
/// </summary>
public sealed class OHLCRenderScene
{
    private readonly ReadOnlyCollection<OHLCRenderBar> _barsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="OHLCRenderScene"/> class.
    /// </summary>
    public OHLCRenderScene(int barCount, OHLCStyle style, IReadOnlyList<OHLCRenderBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(barCount);

        BarCount = barCount;
        Style = style;
        _barsView = Array.AsReadOnly(bars.ToArray());
    }

    /// <summary>
    /// Gets the number of bars.
    /// </summary>
    public int BarCount { get; }

    /// <summary>
    /// Gets the rendering style.
    /// </summary>
    public OHLCStyle Style { get; }

    /// <summary>
    /// Gets the immutable render-ready bars.
    /// </summary>
    public IReadOnlyList<OHLCRenderBar> Bars => _barsView;
}
