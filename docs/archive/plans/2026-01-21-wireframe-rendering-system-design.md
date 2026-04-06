# 真实网格/线框渲染系统设计文档

## 1. 概述

### 1.1 需求
- 实现真正的网格/线框显示效果（模型边缘线渲染）
- 支持两种显示模式：
  - **全部线模式**：显示所有边缘线（包括被遮挡的）
  - **可见线模式**：只显示可见的边缘线（隐藏线移除）
- 跨平台兼容：D3D11 / Metal / Vulkan

### 1.2 技术选型分析

| 方案 | 优点 | 缺点 | 跨平台 |
|------|------|------|--------|
| **Geometry Shader** | 实现简单 | Metal不支持GS，性能差 | ✗ |
| **重心坐标法** | 跨平台、抗锯齿、单Pass | 需要修改顶点数据 | ✓ |
| **双Pass光栅化** | 简单、无需改数据 | 两次Draw Call | ✓ |
| **Line Index Buffer** | 跨平台、性能好 | 需要额外索引缓冲 | ✓ |

**推荐方案**：采用 **Line Index Buffer 方案** 作为主方案，同时在着色器中支持 **双Pass模式** 实现可见线功能。

理由：
1. 完全跨平台兼容
2. 不需要修改现有顶点数据结构
3. 性能可预测
4. 可以利用现有的 `MeshTopology.Lines` 支持

---

## 2. 架构设计

### 2.1 核心组件

```
Videra.Core/Graphics/
├── Wireframe/                           # 新增线框模块
│   ├── WireframeMode.cs                 # 线框模式枚举
│   ├── WireframeRenderer.cs             # 线框渲染器
│   └── EdgeExtractor.cs                 # 边缘提取工具
├── Object3D.cs                          # 修改：添加线框缓冲
└── VideraEngine.cs                      # 修改：集成线框渲染
```

### 2.2 类图

```
┌─────────────────────────────────────────────────────────────┐
│                      WireframeMode                          │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  None        - 不显示线框                            │   │
│  │  AllEdges    - 显示所有边缘线（含隐藏线）            │   │
│  │  VisibleOnly - 只显示可见边缘线                      │   │
│  │  Overlay     - 线框叠加在实体上                      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Object3D                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  VertexBuffer      - 顶点缓冲                        │   │
│  │  IndexBuffer       - 三角形索引缓冲                   │   │
│  │  LineIndexBuffer   - 边缘线索引缓冲 (新增)           │   │
│  │  LineIndexCount    - 边缘线索引数量 (新增)           │   │
│  │  WorldBuffer       - 世界矩阵缓冲                    │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   WireframeRenderer                         │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Mode              - 当前线框模式                     │   │
│  │  LineColor         - 线框颜色                         │   │
│  │  LineWidth         - 线宽 (仅部分API支持)             │   │
│  │  DepthBias         - 深度偏移 (防止Z-fighting)        │   │
│  │                                                       │   │
│  │  + RenderWireframe(objects, camera, executor)        │   │
│  │  + RenderAllEdges(obj, executor)                     │   │
│  │  + RenderVisibleEdges(obj, executor)                 │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    EdgeExtractor                            │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  + ExtractEdges(triangleIndices) -> lineIndices      │   │
│  │  + ExtractUniqueEdges(mesh) -> HashSet<Edge>         │   │
│  │  + BuildLineIndexBuffer(edges) -> uint[]             │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 详细设计

### 3.1 WireframeMode 枚举

```csharp
namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 线框显示模式
/// </summary>
public enum WireframeMode
{
    /// <summary>不显示线框</summary>
    None = 0,

    /// <summary>显示所有边缘线（包括被遮挡的线，使用半透明或虚线）</summary>
    AllEdges,

    /// <summary>只显示可见的边缘线（隐藏线移除）</summary>
    VisibleOnly,

    /// <summary>线框叠加在实体渲染之上</summary>
    Overlay,

