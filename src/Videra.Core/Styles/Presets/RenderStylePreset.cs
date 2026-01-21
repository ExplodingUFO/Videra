namespace Videra.Core.Styles.Presets;

/// <summary>
/// 渲染风格预设枚举
/// </summary>
public enum RenderStylePreset
{
    /// <summary>真实 - 接近物理真实的光照</summary>
    Realistic,

    /// <summary>科技 - 蓝色调、科幻风格</summary>
    Tech,

    /// <summary>卡通 - 描边、色块分明</summary>
    Cartoon,

    /// <summary>X光 - 半透明透视效果</summary>
    XRay,

    /// <summary>黏土 - 单色哑光材质</summary>
    Clay,

    /// <summary>线框 - 纯线框显示</summary>
    Wireframe,

    /// <summary>自定义 - 用户调整后的参数</summary>
    Custom
}
