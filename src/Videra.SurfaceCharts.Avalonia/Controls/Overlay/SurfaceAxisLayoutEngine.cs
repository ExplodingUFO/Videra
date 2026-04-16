using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceAxisLayoutEngine
{
    public static IReadOnlyList<SurfaceAxisTickState> CullDenseLabels(IReadOnlyList<SurfaceAxisTickState> ticks)
    {
        ArgumentNullException.ThrowIfNull(ticks);

        if (ticks.Count <= 2)
        {
            return ticks;
        }

        List<SurfaceAxisTickState> kept = [ticks[0]];

        for (var index = 1; index < ticks.Count - 1; index++)
        {
            var candidate = ticks[index];
            var previous = kept[^1];
            var requiredSpacing = Math.Max(36d, Math.Max(previous.LabelText.Length, candidate.LabelText.Length) * 6d);
            if (Distance(previous.LabelPosition, candidate.LabelPosition) >= requiredSpacing)
            {
                kept.Add(candidate);
            }
        }

        var lastTick = ticks[^1];
        if (!ReferenceEquals(kept[^1], lastTick))
        {
            kept.Add(lastTick);
        }

        return kept;
    }

    private static double Distance(Point first, Point second)
    {
        var dx = second.X - first.X;
        var dy = second.Y - first.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
