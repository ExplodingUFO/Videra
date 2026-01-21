# 渲染风格预设系统设计文档

## 1. 概述

### 1.1 背景
当前 Videra 3D 引擎的渲染颜色较为单调，仅支持基础的顶点着色和简单 Phong 光照。为了满足数字孪生系统在工业产品展示、建筑可视化和通用3D查看器场景下的需求，需要增加渲染风格预设系统。

### 1.2 目标
- 提供 6 种预设风格：真实(Realistic)、科技(Tech)、卡通(Cartoon)、X光(XRay)、黏土(Clay)、线框(Wireframe)
- 支持下拉菜单快速切换预设
- 支持滑块实时微调参数
- 支持 JSON 配置导入/导出

### 1.3 设计原则
- **低耦合**：风格系统作为独立模块，与渲染核心通过接口和事件通信
- **高内聚**：所有风格相关代码集中在 `Videra.Core/Styles/` 目录
- **可扩展**：新增风格只需添加预设配置，无需修改核心逻辑
- **跨平台**：参数结构与平台无关，各平台着色器独立实现

---

## 2. 架构设计

### 2.1 模块结构

```
Videra.Core/
├── Styles/                          # 风格系统模块 (新增)
│   ├── Parameters/                  # 参数定义
│   │   ├── RenderStyleParameters.cs # 主参数类
│   │   ├── LightingParameters.cs    # 光照参数
│   │   ├── ColorParameters.cs       # 色彩参数
│   │   ├── OutlineParameters.cs     # 描边参数
│   │   └── MaterialParameters.cs    # 材质参数
│   ├── Presets/                     # 预设定义
│   │   ├── RenderStylePreset.cs     # 预设枚举
│   │   └── RenderStylePresets.cs    # 预设工厂
│   ├── Services/                    # 服务层
│   │   ├── IRenderStyleService.cs   # 服务接口
│   │   └── RenderStyleService.cs    # 服务实现
│   └── Serialization/               # 序列化
│       └── StyleJsonConverter.cs    # JSON转换器
├── Graphics/
│   ├── Abstractions/
│   │   └── IStyleUniformBuffer.cs   # 风格Uniform接口 (新增)
│   └── VideraEngine.cs              # 集成风格服务
```

### 2.2 依赖关系

```
┌─────────────────────────────────────────────────────────────┐
│                    Videra.Avalonia                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  VideraView                                          │   │
│  │  - RenderStyleProperty (DependencyProperty)          │   │
│  │  - RenderStyleParametersProperty                     │   │
│  └───────────────────────┬─────────────────────────────┘   │
└──────────────────────────┼──────────────────────────────────┘
                           │ 绑定
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                      Videra.Core                            │
│  ┌─────────────────┐     ┌─────────────────────────────┐   │
│  │  VideraEngine   │────▶│   IRenderStyleService       │   │
│  │                 │     │   (通过接口依赖，非具体实现)   │   │
│  └────────┬────────┘     └──────────────┬──────────────┘   │
│           │                             │                   │
│           │ 更新Uniform                  │ StyleChanged事件  │
│           ▼                             ▼                   │
│  ┌─────────────────┐     ┌─────────────────────────────┐   │
│  │ ICommandExecutor│     │  RenderStyleParameters      │   │
│  │ (绑定Buffer)    │     │  (纯数据，无依赖)            │   │
│  └─────────────────┘     └─────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼ 平台实现
┌─────────────────────────────────────────────────────────────┐
│  Platform.Windows / Platform.macOS / Platform.Linux         │
│  - StyleUniformBuffer 实现                                  │
│  - 着色器更新 (HLSL / Metal / GLSL)                         │
└─────────────────────────────────────────────────────────────┘
```

### 2.3 数据流

