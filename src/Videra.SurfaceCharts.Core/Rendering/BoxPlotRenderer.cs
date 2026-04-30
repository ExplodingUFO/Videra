using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Stateless renderer that builds a box plot render scene from chart data.
/// </summary>
public static class BoxPlotRenderer
{
    public static BoxPlotRenderScene BuildScene(BoxPlotData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var boxes = new List<BoxPlotRenderBox>(data.CategoryCount);
        var whiskers = new List<BoxPlotRenderWhisker>(data.CategoryCount * 2);
        var outliers = new List<BoxPlotRenderOutlier>();

        for (var i = 0; i < data.CategoryCount; i++)
        {
            var cat = data.Categories[i];
            var x = (float)i;
            var boxWidth = 0.6f;
            var boxDepth = 0.4f;

            // Box: IQR rectangle from Q1 to Q3
            boxes.Add(new BoxPlotRenderBox(
                new Vector3(x, (float)cat.Q1, 0f),
                new Vector3(boxWidth, (float)(cat.Q3 - cat.Q1), boxDepth),
                cat.Color));

            // Lower whisker: Min to Q1
            whiskers.Add(new BoxPlotRenderWhisker(
                new Vector3(x, (float)cat.Min, 0f),
                new Vector3(x, (float)cat.Q1, 0f),
                cat.Color));

            // Upper whisker: Q3 to Max
            whiskers.Add(new BoxPlotRenderWhisker(
                new Vector3(x, (float)cat.Q3, 0f),
                new Vector3(x, (float)cat.Max, 0f),
                cat.Color));

            // Median line (rendered as a whisker at median height)
            whiskers.Add(new BoxPlotRenderWhisker(
                new Vector3(x - boxWidth / 2, (float)cat.Median, 0f),
                new Vector3(x + boxWidth / 2, (float)cat.Median, 0f),
                0xFFFFFFFFu));

            // Outliers
            foreach (var outlier in cat.Outliers)
            {
                outliers.Add(new BoxPlotRenderOutlier(
                    new Vector3(x, (float)outlier, 0f),
                    cat.Color));
            }
        }

        return new BoxPlotRenderScene(data.CategoryCount, boxes, whiskers, outliers);
    }
}