    /// <summary>只显示线框，不显示实体</summary>
    WireframeOnly
}
```

### 3.2 EdgeExtractor 边缘提取器

```csharp
namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 从三角形网格中提取边缘线
/// </summary>
public static class EdgeExtractor
{
    /// <summary>
    /// 边缘结构（规范化存储，确保唯一性）
    /// </summary>
    public readonly struct Edge : IEquatable<Edge>
    {
        public readonly uint V0;
        public readonly uint V1;

        public Edge(uint a, uint b)
        {
            // 规范化：小的索引在前
            if (a < b) { V0 = a; V1 = b; }
            else { V0 = b; V1 = a; }
        }

        public bool Equals(Edge other) => V0 == other.V0 && V1 == other.V1;
        public override int GetHashCode() => HashCode.Combine(V0, V1);
    }

    /// <summary>
    /// 从三角形索引数组中提取唯一边缘
    /// </summary>
    /// <param name="triangleIndices">三角形索引数组（每3个索引一个三角形）</param>
    /// <returns>去重后的边缘线索引数组（每2个索引一条线）</returns>
    public static uint[] ExtractUniqueEdges(uint[] triangleIndices)
    {
        if (triangleIndices == null || triangleIndices.Length < 3)
            return Array.Empty<uint>();

        var uniqueEdges = new HashSet<Edge>();

        // 遍历每个三角形，提取3条边
        for (int i = 0; i < triangleIndices.Length; i += 3)
        {
            uint v0 = triangleIndices[i];
            uint v1 = triangleIndices[i + 1];
            uint v2 = triangleIndices[i + 2];

            uniqueEdges.Add(new Edge(v0, v1));
            uniqueEdges.Add(new Edge(v1, v2));
            uniqueEdges.Add(new Edge(v2, v0));
        }

        // 转换为线索引数组
        var lineIndices = new uint[uniqueEdges.Count * 2];
        int idx = 0;
        foreach (var edge in uniqueEdges)
        {
            lineIndices[idx++] = edge.V0;
            lineIndices[idx++] = edge.V1;
        }

        return lineIndices;
    }

    /// <summary>
    /// 计算边缘数量（用于预估内存）
    /// 对于封闭流形网格：E = 3F/2（每条边被2个三角形共享）
    /// </summary>
    public static int EstimateEdgeCount(int triangleCount)
    {
        return (int)(triangleCount * 1.5); // 保守估计
    }
}
```

### 3.3 WireframeRenderer 线框渲染器

```csharp
namespace Videra.Core.Graphics.Wireframe;

/// <summary>
/// 线框渲染器 - 负责渲染物体的边缘线
/// </summary>
public class WireframeRenderer : IDisposable
{
    private IPipeline? _linePipeline;
    private IBuffer? _wireframeUniformBuffer;
    private IResourceFactory? _factory;

    /// <summary>线框显示模式</summary>
    public WireframeMode Mode { get; set; } = WireframeMode.None;

    /// <summary>线框颜色</summary>
    public RgbaFloat LineColor { get; set; } = new(0f, 0f, 0f, 1f); // 黑色

