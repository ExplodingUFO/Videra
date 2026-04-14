using System.Numerics;
using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceChartProjection
{
    private readonly Vector3 _modelCenter;
    private readonly SurfaceChartProjectionSettings _settings;
    private readonly double _scale;
    private readonly double _offsetX;
    private readonly double _offsetY;

    private SurfaceChartProjection(
        Vector3 modelCenter,
        SurfaceChartProjectionSettings settings,
        Size viewSize,
        double scale,
        double offsetX,
        double offsetY,
        Rect screenBounds)
    {
        _modelCenter = modelCenter;
        _settings = settings;
        ViewSize = viewSize;
        _scale = scale;
        _offsetX = offsetX;
        _offsetY = offsetY;
        ScreenBounds = screenBounds;
    }

    public Size ViewSize { get; }

    public Rect ScreenBounds { get; }

    public SurfaceChartProjectionSettings Settings => _settings;

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

        var resolvedSettings = settings ?? SurfaceChartProjectionSettings.Default;
        var modelCenter = GetModelCenter(modelPoints);
        var rawBounds = GetRawBounds(modelPoints, modelCenter, resolvedSettings);
        var transform = CreateTransform(viewSize, rawBounds);
        var screenBounds = new Rect(
            transform.OffsetX + (rawBounds.MinX * transform.Scale),
            transform.OffsetY + (rawBounds.MinY * transform.Scale),
            Math.Max((rawBounds.MaxX - rawBounds.MinX) * transform.Scale, 0d),
            Math.Max((rawBounds.MaxY - rawBounds.MinY) * transform.Scale, 0d));

        return new SurfaceChartProjection(
            modelCenter,
            resolvedSettings,
            viewSize,
            transform.Scale,
            transform.OffsetX,
            transform.OffsetY,
            screenBounds);
    }

    public SurfaceChartProjectedPoint ProjectPoint(Vector3 position)
    {
        var raw = ProjectRaw(position);
        return new SurfaceChartProjectedPoint(
            new Point(
                x: _offsetX + (raw.X * _scale),
                y: _offsetY + (raw.Y * _scale)),
            raw.Y);
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

    private static Vector3 GetModelCenter(IReadOnlyList<Vector3> modelPoints)
    {
        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var minY = float.PositiveInfinity;
        var maxY = float.NegativeInfinity;
        var minZ = float.PositiveInfinity;
        var maxZ = float.NegativeInfinity;

        foreach (var point in modelPoints)
        {
            minX = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            minY = Math.Min(minY, point.Y);
            maxY = Math.Max(maxY, point.Y);
            minZ = Math.Min(minZ, point.Z);
            maxZ = Math.Max(maxZ, point.Z);
        }

        return new Vector3(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f,
            (minZ + maxZ) * 0.5f);
    }

    private static RawBounds GetRawBounds(
        IReadOnlyList<Vector3> modelPoints,
        Vector3 modelCenter,
        SurfaceChartProjectionSettings settings)
    {
        var minX = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var minY = double.PositiveInfinity;
        var maxY = double.NegativeInfinity;

        foreach (var point in modelPoints)
        {
            var raw = ProjectRaw(point, modelCenter, settings);
            minX = Math.Min(minX, raw.X);
            maxX = Math.Max(maxX, raw.X);
            minY = Math.Min(minY, raw.Y);
            maxY = Math.Max(maxY, raw.Y);
        }

        return new RawBounds(minX, maxX, minY, maxY);
    }

    private RawProjection ProjectRaw(Vector3 position)
    {
        return ProjectRaw(position, _modelCenter, _settings);
    }

    private static RawProjection ProjectRaw(
        Vector3 position,
        Vector3 modelCenter,
        SurfaceChartProjectionSettings settings)
    {
        var local = position - modelCenter;
        local = RotateYaw(local, settings.YawDegrees);
        local = RotatePitch(local, settings.PitchDegrees);

        return new RawProjection(
            X: local.X - local.Z,
            Y: ((local.X + local.Z) * 0.5d) - local.Y);
    }

    private static Vector3 RotateYaw(Vector3 vector, double yawDegrees)
    {
        if (Math.Abs(yawDegrees) <= double.Epsilon)
        {
            return vector;
        }

        var radians = DegreesToRadians(yawDegrees);
        var sin = (float)Math.Sin(radians);
        var cos = (float)Math.Cos(radians);

        return new Vector3(
            (vector.X * cos) - (vector.Z * sin),
            vector.Y,
            (vector.X * sin) + (vector.Z * cos));
    }

    private static Vector3 RotatePitch(Vector3 vector, double pitchDegrees)
    {
        if (Math.Abs(pitchDegrees) <= double.Epsilon)
        {
            return vector;
        }

        var radians = DegreesToRadians(pitchDegrees);
        var sin = (float)Math.Sin(radians);
        var cos = (float)Math.Cos(radians);

        return new Vector3(
            vector.X,
            (vector.Y * cos) - (vector.Z * sin),
            (vector.Y * sin) + (vector.Z * cos));
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180d);
    }

    private static ProjectionTransform CreateTransform(Size viewSize, RawBounds rawBounds)
    {
        var spanX = Math.Max(rawBounds.MaxX - rawBounds.MinX, 1d);
        var spanY = Math.Max(rawBounds.MaxY - rawBounds.MinY, 1d);
        var availableWidth = Math.Max(viewSize.Width * 0.9d, 1d);
        var availableHeight = Math.Max(viewSize.Height * 0.9d, 1d);
        var scale = Math.Min(availableWidth / spanX, availableHeight / spanY);
        var projectedWidth = spanX * scale;
        var projectedHeight = spanY * scale;
        var offsetX = (viewSize.Width - projectedWidth) / 2d - (rawBounds.MinX * scale);
        var offsetY = (viewSize.Height - projectedHeight) / 2d - (rawBounds.MinY * scale);

        return new ProjectionTransform(scale, offsetX, offsetY);
    }

    private readonly record struct RawProjection(double X, double Y);

    private readonly record struct RawBounds(double MinX, double MaxX, double MinY, double MaxY);

    private readonly record struct ProjectionTransform(double Scale, double OffsetX, double OffsetY);
}

internal readonly record struct SurfaceChartProjectedPoint(Point ScreenPoint, double SortKey);

internal readonly record struct SurfaceChartProjectionSettings(double YawDegrees, double PitchDegrees)
{
    public static SurfaceChartProjectionSettings Default { get; } = new(0d, 0d);
}
