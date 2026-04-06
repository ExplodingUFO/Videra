# 渲染风格系统 - 着色器更新与Demo程序实施计划

## 1. 当前状态分析

### 1.1 已完成部分
- `Videra.Core/Styles/` 模块：参数类、预设、服务、JSON序列化 ✅
- `VideraEngine` 集成风格服务：创建 `_styleUniformBuffer`，绑定到 slot 3 ✅
- `VideraView` 添加 `RenderStyle` 和 `RenderStyleParameters` 属性 ✅

### 1.2 待完成部分
- **着色器未更新**：当前着色器只是简单的颜色传递，未读取 StyleBuffer
- **Demo程序未添加风格切换UI**：需要下拉菜单和滑块控件

---

## 2. 着色器更新计划

### 2.1 Windows D3D11 着色器
**文件**：`D3D11ResourceFactory.cs` 中的 `GetShaderSource()` 方法

**修改内容**：
```hlsl
// 新增 StyleBuffer (register b3)
cbuffer StyleBuffer : register(b3)
{
    // 光照
    float ambientIntensity;
    float diffuseIntensity;
    float specularIntensity;
    float specularPower;
    float3 lightDirection;
    float _pad0;

    // 色彩
    float3 tintColor;
    float saturation;
    float contrast;
    float brightness;
    float2 _pad1;

    // 描边
    float4 outlineColor;
    float outlineWidth;
    int outlineEnabled;
    float2 _pad2;

    // 材质
    float opacity;
    int useVertexColor;
    float2 _pad3;
    float4 overrideColor;
    int wireframeMode;
    float3 _pad4;
};

// VSOutput 需要添加 worldNormal 和 worldPos
struct VSOutput
{
    float4 position : SV_POSITION;
    float3 worldNormal : NORMAL;
    float4 color : COLOR;
    float3 worldPos : TEXCOORD0;
};

// 片段着色器实现光照和色彩效果
```

### 2.2 macOS Metal 着色器
**文件**：`src/Videra.Platform.macOS/Shaders.metal`

**修改内容**：
- 添加 `StyleParams` 结构体
- 片段着色器读取 `[[buffer(3)]]`
- 实现与 HLSL 相同的光照和色彩计算

### 2.3 D3D11CommandExecutor 修改
**文件**：`D3D11CommandExecutor.cs`

**修改内容**：
- `SetVertexBuffer` 方法需要支持将 Constant Buffer 绑定到 Pixel Shader
- 或添加新方法 `SetPixelShaderConstantBuffer(int slot, IBuffer buffer)`

---

## 3. Demo程序更新计划

### 3.1 MainWindowViewModel 更新
**文件**：`samples/Videra.Demo/ViewModels/MainWindowViewModel.cs`

**新增属性**：
```csharp
// 渲染风格
[ObservableProperty]
private RenderStylePreset _renderStyle = RenderStylePreset.Realistic;

// 可用预设列表 (用于 ComboBox)
public IEnumerable<RenderStylePreset> AvailablePresets =>
    Enum.GetValues<RenderStylePreset>().Where(p => p != RenderStylePreset.Custom);

// 是否显示自定义参数面板
public bool IsCustomMode => RenderStyle == RenderStylePreset.Custom;

// 自定义参数 (当用户调整滑块时使用)
[ObservableProperty]
private RenderStyleParameters _customParameters = new();

// 导入/导出命令
[RelayCommand] private async Task ExportStyleAsync();
[RelayCommand] private async Task ImportStyleAsync();
```

### 3.2 MainWindow.axaml 更新
**文件**：`samples/Videra.Demo/Views/MainWindow.axaml`

**在 ENVIRONMENT section 下添加**：
```xml
<StackPanel Spacing="10">
    <TextBlock Text="RENDER STYLE" FontWeight="Bold" Foreground="#007ACC" FontSize="12"/>

    <!-- 预设下拉菜单 -->
    <ComboBox ItemsSource="{Binding AvailablePresets}"
              SelectedItem="{Binding RenderStyle}"
              HorizontalAlignment="Stretch"/>

    <!-- 导入/导出按钮 -->
    <StackPanel Orientation="Horizontal" Spacing="5">
        <Button Content="导入" Command="{Binding ImportStyleCommand}" />
        <Button Content="导出" Command="{Binding ExportStyleCommand}" />
    </StackPanel>
</StackPanel>
```

### 3.3 VideraView 绑定
**修改**：
```xml
<controls:VideraView Name="View3D"
    ...
    RenderStyle="{Binding RenderStyle}" />
```

---

## 4. 实施步骤

### 第1步：更新 D3D11 着色器
1. 修改 `D3D11ResourceFactory.cs` 中的 `GetShaderSource()` 方法
2. 添加 StyleBuffer 定义
3. 在 VSOutput 中添加 worldNormal 和 worldPos
4. 在 main_ps 中实现光照、色彩、透明度计算

### 第2步：修复 ConstantBuffer 绑定
1. 检查 `D3D11CommandExecutor.SetVertexBuffer()` 是否正确绑定 Constant Buffer
2. 确保 StyleBuffer 绑定到 Pixel Shader 的 slot 3

### 第3步：更新 Metal 着色器
1. 修改 `Shaders.metal`
2. 添加 StyleParams 结构体
3. 实现相同的光照和色彩计算逻辑

### 第4步：更新 Demo 程序
1. 修改 `MainWindowViewModel.cs` 添加风格相关属性和命令
2. 修改 `MainWindow.axaml` 添加风格选择 UI
3. 添加 using 引用 `Videra.Core.Styles.Presets`

### 第5步：测试验证
1. 运行 Demo 程序
2. 验证各预设风格切换效果
3. 验证导入/导出功能

---

## 5. StyleBuffer 内存布局对照

确保 C# 和 HLSL 的内存布局完全一致：

| Offset | C# FieldOffset | HLSL Field |
|--------|----------------|------------|
| 0 | AmbientIntensity (float) | ambientIntensity (float) |
| 4 | DiffuseIntensity (float) | diffuseIntensity (float) |
| 8 | SpecularIntensity (float) | specularIntensity (float) |
| 12 | SpecularPower (float) | specularPower (float) |
| 16 | LightDirection (Vector3) | lightDirection (float3) |
| 28 | - | _pad0 (float) |
| 32 | TintColor (Vector3) | tintColor (float3) |
| 44 | Saturation (float) | saturation (float) |
| 48 | Contrast (float) | contrast (float) |
| 52 | Brightness (float) | brightness (float) |
| 56 | - | _pad1 (float2) |
| 64 | OutlineColor (Vector4) | outlineColor (float4) |
| 80 | OutlineWidth (float) | outlineWidth (float) |
| 84 | OutlineEnabled (int) | outlineEnabled (int) |
| 88 | - | _pad2 (float2) |
| 96 | Opacity (float) | opacity (float) |
| 100 | UseVertexColor (int) | useVertexColor (int) |
| 104 | OverrideColor (Vector4) | overrideColor (float4) |
| 120 | WireframeMode (int) | wireframeMode (int) |
| 124 | - | _pad4 (float3) → 128 bytes total |

**注意**：HLSL cbuffer 需要 16 字节对齐，需要添加 padding 字段。

---

## 6. 风险与注意事项

1. **内存对齐**：HLSL cbuffer 要求 16 字节对齐，需确保 padding 正确
2. **Constant Buffer 绑定**：D3D11 中 Constant Buffer 需要同时绑定到 VS 和 PS
3. **跨平台一致性**：确保 Metal 和 HLSL 的光照计算结果一致
4. **默认值**：确保引擎启动时使用 Realistic 预设的默认值
