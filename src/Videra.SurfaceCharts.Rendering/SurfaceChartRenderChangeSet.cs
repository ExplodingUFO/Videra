using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

public sealed class SurfaceChartRenderChangeSet
{
    public bool FullResetRequired { get; init; }

    public bool ResidencyDirty { get; init; }

    public bool ColorDirty { get; init; }

    public bool ProjectionDirty { get; init; }

    public IReadOnlyList<SurfaceTileKey> AddedResidentKeys { get; init; } = Array.Empty<SurfaceTileKey>();

    public IReadOnlyList<SurfaceTileKey> RemovedResidentKeys { get; init; } = Array.Empty<SurfaceTileKey>();

    public IReadOnlyList<SurfaceTileKey> ColorChangedKeys { get; init; } = Array.Empty<SurfaceTileKey>();
}
