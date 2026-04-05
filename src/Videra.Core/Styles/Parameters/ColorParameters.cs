using System.Numerics;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 色彩后处理参数
/// </summary>
public sealed class ColorParameters : IEquatable<ColorParameters>
{
    /// <summary>色调叠加 RGB [0-1]</summary>
    public Vector3 TintColor { get; set; } = Vector3.One;

    /// <summary>饱和度 [0-2], 1=原始</summary>
    public float Saturation { get; set; } = 1.0f;

    /// <summary>对比度 [0-2], 1=原始</summary>
    public float Contrast { get; set; } = 1.0f;

    /// <summary>亮度偏移 [-1, 1], 0=原始</summary>
    public float Brightness { get; set; }

    public ColorParameters Clone() => (ColorParameters)MemberwiseClone();

    public bool Equals(ColorParameters? other) => other is not null
        && TintColor == other.TintColor
        && Saturation == other.Saturation
        && Contrast == other.Contrast
        && Brightness == other.Brightness;

    public override bool Equals(object? obj) => obj is ColorParameters other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(TintColor, Saturation, Contrast, Brightness);
}