    /// <summary>隐藏线颜色（AllEdges模式下使用）</summary>
    public RgbaFloat HiddenLineColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.3f); // 半透明灰

    /// <summary>深度偏移（用于防止Z-fighting）</summary>
    public float DepthBias { get; set; } = -0.0001f;

    /// <summary>是否显示隐藏线（AllEdges模式专用）</summary>
    public bool ShowHiddenLines { get; set; } = true;

    public void Initialize(IResourceFactory factory)
    {
        _factory = factory;

        // 创建线框专用Pipeline（使用Line拓扑）
        _linePipeline = factory.CreatePipeline(
            vertexSize: VertexPositionNormalColor.SizeInBytes,
            hasNormals: true,
            hasColors: true,
            topology: PrimitiveTopology.LineList  // 需要扩展CreatePipeline
        );

        // 创建线框参数Uniform Buffer
        _wireframeUniformBuffer = factory.CreateUniformBuffer(32);
    }

    /// <summary>
    /// 渲染场景中所有物体的线框
    /// </summary>
    public void RenderWireframes(
        IEnumerable<Object3D> objects,
        ICommandExecutor executor,
        OrbitCamera camera)
    {
        if (Mode == WireframeMode.None)
            return;

        foreach (var obj in objects)
        {
            if (obj.LineIndexBuffer == null || obj.LineIndexCount == 0)
                continue;

            RenderObjectWireframe(obj, executor, camera);
        }
    }

    private void RenderObjectWireframe(
        Object3D obj,
        ICommandExecutor executor,
        OrbitCamera camera)
    {
        switch (Mode)
        {
            case WireframeMode.AllEdges:
                RenderAllEdges(obj, executor);
                break;

            case WireframeMode.VisibleOnly:
            case WireframeMode.Overlay:
                RenderVisibleEdges(obj, executor);
                break;

            case WireframeMode.WireframeOnly:
                RenderWireframeOnly(obj, executor);
                break;
        }
    }

    /// <summary>
    /// 渲染所有边缘线（包括隐藏线）
    /// </summary>
    private void RenderAllEdges(Object3D obj, ICommandExecutor executor)
    {
        // Pass 1: 渲染隐藏线（禁用深度测试，使用半透明颜色）
        if (ShowHiddenLines)
        {
            executor.SetDepthState(testEnabled: false, writeEnabled: false);
            SetLineColor(HiddenLineColor);
            DrawLines(obj, executor);
        }

        // Pass 2: 渲染可见线（启用深度测试）
        executor.SetDepthState(testEnabled: true, writeEnabled: false);
        executor.SetDepthBias(DepthBias, -1.0f);
        SetLineColor(LineColor);
        DrawLines(obj, executor);

        // 恢复深度状态
        executor.ResetDepthState();
    }

    /// <summary>
    /// 只渲染可见边缘线（隐藏线移除）
    /// </summary>
    private void RenderVisibleEdges(Object3D obj, ICommandExecutor executor)
    {
        // 假设实体已经渲染并写入深度缓冲
        // 使用深度测试来过滤隐藏线
        executor.SetDepthState(testEnabled: true, writeEnabled: false);
        executor.SetDepthBias(DepthBias, -1.0f);
        SetLineColor(LineColor);
        DrawLines(obj, executor);
        executor.ResetDepthState();
    }

    /// <summary>
    /// 只渲染线框（不渲染实体）
    /// </summary>
    private void RenderWireframeOnly(Object3D obj, ICommandExecutor executor)
    {
        executor.SetDepthState(testEnabled: true, writeEnabled: true);
        SetLineColor(LineColor);
        DrawLines(obj, executor);
    }

    private void DrawLines(Object3D obj, ICommandExecutor executor)
    {
        executor.SetPipeline(_linePipeline);
        executor.SetVertexBuffer(obj.VertexBuffer, 0);
        executor.SetVertexBuffer(obj.WorldBuffer, 2);
        executor.SetIndexBuffer(obj.LineIndexBuffer);

        // 使用Line拓扑绘制
        executor.DrawIndexed(1, obj.LineIndexCount, 1, 0, 0, 0);
    }

    private void SetLineColor(RgbaFloat color)
    {
        // 更新Uniform Buffer中的线颜色
        _wireframeUniformBuffer?.SetData(color, 0);
    }

    public void Dispose()
    {
        _linePipeline?.Dispose();
        _wireframeUniformBuffer?.Dispose();
    }
}
```

### 3.4 Object3D 扩展

```csharp
// 在 Object3D.cs 中添加

public class Object3D : IDisposable
{
    // ... 现有属性 ...

    // --- 新增：线框缓冲 ---
    internal IBuffer? LineIndexBuffer { get; private set; }
    internal uint LineIndexCount { get; private set; }

    /// <summary>
    /// 初始化线框渲染资源（从三角形网格提取边缘）
    /// </summary>
    public void InitializeWireframe(IResourceFactory factory)
    {
        if (IndexBuffer == null)
            return;

        // 从三角形索引中提取唯一边缘
        var lineIndices = EdgeExtractor.ExtractUniqueEdges(GetTriangleIndices());

        if (lineIndices.Length == 0)
            return;

        LineIndexCount = (uint)lineIndices.Length;
        LineIndexBuffer = factory.CreateIndexBuffer(lineIndices);

        Console.WriteLine($"[Object3D '{Name}'] Wireframe: {LineIndexCount / 2} edges");
    }

    // 获取三角形索引（需要从GPU回读或缓存）
    private uint[] _cachedTriangleIndices; // 在Initialize时缓存
    private uint[] GetTriangleIndices() => _cachedTriangleIndices;

    public void Dispose()
    {
        // ... 现有清理 ...
        LineIndexBuffer?.Dispose();
    }
}
```

---

## 4. ICommandExecutor 接口扩展

```csharp
// 在 ICommandExecutor.cs 中添加

public interface ICommandExecutor
{
    // ... 现有方法 ...

