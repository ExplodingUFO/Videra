using System.Collections.ObjectModel;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Represents a render-ready pie-chart snapshot.
/// </summary>
public sealed class PieRenderScene
{
    private readonly ReadOnlyCollection<PieRenderSlice> _slicesView;

    /// <summary>
    /// Initializes a new instance of the <see cref="PieRenderScene"/> class.
    /// </summary>
    /// <param name="sliceCount">The number of slices.</param>
    /// <param name="holeRatio">The donut hole ratio.</param>
    /// <param name="slices">The render-ready slices.</param>
    public PieRenderScene(int sliceCount, double holeRatio, IReadOnlyList<PieRenderSlice> slices)
    {
        ArgumentNullException.ThrowIfNull(slices);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sliceCount);

        SliceCount = sliceCount;
        HoleRatio = holeRatio;
        _slicesView = Array.AsReadOnly(slices.ToArray());
    }

    /// <summary>
    /// Gets the number of slices.
    /// </summary>
    public int SliceCount { get; }

    /// <summary>
    /// Gets the donut hole ratio.
    /// </summary>
    public double HoleRatio { get; }

    /// <summary>
    /// Gets the immutable render-ready slices.
    /// </summary>
    public IReadOnlyList<PieRenderSlice> Slices => _slicesView;
}
