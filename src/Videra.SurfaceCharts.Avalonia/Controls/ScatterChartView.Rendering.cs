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
        Dictionary<uint, Pen> penCache = [];
        List<ProjectedScatterDrawItem> drawItems = [];

        foreach (var series in scene.Series)
        {
            Point? previousProjected = null;
            double? previousSortKey = null;

            foreach (var point in series.Points)
            {
                if (TryProjectPoint(point, frame, out var projected))
                {
                    var screenPosition = new Point(projected.X, projected.Y);
                    drawItems.Add(
                        new ProjectedScatterDrawItem(
                            screenPosition,
                            point.Color,
                            point.Size,
                            projected.Z));

                    if (series.ConnectPoints && previousProjected is not null)
                    {
                        drawItems.Add(
                            new ProjectedScatterDrawItem(
                                previousProjected.Value,
                                screenPosition,
                                series.Color,
                                (previousSortKey!.Value + projected.Z) / 2d));
                    }

                    previousProjected = screenPosition;
                    previousSortKey = projected.Z;
                }
                else if (series.ConnectPoints)
                {
                    previousProjected = null;
                    previousSortKey = null;
                }
            }
        }

        drawItems.Sort(static (left, right) => right.SortKey.CompareTo(left.SortKey));

        foreach (var item in drawItems)
        {
            if (item.IsLine)
            {
                if (!penCache.TryGetValue(item.Color, out var pen))
                {
                    pen = new Pen(new SolidColorBrush(ToColor(item.Color)), 1d);
                    penCache.Add(item.Color, pen);
                }

                context.DrawLine(pen, item.StartPosition!.Value, item.EndPosition!.Value);
                continue;
            }

            if (!brushCache.TryGetValue(item.Color, out var brush))
            {
                brush = new SolidColorBrush(ToColor(item.Color));
                brushCache.Add(item.Color, brush);
            }

            var radius = (2.5d + ((1d - Math.Clamp(item.SortKey, 0d, 1d)) * 2.5d)) * item.Size;
            context.DrawEllipse(brush, null, item.ScreenPosition!.Value, radius, radius);
        }
    }

    private static bool TryProjectPoint(ScatterRenderPoint point, SurfaceCameraFrame frame, out Vector3 projected)
    {
        projected = SurfaceProjectionMath.ProjectToScreen(point.Position, frame);
        return float.IsFinite(projected.X) && float.IsFinite(projected.Y) && float.IsFinite(projected.Z);
    }

    private readonly record struct ProjectedScatterDrawItem
    {
        public ProjectedScatterDrawItem(Point screenPosition, uint color, double size, double sortKey)
        {
            ScreenPosition = screenPosition;
            Color = color;
            Size = size;
            SortKey = sortKey;
            StartPosition = null;
            EndPosition = null;
            IsLine = false;
        }

        public ProjectedScatterDrawItem(Point startPosition, Point endPosition, uint color, double sortKey)
        {
            ScreenPosition = default;
            Color = color;
            Size = 1d;
            SortKey = sortKey;
            StartPosition = startPosition;
            EndPosition = endPosition;
            IsLine = true;
        }

        public Point? ScreenPosition { get; }

        public uint Color { get; }

        public double Size { get; }

        public double SortKey { get; }

        public Point? StartPosition { get; }

        public Point? EndPosition { get; }

        public bool IsLine { get; }
    }

    private static Color ToColor(uint argb)
    {
        return Color.FromArgb(
            (byte)((argb >> 24) & 0xFFu),
            (byte)((argb >> 16) & 0xFFu),
            (byte)((argb >> 8) & 0xFFu),
            (byte)(argb & 0xFFu));
    }
}
