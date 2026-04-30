using System.Numerics;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Builds an <see cref="OHLCRenderScene"/> from <see cref="OHLCData"/>.
/// </summary>
public static class OHLCRenderer
{
    /// <summary>
    /// Renders OHLC data into a render-ready scene.
    /// </summary>
    /// <param name="data">The OHLC data.</param>
    /// <param name="barWidth">The width of each bar in data units.</param>
    /// <returns>The render-ready scene.</returns>
    public static OHLCRenderScene Render(OHLCData data, float barWidth = 0.6f)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(barWidth);

        var bars = new OHLCRenderBar[data.BarCount];
        var halfWidth = barWidth / 2f;

        for (var i = 0; i < data.BarCount; i++)
        {
            var bar = data.Bars[i];
            var x = (float)bar.X;
            var open = (float)bar.Open;
            var high = (float)bar.High;
            var low = (float)bar.Low;
            var close = (float)bar.Close;
            var isBullish = close >= open;
            var color = isBullish ? data.UpColor : data.DownColor;

            var bodyBottom = MathF.Min(open, close);
            var bodyTop = MathF.Max(open, close);

            bars[i] = new OHLCRenderBar(
                BodyMin: new Vector3(x - halfWidth, bodyBottom, 0f),
                BodyMax: new Vector3(x + halfWidth, bodyTop, 0f),
                WickTop: new Vector3(x, high, 0f),
                WickBottom: new Vector3(x, low, 0f),
                Color: color,
                IsBullish: isBullish);
        }

        return new OHLCRenderScene(data.BarCount, data.Style, bars);
    }
}
