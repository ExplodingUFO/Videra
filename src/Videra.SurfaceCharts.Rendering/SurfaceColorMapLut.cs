using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

internal sealed class SurfaceColorMapLut
{
    private readonly uint[] _palette;

    public SurfaceColorMapLut(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        Range = colorMap.Range;
        _palette = new uint[colorMap.Palette.Count];
        for (var index = 0; index < _palette.Length; index++)
        {
            _palette[index] = colorMap.Palette[index];
        }
    }

    public SurfaceValueRange Range { get; }

    public int PaletteCount => _palette.Length;

    public uint GetPaletteColor(int index)
    {
        return _palette[index];
    }

    public uint Map(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Surface color map values must be finite.");
        }

        if (value <= Range.Minimum)
        {
            return _palette[0];
        }

        if (value >= Range.Maximum)
        {
            return _palette[^1];
        }

        if (Range.Maximum <= Range.Minimum)
        {
            return _palette[0];
        }

        var scale = Math.Max(Math.Abs(Range.Minimum), Math.Abs(Range.Maximum));
        scale = Math.Max(scale, Math.Abs(value));

        var scaledMinimum = Range.Minimum / scale;
        var scaledMaximum = Range.Maximum / scale;
        var scaledValue = value / scale;
        var normalized = (scaledValue - scaledMinimum) / (scaledMaximum - scaledMinimum);
        normalized = Math.Clamp(normalized, 0.0, 1.0);

        if (normalized >= 1.0)
        {
            return _palette[^1];
        }

        var scaled = normalized * (_palette.Length - 1);
        var lowerIndex = (int)Math.Floor(scaled);
        var upperIndex = lowerIndex + 1;
        var fraction = scaled - lowerIndex;

        return Interpolate(_palette[lowerIndex], _palette[upperIndex], fraction);
    }

    public bool Matches(SurfaceColorMap colorMap)
    {
        ArgumentNullException.ThrowIfNull(colorMap);

        if (Range != colorMap.Range || _palette.Length != colorMap.Palette.Count)
        {
            return false;
        }

        for (var index = 0; index < _palette.Length; index++)
        {
            if (_palette[index] != colorMap.Palette[index])
            {
                return false;
            }
        }

        return true;
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
