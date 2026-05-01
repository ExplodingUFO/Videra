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
    private static readonly Pen MinorTickPen = new(new SolidColorBrush(Color.FromArgb(180, 220, 220, 220)), 0.8d);
    private static readonly Pen GridPen = new(new SolidColorBrush(Color.FromArgb(90, 180, 210, 240)), 0.75d);
    private const double TickLength = 6d;
    private const double MinorTickLength = 3.5d;
    private const double LabelOffset = 3d;
    private const double TitleOffset = 12d;

    public static SurfaceAxisOverlayState CreateState(
        SurfaceMetadata? metadata,
        SurfaceChartProjection? projection,
        SurfaceChartOverlayOptions? overlayOptions)
    {
        if (metadata is null || projection is null)
        {
            return SurfaceAxisOverlayState.Empty;
        }

        overlayOptions ??= SurfaceChartOverlayOptions.Default;
        var valueRange = metadata.ValueRange;
        var projectedCenter = projection.ProjectCenter(metadata, valueRange);
        var frontCorner = SelectFrontCorner(metadata, valueRange, projection, overlayOptions);

        var xAxis = CreateAxisState(
            axisKey: "X",
            titleText: FormatAxisTitle(
                overlayOptions.HorizontalAxisTitleOverride ?? metadata.HorizontalAxis.Label,
                overlayOptions.HorizontalAxisUnitOverride ?? metadata.HorizontalAxis.Unit),
            axisMinimum: metadata.HorizontalAxis.Minimum,
            axisMaximum: metadata.HorizontalAxis.Maximum,
            scaleKind: metadata.HorizontalAxis.ScaleKind,
            isInverted: metadata.HorizontalAxis.IsInverted,
            start: projection.Project(new Vector3((float)metadata.HorizontalAxis.Minimum, (float)valueRange.Minimum, frontCorner.Z)),
            end: projection.Project(new Vector3((float)metadata.HorizontalAxis.Maximum, (float)valueRange.Minimum, frontCorner.Z)),
            projectedCenter,
            overlayOptions);

        var yAxis = CreateAxisState(
            axisKey: "Y",
            titleText: FormatAxisTitle(
                overlayOptions.ValueAxisTitleOverride ?? "Value",
                overlayOptions.ValueAxisUnitOverride),
            axisMinimum: valueRange.Minimum,
            axisMaximum: valueRange.Maximum,
            scaleKind: overlayOptions.ValueAxisScaleKind,
            isInverted: false,
            start: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, frontCorner.Z)),
            end: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Maximum, frontCorner.Z)),
            projectedCenter,
            overlayOptions);

        var zAxis = CreateAxisState(
            axisKey: "Z",
            titleText: FormatAxisTitle(
                overlayOptions.DepthAxisTitleOverride ?? metadata.VerticalAxis.Label,
                overlayOptions.DepthAxisUnitOverride ?? metadata.VerticalAxis.Unit),
            axisMinimum: metadata.VerticalAxis.Minimum,
            axisMaximum: metadata.VerticalAxis.Maximum,
            scaleKind: metadata.VerticalAxis.ScaleKind,
            isInverted: metadata.VerticalAxis.IsInverted,
            start: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Minimum)),
            end: projection.Project(new Vector3(frontCorner.X, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Maximum)),
            projectedCenter,
            overlayOptions);

        var gridLines = CreateGridLines(metadata, projection, valueRange, frontCorner, xAxis, yAxis, zAxis, overlayOptions);

        List<SurfaceAxisState> axes = [xAxis, yAxis, zAxis];

        // Add secondary Y axis if configured
        if (overlayOptions.SecondaryValueAxisMinimum is { } y2Min && overlayOptions.SecondaryValueAxisMaximum is { } y2Max)
        {
            var y2Axis = CreateAxisState(
                axisKey: "Y2",
                titleText: FormatAxisTitle(
                    overlayOptions.SecondaryValueAxisTitleOverride ?? "Y2",
                    overlayOptions.SecondaryValueAxisUnitOverride),
                axisMinimum: y2Min,
                axisMaximum: y2Max,
                scaleKind: overlayOptions.SecondaryValueAxisScaleKind,
                isInverted: false,
                start: projection.Project(new Vector3(frontCorner.X, (float)y2Min, frontCorner.Z)),
                end: projection.Project(new Vector3(frontCorner.X, (float)y2Max, frontCorner.Z)),
                projectedCenter,
                overlayOptions);
            axes.Add(y2Axis);
        }

        return new SurfaceAxisOverlayState(
            axes,
            ResolveGridPlaneKey(overlayOptions.GridPlane),
            gridLines,
            frontCorner.X,
            frontCorner.Z);
    }

    public static void Render(DrawingContext context, SurfaceAxisOverlayState axisOverlayState)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(axisOverlayState);

        foreach (var gridLine in axisOverlayState.GridLines)
        {
            context.DrawLine(GridPen, gridLine.Start, gridLine.End);
        }

        foreach (var axis in axisOverlayState.Axes)
        {
            context.DrawLine(AxisPen, axis.AxisLine.Start, axis.AxisLine.End);

            foreach (var minorTick in axis.MinorTicks)
            {
                context.DrawLine(MinorTickPen, minorTick.Start, minorTick.End);
            }

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
        SurfaceAxisScaleKind scaleKind,
        bool isInverted,
        Point start,
        Point end,
        Point projectedCenter,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var outwardNormal = GetOutwardNormal(start, end, projectedCenter);
        var titleAnchor = GetOuterEndpoint(start, end, projectedCenter);
        var titlePosition = titleAnchor + (outwardNormal * TitleOffset);

        // Dispatch tick generation based on scale kind
        var majorTickValues = scaleKind switch
        {
            SurfaceAxisScaleKind.Log =>
                SurfaceAxisTickGenerator.CreateLogTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
            SurfaceAxisScaleKind.DateTime =>
                SurfaceAxisTickGenerator.CreateDateTimeTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
            _ =>
                SurfaceAxisTickGenerator.CreateMajorTickValues(axisMinimum, axisMaximum, GetDistance(start, end)),
        };

        var ticks = CreateTicks(axisKey, majorTickValues, start, end, outwardNormal, overlayOptions, scaleKind, isInverted);
        var minorTicks = overlayOptions.ShowMinorTicks
            ? CreateMinorTicks(
                scaleKind == SurfaceAxisScaleKind.Log
                    ? SurfaceAxisTickGenerator.CreateLogMinorTickValues(majorTickValues, axisMinimum, axisMaximum)
                    : SurfaceAxisTickGenerator.CreateMinorTickValues(majorTickValues, overlayOptions.MinorTickDivisions),
                axisMinimum,
                axisMaximum,
                start,
                end,
                outwardNormal,
                isInverted)
            : Array.Empty<SurfaceAxisLineGeometry>();

        return new SurfaceAxisState(
            axisKey,
            new SurfaceAxisLineGeometry(start, end),
            titleText,
            titlePosition,
            SurfaceAxisLayoutEngine.CullDenseLabels(ticks),
            minorTicks,
            ticks.Count);
    }

    private static IReadOnlyList<SurfaceAxisTickState> CreateTicks(
        string axisKey,
        IReadOnlyList<double> tickValues,
        Point start,
        Point end,
        AvaVector outwardNormal,
        SurfaceChartOverlayOptions overlayOptions,
        SurfaceAxisScaleKind scaleKind,
        bool isInverted)
    {
        List<SurfaceAxisTickState> ticks = [];
        if (tickValues.Count == 0)
        {
            return ticks;
        }

        var axisMinimum = tickValues[0];
        var axisMaximum = tickValues[^1];
        var axisSpan = axisMaximum - axisMinimum;

        foreach (var tickValue in tickValues)
        {
            var t = axisSpan <= 0d ? 0d : (tickValue - axisMinimum) / axisSpan;
            t = Math.Clamp(t, 0d, 1d);
            if (isInverted)
            {
                t = 1d - t;
            }

            var tickStart = Lerp(start, end, t);
            var tickEnd = tickStart + (outwardNormal * TickLength);
            var labelPosition = tickEnd + (outwardNormal * LabelOffset);

            // Format label: DateTime uses specialized formatter, others use overlay options
            var labelText = scaleKind == SurfaceAxisScaleKind.DateTime
                ? SurfaceChartOverlayOptions.FormatDateTimeLabel(tickValue, axisSpan)
                : overlayOptions.FormatLabel(axisKey, tickValue);

            ticks.Add(
                new SurfaceAxisTickState(
                    tickValue,
                    labelText,
                    new SurfaceAxisLineGeometry(tickStart, tickEnd),
                    labelPosition));
        }

        return ticks;
    }

    private static IReadOnlyList<SurfaceAxisLineGeometry> CreateMinorTicks(
        IReadOnlyList<double> tickValues,
        double axisMinimum,
        double axisMaximum,
        Point start,
        Point end,
        AvaVector outwardNormal,
        bool isInverted)
    {
        List<SurfaceAxisLineGeometry> ticks = [];
        var axisSpan = axisMaximum - axisMinimum;
        foreach (var tickValue in tickValues)
        {
            var t = axisSpan <= 0d ? 0d : (tickValue - axisMinimum) / axisSpan;
            t = Math.Clamp(t, 0d, 1d);
            if (isInverted)
            {
                t = 1d - t;
            }

            var tickStart = Lerp(start, end, t);
            var tickEnd = tickStart + (outwardNormal * MinorTickLength);
            ticks.Add(new SurfaceAxisLineGeometry(tickStart, tickEnd));
        }

        return ticks;
    }

    private static string FormatAxisTitle(string label, string? unit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        return string.IsNullOrWhiteSpace(unit)
            ? label
            : $"{label} ({unit})";
    }

    private static FrontCorner SelectFrontCorner(
        SurfaceMetadata metadata,
        SurfaceValueRange valueRange,
        SurfaceChartProjection projection,
        SurfaceChartOverlayOptions overlayOptions)
    {
        return overlayOptions.AxisSideMode switch
        {
            SurfaceChartAxisSideMode.MinimumBounds => CreateFrontCorner(metadata.HorizontalAxis.Minimum, metadata.VerticalAxis.Minimum, valueRange.Minimum, projection),
            SurfaceChartAxisSideMode.MaximumBounds => CreateFrontCorner(metadata.HorizontalAxis.Maximum, metadata.VerticalAxis.Maximum, valueRange.Minimum, projection),
            _ => SelectAutoFrontCorner(metadata, valueRange, projection),
        };
    }

    private static FrontCorner SelectAutoFrontCorner(
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

    private static IReadOnlyList<SurfaceAxisLineGeometry> CreateGridLines(
        SurfaceMetadata metadata,
        SurfaceChartProjection projection,
        SurfaceValueRange valueRange,
        FrontCorner frontCorner,
        SurfaceAxisState xAxis,
        SurfaceAxisState yAxis,
        SurfaceAxisState zAxis,
        SurfaceChartOverlayOptions overlayOptions)
    {
        var gridPlane = overlayOptions.GridPlane == SurfaceChartGridPlane.Auto
            ? SurfaceChartGridPlane.XZ
            : overlayOptions.GridPlane;

        List<SurfaceAxisLineGeometry> lines = [];
        switch (gridPlane)
        {
            case SurfaceChartGridPlane.XY:
                lines.AddRange(
                    xAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3((float)tick.Value, (float)valueRange.Minimum, frontCorner.Z),
                        new Vector3((float)tick.Value, (float)valueRange.Maximum, frontCorner.Z))));
                lines.AddRange(
                    yAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3((float)metadata.HorizontalAxis.Minimum, (float)tick.Value, frontCorner.Z),
                        new Vector3((float)metadata.HorizontalAxis.Maximum, (float)tick.Value, frontCorner.Z))));

                break;
            case SurfaceChartGridPlane.YZ:
                lines.AddRange(
                    zAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3(frontCorner.X, (float)valueRange.Minimum, (float)tick.Value),
                        new Vector3(frontCorner.X, (float)valueRange.Maximum, (float)tick.Value))));
                lines.AddRange(
                    yAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3(frontCorner.X, (float)tick.Value, (float)metadata.VerticalAxis.Minimum),
                        new Vector3(frontCorner.X, (float)tick.Value, (float)metadata.VerticalAxis.Maximum))));

                break;
            default:
                lines.AddRange(
                    xAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3((float)tick.Value, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Minimum),
                        new Vector3((float)tick.Value, (float)valueRange.Minimum, (float)metadata.VerticalAxis.Maximum))));
                lines.AddRange(
                    zAxis.Ticks.Select(tick => ProjectLine(
                        projection,
                        new Vector3((float)metadata.HorizontalAxis.Minimum, (float)valueRange.Minimum, (float)tick.Value),
                        new Vector3((float)metadata.HorizontalAxis.Maximum, (float)valueRange.Minimum, (float)tick.Value))));

                break;
        }

        return lines;
    }

    private static SurfaceAxisLineGeometry ProjectLine(SurfaceChartProjection projection, Vector3 start, Vector3 end)
    {
        return new SurfaceAxisLineGeometry(projection.Project(start), projection.Project(end));
    }

    private static string ResolveGridPlaneKey(SurfaceChartGridPlane gridPlane)
    {
        return (gridPlane == SurfaceChartGridPlane.Auto ? SurfaceChartGridPlane.XZ : gridPlane).ToString();
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
