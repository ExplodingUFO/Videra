using System.Linq.Expressions;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;

namespace Videra.Core.Styles.Services;

/// <summary>
/// 渲染风格服务接口 - 定义风格管理的核心能力
/// </summary>
public interface IRenderStyleService
{
    /// <summary>当前风格参数</summary>
    RenderStyleParameters CurrentParameters { get; }

    /// <summary>当前预设类型</summary>
    RenderStylePreset CurrentPreset { get; }

    /// <summary>风格参数变更事件</summary>
    event EventHandler<StyleChangedEventArgs>? StyleChanged;

    /// <summary>应用预设</summary>
    void ApplyPreset(RenderStylePreset preset);

    /// <summary>更新参数 (触发StyleChanged事件)</summary>
    void UpdateParameters(RenderStyleParameters parameters);

    /// <summary>更新单个参数值</summary>
    void UpdateParameter<T>(Expression<Func<RenderStyleParameters, T>> selector, T value);

    /// <summary>导出为JSON字符串</summary>
    string ExportToJson();

    /// <summary>从JSON字符串导入</summary>
    void ImportFromJson(string json);

    /// <summary>保存到文件</summary>
    Task SaveToFileAsync(string filePath);

    /// <summary>从文件加载</summary>
    Task LoadFromFileAsync(string filePath);
}
