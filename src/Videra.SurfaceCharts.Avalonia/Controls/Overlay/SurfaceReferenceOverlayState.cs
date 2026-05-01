using Avalonia;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceReferenceOverlayState
{
    public static SurfaceReferenceOverlayState Empty { get; } = new([], [], []);

    public SurfaceReferenceOverlayState(
        IReadOnlyList<SurfaceReferenceLineState> lines,
        IReadOnlyList<SurfaceReferenceSpanState> spans,
        IReadOnlyList<SurfaceShapeAnnotationState> shapes)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(spans);
        ArgumentNullException.ThrowIfNull(shapes);
        Lines = lines;
        Spans = spans;
        Shapes = shapes;
    }

    public IReadOnlyList<SurfaceReferenceLineState> Lines { get; }
    public IReadOnlyList<SurfaceReferenceSpanState> Spans { get; }
    public IReadOnlyList<SurfaceShapeAnnotationState> Shapes { get; }
}

internal sealed class SurfaceReferenceLineState
{
    public SurfaceReferenceLineState(
        Point startScreen,
        Point endScreen,
        uint color,
        double lineWidth,
        string? label,
        Point? labelPosition)
    {
        StartScreen = startScreen;
        EndScreen = endScreen;
        Color = color;
        LineWidth = lineWidth;
        Label = label;
        LabelPosition = labelPosition;
    }

    public Point StartScreen { get; }
    public Point EndScreen { get; }
    public uint Color { get; }
    public double LineWidth { get; }
    public string? Label { get; }
    public Point? LabelPosition { get; }
}

internal sealed class SurfaceReferenceSpanState
{
    public SurfaceReferenceSpanState(
        Point topLeft,
        Point topRight,
        Point bottomRight,
        Point bottomLeft,
        uint color,
        uint? borderColor,
        double borderWidth,
        string? label,
        Point? labelPosition)
    {
        TopLeft = topLeft;
        TopRight = topRight;
        BottomRight = bottomRight;
        BottomLeft = bottomLeft;
        Color = color;
        BorderColor = borderColor;
        BorderWidth = borderWidth;
        Label = label;
        LabelPosition = labelPosition;
    }

    public Point TopLeft { get; }
    public Point TopRight { get; }
    public Point BottomRight { get; }
    public Point BottomLeft { get; }
    public uint Color { get; }
    public uint? BorderColor { get; }
    public double BorderWidth { get; }
    public string? Label { get; }
    public Point? LabelPosition { get; }
}

internal sealed class SurfaceShapeAnnotationState
{
    public SurfaceShapeAnnotationState(
        ShapeKind kind,
        Point centerScreen,
        double screenWidth,
        double screenHeight,
        double rotationDegrees,
        uint fillColor,
        uint? borderColor,
        double borderWidth,
        string? label)
    {
        Kind = kind;
        CenterScreen = centerScreen;
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
        RotationDegrees = rotationDegrees;
        FillColor = fillColor;
        BorderColor = borderColor;
        BorderWidth = borderWidth;
        Label = label;
    }

    public ShapeKind Kind { get; }
    public Point CenterScreen { get; }
    public double ScreenWidth { get; }
    public double ScreenHeight { get; }
    public double RotationDegrees { get; }
    public uint FillColor { get; }
    public uint? BorderColor { get; }
    public double BorderWidth { get; }
    public string? Label { get; }
}
