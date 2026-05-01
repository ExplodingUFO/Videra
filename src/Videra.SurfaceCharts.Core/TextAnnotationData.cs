using System.Numerics;

namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Describes a text annotation anchored to a position in 3D data space.
/// </summary>
public sealed class TextAnnotationData
{
    public TextAnnotationData(
        Vector3 position,
        string text,
        uint color = 0xFFFFFFFFu,
        double fontSize = 12d,
        string? fontFamily = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        Position = position;
        Text = text;
        Color = color;
        FontSize = fontSize;
        FontFamily = fontFamily;
    }

    /// <summary>
    /// Gets the anchor position in data coordinates (X, Y, Z).
    /// </summary>
    public Vector3 Position { get; }

    /// <summary>
    /// Gets the annotation text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the ARGB color for the text.
    /// </summary>
    public uint Color { get; }

    /// <summary>
    /// Gets the font size in device-independent pixels.
    /// </summary>
    public double FontSize { get; }

    /// <summary>
    /// Gets the optional font family name. Uses the overlay default when null.
    /// </summary>
    public string? FontFamily { get; }
}
