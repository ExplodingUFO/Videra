using System.Numerics;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Render-ready representation of a single OHLC/Candlestick bar.
/// </summary>
public readonly record struct OHLCRenderBar(
    Vector3 BodyMin,
    Vector3 BodyMax,
    Vector3 WickTop,
    Vector3 WickBottom,
    uint Color,
    bool IsBullish);