    /// <summary>设置深度状态</summary>
    void SetDepthState(bool testEnabled, bool writeEnabled);

    /// <summary>设置深度偏移（用于防止Z-fighting）</summary>
    void SetDepthBias(float constantBias, float slopeBias);

    /// <summary>重置深度状态为默认</summary>
    void ResetDepthState();
}
```

---

## 5. 渲染流程

### 5.1 完整渲染流程

```
┌─────────────────────────────────────────────────────────────┐
│                    VideraEngine.Draw()                      │
├─────────────────────────────────────────────────────────────┤
│  1. BeginFrame()                                            │
│  2. Clear()                                                 │
│  3. 更新相机Uniform                                          │
│                                                             │
│  4. 渲染Grid                                                 │
│                                                             │
│  5. 渲染实体 (根据WireframeMode决定是否渲染)                  │
│     └─ if (Mode != WireframeOnly) RenderSolid()             │
│                                                             │
│  6. 渲染线框 (如果启用)                                       │
│     └─ WireframeRenderer.RenderWireframes()                 │
│        ├─ AllEdges模式:                                      │
│        │   ├─ Pass 1: 禁用深度测试，渲染隐藏线               │
│        │   └─ Pass 2: 启用深度测试，渲染可见线               │
│        ├─ VisibleOnly/Overlay模式:                          │
│        │   └─ 启用深度测试，渲染可见线                       │
│        └─ WireframeOnly模式:                                 │
│            └─ 清除深度，渲染所有线                           │
│                                                             │
│  7. 渲染坐标轴                                               │
│  8. EndFrame()                                              │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 深度处理策略

| 模式 | 实体渲染 | 深度测试 | 深度写入 | 深度偏移 | 效果 |
|------|---------|---------|---------|---------|------|
| AllEdges (隐藏线) | ✓ | ✗ | ✗ | - | 半透明隐藏线 |
| AllEdges (可见线) | ✓ | ✓ | ✗ | ✓ | 清晰可见线 |
| VisibleOnly | ✓ | ✓ | ✗ | ✓ | 只有可见线 |
| Overlay | ✓ | ✓ | ✗ | ✓ | 线框覆盖实体 |
| WireframeOnly | ✗ | ✓ | ✓ | - | 纯线框 |

---

## 6. 平台实现

### 6.1 D3D11 深度状态实现

```csharp
// D3D11CommandExecutor.cs

private ComPtr<ID3D11DepthStencilState> _depthTestWriteState;   // 默认
private ComPtr<ID3D11DepthStencilState> _depthTestOnlyState;    // 只测试不写入
private ComPtr<ID3D11DepthStencilState> _depthDisabledState;    // 禁用深度

public void SetDepthState(bool testEnabled, bool writeEnabled)
{
    if (!testEnabled)
    {
        _context.Handle->OMSetDepthStencilState(_depthDisabledState.Handle, 0);
    }
    else if (!writeEnabled)
    {
        _context.Handle->OMSetDepthStencilState(_depthTestOnlyState.Handle, 0);
    }
    else
    {
        _context.Handle->OMSetDepthStencilState(_depthTestWriteState.Handle, 0);
    }
}

public void SetDepthBias(float constantBias, float slopeBias)
{
    // 需要动态创建或切换RasterizerState
    // 或者在着色器中实现深度偏移
}
```

### 6.2 Metal 深度状态实现

```swift
// MetalCommandExecutor.swift

let depthStencilDescriptor = MTLDepthStencilDescriptor()
depthStencilDescriptor.depthCompareFunction = testEnabled ? .lessEqual : .always
depthStencilDescriptor.isDepthWriteEnabled = writeEnabled

let depthState = device.makeDepthStencilState(descriptor: depthStencilDescriptor)
renderEncoder.setDepthStencilState(depthState)
```

---

## 7. VideraView / Demo 集成

### 7.1 VideraView 新增属性

```csharp
// VideraView.cs

public static readonly StyledProperty<WireframeMode> WireframeModeProperty =
    AvaloniaProperty.Register<VideraView, WireframeMode>(
        nameof(WireframeMode),
        defaultValue: WireframeMode.None);

public static readonly StyledProperty<Color> WireframeColorProperty =
    AvaloniaProperty.Register<VideraView, Color>(
        nameof(WireframeColor),
        Colors.Black);

public WireframeMode WireframeMode
{
    get => GetValue(WireframeModeProperty);
    set => SetValue(WireframeModeProperty, value);
}

public Color WireframeColor
{
    get => GetValue(WireframeColorProperty);
    set => SetValue(WireframeColorProperty, value);
}
```