```
用户操作 (下拉菜单/滑块/导入JSON)
         │
         ▼
┌─────────────────────────┐
│  IRenderStyleService    │
│  - ApplyPreset()        │
│  - UpdateParameter()    │
│  - ImportFromJson()     │
└��──────────┬─────────────┘
            │ 触发 StyleChanged 事件
            ▼
┌─────────────────────────┐
│  VideraEngine           │
│  - OnStyleChanged()     │
│  - UpdateStyleBuffer()  │
└───────────┬─────────────┘
            │ 更新 GPU Buffer
            ▼
┌─────────────────────────┐
│  着色器 (各平台)         │
│  - 读取 StyleParams     │
│  - 应用光照/色彩/描边    │
└─────────────────────────┘
```

---

## 3. 详细设计

### 3.1 参数类设计

将参数按职责拆分为4个子类，保持单一职责原则：

#### 3.1.1 LightingParameters (光照参数)
```csharp
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

    public LightingParameters Clone() => (LightingParameters)MemberwiseClone();

    public bool Equals(LightingParameters? other) => other is not null
        && AmbientIntensity == other.AmbientIntensity
        && DiffuseIntensity == other.DiffuseIntensity
        && SpecularIntensity == other.SpecularIntensity
        && SpecularPower == other.SpecularPower
        && LightDirection == other.LightDirection;
}
```

#### 3.1.2 ColorParameters (色彩参数)
```csharp
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
    public float Brightness { get; set; } = 0f;

    public ColorParameters Clone() => (ColorParameters)MemberwiseClone();

    public bool Equals(ColorParameters? other) => other is not null
        && TintColor == other.TintColor
        && Saturation == other.Saturation
        && Contrast == other.Contrast
        && Brightness == other.Brightness;
}
```

#### 3.1.3 OutlineParameters (描边参数)
```csharp
namespace Videra.Core.Styles.Parameters;

/// <summary>
/// 描边/轮廓线参数
/// </summary>
public sealed class OutlineParameters : IEquatable<OutlineParameters>
{
    /// <summary>是否启用描边</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>描边颜色</summary>
    public RgbaFloat Color { get; set; } = RgbaFloat.Black;

    /// <summary>描边宽度 (像素) [0.5-5]</summary>
    public float Width { get; set; } = 1.0f;

    public OutlineParameters Clone() => new()
    {
        Enabled = Enabled,
        Color = Color,
        Width = Width
    };

    public bool Equals(OutlineParameters? other) => other is not null
        && Enabled == other.Enabled
        && Color.Equals(other.Color)
        && Width == other.Width;
}
```

#### 3.1.4 MaterialParameters (材质参数)
```csharp
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
    public bool WireframeMode { get; set; } = false;

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
}
```

#### 3.1.5 RenderStyleParameters (组合类)
```csharp
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

    /// <summary>转换为GPU Uniform结构 (128 bytes aligned)</summary>
    public StyleUniformData ToUniformData() => new()
    {
        AmbientIntensity = Lighting.AmbientIntensity,
        DiffuseIntensity = Lighting.DiffuseIntensity,
        SpecularIntensity = Lighting.SpecularIntensity,
        SpecularPower = Lighting.SpecularPower,
        LightDirection = Lighting.LightDirection,
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
```

#### 3.1.6 StyleUniformData (GPU数据结构)
```csharp
namespace Videra.Core.Styles.Parameters;

using System.Runtime.InteropServices;

/// <summary>
/// GPU Uniform Buffer 数据结构
/// 对齐到 128 bytes 以满足 GPU 常量缓冲区要求
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct StyleUniformData
{
    // 光照 (offset 0-28)
    [FieldOffset(0)]  public float AmbientIntensity;
    [FieldOffset(4)]  public float DiffuseIntensity;
    [FieldOffset(8)]  public float SpecularIntensity;
    [FieldOffset(12)] public float SpecularPower;
    [FieldOffset(16)] public Vector3 LightDirection;

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
    [FieldOffset(96)]  public float Opacity;
    [FieldOffset(100)] public int UseVertexColor;
    [FieldOffset(104)] public Vector4 OverrideColor;
    [FieldOffset(120)] public int WireframeMode;
}
```

---

### 3.2 预设定义

#### 3.2.1 RenderStylePreset (枚举)
```csharp
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
```

#### 3.2.2 RenderStylePresets (预设工厂)
```csharp
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
```

---

### 3.3 服务层设计

