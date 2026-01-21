using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Videra.Core.Geometry;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;

namespace Videra.Core.Styles.Serialization;

/// <summary>
/// 风格参数 JSON 序列化转换器
/// </summary>
public static class StyleJsonConverter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new Vector3JsonConverter(),
            new RgbaFloatJsonConverter(),
            new JsonStringEnumConverter()
        }
    };

    /// <summary>序列化为JSON</summary>
    public static string Serialize(RenderStyleParameters parameters, RenderStylePreset preset)
    {
        var wrapper = new StyleExportWrapper
        {
            Version = "1.0",
            Preset = preset,
            Parameters = parameters
        };
        return JsonSerializer.Serialize(wrapper, Options);
    }

    /// <summary>从JSON反序列化</summary>
    public static (RenderStyleParameters Parameters, RenderStylePreset Preset) Deserialize(string json)
    {
        var wrapper = JsonSerializer.Deserialize<StyleExportWrapper>(json, Options)
            ?? throw new JsonException("Invalid style JSON");
        return (wrapper.Parameters, wrapper.Preset);
    }

    /// <summary>导出包装类</summary>
    private sealed class StyleExportWrapper
    {
        public string Version { get; set; } = "1.0";
        public RenderStylePreset Preset { get; set; }
        public RenderStyleParameters Parameters { get; set; } = new();
    }
}

/// <summary>Vector3 JSON 转换器</summary>
internal sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        reader.Read();
        var x = reader.GetSingle();
        reader.Read();
        var y = reader.GetSingle();
        reader.Read();
        var z = reader.GetSingle();
        reader.Read(); // EndArray

        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}

/// <summary>RgbaFloat JSON 转换器</summary>
internal sealed class RgbaFloatJsonConverter : JsonConverter<RgbaFloat>
{
    public override RgbaFloat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        reader.Read();
        var r = reader.GetSingle();
        reader.Read();
        var g = reader.GetSingle();
        reader.Read();
        var b = reader.GetSingle();
        reader.Read();
        var a = reader.GetSingle();
        reader.Read(); // EndArray

        return new RgbaFloat(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, RgbaFloat value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}
