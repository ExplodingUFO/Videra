namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceAxisTickGenerator
{
    public static IReadOnlyList<double> CreateMajorTickValues(double axisMinimum, double axisMaximum, double axisLength)
    {
        if (axisMaximum <= axisMinimum)
        {
            return [axisMinimum];
        }

        var axisSpan = axisMaximum - axisMinimum;
        var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 72d), 2, 7);
        var step = ComputeNiceStep(axisSpan / targetTickCount);
        var firstTick = Math.Ceiling(axisMinimum / step) * step;
        var ticks = new List<double>();

        for (var tick = firstTick; tick <= axisMaximum + (step * 0.5d); tick += step)
        {
            ticks.Add(Math.Round(tick, 12, MidpointRounding.AwayFromZero));
        }

        if (ticks.Count == 0)
        {
            return [axisMinimum, axisMaximum];
        }

        return ticks;
    }

    public static IReadOnlyList<double> CreateMinorTickValues(IReadOnlyList<double> majorTickValues, int minorTickDivisions)
    {
        ArgumentNullException.ThrowIfNull(majorTickValues);

        if (majorTickValues.Count < 2 || minorTickDivisions <= 1)
        {
            return Array.Empty<double>();
        }

        List<double> ticks = [];
        for (var index = 0; index < majorTickValues.Count - 1; index++)
        {
            var start = majorTickValues[index];
            var end = majorTickValues[index + 1];
            var step = (end - start) / minorTickDivisions;

            for (var division = 1; division < minorTickDivisions; division++)
            {
                ticks.Add(Math.Round(start + (step * division), 12, MidpointRounding.AwayFromZero));
            }
        }

        return ticks;
    }

    private static double ComputeNiceStep(double roughStep)
    {
        roughStep = Math.Max(roughStep, double.Epsilon);

        var exponent = Math.Pow(10d, Math.Floor(Math.Log10(roughStep)));
        foreach (var factor in new[] { 1d, 2d, 2.5d, 5d, 10d })
        {
            var candidate = factor * exponent;
            if (candidate >= roughStep)
            {
                return candidate;
            }
        }

        return exponent * 10d;
    }
}
