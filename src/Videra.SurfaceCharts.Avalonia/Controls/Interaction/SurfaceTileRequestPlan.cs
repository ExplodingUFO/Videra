using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Interaction;

internal sealed class SurfaceTileRequestPlan
{
    public SurfaceTileRequestPlan(
        IReadOnlyList<SurfaceTileKey> orderedKeys,
        IReadOnlySet<SurfaceTileKey> retainedKeys,
        bool includesOverview)
    {
        ArgumentNullException.ThrowIfNull(orderedKeys);
        ArgumentNullException.ThrowIfNull(retainedKeys);

        OrderedKeys = orderedKeys;
        RetainedKeys = retainedKeys;
        IncludesOverview = includesOverview;
    }

    public IReadOnlyList<SurfaceTileKey> OrderedKeys { get; }

    public IReadOnlySet<SurfaceTileKey> RetainedKeys { get; }

    public bool IncludesOverview { get; }

    public bool IsEquivalentTo(SurfaceTileRequestPlan? other)
    {
        if (other is null || IncludesOverview != other.IncludesOverview)
        {
            return false;
        }

        return OrderedKeys.SequenceEqual(other.OrderedKeys) &&
               RetainedKeys.SetEquals(other.RetainedKeys);
    }
}
