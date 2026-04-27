using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 光照相关参数
/// </summary>
public sealed class LightingParameters : IEquatable<LightingParameters>
{
    /// <summary>环境光强度 [0-1]</summary>
    public float AmbientIntensity { get; set; } = 0.3f;

    /// <summary>漫反射强度 [0-1]</summary>
    public float DiffuseIntensity { get; set; } = 0.7f;

    /// <summary>高光强度 [0-1]</summary>
    public float SpecularIntensity { get; set; } = 0.5f;

    /// <summary>高光锐度 [1-256]</summary>
    public float SpecularPower { get; set; } = 32f;

    /// <summary>主光源方向 (归一化向量)</summary>
    public Vector3 LightDirection { get; set; } = Vector3.Normalize(new(0.5f, 1.0f, 0.3f));

    /// <summary>填充/包裹光强度 [0-1]</summary>
    [SuppressMessage("Usage", "CA1805:Do not initialize unnecessarily", Justification = "The explicit default is part of the documented native-validation contract.")]
    public float FillIntensity { get; set; } = 0f;

    public LightingParameters Clone() => (LightingParameters)MemberwiseClone();

    public bool Equals(LightingParameters? other) => other is not null
        && AmbientIntensity == other.AmbientIntensity
        && DiffuseIntensity == other.DiffuseIntensity
        && SpecularIntensity == other.SpecularIntensity
        && SpecularPower == other.SpecularPower
        && LightDirection == other.LightDirection
        && FillIntensity == other.FillIntensity;

    public override bool Equals(object? obj) => obj is LightingParameters other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        AmbientIntensity, DiffuseIntensity, SpecularIntensity, SpecularPower, LightDirection, FillIntensity);
}
