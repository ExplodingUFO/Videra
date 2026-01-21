using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;

namespace Videra.Core.Styles.Services;

/// <summary>风格变更事件参数</summary>
public sealed class StyleChangedEventArgs : EventArgs
{
    public RenderStyleParameters Parameters { get; }
    public RenderStylePreset Preset { get; }
    public StyleUniformData UniformData { get; }

    public StyleChangedEventArgs(RenderStyleParameters parameters, RenderStylePreset preset)
    {
        Parameters = parameters;
        Preset = preset;
        UniformData = parameters.ToUniformData();
    }
}