#### 3.3.1 IRenderStyleService (服务接口)
```csharp
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
```

#### 3.3.2 RenderStyleService (服务实现)
```csharp
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
```

---

### 3.4 JSON 序列化

#### 3.4.1 StyleJsonConverter
```csharp
namespace Videra.Core.Styles.Serialization;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// 风格参数 JSON 序列化转换器
/// </summary>
public static class StyleJsonConverter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new Vector3JsonConverter(),
            new RgbaFloatJsonConverter(),
            new JsonStringEnumConverter()
        }
    };

    /// <summary>序列化为JSON</summary>
    public static string Serialize(RenderStyleParameters parameters, RenderStylePreset preset)
    {
        var wrapper = new StyleExportWrapper
        {
            Version = "1.0",
            Preset = preset,
            Parameters = parameters
        };
        return JsonSerializer.Serialize(wrapper, Options);
    }

    /// <summary>从JSON反序列化</summary>
    public static (RenderStyleParameters Parameters, RenderStylePreset Preset) Deserialize(string json)
    {
        var wrapper = JsonSerializer.Deserialize<StyleExportWrapper>(json, Options)
            ?? throw new JsonException("Invalid style JSON");
        return (wrapper.Parameters, wrapper.Preset);
    }

    /// <summary>导出包装类</summary>
    private sealed class StyleExportWrapper
    {
        public string Version { get; set; } = "1.0";
        public RenderStylePreset Preset { get; set; }
        public RenderStyleParameters Parameters { get; set; } = new();
    }
}

/// <summary>Vector3 JSON 转换器</summary>
internal sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        reader.Read();
        var x = reader.GetSingle();
        reader.Read();
        var y = reader.GetSingle();
        reader.Read();
        var z = reader.GetSingle();
        reader.Read(); // EndArray

        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}

/// <summary>RgbaFloat JSON 转换器</summary>
internal sealed class RgbaFloatJsonConverter : JsonConverter<RgbaFloat>
{
    public override RgbaFloat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        reader.Read();
        var r = reader.GetSingle();
        reader.Read();
        var g = reader.GetSingle();
        reader.Read();
        var b = reader.GetSingle();
        reader.Read();
        var a = reader.GetSingle();
        reader.Read(); // EndArray

        return new RgbaFloat(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, RgbaFloat value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}
```

#### 3.4.2 JSON 示例输出
```json
{
  "version": "1.0",
  "preset": "Tech",
  "parameters": {
    "lighting": {
      "ambientIntensity": 0.4,
      "diffuseIntensity": 0.6,
      "specularIntensity": 0.8,
      "specularPower": 64,
      "lightDirection": [0.408, 0.816, 0.408]
    },
    "color": {
      "tintColor": [0.2, 0.5, 1.0],
      "saturation": 0.7,
      "contrast": 1.3,
      "brightness": 0.05
    },
    "outline": {
      "enabled": true,
      "color": [0, 1, 1, 1],
      "width": 1.0
    },
    "material": {
      "opacity": 1.0,
      "useVertexColor": true,
      "overrideColor": [0.75, 0.75, 0.75, 1],
      "wireframeMode": false
    }
  }
}
```

---

## 4. 集成设计

### 4.1 VideraEngine 集成

```csharp
// Videra.Core/Graphics/VideraEngine.cs (修改)

public sealed class VideraEngine : IDisposable
{
    // 新增：风格服务
    private readonly IRenderStyleService _styleService;
    private IBuffer? _styleUniformBuffer;

    // 新增：风格相关属性
    public IRenderStyleService StyleService => _styleService;

    public VideraEngine(IGraphicsBackend backend, IRenderStyleService? styleService = null)
    {
        // ... 现有初始化代码 ...

        _styleService = styleService ?? new RenderStyleService();
        _styleService.StyleChanged += OnStyleChanged;
    }

    private void OnStyleChanged(object? sender, StyleChangedEventArgs e)
    {
        // 更新 GPU Uniform Buffer
        UpdateStyleUniformBuffer(e.UniformData);
    }

    private void UpdateStyleUniformBuffer(StyleUniformData data)
    {
        _styleUniformBuffer ??= ResourceFactory.CreateUniformBuffer<StyleUniformData>();
        _styleUniformBuffer.UpdateData(data);
    }

    public void Draw()
    {
        // ... 现有绘制逻辑 ...

        // 绑定风格 Uniform Buffer (slot 3)
        Executor.SetUniformBuffer(3, _styleUniformBuffer);

        // 继续绘制对象
        foreach (var obj in _objects)
        {
            // ... 绘制代码 ...
        }
    }

    public void Dispose()
    {
        _styleService.StyleChanged -= OnStyleChanged;
        _styleUniformBuffer?.Dispose();
        // ... 其他清理 ...
    }
}
```

