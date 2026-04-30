using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents the rendering style for OHLC data.
/// </summary>
public enum OHLCStyle
{
    /// <summary>
    /// Candlestick style with filled bodies.
    /// </summary>
    Candlestick,

    /// <summary>
    /// OHLC style with tick marks for open/close.
    /// </summary>
    OHLC,
}

/// <summary>
/// Represents one immutable OHLC/Candlestick dataset.
/// </summary>
public sealed class OHLCData
{
    private readonly ReadOnlyCollection<OHLCBar> _barsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="OHLCData"/> class.
    /// </summary>
    /// <param name="bars">The OHLC bars. Must not be empty.</param>
    /// <param name="style">The rendering style.</param>
    /// <param name="upColor">The color for bullish bars (close &gt;= open).</param>
    /// <param name="downColor">The color for bearish bars (close &lt; open).</param>
    public OHLCData(
        IReadOnlyList<OHLCBar> bars,
        OHLCStyle style = OHLCStyle.Candlestick,
        uint upColor = 0xFF2DD4BFu,
        uint downColor = 0xFFFF6B6Bu)
    {
        ArgumentNullException.ThrowIfNull(bars);

        if (bars.Count == 0)
        {
            throw new ArgumentException("OHLC data must contain at least one bar.", nameof(bars));
        }

        _barsView = Array.AsReadOnly(bars.ToArray());
        Style = style;
        UpColor = upColor;
        DownColor = downColor;

        var minLow = double.MaxValue;
        var maxHigh = double.MinValue;
        foreach (var bar in bars)
        {
            if (bar.Low < minLow) minLow = bar.Low;
            if (bar.High > maxHigh) maxHigh = bar.High;
        }
        MinLow = minLow;
        MaxHigh = maxHigh;
    }

    /// <summary>
    /// Gets the immutable OHLC bars.
    /// </summary>
    public IReadOnlyList<OHLCBar> Bars => _barsView;

    /// <summary>
    /// Gets the number of bars.
    /// </summary>
    public int BarCount => _barsView.Count;

    /// <summary>
    /// Gets the rendering style.
    /// </summary>
    public OHLCStyle Style { get; }

    /// <summary>
    /// Gets the color for bullish bars (close &gt;= open).
    /// </summary>
    public uint UpColor { get; }

    /// <summary>
    /// Gets the color for bearish bars (close &lt; open).
    /// </summary>
    public uint DownColor { get; }

    /// <summary>
    /// Gets the minimum low value across all bars.
    /// </summary>
    public double MinLow { get; }

    /// <summary>
    /// Gets the maximum high value across all bars.
    /// </summary>
    public double MaxHigh { get; }
}

/// <summary>
/// Represents a single OHLC bar.
/// </summary>
public readonly record struct OHLCBar
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OHLCBar"/> struct.
    /// </summary>
    /// <param name="open">The opening price.</param>
    /// <param name="high">The high price.</param>
    /// <param name="low">The low price.</param>
    /// <param name="close">The closing price.</param>
    /// <param name="x">The optional X position (e.g., timestamp index).</param>
    public OHLCBar(double open, double high, double low, double close, double x = 0d)
    {
        Open = open;
        High = high;
        Low = low;
        Close = close;
        X = x;
    }

    /// <summary>
    /// Gets the opening price.
    /// </summary>
    public double Open { get; }

    /// <summary>
    /// Gets the high price.
    /// </summary>
    public double High { get; }

    /// <summary>
    /// Gets the low price.
    /// </summary>
    public double Low { get; }

    /// <summary>
    /// Gets the closing price.
    /// </summary>
    public double Close { get; }

    /// <summary>
    /// Gets the X position.
    /// </summary>
    public double X { get; }
}
