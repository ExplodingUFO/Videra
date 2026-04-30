using System.Numerics;

namespace Videra.SurfaceCharts.Core.Rendering;

/// <summary>
/// Builds a <see cref="PieRenderScene"/> from <see cref="PieChartData"/>.
/// </summary>
public static class PieRenderer
{
    /// <summary>
    /// Renders pie data into a render-ready scene.
    /// </summary>
    /// <param name="data">The pie chart data.</param>
    /// <param name="radius">The outer radius of the pie.</param>
    /// <returns>The render-ready scene.</returns>
    public static PieRenderScene Render(PieChartData data, float radius = 1f)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);

        var slices = new PieRenderSlice[data.SliceCount];
        var currentAngle = 0f;
        var total = data.TotalValue > 0d ? data.TotalValue : 1d;
        var innerRadius = (float)data.HoleRatio * radius;

        for (var i = 0; i < data.SliceCount; i++)
        {
            var slice = data.Slices[i];
            var fraction = (float)(slice.Value / total);
            var sweep = fraction * 360f;
            var midAngle = currentAngle + (sweep / 2f);
            var midAngleRad = midAngle * MathF.PI / 180f;
            var explodeDist = (float)slice.ExplodeOffset * radius;

            slices[i] = new PieRenderSlice(
                Center: new Vector3(
                    MathF.Cos(midAngleRad) * explodeDist,
                    0f,
                    MathF.Sin(midAngleRad) * explodeDist),
                InnerRadius: innerRadius,
                OuterRadius: radius,
                StartAngle: currentAngle,
                SweepAngle: sweep,
                ExplodeDistance: explodeDist,
                Color: slice.Color);

            currentAngle += sweep;
        }

        return new PieRenderScene(data.SliceCount, data.HoleRatio, slices);
    }
}
