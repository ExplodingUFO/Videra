using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a shape annotation (rectangle or ellipse) anchored to data coordinates.
/// </summary>
public sealed class ShapeAnnotationData
{
    public ShapeAnnotationData(
        ShapeKind kind,
        Vector3 center,
        double width,
        double height,
        uint fillColor = 0x404DA3FFu,
        uint? borderColor = null,
        double borderWidth = 1d,
        double rotationDegrees = 0d,
        string? label = null)
    {
        Kind = kind;
        Center = center;
        Width = width;
        Height = height;
        FillColor = fillColor;
        BorderColor = borderColor;
        BorderWidth = borderWidth;
        RotationDegrees = rotationDegrees;
        Label = label;
    }

    /// <summary>
    /// Gets the shape kind.
    /// </summary>
    public ShapeKind Kind { get; }

    /// <summary>
    /// Gets the center position in data coordinates.
    /// </summary>
    public Vector3 Center { get; }

    /// <summary>
    /// Gets the width in data units.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Gets the height in data units.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Gets the ARGB fill color.
    /// </summary>
    public uint FillColor { get; }

    /// <summary>
    /// Gets the optional ARGB border color. When null, no border is drawn.
    /// </summary>
    public uint? BorderColor { get; }

    /// <summary>
    /// Gets the border width in device-independent pixels.
    /// </summary>
    public double BorderWidth { get; }

    /// <summary>
    /// Gets the rotation angle in degrees.
    /// </summary>
    public double RotationDegrees { get; }

    /// <summary>
    /// Gets the optional label rendered at the center.
    /// </summary>
    public string? Label { get; }
}

/// <summary>
/// Identifies the kind of shape annotation.
/// </summary>
public enum ShapeKind
{
    /// <summary>Rectangle shape.</summary>
    Rectangle,

    /// <summary>Ellipse shape.</summary>
    Ellipse,
}
