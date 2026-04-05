using Videra.Core.Geometry;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 材质表现参数
/// </summary>
public sealed class MaterialParameters : IEquatable<MaterialParameters>
{
    /// <summary>整体透明度 [0-1]</summary>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>是否使用顶点色 (false时使用OverrideColor)</summary>
    public bool UseVertexColor { get; set; } = true;

    /// <summary>覆盖颜色 (当UseVertexColor=false时生效)</summary>
    public RgbaFloat OverrideColor { get; set; } = RgbaFloat.LightGrey;

    /// <summary>线框渲染模式</summary>
    public bool WireframeMode { get; set; }

    public MaterialParameters Clone() => new()
    {
        Opacity = Opacity,
        UseVertexColor = UseVertexColor,
        OverrideColor = OverrideColor,
        WireframeMode = WireframeMode
    };

    public bool Equals(MaterialParameters? other) => other is not null
        && Opacity == other.Opacity
        && UseVertexColor == other.UseVertexColor
        && OverrideColor.Equals(other.OverrideColor)
        && WireframeMode == other.WireframeMode;

    public override bool Equals(object? obj) => obj is MaterialParameters other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Opacity, UseVertexColor, OverrideColor, WireframeMode);
}
