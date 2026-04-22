using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls;

public sealed partial class ScatterChartView
{
    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        base.Render(context);

        var scene = RenderScene;
        if (scene is null || scene.Series.Count == 0)
        {
            return;
        }

        var frame = TryCreateCameraFrame();
        if (frame is null)
        {
            return;
        }

        DrawScene(context, scene, frame.Value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arrangedSize = base.ArrangeOverride(finalSize);
        UpdateRenderingStatus();
        return arrangedSize;
    }

    private SurfaceCameraFrame? TryCreateCameraFrame()
    {
        if (RenderScene is null)
        {
            return null;
        }

        var viewSize = Bounds.Size;
        if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return null;
        }

        return SurfaceProjectionMath.CreateCameraFrame(_dataBounds, _camera, viewSize.Width, viewSize.Height, (float)(VisualRoot?.RenderScaling ?? 1d));
    }

    private static void DrawScene(DrawingContext context, ScatterRenderScene scene, SurfaceCameraFrame frame)
    {
        Dictionary<uint, IBrush> brushCache = [];
        List<ProjectedScatterPoint> projectedPoints = [];

        foreach (var series in scene.Series)
        {
            foreach (var point in series.Points)
            {
                var projected = SurfaceProjectionMath.ProjectToScreen(
                    point.Position,
                    frame);

                if (!float.IsFinite(projected.X) || !float.IsFinite(projected.Y) || !float.IsFinite(projected.Z))
                {
                    continue;
                }

                projectedPoints.Add(
                    new ProjectedScatterPoint(
                        new Point(projected.X, projected.Y),
                        point.Color,
                        projected.Z));
            }
        }

        projectedPoints.Sort(static (left, right) => right.SortKey.CompareTo(left.SortKey));

        foreach (var point in projectedPoints)
        {
            if (!brushCache.TryGetValue(point.Color, out var brush))
            {
                brush = new SolidColorBrush(ToColor(point.Color));
                brushCache.Add(point.Color, brush);
            }

            var radius = 2.5d + ((1d - Math.Clamp(point.SortKey, 0d, 1d)) * 2.5d);
            context.DrawEllipse(brush, null, point.ScreenPosition, radius, radius);
        }
    }

    private readonly record struct ProjectedScatterPoint(Point ScreenPosition, uint Color, double SortKey);

    private static Color ToColor(uint argb)
    {
        return Color.FromArgb(
            (byte)((argb >> 24) & 0xFFu),
            (byte)((argb >> 16) & 0xFFu),
            (byte)((argb >> 8) & 0xFFu),
            (byte)(argb & 0xFFu));
    }
}
