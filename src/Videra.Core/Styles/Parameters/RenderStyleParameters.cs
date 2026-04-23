using System.Numerics;
using Videra.Core.Geometry;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 渲染风格参数 - 聚合所有子参数
/// </summary>
public sealed class RenderStyleParameters : IEquatable<RenderStyleParameters>
{
    public LightingParameters Lighting { get; set; } = new();
    public ColorParameters Color { get; set; } = new();
    public OutlineParameters Outline { get; set; } = new();
    public MaterialParameters Material { get; set; } = new();

    /// <summary>深拷贝</summary>
    public RenderStyleParameters Clone() => new()
    {
        Lighting = Lighting.Clone(),
        Color = Color.Clone(),
        Outline = Outline.Clone(),
        Material = Material.Clone()
    };

    public bool Equals(RenderStyleParameters? other) => other is not null
        && Lighting.Equals(other.Lighting)
        && Color.Equals(other.Color)
        && Outline.Equals(other.Outline)
        && Material.Equals(other.Material);

    public override bool Equals(object? obj) => obj is RenderStyleParameters other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Lighting, Color, Outline, Material);

    /// <summary>转换为GPU Uniform结构 (128 bytes aligned)</summary>
    public StyleUniformData ToUniformData() => new()
    {
        AmbientIntensity = Lighting.AmbientIntensity,
        DiffuseIntensity = Lighting.DiffuseIntensity,
        SpecularIntensity = Lighting.SpecularIntensity,
        SpecularPower = Lighting.SpecularPower,
        LightDirection = Lighting.LightDirection,
        FillIntensity = Lighting.FillIntensity,
        TintColor = Color.TintColor,
        Saturation = Color.Saturation,
        Contrast = Color.Contrast,
        Brightness = Color.Brightness,
        OutlineColor = Outline.Color.ToVector4(),
        OutlineWidth = Outline.Width,
        OutlineEnabled = Outline.Enabled ? 1 : 0,
        Opacity = Material.Opacity,
        UseVertexColor = Material.UseVertexColor ? 1 : 0,
        OverrideColor = Material.OverrideColor.ToVector4(),
        WireframeMode = Material.WireframeMode ? 1 : 0
    };
}
