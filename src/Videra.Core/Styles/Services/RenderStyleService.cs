using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Videra.Core.Styles.Parameters;
using Videra.Core.Styles.Presets;
using Videra.Core.Styles.Serialization;

namespace Videra.Core.Styles.Services;

/// <summary>
/// 渲染风格服务实现
/// </summary>
public sealed class RenderStyleService : IRenderStyleService, INotifyPropertyChanged
{
    private RenderStyleParameters _currentParameters;
    private RenderStylePreset _currentPreset;

    public RenderStyleParameters CurrentParameters
    {
        get => _currentParameters;
        private set
        {
            if (!_currentParameters.Equals(value))
            {
                _currentParameters = value;
                OnPropertyChanged();
                OnStyleChanged();
            }
        }
    }

    public RenderStylePreset CurrentPreset
    {
        get => _currentPreset;
        private set
        {
            if (_currentPreset != value)
            {
                _currentPreset = value;
                OnPropertyChanged();
            }
        }
    }

    public event EventHandler<StyleChangedEventArgs>? StyleChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public RenderStyleService()
    {
        _currentPreset = RenderStylePreset.Realistic;
        _currentParameters = RenderStylePresets.GetParameters(_currentPreset);
    }

    public void ApplyPreset(RenderStylePreset preset)
    {
        CurrentPreset = preset;
        CurrentParameters = RenderStylePresets.GetParameters(preset);
    }

    public void UpdateParameters(RenderStyleParameters parameters)
    {
        CurrentPreset = RenderStylePreset.Custom;
        CurrentParameters = parameters.Clone();
    }

    public void UpdateParameter<T>(Expression<Func<RenderStyleParameters, T>> selector, T value)
    {
        // 使用表达式树更新单个参数
        var newParams = _currentParameters.Clone();
        var memberExpr = (MemberExpression)selector.Body;
        var property = (PropertyInfo)memberExpr.Member;

        // 处理嵌套属性 (如 Lighting.AmbientIntensity)
        if (memberExpr.Expression is MemberExpression parentExpr)
        {
            var parentProperty = (PropertyInfo)parentExpr.Member;
            var parentValue = parentProperty.GetValue(newParams);
            property.SetValue(parentValue, value);
        }
        else
        {
            property.SetValue(newParams, value);
        }

        CurrentPreset = RenderStylePreset.Custom;
        CurrentParameters = newParams;
    }

    public string ExportToJson()
    {
        return StyleJsonConverter.Serialize(_currentParameters, _currentPreset);
    }

    public void ImportFromJson(string json)
    {
        var (parameters, preset) = StyleJsonConverter.Deserialize(json);
        CurrentPreset = preset;
        CurrentParameters = parameters;
    }

    public async Task SaveToFileAsync(string filePath)
    {
        var json = ExportToJson();
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task LoadFromFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        ImportFromJson(json);
    }

    private void OnStyleChanged()
    {
        StyleChanged?.Invoke(this, new StyleChangedEventArgs(_currentParameters, _currentPreset));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
