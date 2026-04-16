using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Core;
using Videra.SurfaceCharts.Core.Rendering;
using Videra.SurfaceCharts.Rendering;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceChartProjection
{
    private readonly SurfaceCameraFrame _cameraFrame;

    private SurfaceChartProjection(
        SurfaceCameraFrame cameraFrame,
        Rect screenBounds)
    {
        _cameraFrame = cameraFrame;
        ViewSize = new Size(cameraFrame.ViewportPixels.X, cameraFrame.ViewportPixels.Y);
        ScreenBounds = screenBounds;
    }

    public Size ViewSize { get; }

    public Rect ScreenBounds { get; }

    public SurfaceChartProjectionSettings Settings => _cameraFrame.ProjectionSettings;

    public static SurfaceChartProjection? Create(
        SurfaceRenderScene? scene,
        Size viewSize,
        IEnumerable<Vector3>? anchorPoints = null,
        SurfaceChartProjectionSettings? settings = null)
    {
        if (viewSize.Width <= 0d || viewSize.Height <= 0d)
        {
            return null;
        }

        List<Vector3> modelPoints = [];

        if (scene is not null)
        {
            foreach (var tile in scene.Tiles)
            {
                for (var index = 0; index < tile.Vertices.Count; index++)
                {
                    modelPoints.Add(tile.Vertices[index].Position);
                }
            }
        }

        if (anchorPoints is not null)
        {
            modelPoints.AddRange(anchorPoints);
        }

        if (modelPoints.Count == 0)
        {
            return null;
        }

        var plotBounds = CreateBoundsFromPoints(modelPoints);
        var resolvedSettings = settings ?? SurfaceChartProjectionSettings.Default;
        var diagonal = plotBounds.Size.Length();
        var defaultCamera = new SurfaceCameraPose(
            plotBounds.Center,
            resolvedSettings.YawDegrees,
            resolvedSettings.PitchDegrees,
            Math.Max((diagonal * 0.5d) / Math.Tan((SurfaceCameraPose.DefaultFieldOfViewDegrees * (Math.PI / 180d)) * 0.5d), 1d),
            SurfaceCameraPose.DefaultFieldOfViewDegrees);
        var cameraFrame = SurfaceProjectionMath.CreateCameraFrame(plotBounds, defaultCamera, viewSize.Width, viewSize.Height, 1f);
        return Create(modelPoints, cameraFrame);
    }

    public static SurfaceChartProjection? Create(
        SurfaceRenderScene? scene,
        SurfaceCameraFrame cameraFrame,
        IEnumerable<Vector3>? anchorPoints = null)
    {
        List<Vector3> modelPoints = [];

        if (scene is not null)
        {
            foreach (var tile in scene.Tiles)
            {
                for (var index = 0; index < tile.Vertices.Count; index++)
                {
                    modelPoints.Add(tile.Vertices[index].Position);
                }
            }
        }

        if (anchorPoints is not null)
        {
            modelPoints.AddRange(anchorPoints);
        }

        return Create(modelPoints, cameraFrame);
    }

    public SurfaceChartProjectedPoint ProjectPoint(Vector3 position)
    {
        var screen = SurfaceProjectionMath.ProjectToScreen(position, _cameraFrame);
        return new SurfaceChartProjectedPoint(
            new Point(screen.X, screen.Y),
            -Vector3.Distance(position, _cameraFrame.Position));
    }

    public Point Project(Vector3 position)
    {
        return ProjectPoint(position).ScreenPoint;
    }

    public Point ProjectCenter(SurfaceMetadata metadata, SurfaceValueRange valueRange)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        return Project(
            new Vector3(
                (float)((metadata.HorizontalAxis.Minimum + metadata.HorizontalAxis.Maximum) * 0.5d),
                (float)((valueRange.Minimum + valueRange.Maximum) * 0.5d),
                (float)((metadata.VerticalAxis.Minimum + metadata.VerticalAxis.Maximum) * 0.5d)));
    }

    public static IReadOnlyList<Vector3> CreateChartBoundsPoints(SurfaceMetadata metadata, SurfaceValueRange valueRange)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var xValues = new[] { (float)metadata.HorizontalAxis.Minimum, (float)metadata.HorizontalAxis.Maximum };
        var yValues = new[] { (float)valueRange.Minimum, (float)valueRange.Maximum };
        var zValues = new[] { (float)metadata.VerticalAxis.Minimum, (float)metadata.VerticalAxis.Maximum };

        List<Vector3> points = new(capacity: 8);

        foreach (var x in xValues)
        {
            foreach (var y in yValues)
            {
                foreach (var z in zValues)
                {
                    points.Add(new Vector3(x, y, z));
                }
            }
        }

        return points;
    }

    private static SurfaceChartProjection? Create(IReadOnlyList<Vector3> modelPoints, SurfaceCameraFrame cameraFrame)
    {
        if (modelPoints.Count == 0)
        {
            return null;
        }

        var minX = double.PositiveInfinity;
        var minY = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var maxY = double.NegativeInfinity;

        foreach (var point in modelPoints)
        {
            var projected = SurfaceProjectionMath.ProjectToScreen(point, cameraFrame);
            minX = Math.Min(minX, projected.X);
            minY = Math.Min(minY, projected.Y);
            maxX = Math.Max(maxX, projected.X);
            maxY = Math.Max(maxY, projected.Y);
        }

        return new SurfaceChartProjection(
            cameraFrame,
            new Rect(minX, minY, Math.Max(maxX - minX, 0d), Math.Max(maxY - minY, 0d)));
    }

    private static SurfacePlotBounds CreateBoundsFromPoints(IReadOnlyList<Vector3> modelPoints)
    {
        var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        foreach (var point in modelPoints)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        return new SurfacePlotBounds(min, max);
    }
}

internal readonly record struct SurfaceChartProjectedPoint(Point ScreenPoint, double SortKey);
