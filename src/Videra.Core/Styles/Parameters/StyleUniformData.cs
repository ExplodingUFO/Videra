using System.Numerics;
using System.Runtime.InteropServices;

namespace Videra.Core.Styles.Parameters;

/// <summary>
/// GPU Uniform Buffer ��据结构
/// 对齐到 128 bytes 以满足 GPU 常量缓冲区要求
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct StyleUniformData
{
    // 光照 (offset 0-31)
    [FieldOffset(0)] public float AmbientIntensity;
    [FieldOffset(4)] public float DiffuseIntensity;
    [FieldOffset(8)] public float SpecularIntensity;
    [FieldOffset(12)] public float SpecularPower;
    [FieldOffset(16)] public Vector3 LightDirection;
    [FieldOffset(28)] public float FillIntensity;

    // 色彩 (offset 32-60)
    [FieldOffset(32)] public Vector3 TintColor;
    [FieldOffset(44)] public float Saturation;
    [FieldOffset(48)] public float Contrast;
    [FieldOffset(52)] public float Brightness;

    // 描边 (offset 64-84)
    [FieldOffset(64)] public Vector4 OutlineColor;
    [FieldOffset(80)] public float OutlineWidth;
    [FieldOffset(84)] public int OutlineEnabled;

    // 材质 (offset 96-124)
    [FieldOffset(96)] public float Opacity;
    [FieldOffset(100)] public int UseVertexColor;
    [FieldOffset(104)] public Vector4 OverrideColor;
    [FieldOffset(120)] public int WireframeMode;
}
