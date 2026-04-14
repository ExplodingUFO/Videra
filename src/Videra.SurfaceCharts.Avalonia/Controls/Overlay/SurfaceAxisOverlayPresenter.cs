using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Media;
using Videra.SurfaceCharts.Core;
using AvaVector = global::Avalonia.Vector;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal static class SurfaceAxisOverlayPresenter
{
    private static readonly Typeface OverlayTypeface = new("Consolas");
    private static readonly IBrush AxisBrush = new SolidColorBrush(Color.FromArgb(220, 245, 245, 245));
    private static readonly Pen AxisPen = new(AxisBrush, 1.5d);
    private static readonly Pen TickPen = new(AxisBrush, 1d);
    private const double TickLength = 6d;
    private const double LabelOffset = 3d;
    private const double TitleOffset = 12d;

    public static SurfaceAxisOverlayState CreateState(
        SurfaceMetadata? metadata,
        SurfaceChartProjection? projection)
    {
        if (metadata is null || projection is null)
        {
            return SurfaceAxisOverlayState.Empty;
        }

        var valueRange = metadata.ValueRange;
        var projectedCenter = projection.ProjectCenter(metadata, valueRange);
        var frontCorner = SelectFrontCorner(metadata, valueRange, projection);

        var xAxis = CreateAxisState(
            axisKey: "X",
            titleText: FormatAxisTitle(metadata.HorizontalAxis.Label, metadata.HorizontalAxis.Unit),
            axisMinimum: metadata.HorizontalAxis.Minimum,
            axisMaximum: metadata.HorizontalAxis.Maximum,
            start: projection.Project(new Vector3((float)metadata.HorizontalAxis.Minimum, (float)valueRange.Minimum, frontCorner.Z)),
            end: projection.Project(new Vector3((float)metadata.HorizontalAxis.Maximum, (float)valueRange.Minimum, frontCorner.Z)),
            projectedCenter);

        var yAxis = CreateAxisState(
            axisKey: "Y",
            titleText: "Value",
            axisMinimum: valueRange.Minimum,
            axisMaximum: valueRange.Maximum,
            start: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, frontCorner.Z)),
            end: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Maximum, frontCorner.Z)),
            projectedCenter);

        var zAxis = CreateAxisState(
            axisKey: "Z",
            titleText: FormatAxisTitle(metadata.VerticalAxis.Label, metadata.VerticalAxis.Unit),
            axisMinimum: metadata.VerticalAxis.Minimum,
            axisMaximum: metadata.VerticalAxis.Maximum,
            start: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Minimum)),
            end: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Maximum)),
            projectedCenter);

        return new SurfaceAxisOverlayState([xAxis, yAxis, zAxis]);
    }

    public static void Render(DrawingContext context, SurfaceAxisOverlayState axisOverlayState)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(axisOverlayState);

        foreach (var axis in axisOverlayState.Axes)
        {
            context.DrawLine(AxisPen, axis.AxisLine.Start, axis.AxisLine.End);

            foreach (var tick in axis.Ticks)
            {
                context.DrawLine(TickPen, tick.TickLine.Start, tick.TickLine.End);
                context.DrawText(CreateText(tick.LabelText), tick.LabelPosition);
            }

            context.DrawText(CreateText(axis.TitleText), axis.TitlePosition);
        }
    }

    private static SurfaceAxisState CreateAxisState(
        string axisKey,
        string titleText,
        double axisMinimum,
        double axisMaximum,
        Point start,
        Point end,
        Point projectedCenter)
    {
        var outwardNormal = GetOutwardNormal(start, end, projectedCenter);
        var titleAnchor = GetOuterEndpoint(start, end, projectedCenter);
        var titlePosition = titleAnchor + (outwardNormal * TitleOffset);
        var ticks = CreateTicks(axisMinimum, axisMaximum, start, end, outwardNormal);

        return new SurfaceAxisState(
            axisKey,
            new SurfaceAxisLineGeometry(start, end),
            titleText,
            titlePosition,
            ticks);
    }

    private static IReadOnlyList<SurfaceAxisTickState> CreateTicks(
        double axisMinimum,
        double axisMaximum,
        Point start,
        Point end,
        AvaVector outwardNormal)
    {
        List<SurfaceAxisTickState> ticks = [];
        var tickValues = CreateTickValues(axisMinimum, axisMaximum, GetDistance(start, end));
        var axisSpan = axisMaximum - axisMinimum;

        foreach (var tickValue in tickValues)
        {
            var t = axisSpan <= 0d ? 0d : (tickValue - axisMinimum) / axisSpan;
            t = Math.Clamp(t, 0d, 1d);

            var tickStart = Lerp(start, end, t);
            var tickEnd = tickStart + (outwardNormal * TickLength);
            var labelPosition = tickEnd + (outwardNormal * LabelOffset);

            ticks.Add(
                new SurfaceAxisTickState(
                    tickValue,
                    FormatNumber(tickValue),
                    new SurfaceAxisLineGeometry(tickStart, tickEnd),
                    labelPosition));
        }

        return ticks;
    }

    private static IReadOnlyList<double> CreateTickValues(double axisMinimum, double axisMaximum, double axisLength)
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

    private static string FormatAxisTitle(string label, string? unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        return string.IsNullOrWhiteSpace(unit)
            ? label
            : $"{label} ({unit})";
    }

    private static string FormatNumber(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static FrontCorner SelectFrontCorner(
        SurfaceMetadata metadata,
        SurfaceValueRange valueRange,
        SurfaceChartProjection projection)
    {
        var candidates = new[]
        {
            CreateFrontCorner(metadata.HorizontalAxis.Minimum, metadata.VerticalAxis.Minimum, valueRange.Minimum, projection),
            CreateFrontCorner(metadata.HorizontalAxis.Minimum, metadata.VerticalAxis.Maximum, valueRange.Minimum, projection),
            CreateFrontCorner(metadata.HorizontalAxis.Maximum, metadata.VerticalAxis.Minimum, valueRange.Minimum, projection),
            CreateFrontCorner(metadata.HorizontalAxis.Maximum, metadata.VerticalAxis.Maximum, valueRange.Minimum, projection),
        };

        return candidates
            .OrderByDescending(static corner => corner.ScreenPoint.Y)
            .ThenBy(static corner => corner.ScreenPoint.X)
            .First();
    }

    private static FrontCorner CreateFrontCorner(
        double x,
        double z,
        double y,
        SurfaceChartProjection projection)
    {
        return new FrontCorner(
            (float)x,
            (float)z,
            projection.Project(new Vector3((float)x, (float)y, (float)z)));
    }

    private static Point GetOuterEndpoint(Point start, Point end, Point projectedCenter)
    {
        return GetDistance(start, projectedCenter) >= GetDistance(end, projectedCenter) ? start : end;
    }

    private static AvaVector GetOutwardNormal(Point start, Point end, Point projectedCenter)
    {
        var direction = end - start;
        var length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y));
        if (length <= double.Epsilon)
        {
            return new AvaVector(0d, -1d);
        }

        var normal = new AvaVector(-direction.Y / length, direction.X / length);
        var midpoint = Lerp(start, end, 0.5d);
        var firstCandidate = midpoint + (normal * 8d);
        var secondCandidate = midpoint - (normal * 8d);

        return GetDistance(firstCandidate, projectedCenter) >= GetDistance(secondCandidate, projectedCenter)
            ? normal
            : -normal;
    }

    private static double GetDistance(Point first, Point second)
    {
        var dx = second.X - first.X;
        var dy = second.Y - first.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static Point Lerp(Point start, Point end, double t)
    {
        return new Point(
            start.X + ((end.X - start.X) * t),
            start.Y + ((end.Y - start.Y) * t));
    }

    private static FormattedText CreateText(string text)
    {
        return new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            OverlayTypeface,
            12d,
            AxisBrush);
    }

    private readonly record struct FrontCorner(float X, float Z, Point ScreenPoint);
}
