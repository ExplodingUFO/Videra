using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Builds a <see cref="HistogramRenderScene"/> from <see cref="HistogramData"/>.
/// </summary>
public static class HistogramRenderer
{
    /// <summary>
    /// Creates a render scene from histogram data.
    /// </summary>
    public static HistogramRenderScene Render(HistogramData data, uint color = 0xFF38BDF8u)
    {
        ArgumentNullException.ThrowIfNull(data);

        var bins = new List<HistogramRenderBin>(data.BinCount);
        var binWidth = data.BinWidth;
        var maxHeight = data.Bins.Count > 0 ? data.Bins.Max() : 1.0;
        if (maxHeight <= 0) maxHeight = 1.0;

        for (var i = 0; i < data.BinCount; i++)
        {
            var height = data.Bins[i] / maxHeight;
            var x = data.RangeMin + (i * binWidth);
            bins.Add(new HistogramRenderBin(
                new Vector3((float)x, 0, 0),
                new Vector3((float)binWidth, (float)height, (float)binWidth),
                color));
        }

        return new HistogramRenderScene(data.BinCount, data.Mode, bins);
    }
}