### 7.2 Demo UI 更新

```xml
<!-- MainWindow.axaml -->
<StackPanel Spacing="10">
    <TextBlock Text="WIREFRAME" FontWeight="Bold" Foreground="#007ACC" FontSize="12"/>

    <ComboBox ItemsSource="{Binding WireframeModes}"
              SelectedItem="{Binding WireframeMode}"
              HorizontalAlignment="Stretch">
        <ComboBox.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding}" />
            </DataTemplate>
        </ComboBox.ItemTemplate>
    </ComboBox>

    <Grid ColumnDefinitions="Auto, *" IsVisible="{Binding IsWireframeEnabled}">
        <TextBlock Text="Line Color" Foreground="#CCCCCC" VerticalAlignment="Center"/>
        <ColorPicker Grid.Column="1" Color="{Binding WireframeColor}" HorizontalAlignment="Right"/>
    </Grid>

    <CheckBox IsChecked="{Binding ShowHiddenLines}"
              Content="Show Hidden Lines"
              Foreground="#CCCCCC"
              IsVisible="{Binding IsAllEdgesMode}" />
</StackPanel>
```

---

## 8. 实施步骤

### Phase 1: 核心基础设施
1. 创建 `Videra.Core/Graphics/Wireframe/` 目录
2. 实现 `WireframeMode.cs` 枚举
3. 实现 `EdgeExtractor.cs` 边缘提取工具
4. 扩展 `Object3D.cs` 添加线框缓冲

### Phase 2: 渲染器实现
5. 实现 `WireframeRenderer.cs`
6. 扩展 `ICommandExecutor` 接口
7. D3D11 实现 `SetDepthState` / `SetDepthBias`
8. Metal 实现 `SetDepthState` / `SetDepthBias`

### Phase 3: 引擎集成
9. 修改 `VideraEngine.cs` 集成 `WireframeRenderer`
10. 在 `MeshData` 加载时自动生成线框缓冲
11. 修改渲染流程支持不同线框模式

### Phase 4: UI 集成
12. 扩展 `VideraView` 添加线框相关属性
13. 更新 Demo `MainWindowViewModel`
14. 更新 Demo `MainWindow.axaml` UI

### Phase 5: 测试验证
15. 测试各种线框模式效果
16. 验证跨平台兼容性
17. 性能测试

---

## 9. 文件清单

### 9.1 新增文件
- `Videra.Core/Graphics/Wireframe/WireframeMode.cs`
- `Videra.Core/Graphics/Wireframe/EdgeExtractor.cs`
- `Videra.Core/Graphics/Wireframe/WireframeRenderer.cs`

### 9.2 修改文件
- `Videra.Core/Graphics/Object3D.cs` - 添加线框缓冲
- `Videra.Core/Graphics/VideraEngine.cs` - 集成线框渲染
- `Videra.Core/Graphics/Abstractions/ICommandExecutor.cs` - 添加深度控制接口
- `Videra.Platform.Windows/D3D11CommandExecutor.cs` - 实现深度状态
- `Videra.Platform.macOS/MetalCommandExecutor.swift` - 实现深度状态
- `Videra.Avalonia/Controls/VideraView.cs` - 添加线框属性
- `Videra.Demo/ViewModels/MainWindowViewModel.cs` - 添加线框控制
- `Videra.Demo/Views/MainWindow.axaml` - 添加线框UI

---

## 10. 性能考虑

### 10.1 内存开销
- Line Index Buffer 大小：约为 Triangle Index Buffer 的 1-1.5 倍
- 例：10万三角形 ≈ 15万边缘 ≈ 30万索引 ≈ 1.2MB

### 10.2 渲染开销
- AllEdges 模式：2次 Draw Call（隐藏线 + 可见线）
- 其他模式：1次 Draw Call
- 建议：大型场景考虑 LOD 或距离裁剪

### 10.3 优化建议
- 批处理相同材质的物体
- 对远距离物体禁用线框
- 考虑使用 GPU Instancing 渲染多个相同物体的线框