### 4.2 ICommandExecutor 接口扩展

```csharp
// Videra.Core/Graphics/Abstractions/ICommandExecutor.cs (修改)

public interface ICommandExecutor
{
    // 现有方法...

    /// <summary>绑定 Uniform Buffer 到指定槽位</summary>
    void SetUniformBuffer(int slot, IBuffer buffer);
}
```

---

## 5. 着色器设计

### 5.1 HLSL (D3D11 - Windows)

```hlsl
// Shaders/StyleShader.hlsl

// 现有 Uniform Buffers
cbuffer CameraBuffer : register(b1)
{
    float4x4 ViewMatrix;
    float4x4 ProjectionMatrix;
};

cbuffer WorldBuffer : register(b2)
{
    float4x4 WorldMatrix;
};

// 新增：风格参数 Buffer
cbuffer StyleBuffer : register(b3)
{
    // 光照
    float AmbientIntensity;
    float DiffuseIntensity;
    float SpecularIntensity;
    float SpecularPower;
    float3 LightDirection;
    float _pad0;

    // 色彩
    float3 TintColor;
    float Saturation;
    float Contrast;
    float Brightness;
    float2 _pad1;

    // 描边
    float4 OutlineColor;
    float OutlineWidth;
    int OutlineEnabled;
    float2 _pad2;

    // 材质
    float Opacity;
    int UseVertexColor;
    float2 _pad3;
    float4 OverrideColor;
    int WireframeMode;
    float3 _pad4;
};

struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 WorldNormal : NORMAL;
    float4 Color : COLOR;
    float3 WorldPos : TEXCOORD0;
};

PSInput VSMain(VSInput input)
{
    PSInput output;

    float4 worldPos = mul(float4(input.Position, 1.0), WorldMatrix);
    float4 viewPos = mul(worldPos, ViewMatrix);
    output.Position = mul(viewPos, ProjectionMatrix);

    output.WorldNormal = normalize(mul(input.Normal, (float3x3)WorldMatrix));
    output.Color = UseVertexColor ? input.Color : OverrideColor;
    output.WorldPos = worldPos.xyz;

    return output;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float3 normal = normalize(input.WorldNormal);
    float3 lightDir = normalize(LightDirection);

    // 基础光照
    float ambient = AmbientIntensity;
    float diffuse = max(dot(normal, lightDir), 0.0) * DiffuseIntensity;

    // 高光 (Blinn-Phong)
    float3 viewDir = normalize(-input.WorldPos);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(max(dot(normal, halfDir), 0.0), SpecularPower) * SpecularIntensity;

    float3 lighting = ambient + diffuse + specular;

    // 应用光照
    float3 color = input.Color.rgb * lighting;

    // 色调叠加
    color *= TintColor;

    // 饱和度调整
    float grey = dot(color, float3(0.299, 0.587, 0.114));
    color = lerp(float3(grey, grey, grey), color, Saturation);

    // 对比度调整
    color = (color - 0.5) * Contrast + 0.5;

    // 亮度调整
    color += Brightness;

    return float4(saturate(color), input.Color.a * Opacity);
}
```

### 5.2 Metal (macOS)

