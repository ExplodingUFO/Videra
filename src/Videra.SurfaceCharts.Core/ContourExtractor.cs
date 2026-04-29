namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Orchestrates extraction of all contour levels from a contour dataset.
/// </summary>
public static class ContourExtractor
{
    /// <summary>
    /// Extracts all contour lines from the specified contour dataset.
    /// </summary>
    /// <param name="data">The contour dataset.</param>
    /// <returns>The extracted contour lines, one per iso-level.</returns>
    public static IReadOnlyList<ContourLine> ExtractAll(ContourChartData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var field = data.Field;
        var range = field.Range;
        var levelCount = data.LevelCount;

        // If range is zero (flat field), return empty
        if (Math.Abs(range.Maximum - range.Minimum) < 1e-10f)
        {
            return Array.Empty<ContourLine>();
        }

        var lines = new List<ContourLine>(levelCount);
        var step = (float)(range.Maximum - range.Minimum) / (levelCount + 1);

        for (var i = 1; i <= levelCount; i++)
        {
            var isoValue = (float)range.Minimum + step * i;
            var segments = MarchingSquaresExtractor.Extract(field, isoValue, data.Mask);

            if (segments.Count > 0)
            {
                lines.Add(new ContourLine(isoValue, segments));
            }
        }

        return lines;
    }
}
