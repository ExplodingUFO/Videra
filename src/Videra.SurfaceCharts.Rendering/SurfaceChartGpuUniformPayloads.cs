using System.Buffers.Binary;
using Videra.SurfaceCharts.Core;

namespace Videra.SurfaceCharts.Rendering;

internal static class SurfaceChartGpuUniformPayloads
{
    public const int MaxPaletteEntries = 256;
    public const int MaxScalarEntries = 16384;
    public const int ScalarGroupWidth = 4;
    public const int ScalarGroupCount = MaxScalarEntries / ScalarGroupWidth;
    public const int FloatBytes = sizeof(float);
    public const int Vector4Bytes = FloatBytes * 4;
    public const int ColorMapBufferSize = Vector4Bytes + (MaxPaletteEntries * Vector4Bytes);
    public const int ScalarBufferSize = ScalarGroupCount * Vector4Bytes;

    public static byte[] CreateColorMapPayload(SurfaceColorMapLut colorMapLut)
    {
        ArgumentNullException.ThrowIfNull(colorMapLut);

        var payload = new byte[ColorMapBufferSize];
        var outputPaletteCount = Math.Min(colorMapLut.PaletteCount, MaxPaletteEntries);
        WriteSingle(payload, 0, (float)colorMapLut.Range.Minimum);
        WriteSingle(payload, FloatBytes, (float)colorMapLut.Range.Maximum);
        WriteSingle(payload, FloatBytes * 2, outputPaletteCount);
        WriteSingle(payload, FloatBytes * 3, Math.Max(outputPaletteCount - 1f, 0f));

        for (var index = 0; index < outputPaletteCount; index++)
        {
            var offset = Vector4Bytes + (index * Vector4Bytes);
            WriteColor(payload, offset, ResolvePaletteColor(colorMapLut, index, outputPaletteCount));
        }

        var fillColor = ResolvePaletteColor(colorMapLut, outputPaletteCount - 1, outputPaletteCount);
        for (var index = outputPaletteCount; index < MaxPaletteEntries; index++)
        {
            var offset = Vector4Bytes + (index * Vector4Bytes);
            WriteColor(payload, offset, fillColor);
        }

        return payload;
    }

    private static uint ResolvePaletteColor(SurfaceColorMapLut colorMapLut, int outputIndex, int outputPaletteCount)
    {
        if (colorMapLut.PaletteCount <= MaxPaletteEntries)
        {
            return colorMapLut.GetPaletteColor(outputIndex);
        }

        if (outputPaletteCount <= 1 || colorMapLut.Range.Maximum <= colorMapLut.Range.Minimum)
        {
            return colorMapLut.GetPaletteColor(0);
        }

        var fraction = outputIndex / (double)(outputPaletteCount - 1);
        var sampledValue = colorMapLut.Range.Minimum + ((colorMapLut.Range.Maximum - colorMapLut.Range.Minimum) * fraction);
        return colorMapLut.Map(sampledValue);
    }

    public static byte[] CreateScalarPayload(ReadOnlySpan<float> sampleValues)
    {
        if (sampleValues.Length > MaxScalarEntries)
        {
            throw new InvalidOperationException($"Surface tiles currently support at most {MaxScalarEntries} scalar samples on the GPU recolor path.");
        }

        var payload = new byte[((sampleValues.Length + (ScalarGroupWidth - 1)) / ScalarGroupWidth) * Vector4Bytes];

        for (var index = 0; index < sampleValues.Length; index++)
        {
            var groupIndex = index / ScalarGroupWidth;
            var componentIndex = index % ScalarGroupWidth;
            var offset = (groupIndex * Vector4Bytes) + (componentIndex * FloatBytes);
            WriteSingle(payload, offset, sampleValues[index]);
        }

        return payload;
    }

    private static void WriteColor(byte[] payload, int offset, uint argb)
    {
        WriteSingle(payload, offset, ((argb >> 16) & 0xFF) / 255f);
        WriteSingle(payload, offset + FloatBytes, ((argb >> 8) & 0xFF) / 255f);
        WriteSingle(payload, offset + (FloatBytes * 2), (argb & 0xFF) / 255f);
        WriteSingle(payload, offset + (FloatBytes * 3), ((argb >> 24) & 0xFF) / 255f);
    }

    private static void WriteSingle(byte[] payload, int offset, float value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset, FloatBytes), BitConverter.SingleToInt32Bits(value));
    }
}