```metal
// Shaders/StyleShader.metal

#include <metal_stdlib>
using namespace metal;

struct StyleParams
{
    float ambientIntensity;
    float diffuseIntensity;
    float specularIntensity;
    float specularPower;
    float3 lightDirection;
    float _pad0;

    float3 tintColor;
    float saturation;
    float contrast;
    float brightness;
    float2 _pad1;

    float4 outlineColor;
    float outlineWidth;
    int outlineEnabled;
    float2 _pad2;

    float opacity;
    int useVertexColor;
    float2 _pad3;
    float4 overrideColor;
    int wireframeMode;
    float3 _pad4;
};

struct VertexIn
{
    float3 position [[attribute(0)]];
    float3 normal [[attribute(1)]];
    float4 color [[attribute(2)]];
};

struct VertexOut
{
    float4 position [[position]];
    float3 worldNormal;
    float4 color;
    float3 worldPos;
};

vertex VertexOut vertexShader(
    VertexIn in [[stage_in]],
    constant float4x4& world [[buffer(2)]],
    constant float4x4& view [[buffer(1)]],
    constant float4x4& projection [[buffer(1)]],
    constant StyleParams& style [[buffer(3)]])
{
    VertexOut out;

    float4 worldPos = world * float4(in.position, 1.0);
    out.position = projection * view * worldPos;
    out.worldNormal = normalize((world * float4(in.normal, 0.0)).xyz);
    out.color = style.useVertexColor ? in.color : style.overrideColor;
    out.worldPos = worldPos.xyz;

    return out;
}

fragment float4 fragmentShader(
    VertexOut in [[stage_in]],
    constant StyleParams& style [[buffer(3)]])
{
    float3 normal = normalize(in.worldNormal);
    float3 lightDir = normalize(style.lightDirection);

    // 基础光照
    float ambient = style.ambientIntensity;
    float diffuse = max(dot(normal, lightDir), 0.0) * style.diffuseIntensity;

    // 高光
    float3 viewDir = normalize(-in.worldPos);
    float3 halfDir = normalize(lightDir + viewDir);
    float specular = pow(max(dot(normal, halfDir), 0.0), style.specularPower) * style.specularIntensity;

    float lighting = ambient + diffuse + specular;
    float3 color = in.color.rgb * lighting;

    // 色调、饱和度、对比度、亮度
    color *= style.tintColor;
    float grey = dot(color, float3(0.299, 0.587, 0.114));
    color = mix(float3(grey), color, style.saturation);
    color = (color - 0.5) * style.contrast + 0.5;
    color += style.brightness;

    return float4(saturate(color), in.color.a * style.opacity);
}
```

### 5.3 GLSL (Vulkan - Linux)

着色器结构与 HLSL 类似，使用 `layout(binding = 3)` 绑定风格参数 Uniform Buffer。

---

## 6. UI 集成设计

### 6.1 VideraView 属性扩展

```csharp
// Videra.Avalonia/Controls/VideraView.cs (修改)

public partial class VideraView : Control
{
    // 新增：风格预设属性
    public static readonly StyledProperty<RenderStylePreset> RenderStyleProperty =
        AvaloniaProperty.Register<VideraView, RenderStylePreset>(
            nameof(RenderStyle),
            defaultValue: RenderStylePreset.Realistic);

    // 新增：自定义参数属性 (用于滑块绑定)
    public static readonly StyledProperty<RenderStyleParameters?> RenderStyleParametersProperty =
        AvaloniaProperty.Register<VideraView, RenderStyleParameters?>(
            nameof(RenderStyleParameters));

    public RenderStylePreset RenderStyle
    {
        get => GetValue(RenderStyleProperty);
        set => SetValue(RenderStyleProperty, value);
    }

    public RenderStyleParameters? RenderStyleParameters
    {
        get => GetValue(RenderStyleParametersProperty);
        set => SetValue(RenderStyleParametersProperty, value);
    }

    // 属性变更处理
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == RenderStyleProperty)
        {
            _engine?.StyleService.ApplyPreset(RenderStyle);
        }
        else if (change.Property == RenderStyleParametersProperty && RenderStyleParameters != null)
        {
            _engine?.StyleService.UpdateParameters(RenderStyleParameters);
        }
    }
}
```

### 6.2 XAML 使用示例

