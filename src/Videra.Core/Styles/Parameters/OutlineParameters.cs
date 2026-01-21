using Videra.Core.Geometry;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 描边/轮廓线参数
/// </summary>
public sealed class OutlineParameters : IEquatable<OutlineParameters>
{
    /// <summary>是否启用描边</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>描边颜色</summary>
    public RgbaFloat Color { get; set; } = RgbaFloat.Black;

    /// <summary>描边宽度 (像素) [0.5-5]</summary>
    public float Width { get; set; } = 1.0f;

    public OutlineParameters Clone() => new()
    {
        Enabled = Enabled,
        Color = Color,
        Width = Width
    };

    public bool Equals(OutlineParameters? other) => other is not null
        && Enabled == other.Enabled
        && Color.Equals(other.Color)
        && Width == other.Width;

    public override bool Equals(object? obj) => obj is OutlineParameters other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Enabled, Color, Width);
}
