namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceAxisTickGenerator
{
    private static readonly double[] NiceTimeSteps =
    [
        1, 2, 5, 10, 15, 30,           // seconds
        60, 120, 300, 600, 1800, 3600,  // minutes/hours
        7200, 14400, 43200, 86400,      // hours/day
        172800, 604800, 2592000,        // days/weeks/months
        7776000, 15552000, 31536000,    // months/year
    ];

    public static IReadOnlyList<double> CreateLogTickValues(double axisMinimum, double axisMaximum, double axisLength)
    {
        if (axisMinimum <= 0d || axisMaximum <= axisMinimum)
        {
            return [axisMinimum, axisMaximum];
        }

        var logMin = Math.Log10(axisMinimum);
        var logMax = Math.Log10(axisMaximum);
        var logSpan = logMax - logMin;
        var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 72d), 2, 7);
        var logStep = ComputeNiceStep(logSpan / targetTickCount);
        var firstLogTick = Math.Ceiling(logMin / logStep) * logStep;
        var ticks = new List<double>();

        for (var logTick = firstLogTick; logTick <= logMax + (logStep * 0.5d); logTick += logStep)
        {
            var value = Math.Pow(10d, logTick);
            if (value >= axisMinimum && value <= axisMaximum)
            {
                ticks.Add(Math.Round(value, 12, MidpointRounding.AwayFromZero));
            }
        }

        if (ticks.Count == 0)
        {
            return [axisMinimum, axisMaximum];
        }

        return ticks;
    }

    public static IReadOnlyList<double> CreateDateTimeTickValues(double axisMinimum, double axisMaximum, double axisLength)
    {
        if (axisMaximum <= axisMinimum)
        {
            return [axisMinimum, axisMaximum];
        }

        var axisSpan = axisMaximum - axisMinimum;
        var targetTickCount = Math.Clamp((int)Math.Round(axisLength / 100d), 2, 7);
        var roughStep = axisSpan / targetTickCount;

        // Find the smallest nice step >= roughStep
        var step = NiceTimeSteps.FirstOrDefault(s => s >= roughStep);
        if (step == 0d) step = NiceTimeSteps[^1];

        var firstTick = Math.Ceiling(axisMinimum / step) * step;
        var ticks = new List<double>();

        for (var tick = firstTick; tick <= axisMaximum + (step * 0.5d); tick += step)
        {
            var rounded = Math.Round(tick, 6, MidpointRounding.AwayFromZero);
            if (rounded >= axisMinimum && rounded <= axisMaximum)
            {
                ticks.Add(rounded);
            }
        }

        if (ticks.Count == 0)
        {
            return [axisMinimum, axisMaximum];
        }

        return ticks;
    }

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