```xml
<!-- 基础用法：预设切换 -->
<v:VideraView RenderStyle="Tech" />

<!-- 绑定用法：下拉菜单 + 滑块 -->
<StackPanel>
    <ComboBox ItemsSource="{Binding AvailablePresets}"
              SelectedItem="{Binding SelectedPreset}" />

    <v:VideraView RenderStyle="{Binding SelectedPreset}"
                  RenderStyleParameters="{Binding CustomParameters}" />

    <!-- 滑块微调 (当选择 Custom 时显示) -->
    <StackPanel IsVisible="{Binding IsCustomMode}">
        <Slider Value="{Binding CustomParameters.Lighting.AmbientIntensity}"
                Minimum="0" Maximum="1" />
        <Slider Value="{Binding CustomParameters.Color.Saturation}"
                Minimum="0" Maximum="2" />
        <!-- ... 更多滑块 ... -->
    </StackPanel>

    <!-- 导入/导出按钮 -->
    <Button Command="{Binding ExportStyleCommand}" Content="导出风格" />
    <Button Command="{Binding ImportStyleCommand}" Content="导入风格" />
</StackPanel>
```

---

## 7. 文件清单

### 7.1 新增文件

| 路径 | 说明 |
|------|------|
| `Videra.Core/Styles/Parameters/LightingParameters.cs` | 光照参数类 |
| `Videra.Core/Styles/Parameters/ColorParameters.cs` | 色彩参数类 |
| `Videra.Core/Styles/Parameters/OutlineParameters.cs` | 描边参数类 |
| `Videra.Core/Styles/Parameters/MaterialParameters.cs` | 材质参数类 |
| `Videra.Core/Styles/Parameters/RenderStyleParameters.cs` | 聚合参数类 |
| `Videra.Core/Styles/Parameters/StyleUniformData.cs` | GPU数据结构 |
| `Videra.Core/Styles/Presets/RenderStylePreset.cs` | 预设枚举 |
| `Videra.Core/Styles/Presets/RenderStylePresets.cs` | 预设工厂 |
| `Videra.Core/Styles/Services/IRenderStyleService.cs` | 服务接口 |
| `Videra.Core/Styles/Services/RenderStyleService.cs` | 服务实现 |
| `Videra.Core/Styles/Services/StyleChangedEventArgs.cs` | 事件参数 |
| `Videra.Core/Styles/Serialization/StyleJsonConverter.cs` | JSON序列化 |

### 7.2 修改文件

| 路径 | 修改内容 |
|------|----------|
| `Videra.Core/Graphics/VideraEngine.cs` | 集成风格服务 |
| `Videra.Core/Graphics/Abstractions/ICommandExecutor.cs` | 添加 SetUniformBuffer |
| `Videra.Avalonia/Controls/VideraView.cs` | 添加风格属性 |
| `Videra.Platform.Windows/D3D11Backend.cs` | 支持风格Uniform Buffer |
| `Videra.Platform.Windows/Shaders/*.hlsl` | 更新着色器 |
| `Videra.Platform.macOS/Shaders.metal` | 更新着色器 |
| `Videra.Platform.Linux/VulkanBackend.cs` | 支持风格Uniform Buffer |

---

## 8. 实现步骤

1. **创建参数类** - 在 `Videra.Core/Styles/Parameters/` 创建所有参数类
2. **创建预设** - 实现预设枚举和工厂类
3. **实现服务** - 创建 `IRenderStyleService` 接口和实现
4. **JSON序列化** - 实现导入导出功能
5. **引擎集成** - 修改 `VideraEngine` 集成风格服务
6. **着色器更新** - 更新各平台着色器支持风格参数
7. **UI集成** - 扩展 `VideraView` 属性
8. **测试验证** - 验证各预设效果和参数调节

---

## 9. 扩展考虑

### 9.1 后续可扩展功能 (本次不实现)
- 描边渲染 (需要多Pass渲染)
- 卡通色阶量化 (离散光照)
- 后处理效果 (需要全屏Pass)
- PBR材质支持

### 9.2 兼容性
- 风格系统向后兼容，不影响现有渲染行为
- 默认使用 `Realistic` 预设，与当前渲染效果接近
