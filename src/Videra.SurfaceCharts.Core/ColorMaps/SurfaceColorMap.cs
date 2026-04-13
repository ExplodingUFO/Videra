namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Maps surface values into ARGB colors across a value range.
/// </summary>
public sealed class SurfaceColorMap
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceColorMap"/> class.
    /// </summary>
    /// <param name="range">The value range covered by the color map.</param>
    /// <param name="palette">The ordered palette used for interpolation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palette"/> is <c>null</c>.</exception>
    public SurfaceColorMap(SurfaceValueRange range, SurfaceColorMapPalette palette)
    {
        ArgumentNullException.ThrowIfNull(palette);

        Range = range;
        Palette = palette;
    }

    /// <summary>
    /// Gets the value range covered by the color map.
    /// </summary>
    public SurfaceValueRange Range { get; }

    /// <summary>
    /// Gets the palette used for interpolation.
    /// </summary>
    public SurfaceColorMapPalette Palette { get; }

    /// <summary>
    /// Maps a value to an ARGB color.
    /// </summary>
    /// <param name="value">The value to map.</param>
    /// <returns>The mapped ARGB color.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not finite.</exception>
    public uint Map(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Surface color map values must be finite.");
        }

        if (Range.Span <= double.Epsilon)
        {
            return Palette[0];
        }

        var normalized = (value - Range.Minimum) / Range.Span;
        normalized = Math.Clamp(normalized, 0.0, 1.0);

        if (normalized >= 1.0)
        {
            return Palette[Palette.Count - 1];
        }

        var scaled = normalized * (Palette.Count - 1);
        var lowerIndex = (int)Math.Floor(scaled);
        var upperIndex = lowerIndex + 1;
        var fraction = scaled - lowerIndex;

        return Interpolate(Palette[lowerIndex], Palette[upperIndex], fraction);
    }

    private static uint Interpolate(uint start, uint end, double fraction)
    {
        var startAlpha = (byte)(start >> 24);
        var startRed = (byte)(start >> 16);
        var startGreen = (byte)(start >> 8);
        var startBlue = (byte)start;

        var endAlpha = (byte)(end >> 24);
        var endRed = (byte)(end >> 16);
        var endGreen = (byte)(end >> 8);
        var endBlue = (byte)end;

        var alpha = InterpolateChannel(startAlpha, endAlpha, fraction);
        var red = InterpolateChannel(startRed, endRed, fraction);
        var green = InterpolateChannel(startGreen, endGreen, fraction);
        var blue = InterpolateChannel(startBlue, endBlue, fraction);

        return ((uint)alpha << 24)
            | ((uint)red << 16)
            | ((uint)green << 8)
            | blue;
    }

    private static byte InterpolateChannel(byte start, byte end, double fraction)
    {
        var value = start + ((end - start) * fraction);
        return (byte)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
