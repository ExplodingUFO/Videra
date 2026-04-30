using System.Collections.ObjectModel;
using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// A render-ready cell in a heatmap slice.
/// </summary>
public readonly record struct HeatmapSliceRenderCell(Vector3 Position, Vector2 Size, uint Color);

/// <summary>
/// Represents a render-ready heatmap-slice snapshot containing colored cells.
/// </summary>
public sealed class HeatmapSliceRenderScene
{
    private readonly ReadOnlyCollection<HeatmapSliceRenderCell> _cellsView;

    public HeatmapSliceRenderScene(
        int cellCount,
        HeatmapSliceAxis axis,
        double position,
        IReadOnlyList<HeatmapSliceRenderCell> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cellCount);
        CellCount = cellCount;
        Axis = axis;
        Position = position;
        _cellsView = Array.AsReadOnly(cells.ToArray());
    }

    public int CellCount { get; }
    public HeatmapSliceAxis Axis { get; }
    public double Position { get; }
    public IReadOnlyList<HeatmapSliceRenderCell> Cells => _cellsView;
}
