namespace Videra.SurfaceCharts.Core;

/// <summary>
/// Represents per-sample availability for one surface grid or tile.
/// </summary>
public sealed class SurfaceMask
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SurfaceMask"/> class.
    /// </summary>
    /// <param name="width">The mask width in samples.</param>
    /// <param name="height">The mask height in samples.</param>
    /// <param name="values">The row-major availability values where <see langword="true"/> means the sample is present.</param>
    public SurfaceMask(int width, int height, ReadOnlyMemory<bool> values)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var expectedValueCount = (long)width * height;
        if (expectedValueCount > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Mask shape is too large.");
        }

        if (values.Length != expectedValueCount)
        {
            throw new ArgumentException("Mask values must match the declared shape.", nameof(values));
        }

        Width = width;
        Height = height;
        Values = values;
    }

    /// <summary>
    /// Gets the mask width in samples.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the mask height in samples.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the row-major availability values where <see langword="true"/> means the sample is present.
    /// </summary>
    public ReadOnlyMemory<bool> Values { get; }

    internal static SurfaceMask? Normalize(
        SurfaceMask? explicitMask,
        int width,
        int height,
        ReadOnlySpan<float> heightValues,
        ReadOnlySpan<float> colorValues,
        bool hasColorField)
    {
        bool[]? normalizedValues = explicitMask?.Values.ToArray();

        for (var index = 0; index < heightValues.Length; index++)
        {
            if (float.IsFinite(heightValues[index]) &&
                (!hasColorField || float.IsFinite(colorValues[index])))
            {
                continue;
            }

            normalizedValues ??= CreateAllVisibleValues(heightValues.Length);
            normalizedValues[index] = false;
        }

        if (normalizedValues is null)
        {
            return explicitMask;
        }

        return new SurfaceMask(width, height, normalizedValues);
    }

    private static bool[] CreateAllVisibleValues(int count)
    {
        var values = new bool[count];
        Array.Fill(values, true);
        return values;
    }
}
