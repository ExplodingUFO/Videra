using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents a render-ready histogram snapshot.
/// </summary>
public sealed class HistogramRenderScene
{
    private readonly ReadOnlyCollection<HistogramRenderBin> _binsView;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistogramRenderScene"/> class.
    /// </summary>
    public HistogramRenderScene(int binCount, HistogramMode mode, IReadOnlyList<HistogramRenderBin> bins)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(binCount);
        ArgumentNullException.ThrowIfNull(bins);

        BinCount = binCount;
        Mode = mode;
        _binsView = Array.AsReadOnly(bins.ToArray());
    }

    /// <summary>
    /// Gets the number of bins.
    /// </summary>
    public int BinCount { get; }

    /// <summary>
    /// Gets the histogram mode.
    /// </summary>
    public HistogramMode Mode { get; }

    /// <summary>
    /// Gets the immutable render-ready bins.
    /// </summary>
    public IReadOnlyList<HistogramRenderBin> Bins => _binsView;
}
