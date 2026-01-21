using System.Numerics;
using Videra.Core.Geometry;
using Videra.Core.Styles.Parameters;

namespace Videra.Core.Styles.Presets;

/// <summary>
/// 预设参数工厂 - 提供各风格的默认配置
/// </summary>
public static class RenderStylePresets
{
    /// <summary>根据预设类型获取参数</summary>
    public static RenderStyleParameters GetParameters(RenderStylePreset preset) => preset switch
    {
        RenderStylePreset.Realistic => CreateRealistic(),
        RenderStylePreset.Tech => CreateTech(),
        RenderStylePreset.Cartoon => CreateCartoon(),
        RenderStylePreset.XRay => CreateXRay(),
        RenderStylePreset.Clay => CreateClay(),
        RenderStylePreset.Wireframe => CreateWireframe(),
        _ => new RenderStyleParameters()
    };

    /// <summary>真实 - 物理真实的光照</summary>
    public static RenderStyleParameters CreateRealistic() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 0.2f,
            DiffuseIntensity = 0.8f,
            SpecularIntensity = 0.5f,
            SpecularPower = 32f,
            LightDirection = Vector3.Normalize(new(0.5f, 1.0f, 0.3f))
        },
        Color = new()
        {
            TintColor = Vector3.One,
            Saturation = 1.0f,
            Contrast = 1.0f,
            Brightness = 0f
        },
        Outline = new() { Enabled = false },
        Material = new()
        {
            Opacity = 1.0f,
            UseVertexColor = true,
            WireframeMode = false
        }
    };

    /// <summary>科技 - 蓝色调、科幻风</summary>
    public static RenderStyleParameters CreateTech() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 0.4f,
            DiffuseIntensity = 0.6f,
            SpecularIntensity = 0.8f,
            SpecularPower = 64f
        },
        Color = new()
        {
            TintColor = new(0.2f, 0.5f, 1.0f), // 蓝色调
            Saturation = 0.7f,
            Contrast = 1.3f,
            Brightness = 0.05f
        },
        Outline = new()
        {
            Enabled = true,
            Color = new(0f, 1f, 1f, 1f), // Cyan
            Width = 1.0f
        },
        Material = new()
        {
            Opacity = 1.0f,
            UseVertexColor = true
        }
    };

    /// <summary>卡通 - 描边、色块分明</summary>
    public static RenderStyleParameters CreateCartoon() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 0.5f,
            DiffuseIntensity = 0.5f,
            SpecularIntensity = 0f, // 无高光
            SpecularPower = 1f
        },
        Color = new()
        {
            TintColor = Vector3.One,
            Saturation = 1.3f,
            Contrast = 1.5f,
            Brightness = 0f
        },
        Outline = new()
        {
            Enabled = true,
            Color = RgbaFloat.Black,
            Width = 2.0f
        },
        Material = new()
        {
            UseVertexColor = true
        }
    };

    /// <summary>X光 - 半透明透视</summary>
    public static RenderStyleParameters CreateXRay() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 0.6f,
            DiffuseIntensity = 0.4f,
            SpecularIntensity = 0.2f
        },
        Color = new()
        {
            TintColor = new(0.3f, 0.8f, 1.0f), // 浅蓝
            Saturation = 0.5f,
            Contrast = 1.2f
        },
        Outline = new()
        {
            Enabled = true,
            Color = new(0.5f, 0.9f, 1f, 0.8f),
            Width = 1.0f
        },
        Material = new()
        {
            Opacity = 0.5f,
            UseVertexColor = false,
            OverrideColor = new(0.4f, 0.7f, 0.9f, 0.5f)
        }
    };

    /// <summary>黏土 - 单色哑光</summary>
    public static RenderStyleParameters CreateClay() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 0.4f,
            DiffuseIntensity = 0.6f,
            SpecularIntensity = 0.1f,
            SpecularPower = 8f
        },
        Color = new()
        {
            TintColor = Vector3.One,
            Saturation = 0f, // 去色
            Contrast = 1.0f
        },
        Outline = new() { Enabled = false },
        Material = new()
        {
            Opacity = 1.0f,
            UseVertexColor = false,
            OverrideColor = new(0.85f, 0.82f, 0.78f, 1f) // 黏土色
        }
    };

    /// <summary>线框</summary>
    public static RenderStyleParameters CreateWireframe() => new()
    {
        Lighting = new()
        {
            AmbientIntensity = 1.0f,
            DiffuseIntensity = 0f,
            SpecularIntensity = 0f
        },
        Color = new()
        {
            TintColor = Vector3.One,
            Saturation = 1.0f,
            Contrast = 1.0f
        },
        Outline = new()
        {
            Enabled = false,
            Color = RgbaFloat.White
        },
        Material = new()
        {
            Opacity = 1.0f,
            UseVertexColor = true,
            WireframeMode = true
        }
    };
}
