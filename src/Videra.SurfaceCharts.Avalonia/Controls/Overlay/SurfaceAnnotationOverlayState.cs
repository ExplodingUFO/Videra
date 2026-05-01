using Avalonia;

namespace Videra.SurfaceCharts.Avalonia.Controls.Overlay;

internal sealed class SurfaceAnnotationOverlayState
{
    public static SurfaceAnnotationOverlayState Empty { get; } = new([], []);

    public SurfaceAnnotationOverlayState(
        IReadOnlyList<SurfaceTextAnnotationState> textAnnotations,
        IReadOnlyList<SurfaceArrowAnnotationState> arrowAnnotations)
    {
        ArgumentNullException.ThrowIfNull(textAnnotations);
        ArgumentNullException.ThrowIfNull(arrowAnnotations);
        TextAnnotations = textAnnotations;
        ArrowAnnotations = arrowAnnotations;
    }

    public IReadOnlyList<SurfaceTextAnnotationState> TextAnnotations { get; }
    public IReadOnlyList<SurfaceArrowAnnotationState> ArrowAnnotations { get; }
}

internal sealed class SurfaceTextAnnotationState
{
    public SurfaceTextAnnotationState(
        string text,
        Point screenPosition,
        uint color,
        double fontSize,
        string? fontFamily)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        Text = text;
        ScreenPosition = screenPosition;
        Color = color;
        FontSize = fontSize;
        FontFamily = fontFamily;
    }

    public string Text { get; }
    public Point ScreenPosition { get; }
    public uint Color { get; }
    public double FontSize { get; }
    public string? FontFamily { get; }
}

internal sealed class SurfaceArrowAnnotationState
{
    public SurfaceArrowAnnotationState(
        Point startScreen,
        Point endScreen,
        uint color,
        double lineWidth,
        double headLength,
        double headWidth,
        string? label)
    {
        StartScreen = startScreen;
        EndScreen = endScreen;
        Color = color;
        LineWidth = lineWidth;
        HeadLength = headLength;
        HeadWidth = headWidth;
        Label = label;
    }

    public Point StartScreen { get; }
    public Point EndScreen { get; }
    public uint Color { get; }
    public double LineWidth { get; }
    public double HeadLength { get; }
    public double HeadWidth { get; }
    public string? Label { get; }
}
