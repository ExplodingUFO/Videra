using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Stateless renderer that builds a heatmap slice render scene from chart data.
/// </summary>
public static class HeatmapSliceRenderer
{
    public static HeatmapSliceRenderScene BuildScene(HeatmapSliceData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        var field = data.Field;
        var cells = new List<HeatmapSliceRenderCell>(field.Width * field.Height);
        var cellWidth = field.Width > 1 ? 1f / (field.Width - 1) : 1f;
        var cellHeight = field.Height > 1 ? 1f / (field.Height - 1) : 1f;
        var colorMap = data.ColorMap;
        var values = field.Values.Span;

        for (var y = 0; y < field.Height; y++)
        {
            for (var x = 0; x < field.Width; x++)
            {
                var value = (double)values[y * field.Width + x];
                var color = colorMap is not null
                    ? colorMap.Map(value)
                    : MapValueToColor(value, field.Range);

                var worldX = (float)x / Math.Max(1, field.Width - 1);
                var worldZ = (float)y / Math.Max(1, field.Height - 1);
                var worldY = (float)data.Position;

                var position = data.Axis switch
                {
                    HeatmapSliceAxis.X => new Vector3(worldY, worldX, worldZ),
                    HeatmapSliceAxis.Y => new Vector3(worldX, worldY, worldZ),
                    HeatmapSliceAxis.Z => new Vector3(worldX, worldZ, worldY),
                    _ => new Vector3(worldX, worldY, worldZ),
                };

                cells.Add(new HeatmapSliceRenderCell(position, new Vector2(cellWidth, cellHeight), color));
            }
        }

        return new HeatmapSliceRenderScene(cells.Count, data.Axis, data.Position, cells);
    }

    private static uint MapValueToColor(double value, SurfaceValueRange range)
    {
        var t = range.Span > 0d ? (value - range.Minimum) / range.Span : 0d;
        t = Math.Clamp(t, 0d, 1d);
        var r = (byte)(t * 255);
        var b = (byte)((1d - t) * 255);
        return (uint)(0xFF000000 | ((uint)r << 16) | ((uint)0 << 8) | (uint)b);
    }
}
